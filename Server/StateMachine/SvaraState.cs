
using Riptide.Utils;

namespace Trink_RiptideServer.Library.StateMachine
{
    public class SvaraState : GameState
    {
        private Task _task;
        private CancellationTokenSource _cancellationTokenSource;
        
        private List<int> enterSeats;
        public bool WaitEnter { get; private set; }

        private int _waitEnter;
        protected override void OnEnter()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _task = Task.Run(() => 
            {
                try
                {
                    StartSvaraCoroutine().Wait();
                }
                catch (Exception ex)
                {
                    RiptideLogger.Log(LogType.Error, $"Exception in Wait: {ex}");
                }
            }, _cancellationTokenSource.Token);
        }

        protected override void OnTick()
        {
        }

        protected override void OnExit()
        {
            _cancellationTokenSource.Cancel();
        }
        
        async Task StartSvaraCoroutine()
        {
            WaitEnter = true;
            _stateMachine.OnStartSvaraWaitEnter(_stateMachine.Balance);

            enterSeats = new List<int>();

            foreach (var seatIndex in _stateMachine.PlaySeats)
            {
                var seat = _stateMachine.RoomController.Seats[seatIndex];
                seat.ShowCardsToAll();
                seat.OfferToTopUpBalance();
                enterSeats.Add(seatIndex);
            }

            await Task.Delay((int)(_stateMachine.SvaraStartDelay * 1000));
            
            _waitEnter = 0;

            for (int i = 0; i < _stateMachine.RoomController.Seats.Length; i++)
            {
                var seat = _stateMachine.RoomController.Seats[i];
                if (!_stateMachine.PlaySeats.Contains(i) && seat.Balance >= _stateMachine.SvaraEnterPrice && !seat.IsFree)
                {
                    seat.OfferToJoinSvara(_stateMachine.SvaraEnterPrice);
                    _waitEnter++;
                }
            }
            
            await Task.Delay((int)(2 * 1000));

            _task = Task.Run(WaitEnterCoroutine, _cancellationTokenSource.Token);
        }
        
        async Task WaitEnterCoroutine()
        {
            float time = _stateMachine.SvaraEnterWait;
            while (true)
            {
                if(time < 0)
                    break;
                if(_waitEnter == 0)
                    break;
                
                time-- ;
                await Task.Delay((int)(1000));

            }
            
            
            for (int i = 0; i < _stateMachine.RoomController.Seats.Length; i++)
            {
                var seat = _stateMachine.RoomController.Seats[i];
                if (!_stateMachine.PlaySeats.Contains(i))
                {
                    seat.StopOfferToJoinSvara();
                }
                else
                {
                    seat.OnStartSvara();
                }
            }

            await Task.Delay((int)(2 * 1000));

            
            _stateMachine.PlaySeats.Sort();
            
            //_stateMachine.InfoText.text = "";
            _stateMachine.MinBet = _stateMachine.EnterPrice * 2;
            WaitEnter = false;
            _stateMachine.SetState<DealState>();
        }

        public void OnPlayerEnter()
        {
            _waitEnter--;
        }

        public void OnPlayerPass()
        {
            _waitEnter--;
        }

        public SvaraState(StateMachine stateMachine) : base(stateMachine)
        {
        }
    }
}