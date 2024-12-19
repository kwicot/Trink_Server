using System;
using System.Collections.Generic;
using Riptide;

namespace Trink_RiptideServer
{
    [Serializable]
    public class BetsData
    {
        public Dictionary<int, int> Bets = new();
        public int TotalBank
        {
            get
            {
                int value = 0;
                if (Bets != null)
                {
                    foreach (var bet in Bets)
                    {
                        value += bet.Value;
                    }
                }

                return value;
            }
        }
        
        public Message GetMessageData(Message message)
        {
            message.AddInt(Bets.Count);
            foreach (var bet in Bets)
            {
                message.AddInt(bet.Key);
                message.AddInt(bet.Value);
            }

            return message;
        }
        
        public static BetsData GetDataFromMessage(Message message)
        {
            var betsData = new BetsData();
            var bets = new Dictionary<int, int>();
            
            int count = message.GetInt();
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    int key = message.GetInt();
                    int value = message.GetInt();
                    bets.Add(key, value);
                }
            }
            
            betsData.Bets = bets;

            return betsData;
        }
    }
}