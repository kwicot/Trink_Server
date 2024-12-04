using Kwicot.Server.ClientLibrary.Models.Enums;
using Model;
using Riptide;
using Server.Core.Models;
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
            

            if (isRequest)
            {
                SendMessage(CreateMessage(ServerToClientId.seatRequestResult)
                        .AddBool(true)
                    , clientData.ClientID);
            }
            
            SendData();
        }

        public async void RemovePlayer()
        {
            SeatData = null;
            UserData = null;
            ClientData = null;
            
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

        #endregion
        
        Message CreateMessage(ServerToClientId id) => Message.Create(MessageSendMode.Reliable, id).AddInt(Index);
        void SendMessage(Message message, ushort clientId) => Server.SendMessage(message, clientId);
    }
}