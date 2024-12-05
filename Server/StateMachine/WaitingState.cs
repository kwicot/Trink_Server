
using Riptide.Utils;

namespace Trink_RiptideServer.Library.StateMachine
{
    public class WaitingState : GameState
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
            while (!_stateMachine.IsReady)
            {
                await Task.Delay(1000);
                SendEnterMessage("Очікування");
            }
            
            SendEnterMessage("Початок");

            await Task.Delay((int)(_stateMachine.StartDelay * 1000));
            
            _stateMachine.SetState<WithdrawState>();
        }
        

        public WaitingState(StateMachine stateMachine) : base(stateMachine)
        {
        }
    }
}