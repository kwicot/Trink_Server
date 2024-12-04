﻿using Riptide;

namespace Model
{
    public class SeatData
    {
        public string FirebaseId { get; set; }
        public ushort ClientId { get; set; }
        public int Balance { get; set; }
        
        public Message GetMessageData(Message message)
        {
            message.AddString(FirebaseId);
            message.AddInt(Balance);
            message.AddUShort(ClientId);
            
            return message;
        }

        public static SeatData GetDataFromMessage(Message message)
        {
            string firebaseId = message.GetString();
            int balance = message.GetInt();
            ushort clientId = message.GetUShort();

            return new SeatData()
            {
                FirebaseId = firebaseId,
                Balance = balance,
                ClientId = clientId
            };
        }
    }
}