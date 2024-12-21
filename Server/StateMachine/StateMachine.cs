using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kwicot.Server.ClientLibrary.Models.Enums;
using Model;
using Riptide;
using Server.Core.Rooms;
using Trink_RiptideServer.Library.Cards;
using WindowsFormsApp1;
using WindowsFormsApp1.StateMachine;

namespace Trink_RiptideServer.Library.StateMachine
{
    public class StateMachine
    {
        public StateMachine(RoomController roomController)
        {
            this.RoomController = roomController;
            
            InitializeStates();
            SetUp();
        }
        
        private CardsHolder cardsHolder;

        private WaitingState waitingState;
        private WithdrawState withdrawState;
        private DealState dealState;
        private TurnState turnState;
        private CalcState calcState;
        private EndState endState;

        private Dictionary<Type, GameState> _statesMap;

        public int LapTurns = 0;

        public static string Tag => "State_machine";
        public bool IsHideTurn { get; set; }
        public int Bet { get; set; }
        public int DealerIndex { get; set; } = -1;
        public List<int> PlaySeats { get; set; } = new();
        public BetsData BetsData { get; set; } = new BetsData();
        public WinsData WinsData { get; set; }

        public Dictionary<int, TurnType> LapBets { get; set; } = new();
        public RoomController RoomController { get; }
        public List<string> Actions { get; set; }
        public bool PlayerCheckedCards { get; set; }
        public CardsHolder CardsHolder => cardsHolder;

        private string _status;
        public bool DealEnd = false;

        public int Balance => BetsData.TotalBank;
        public bool CanTopUpBalance => _currentState == waitingState || _currentState == endState;

        public bool IsReady
        {
            get
            {
                int ready = 0;
                foreach (var seat in RoomController.Seats)
                {
                    if (seat.IsReady)
                    {
                        Logger.LogError(Tag, $"Seat {seat.Index} is ready");
                        ready++;
                    }
                }

                return ready >= 2;
            }
        }

        public int InGameSeats
        {
            get
            {
                int value = 0;
                foreach (var playSeat in PlaySeats)
                {
                    if (LapBets.TryGetValue(playSeat, out var turn))
                    {
                        if (turn != TurnType.Pass && !RoomController.Seats[playSeat].IsFree && !RoomController.Seats[playSeat].SeatData.IsOut)
                            value++;
                    }
                    else
                    {
                        value++;
                    }
                }
                
                //Debug.Log($"In game seats {value}");
                return value;
            }
        }
        public int FirstInGameSeatIndex
        {
            get
            {
                foreach (var playSeat in PlaySeats)
                {
                    if (LapBets.TryGetValue(playSeat, out var turn))
                    {
                        if (turn != TurnType.Pass)
                            return playSeat;
                    }
                    else
                    {
                        return playSeat;
                    }
                }
                
                //Debug.Log($"In game seats {value}");
                return -1;
            }
        }

        public int GetMaxBet()
        {
            int maxBet = Int32.MinValue;
            int maxBetIndex;

            foreach (var seatIndex in PlaySeats)
            {
                if (BetsData.Bets.TryGetValue(seatIndex, out var bets))
                {
                    if (bets > maxBet)
                    {
                        maxBet = bets;
                        maxBetIndex = seatIndex;
                    }
                }
            }

            return maxBet;
        }

        private GameState _currentState;

        private void Start()
        {
            
        }

        void SetUp()
        {
            cardsHolder = new CardsHolder();
            Actions = new List<string>();
            
            SetState(waitingState);
        }

        public void OnStartTurns()
        {
            Bet = RoomController.RoomInfo.RoomSettings.StartBet;
        }

        public void OnSeatTurn(int seatIndex, int value)
        {
            try
            {
                if (_currentState == turnState)
                {
                    RoomController.SendToAll(CreateMessage(ServerToClientId.seatTurn)
                        .AddInt(seatIndex)
                        .AddInt(value)
                        .AddInt(BetsData.Bets[seatIndex]));

                    if (!IsHideTurn || PlayerCheckedCards)
                    {
                        IsHideTurn = false;
                        foreach (var playSeat in PlaySeats)
                            RoomController.Seats[playSeat].ShowCardsLocal();
                    }
                    
                    turnState.OnTurn(seatIndex, value);
                
                    SendData();
                }
            }
            catch (Exception e)
            {
                Logger.LogError(Tag, e.Message);
            }
        }

