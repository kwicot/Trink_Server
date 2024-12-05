
using System;
using System.Threading;
using System.Threading.Tasks;
using Riptide.Utils;
using WindowsFormsApp1;
using LogType = Riptide.Utils.LogType;

namespace Trink_RiptideServer.Library.StateMachine
{
    public class WaitingState : GameState
    {
        private Task _task;
        private CancellationTokenSource _cancellationTokenSource;
        protected override void OnEnter()
        {
            Tag = $"{_stateMachine.RoomController.Tag}_State_Waiting";
            Logger.LogInfo(Tag, "Enter");
            
            _cancellationTokenSource = new CancellationTokenSource();
            _task = Task.Run(() => 
            {
                try
                {
                    Wait().Wait();
                }
                catch (Exception ex)
                {
                    Logger.LogError(Tag, $"Exception in Wait: {ex}");
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

        public override void Dispose()
        {
            _cancellationTokenSource?.Cancel();
            _task?.Dispose();
            _cancellationTokenSource?.Dispose();
        }


        async Task Wait()
        {
            while (!_stateMachine.IsReady)
            {
                await Task.Delay(1000);
                Logger.LogInfo(Tag,"Not ready to start. Waiting 1s");
            }

            await Task.Delay((int)(Config.StartDelay));
            
            _stateMachine.SetState<WithdrawState>();
        }
        

        public WaitingState(StateMachine stateMachine) : base(stateMachine)
        {
        }
    }
}