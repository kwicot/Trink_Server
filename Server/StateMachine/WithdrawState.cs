
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Configuration;
using WindowsFormsApp1;

namespace Trink_RiptideServer.Library.StateMachine
{
    public class WithdrawState : GameState
    {
        protected override async void OnEnter()
        {
            Tag = $"{_stateMachine.RoomController.Tag}_State_Withdraw";
            Logger.LogInfo(Tag, "Enter");
            
            _stateMachine.SendStatus("Збір вступних");
            
            _stateMachine.BetsData = new BetsData()
            {
                Bets = new Dictionary<int, int>()
            };
            _stateMachine.Actions = new List<string>();

            if (!_stateMachine.IsReady)
            {
                _stateMachine.SetState<WaitingState>();
            }
            
            List<int> readySeats = new List<int>();
            var seats = _stateMachine.RoomController.Seats;
            for (int i = 0; i < seats.Length; i++)
            {
                if (seats[i].IsReady && seats[i].SeatData.Balance >= RoomSettings.MinBalance)
                {
                    seats[i].Withdraw(RoomSettings.StartBet);
                    _stateMachine.Actions.Add($"{seats[i].UserData.UserProfile.NickName}: Оплатив вступні [{RoomSettings.MinBalance}]");
                    
                    readySeats.Add(i);
                    _stateMachine.BetsData.Bets[seats[i].Index] = RoomSettings.StartBet;
                }
                else
                {
                    seats[i].OfferToTopUpBalance();
                }
            }
            _stateMachine.PlaySeats = readySeats;
            
            await Task.Delay((int)Config.DebugDelay);

            _stateMachine.SendData();
            
            _stateMachine.SetState<DealState>();
        }

        protected override void OnTick()
        {
            
        }

        protected override void OnExit()
        {
        }

        public override void Dispose()
        {
            
        }


        public WithdrawState(StateMachine stateMachine) : base(stateMachine)
        {
        }
    }
}