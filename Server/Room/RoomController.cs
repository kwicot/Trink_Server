using System.Collections.Generic;
using Kwicot.Server.ClientLibrary.Models.Enums;
using Server.Core.Models;
using WindowsFormsApp1;
using WindowsFormsApp1.Room;

namespace Server.Core.Rooms
{
    public class RoomController
    {
        public string Tag => $"Room_{RoomInfo.RoomSettings.RoomName}";
        public RoomInfo RoomInfo { get; }

        
        public RoomController(RoomSettings roomSettings)
        {
            RoomInfo = new RoomInfo()
            {
                RoomSettings = roomSettings,
                Players = new List<string>()
            };
            
            Logger.LogInfo(Tag, $"Room created");
        }

        public void OnServerTick()
        {
            
        }

        public bool AddClient(ClientData clientData)
        {
            return true;
        }

        public void RemoveClient(ClientData clientData)
        {
            
        }
    }
}