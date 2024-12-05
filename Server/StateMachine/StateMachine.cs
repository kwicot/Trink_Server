using Kwicot;
using Kwicot.Server.ClientLibrary.Models.Enums;
using Kwicot.Server.Models;
using Riptide;
using Riptide.Utils;
using Server;
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
        private SvaraState svaraState;
        private EndState endState;

        private Dictionary<Type, GameState> _statesMap;


        public float RoundDelay { get; private set; } = 5;
        public float StartDelay { get; private set; } = 3;
        public float DealDelay { get; private set; } = 0.3f;
        public float TurnDelay { get; private set; } = 2;
        public float TurnWait { get; private set; } = 60;
        public float EngGameWait { get; set; } = 10;
        public float SvaraStartDelay { get; set; } = 5;
        public float SvaraEnterWait { get; set; } = 15;
        public bool IsHideTurn { get; set; }
        
        public bool WaitWithdraw { get; private set; }
        public int EnterPrice { get; private set; }
        public int SvaraEnterPrice { get; set; }
        public int MinBet { get; set; }
        public int HideBet { get; set; }
        public int DealerIndex { get; set; } = -1;
        public int MinAllowedBalance => EnterPrice * 20;
        public int MaxAllowedBalance => EnterPrice * 200;
        public List<int> PlaySeats { get; set; } = new();
        public List<string> Actions { get; private set; } = new List<string>();

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

        public BetsData BetsData { get; set; } = new BetsData();

        public Dictionary<int, TurnType> LapBets { get; set; } = new();
        public int LapTurns = 0;
        
        private int _tablePercentSum;


        public int Balance => BetsData.TotalBank - _tablePercentSum;

        public RoomController RoomController { get; }

        //public bool IsReady => tableController.IsReady;
        public bool PlayerCheckedCards { get; set; }
        public CardsHolder CardsHolder => cardsHolder;
        public bool IsWaitingState => _currentState == waitingState | _currentState == endState;

        public bool CanAddMoney
        {
            get
            {
                bool isWaitingState = _currentState == waitingState;
                bool isEndState = _currentState == endState;
                bool ssSvaraState = _currentState == svaraState && svaraState.WaitEnter; 
                bool isPassTurn = false;

                bool result = isWaitingState || isEndState || ssSvaraState || isPassTurn;

                return result;
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
            
            EnterPrice = RoomController.RoomInfo.RoomSettings.StartBet;
            MinBet = EnterPrice;

            SetState(waitingState);
        }


        public void NewGame()
        {
            MinBet = EnterPrice;
            
            LapBets.Clear();
            PlaySeats.Clear();
            DealerIndex = -1;
            IsHideTurn = true;

            _tablePercentSum = 0;

            BetsData.Bets.Clear();

            SetState<WaitingState>();
        }

        public void TakePercent(int value)
        {
            _tablePercentSum = value;
        }

        public void OnAction(string action)
        {
            Actions.Add(action);
            if(Actions.Count > 50)
                Actions.RemoveAt(0);
        }

        public void OnSeatEnterSvara(SeatController seatController, bool enter)
        {
            int seatIndex = seatController.Index;
            if (_currentState == svaraState)
            {
                if (enter)
                {
                    PlaySeats.Add(seatIndex);
                    svaraState.OnPlayerEnter();
                    
                    RoomController.Seats[seatIndex].Withdraw(SvaraEnterPrice);
                    BetsData.Bets[seatIndex] += SvaraEnterPrice;
                }
                else
                {
                    svaraState.OnPlayerPass();
                }
                
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

        public void OnSeatTurn(SeatController seatController, int value)
        {
            if (_currentState == turnState)
            {
                turnState.OnTurn(seatController.Index, value);
            }
        }

        public void OnStartSvaraWaitEnter(int value)
        {
            foreach (var seatIndex in PlaySeats)
            {
                var seat = RoomController.Seats[seatIndex];
                seat.OnSvara();
            }
        }

        public void OnPlayerLeft(int seatIndex)
        {
            if (_currentState == turnState)
            {
                if(turnState.CurrentTurnSeatIndex == seatIndex)
                    turnState.OnTurn(seatIndex, -1);
                
                PlaySeats.Remove(seatIndex);
            }
            else
            {
                PlaySeats.Remove(seatIndex);
            }
        }



        void InitializeStates()
        {
            waitingState = new WaitingState(this);
            withdrawState = new WithdrawState(this);
            dealState = new DealState(this);
            turnState = new TurnState(this);
            calcState = new CalcState(this);
            endState = new EndState(this);
            svaraState = new SvaraState(this);

            _statesMap = new Dictionary<Type, GameState>();
            _statesMap[typeof(WaitingState)] = waitingState;
            _statesMap[typeof(WithdrawState)] = withdrawState;
            _statesMap[typeof(DealState)] = dealState;
            _statesMap[typeof(TurnState)] = turnState;
            _statesMap[typeof(CalcState)] = calcState;
            _statesMap[typeof(EndState)] = endState;
            _statesMap[typeof(SvaraState)] = svaraState;
        }

        void SetState(GameState state)
        {
            if (_currentState != null)
                _currentState.Exit();

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
        void SendMessage(Message msg, params ushort[] clientIds) => Kwicot.Server.Server.SendMessage(msg, clientIds);

    }
}