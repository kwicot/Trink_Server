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
            _stateMachine.BetsData.Bets.Clear();
            _stateMachine.PlaySeats.Clear();
            
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
            _stateMachine.DealEnd = false;
            _stateMachine.SendData();
            
            foreach (var seat in _stateMachine.RoomController.Seats)
            {
                seat.EndGame();
                seat.ClearOut();
            }

            await Task.Delay((int)(Program.Config.StateMachineConfig.EndDelay));
            NextState();
        }

        void NextState()
        {
            if (WaitingEnd)
            {
                WaitingEnd = false;
                return;
            }
            
            _stateMachine.NewGame();
        }

        public EndState(StateMachine stateMachine) : base(stateMachine)
        {
            
        }
    }
}