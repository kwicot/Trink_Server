﻿using System.Threading.Tasks;
using Kwicot.Server.ClientLibrary.Models.Enums;
using Model;
using Riptide;
using Server.Core.Models;
using Trink_RiptideServer.Library.Cards;
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

        public bool IsOut { get; set; }
        public bool IsFree => SeatData == null;
        public bool IsReady => !IsFree && SeatData.Balance > RoomController.RoomInfo.RoomSettings.MinBalance && !IsOut;

        public async void SetPlayer(ClientData clientData, bool isRequest = false)
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
            _cards = new int[3];
            
            SendData();
            
            if (isRequest)
            {
                SendMessage(CreateMessage(ServerToClientId.seatRequestResult)
                        .AddBool(true)
                    , clientData.ClientID);
                
                OfferToTopUpBalance();
            }
        }
        public async Task RemovePlayer()
        {
            UserData.Balance += SeatData.Balance;
            await UsersDatabase.UpdateUserData(UserData);
            
            SeatData = null;
            UserData = null;
            ClientData = null;
            
            SendData();
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
                , SeatData.ClientId);
            
            RoomController.SendToAll(CreateMessage(ServerToClientId.seatCheckCards)
                , SeatData.ClientId);
        }
        
        public void ShowCardsToAll()
        {
            RoomController.SendToAll(CreateMessage(ServerToClientId.showCards)
                .AddInts(_cards));
        }

        public void Turn(bool isHideTurn, float turnTime, bool isLastTurn, int minBet)
        {
            RoomController.SendToAll(CreateMessage(ServerToClientId.turnRequest)
                .AddBool(isHideTurn)
                .AddFloat(turnTime / 1000)
                .AddBool(isLastTurn)
                .AddInt(minBet));
        }

        public void EndTurn()
        {
            RoomController.SendToAll(CreateMessage(ServerToClientId.endTurn));
        }

        public void Return(int value)
        {
            RoomController.SendToAll(CreateMessage(ServerToClientId.onReturn)
                .AddInt(value));
        }

        public void Win(int value)
        {
            RoomController.SendToAll(CreateMessage(ServerToClientId.onWin)
                .AddInt(value));
        }

        public void EndGame()
        {
            RoomController.SendToAll(CreateMessage(ServerToClientId.onEndGame));
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

        #region Message

        public async void OnRequestSeat(ClientData clientData)
        {
            Logger.LogInfo(Tag, "OnRequestSeat");
            if (IsFree)
            {
                var userData = await UsersDatabase.GetUserData(clientData.FirebaseId);

                if (userData.Balance >= RoomController.RoomInfo.RoomSettings.MinBalance)
                {
                    SetPlayer(clientData, true);
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

                    await RemovePlayer();
                }
                else
                {
                    SendMessage(CreateMessage(ServerToClientId.topUpBalanceResult)
                            .AddBool(false)
                            .AddInt((int)ErrorType.INCORRECT_RANGE)
                        , SeatData.ClientId);
                }
            }
            else if (!IsFree && value <= 0)
                await RemovePlayer();
        }

        #endregion
        
        Message CreateMessage(ServerToClientId id) => Message.Create(MessageSendMode.Reliable, id).AddInt(Index);
        void SendMessage(Message message, ushort clientId) => Server.SendMessage(message, clientId);
    }
}