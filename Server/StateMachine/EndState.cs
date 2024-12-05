
using Riptide.Utils;

namespace Trink_RiptideServer.Library.StateMachine
{
    public class EndState : GameState
    {
        private Task _task;
        private CancellationTokenSource _cancellationTokenSource;
        protected override void OnEnter()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _task = Task.Run(() => 
            {
                try
                {
                    Wait().Wait();
                }
                catch (Exception ex)
                {
                    RiptideLogger.Log(LogType.Error, $"Exception in Wait: {ex}");
                }
            }, _cancellationTokenSource.Token);
            
            SendEnterMessage("Закінчення партії");
        }

        protected override void OnTick()
        {
        }

        protected override void OnExit()
        {
            _cancellationTokenSource.Cancel();
        }

        async Task Wait()
        {
            foreach (var seat in _stateMachine.RoomController.Seats)
            {
                seat.PrepareToNewGame();
            }

            await Task.Delay((int)(_stateMachine.EngGameWait * 1000));

            _stateMachine.NewGame();
        }

        public EndState(StateMachine stateMachine) : base(stateMachine)
        {
            
        }
    }
}