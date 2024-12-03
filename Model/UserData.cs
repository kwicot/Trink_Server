using System;
using Riptide;
using WindowsFormsApp1.Room;

namespace Model
{
    [Serializable]
    public class UserData
    {
        public UserProfile UserProfile { get; set; }
        public int Balance { get; set; }
        
        public DateTime RegisterDate { get; set; }
        public DateTime LastLoginDate { get; set; }

        public ushort ClientId { get; set; }
        
        public Message GetMessageData(Message message)
        {
            if (UserProfile == null)
                UserProfile = new UserProfile() { NickName = "Player", Picture = Array.Empty<byte>()};
            
            message.AddUserProfile(UserProfile);
            message.AddInt(Balance);
            message.AddDateTime(RegisterDate);
            message.AddDateTime(LastLoginDate);
            message.AddUShort(ClientId);
            
            return message;
        }

        public static UserData GetDataFromMessage(Message message)
        {
            UserProfile userProfile = message.GetUserProfile();
            int balance = message.GetInt();
            DateTime registerDate = message.GetDateTime();
            DateTime lastLoginDate = message.GetDateTime();
            ushort clientId = message.GetUShort();

            return new UserData()
            {
                UserProfile = userProfile,
                Balance = balance,
                RegisterDate = registerDate,
                LastLoginDate = lastLoginDate,
                ClientId = clientId
            };
        }
    }
}