using System.Threading.Tasks;
using Kwicot.Server.ClientLibrary.Models.Enums;
using Model;
using Org.BouncyCastle.Crypto.Signers;
using Riptide;
using Server.Core.Models;
using Trink_RiptideServer.Library.Cards;
using Trink_RiptideServer.Library.StateMachine;
using WindowsFormsApp1;

namespace Server.Core.Rooms
{
    public class SeatController
    {
        public string Tag => $"seat_controller_{Index}";
        
        public int Index { get; set; }
        public SeatData SeatData { get; private set; }
        public ClientData ClientData { get; private set; }
        public UserData UserData { get; private set; }
        
        public RoomController RoomController { get; set; }

        private int[] _cards;

        public int CardsSum => CardsHolder.GetCardsSum(_cards);

        public bool IsFree => SeatData == null;
        public bool WaitingTopUpBalance = false;

        private int _timerEndCounter = 0;

        public bool IsReady
        {
            get
            {
                if (IsFree)
                    return false;
                
                if(SeatData.Balance < RoomController.RoomInfo.RoomSettings.StartBet && !WaitingTopUpBalance)
                    OfferToTopUpBalance();
                
                bool ready = SeatData.Balance >= RoomController.RoomInfo.RoomSettings.StartBet && !SeatData.IsOut;
                if(!ready)
                    Logger.LogInfo(Tag, $"Seat {Index} not ready. Free: {IsFree}, Balance: {SeatData.Balance}, IsOut: {SeatData.IsOut}");
                return ready;
            }
        }
        public bool Waiting { get; private set; }

        public async void SetPlayer(ClientData clientData)
        {
            var userData = await UsersDatabase.GetUserData(clientData.FirebaseId);

            SeatData = new SeatData()
            {
                FirebaseId = clientData.FirebaseId,
                Balance = 0,
                ClientId = clientData.ClientID
            };
            ClientData = clientData;
            UserData = userData;
            
            _timerEndCounter = 0;

            _cards = new int[3];

            SendData();

            SendMessage(CreateMessage(ServerToClientId.seatRequestResult)
                    .AddBool(true)
                , clientData.ClientID);

            OfferToTopUpBalance();
        }

        public void ReturnPlayer(ClientData newClientData)
        {
            SeatData.ClientId = newClientData.ClientID;
            ClientData = newClientData;

            SeatData.IsOut = false;
            SendData();
            
            if(WaitingTopUpBalance)
                OfferToTopUpBalance();
        }
        public async Task RemovePlayer(bool waiting)
        {
            UserData.Balance += SeatData.Balance;
            await UsersDatabase.UpdateUserData(UserData);
            Waiting = waiting;
            
            if (RoomController.StateMachine.PlaySeats.Contains(Index))
            {
                Logger.LogInfo(Tag, "Out");
                SeatData.IsOut = true;
            }
            else
            {
                OnLeft();
            }
            
            SendData();
        }

        void OnLeft()
        {
            Logger.LogInfo(Tag, "OnLeft");
            SeatData = null;
            UserData = null;
            ClientData = null;
            WaitingTopUpBalance = false;
        }
        

        public void Withdraw(int value)
        {
            SeatData.Balance -= value;
            
            RoomController.SendToAll(CreateMessage(ServerToClientId.withdraw)
                .AddInt(value));
            
            SendData();
        }
        public void OfferToTopUpBalance()
        {
            if (!IsFree)
            {
                WaitingTopUpBalance = true;
                
                SendMessage(CreateMessage(ServerToClientId.offerToTopUpBalance)
                    .AddUserData(UserData)
                    .AddSeatData(SeatData)
                    , SeatData.ClientId);
            }
        }


        public void AddCard(int index, int card)
        {
            _cards[index] = card;
            RoomController.SendToAll(CreateMessage(ServerToClientId.addCard)
                .AddInt(index));
        }

        public void ShowCardsLocal()
        {
            SendMessage(CreateMessage(ServerToClientId.showCards)
                .AddInts(_cards)
                .AddInt(CardsSum)
                , SeatData.ClientId);
            
            RoomController.SendToAll(CreateMessage(ServerToClientId.seatCheckCards)
                , SeatData.ClientId);
        }
        
        public void ShowCardsToAll()
        {
            TurnType lastTurn = TurnType.No;
            RoomController.StateMachine.LapBets.TryGetValue(Index, out lastTurn);
            
            if (lastTurn != TurnType.Pass)
            {
                RoomController.SendToAll(CreateMessage(ServerToClientId.showCards)
                    .AddInts(_cards)
                    .AddInt(CardsSum));
            }
        }

        public void Turn(bool isHideTurn, float turnTime, bool isLastTurn, int minBet)
        {
            RoomController.SendToAll(CreateMessage(ServerToClientId.turnRequest)
                .AddBool(isHideTurn)
                .AddFloat(turnTime / 1000)
                .AddBool(isLastTurn)
                .AddInt(minBet));
        }

