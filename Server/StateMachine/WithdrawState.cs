
namespace Trink_RiptideServer.Library.StateMachine
{
    public class WithdrawState : GameState
    {
        protected override void OnEnter()
        {
            _stateMachine.BetsData = new BetsData()
            {
                Bets = new Dictionary<int, int>()
            };

            List<int> readySeats = new List<int>();
            var seats = _stateMachine.RoomController.Seats;
            for (int i = 0; i < seats.Length; i++)
            {
                if (seats[i].IsReady && seats[i].Balance >= _stateMachine.MinAllowedBalance)
                {
                    seats[i].Withdraw(_stateMachine.EnterPrice);
                    readySeats.Add(i);
                    _stateMachine.BetsData.Bets[i] = _stateMachine.EnterPrice;
                }
            }

            _stateMachine.PlaySeats = readySeats;
            _stateMachine.MinBet = _stateMachine.EnterPrice * 2;


            _stateMachine.SetState<DealState>();
            
            SendEnterMessage("Збір вступних");
        }

        protected override void OnTick()
        {
            
        }

        protected override void OnExit()
        {
        }



        public WithdrawState(StateMachine stateMachine) : base(stateMachine)
        {
        }
    }
}