        public void OnPlayerRemove(int seatIndex)
        {
            Logger.LogInfo(Tag,$"OnPlayerRemove [{seatIndex}]");
            
            if (_currentState == turnState)
            {
                turnState.OnPlayerRemove(seatIndex);
            }
        }
        
        public void OnSeatCheckCards(int seatIndex)
        {
            if (_currentState == turnState)
            {
                PlayerCheckedCards = true;
            
                SendData();
                
                Actions.Add($"{RoomController.Seats[seatIndex].UserData.UserProfile.NickName}: Подивився карти");
                
                int index = turnState.CurrentTurnSeatIndex;
                var seat = RoomController.Seats[index];
                if (PlaySeats.Contains(index))
                {
                    if (LapBets.TryGetValue(index, out var turn))
                    {
                        if(turn != TurnType.Pass)
                            seat.ShowCardsLocal();
                    }
                    else
                    {
                        seat.ShowCardsLocal();
                    }
                }
            }
            
            
        }

        public void NewGame()
        {
            LapBets.Clear();
            PlaySeats.Clear();
            DealerIndex = -1;
            DealEnd = false;
            IsHideTurn = true;
            LapTurns = 0;
        
            BetsData.Bets.Clear();
        
           SetState<WaitingState>();
        }

        public async Task OnServerStopping()
        {
            if (_currentState == calcState || _currentState == withdrawState || _currentState == endState)
            {
                await _currentState.ProcessServerStopping();
            }
            else if (_currentState == turnState)
            {
                foreach (var betData in BetsData.Bets)
                {
                    var seat = RoomController.Seats[betData.Key];
                    if (seat != null)
                    {
                        seat.Return(betData.Value);
                        await seat.RemovePlayer(false);
                    }
                }
            }
        }

        public void SendData()
        {
            var message = CreateMessage(ServerToClientId.updateStateMachineData);
            
            message.AddBetsData(BetsData);
            message.AddBool(IsHideTurn);
            message.AddBool(DealEnd);
            message.AddBool(CanTopUpBalance);
            if (WinsData != null)
                message.AddInt(WinsData.TotalBank);
            else message.AddInt(BetsData.TotalBank);
            
            message.AddInts(PlaySeats.ToArray());
            message.AddStrings(Actions.ToArray());
            
            RoomController.SendToAll(message);
            SendStatus(_status);
            
            foreach (var seat in RoomController.Seats)
            {
                seat.SendData();
            }
        }

        public void SendStatus(string status)
        {
            _status = status;
            RoomController.SendToAll(CreateMessage(ServerToClientId.updateRoomStatus)
                .AddString(_status));
        }
        

        void InitializeStates()
        {
            waitingState = new WaitingState(this);
            withdrawState = new WithdrawState(this);
            dealState = new DealState(this);
            turnState = new TurnState(this);
            calcState = new CalcState(this);
            endState = new EndState(this);

            _statesMap = new Dictionary<Type, GameState>();
            _statesMap[typeof(WaitingState)] = waitingState;
            _statesMap[typeof(WithdrawState)] = withdrawState;
            _statesMap[typeof(DealState)] = dealState;
            _statesMap[typeof(TurnState)] = turnState;
            _statesMap[typeof(CalcState)] = calcState;
            _statesMap[typeof(EndState)] = endState;
        }

        void SetState(GameState state)
        {
            if (_currentState != null)
            {
                _currentState.Exit();
                _currentState.Dispose();
            }

            _currentState = state;
            _currentState.Enter();
            SendData();
        }

        public T SetState<T>() where T : GameState
        {
            if (!_statesMap.ContainsKey(typeof(T)))
                throw new NullReferenceException($"Cant find state typeof {typeof(T)}");

            var state = _statesMap[typeof(T)];
            SetState(state);
            return state as T;
        }
        
        Message CreateMessage(ServerToClientId id) => Message.Create(MessageSendMode.Reliable, id);
        void SendMessage(Message msg, ushort clientId) => Server.Core.Server.SendMessage(msg, clientId);

    }
}