        public void OnTurn()
        {
            _timerEndCounter = 0;
        }
        public void EndTurn()
        {
            RoomController.SendToAll(CreateMessage(ServerToClientId.endTurn));
            _timerEndCounter++;

            if (_timerEndCounter >= Program.Config.StateMachineConfig.AfkTurnsMax)
                RemovePlayer(false);
        }

        public void Return(int value)
        {
            RoomController.StateMachine.Actions.Add($"{UserData.UserProfile.NickName}: Повернув [{value}]");

            SeatData.Balance += value;
            
            RoomController.SendToAll(CreateMessage(ServerToClientId.onReturn)
                .AddInt(value));
            
            SendData();
        }

        public void Win(int value)
        {
            RoomController.StateMachine.Actions.Add($"{UserData.UserProfile.NickName}: Виграв [{value}]");

            SeatData.Balance += value;

            RoomController.SendToAll(CreateMessage(ServerToClientId.onWin)
                .AddInt(value));

            SendData();
        }
        

        public void EndGame()
        {
            RoomController.SendToAll(CreateMessage(ServerToClientId.onEndGame));
            SendData();
        }
        
        
        
        public void SendData()
        {
            var message = CreateMessage(ServerToClientId.updateSeatData);
            message.AddBool(IsFree);
            if (!IsFree)
                message.AddSeatData(SeatData);
            
            RoomController.SendToAll(message);
        }
        public void SendData(ushort targetClient)
        {
            var message = CreateMessage(ServerToClientId.updateSeatData);
            message.AddBool(IsFree);
            if (!IsFree)
                message.AddSeatData(SeatData);

            SendMessage(message, targetClient);
        }

        public void ClearOut()
        {
             if(SeatData == null || SeatData.IsOut)
                OnLeft();
             
             SendData();
        }

        #region Message

        public async void OnRequestSeat(ClientData clientData)
        {
            Logger.LogInfo(Tag, "OnRequestSeat");
            if (IsFree)
            {
                var userData = await UsersDatabase.GetUserData(clientData.FirebaseId);

                if (userData.Balance >= RoomController.RoomInfo.RoomSettings.MinBalance)
                {
                    SetPlayer(clientData);
                }
                else
                {
                    SendMessage(CreateMessage(ServerToClientId.seatRequestResult)
                            .AddBool(false)
                            .AddInt((int)ErrorType.NOT_ENOUGH_MONEY)
                        , clientData.ClientID);
                    
                    Logger.LogInfo(Tag, $"Request seat from [{clientData.ClientID}] denied. Not enough money.");
                }
            }
            else
            {
                SendMessage(CreateMessage(ServerToClientId.seatRequestResult)
                        .AddBool(false)
                        .AddInt((int)ErrorType.NO_FREE_SPACE)
                    , clientData.ClientID);
                
                Logger.LogInfo(Tag, $"Request seat from [{clientData.ClientID}] denied. No free space.");
            }
        }

        public async void OnRequestTopUpBalance(int value)
        {
            if (!IsFree && value > 0)
            {
                Logger.LogInfo(Tag, $"Request to top up balance for [{value}]");

                var balance = SeatData.Balance;
                var maxAllowedBalance = RoomController.RoomInfo.RoomSettings.MaxBalance;
                var minNeedBalance = RoomController.RoomInfo.RoomSettings.MinBalance;

                var newBalance = balance + value;
                if (newBalance <= maxAllowedBalance && newBalance >= minNeedBalance && UserData.Balance >= value)
                {
                    SeatData.Balance += value;
                    UserData.Balance -= value;
                    
                    await UsersDatabase.UpdateUserData(UserData);
                    
                    SendMessage(CreateMessage(ServerToClientId.topUpBalanceResult)
                        .AddBool(true)
                        , SeatData.ClientId);
                    
                    SendData();
                }
                else if(UserData.Balance < value)
                {
                    SendMessage(CreateMessage(ServerToClientId.topUpBalanceResult)
                            .AddBool(false)
                            .AddInt((int)ErrorType.NOT_ENOUGH_MONEY)
                        , SeatData.ClientId);

                    await RemovePlayer(false);
                }
                else
                {
                    SendMessage(CreateMessage(ServerToClientId.topUpBalanceResult)
                            .AddBool(false)
                            .AddInt((int)ErrorType.INCORRECT_RANGE)
                        , SeatData.ClientId);
                }

                WaitingTopUpBalance = false;
            }
            else if (!IsFree && value <= 0)
            {
                if(SeatData.Balance < RoomController.RoomInfo.RoomSettings.MinBalance)
                    await RemovePlayer(false);
            }
        }

        #endregion
        
        Message CreateMessage(ServerToClientId id) => Message.Create(MessageSendMode.Reliable, id).AddInt(Index);
        void SendMessage(Message message, ushort clientId) => Server.SendMessage(message, clientId);
    }
}