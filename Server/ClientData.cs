using System;
using Server.Core.Rooms;
using WindowsFormsApp1.Room;

namespace Server.Core.Models
{
    public class ClientData
    {
        public ushort ClientID { get; set; }
        public string FirebaseId { get; set; }
        public int Balance { get; set; }
        public DateTime DisconnectionTime { get; set; }
        public DateTime ConnectionTime { get; set; }
        
        public RoomController CurrentRoom { get; set; }
        
        public bool IsConnectedToServer { get; set; }
        public bool IsConnectedToLobby { get; set; }
        public bool IsConnectedToMaster => IsConnectedToServer && !string.IsNullOrWhiteSpace(FirebaseId);
    }
}