using Riptide;

namespace Model
{
    public class UserProfile
    {
        public string NickName { get; set; }
        public byte[] Picture { get; set; }
        
        public int DefaultSpriteIndex { get; set; } = 0;
        
        
        public Message GetMessageData(Message message)
        {
            message.AddString(NickName);
            message.AddBytes(Picture);
            message.AddInt(DefaultSpriteIndex);
            
            return message;
        }

        public static UserProfile GetDataFromMessage(Message message)
        {
            string nickName = message.GetString();
            byte[] picture = message.GetBytes();
            var defaultSpriteIndex = message.GetInt();

            return new UserProfile()
            {
                NickName = nickName,
                Picture = picture,
                DefaultSpriteIndex = defaultSpriteIndex
            };
        }
    }
}