
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
            _stateMachine.SendStatus("Очікування початку");

            _stateMachine.WinsData = null;
            _stateMachine.BetsData.Bets.Clear();

            WaitingEnd = false;
            
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
            
            _stateMachine.SendData();
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
            _cancellationTokenSource?.Dispose();
        }


        async Task Wait()
        {
            await Task.Delay((int)Config.DebugDelay);
            
            while (!_stateMachine.IsReady)
            {
                await Task.Delay(1000);
                if (_cancellationTokenSource.IsCancellationRequested)
                {
                    Logger.LogInfo(Tag, "Cancel waiting");
                    return;
                }
            }

            
            int time = (int)Config.StartDelay / 1000;
            DateTime startTime = DateTime.Now;
            while ( time > 0)
            {
                await Task.Delay(100);

                var timeLeft = DateTime.Now- startTime;
                time -= timeLeft.Seconds;

                if (_cancellationTokenSource.IsCancellationRequested)
                {
                    Logger.LogInfo(Tag, "Cancel waiting");
                    return;
                }
            }
            
            Logger.LogInfo(Tag, "End waiting");
            _stateMachine.SetState<WithdrawState>();
        }
        

        public WaitingState(StateMachine stateMachine) : base(stateMachine)
        {
        }
    }
}