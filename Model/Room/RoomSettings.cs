using System;
using Model;
using Riptide;

namespace WindowsFormsApp1.Room
{
    [Serializable]
    public class RoomSettings
    {
        public string RoomName { get; set; }
        public int MaxPlayers { get; set; }
        public RoomType Type { get; set; }
        
        public int PlayerTtl { get; set; }
        public int RoomTtl { get; set; }
        
        public bool AllowWaiting { get; set; }
        public bool IsHide { get; set; }
        
        public int MinBalance { get; set; }
        public int MaxBalance { get; set; }
        public int StartBet { get; set; }
        public string Password { get; set; }
        

        public Message GetMessageData(Message message)
        {
            message.AddString(RoomName);
            message.AddInt(MaxPlayers);
            message.AddInt((int)Type);
            message.AddInt(PlayerTtl);
            message.AddInt(RoomTtl);
            message.AddBool(AllowWaiting);
            message.AddBool(IsHide);
            message.AddInt(MinBalance);
            message.AddInt(MaxBalance);
            message.AddInt(StartBet);
            message.AddString(Password);
            
            return message;
        }

        public static RoomSettings GetDataFromMessage(Message message)
        {
            string roomName = message.GetString();
            int maxPlayers = message.GetInt();
            RoomType roomType = (RoomType)message.GetInt();
            
            int playerTtl = message.GetInt();
            int roomTtl = message.GetInt();
            bool allowWaiting = message.GetBool();
            bool isHide = message.GetBool();

            int minBalance = message.GetInt();
            int maxBalance = message.GetInt();
            int startBet = message.GetInt();
            string password = message.GetString();

            return new RoomSettings()
            {
                RoomName = roomName,
                MaxPlayers = maxPlayers,
                Type = roomType,
                PlayerTtl = playerTtl,
                RoomTtl = roomTtl,
                AllowWaiting = allowWaiting,
                IsHide = isHide,
                MinBalance = minBalance,
                MaxBalance = maxBalance,
                StartBet = startBet,
                Password = password
            };
        }
    }
}