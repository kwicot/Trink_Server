using Riptide;
using WindowsFormsApp1.Room;

namespace Model.Room
{
    public class RoomSettingPreset
    {
        public RoomType Type { get; set; }
        public int MinBalance { get; set; }
        public int MaxBalance { get; set; }
        public int StartBet { get; set; }
        
        
        public Message GetMessageData(Message message)
        {
            message.AddInt((int)Type);
            message.AddInt(MinBalance);
            message.AddInt(MaxBalance);
            message.AddInt(StartBet);
            
            return message;
        }

        public static RoomSettingPreset GetDataFromMessage(Message message)
        {
            RoomType roomType = (RoomType)message.GetInt();
            int minBalance = message.GetInt();
            int maxBalance = message.GetInt();
            int startBet = message.GetInt();

            return new RoomSettingPreset()
            {
                Type = roomType,
                MinBalance = minBalance,
                MaxBalance = maxBalance,
                StartBet = startBet,
            };
        }
    }
}