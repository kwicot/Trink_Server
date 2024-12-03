using Kwicot.Server.ClientLibrary.Models.Enums;
using Model;
using Riptide;
using Server.Core.Models;
using WindowsFormsApp1;

namespace Server.Core.Rooms
{
    public class SeatController
    {
        public int Index { get; set; }
        public SeatData SeatData { get; private set; }
        public ClientData ClientData { get; private set; }
        public UserData UserData { get; private set; }
        
        public RoomController RoomController { get; set; }
        
        public bool IsOut { get; set; }
        public bool IsFree => SeatData == null;
        public bool IsReady => !IsFree && SeatData.Balance > RoomController.RoomInfo.RoomSettings.MinBalance && !IsOut;


        public async void SetPlayer(ClientData clientData)
        {
            var userData = await UsersDatabase.GetUserData(clientData.FirebaseId);
            
            SeatData = new SeatData()
            {
                FirebaseId = clientData.FirebaseId,
                Balance = 0
            };
            ClientData = clientData;
            UserData = userData;
            
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
        }
        
        Message CreateMessage(ServerToClientId id) => Message.Create(MessageSendMode.Reliable, id).AddInt(Index);
        void SendMessage(Message message, ushort clientId) => Server.SendMessage(message, clientId);
    }
}