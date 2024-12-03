using Riptide;

namespace Model
{
    public class UserProfile
    {
        public string NickName { get; set; }
        public byte[] Picture { get; set; }
        
        
        public Message GetMessageData(Message message)
        {
            message.AddString(NickName);
            message.AddBytes(Picture);
            
            return message;
        }

        public static UserProfile GetDataFromMessage(Message message)
        {
            string nickName = message.GetString();
            byte[] picture = message.GetBytes();

            return new UserProfile()
            {
                NickName = nickName,
                Picture = picture
            };
        }
    }
}