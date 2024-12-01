using System;
using Riptide;
using WindowsFormsApp1.Room;

namespace Model
{
    [Serializable]
    public class UserData
    {
        public string NickName { get; set; }
        public int Balance { get; set; }
        
        public DateTime RegisterDate { get; set; }
        public DateTime LastLoginDate { get; set; }
        
        public Message GetMessageData(Message message)
        {
            message.AddString(NickName);
            message.AddInt(Balance);
            message.AddDateTime(RegisterDate);
            message.AddDateTime(LastLoginDate);
            
            return message;
        }

        public static UserData GetDataFromMessage(Message message)
        {
            string nickName = message.GetString();
            int balance = message.GetInt();
            DateTime registerDate = message.GetDateTime();
            DateTime lastLoginDate = message.GetDateTime();

            return new UserData()
            {
                NickName = nickName,
                Balance = balance,
                RegisterDate = registerDate,
                LastLoginDate = lastLoginDate
            };
        }
    }
}