using System;
using System.Collections.Generic;
using Kwicot.Server.ClientLibrary.Models.Enums;
using Model;
using Riptide;
using Server.Core.Rooms;
using Trink_RiptideServer.Library.Cards;

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
        private int _tablePercentSum;
        
        
        public bool IsHideTurn { get; set; }
        public int Bet { get; set; }
        public int DealerIndex { get; set; } = -1;
        public List<int> PlaySeats { get; set; } = new();
        public BetsData BetsData { get; set; } = new BetsData();
        public Dictionary<int, TurnType> LapBets { get; set; } = new();
        public RoomController RoomController { get; }
        public bool PlayerCheckedCards { get; set; }
        public CardsHolder CardsHolder => cardsHolder;

        public int Balance => BetsData.TotalBank - _tablePercentSum;

        public bool IsReady
        {
            get
            {
                int ready = 0;
                foreach (var seat in RoomController.Seats)
                {
                    if (seat.IsReady)
                        ready++;
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
                        if (turn != TurnType.Pass)
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
            
            SetState(waitingState);
        }

        public void OnStartTurns()
        {
            Bet = RoomController.RoomInfo.RoomSettings.StartBet;
        }

        public void OnSeatTurn(int seatIndex, int value)
        {
            if (_currentState == turnState)
            {
                turnState.OnTurn(seatIndex, value);
                
                RoomController.SendToAll(CreateMessage(ServerToClientId.seatTurn)
                    .AddInt(seatIndex)
                    .AddInt(value));
                
                SendData();
            }
        }
        
        public void OnSeatCheckCards()
        {
            if (_currentState == turnState)
            {
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
            
            PlayerCheckedCards = true;
        }

        public void NewGame()
        {
            LapBets.Clear();
            PlaySeats.Clear();
            DealerIndex = -1;
            IsHideTurn = true;
        
            _tablePercentSum = 0;
        
            BetsData.Bets.Clear();
        
            SetState<WaitingState>();
        }

        // public void TakePercent(int value)
        // {
        //     _tablePercentSum = value;
        // }
        //
        
        //
        // public void OnSeatTurn(SeatController seatController, int value)
        // {
        //     if (_currentState == turnState)
        //     {
        //         turnState.OnTurn(seatController.Index, value);
        //     }
        // }
        //
        // public void OnPlayerLeft(int seatIndex)
        // {
        //     if (_currentState == turnState)
        //     {
        //         if(turnState.CurrentTurnSeatIndex == seatIndex)
        //             turnState.OnTurn(seatIndex, -1);
        //         
        //         PlaySeats.Remove(seatIndex);
        //     }
        //     else
        //     {
        //         PlaySeats.Remove(seatIndex);
        //     }
        // }

        public void SendData()
        {
            var message = CreateMessage(ServerToClientId.updateStateMachineData);
            message.AddBetsData(BetsData);
            message.AddInts(PlaySeats.ToArray());

            RoomController.SendToAll(message);
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