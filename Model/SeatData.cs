using Riptide;

namespace Model
{
    public class SeatData
    {
        public string FirebaseId { get; set; }
        public int Balance { get; set; }
        
        public Message GetMessageData(Message message)
        {
            message.AddString(FirebaseId);
            message.AddInt(Balance);
            
            return message;
        }

        public static SeatData GetDataFromMessage(Message message)
        {
            string firebaseId = message.GetString();
            int balance = message.GetInt();

            return new SeatData()
            {
                FirebaseId = firebaseId,
                Balance = balance
            };
        }
    }
}