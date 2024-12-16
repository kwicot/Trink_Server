using System;
using System.Threading;
using System.Threading.Tasks;
using WindowsFormsApp1;

namespace Trink_RiptideServer.Library.StateMachine
{
    public class EndState : GameState
    {
        private Task _task;
        private CancellationTokenSource _cancellationTokenSource;
        protected override void OnEnter()
        {
            Tag = $"{_stateMachine.RoomController.Tag}_State_EndGame";
            _stateMachine.BetsData.TableCommission = 0;
            _stateMachine.BetsData.Bets.Clear();
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
                    Logger.LogError(Tag,$"Exception: {ex}" );
                }
            }, _cancellationTokenSource.Token);
            
            _stateMachine.SendStatus("Закінчення партії");
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
            _stateMachine.SendData();
            
            foreach (var seat in _stateMachine.RoomController.Seats)
            {
                seat.EndGame();
                seat.ClearOut();
            }

            await Task.Delay((int)(Program.Config.StateMachineConfig.EndDelay));

            _stateMachine.NewGame();
        }

        public EndState(StateMachine stateMachine) : base(stateMachine)
        {
            
        }
    }
}