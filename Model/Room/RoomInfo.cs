using System.Collections.Generic;
using System.Linq;
using Model;
using Riptide;
using WindowsFormsApp1.Room;

namespace Kwicot.Server.ClientLibrary.Models.Enums
{
    public class RoomInfo
    {
        public RoomSettings RoomSettings { get; set; }
        
        public List<string> Players { get; set; }

        public bool Removed { get; set; }
        
        public int PlayersCount => Players.Count;

        public bool IsVisible
        {
            get
            {
                return !RoomSettings.IsHide && PlayersCount < RoomSettings.MaxPlayers;
            }
        }

        public Message GetMessageData(Message message)
        {
            message.AddRoomSettings(RoomSettings);
            message.AddStrings(Players.ToArray());
            message.AddBool(Removed);

            return message;
        }

        public static RoomInfo GetDataFromMessage(Message message)
        {
            RoomSettings roomSettings = message.GetRoomSettings();
            string[] players = message.GetStrings();
            bool removed = message.GetBool();

            return new RoomInfo()
            {
                RoomSettings = roomSettings,
                Players = players.ToList(),
                Removed = removed
            };
        }
    }
}