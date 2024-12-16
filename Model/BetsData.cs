
using System;
using System.Collections.Generic;
using Model;
using Riptide;

namespace Trink_RiptideServer
{
    [Serializable]
    public class BetsData
    {
        public Dictionary<int, int> Bets = new();
        public int TableCommission = 0;
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

                value -= TableCommission;

                return value;
            }
        }
        
        public void Remove(int index)
        {
            var playerBet = Bets[index];

            for (int i = 0; i < 20; i++)
            {
                if (Bets.TryGetValue(i, out var bets))
                {
                    Bets[i] -= playerBet;
                }
            }
            
        }

        public int PlayerWin(int index)
        {
            int count = 0;
            foreach (var bet in Bets)
            {
                if (bet.Value > 0)
                    count++;
            }
            return Bets[index] * count;            
        }
        
        
        public Message GetMessageData(Message message)
        {
            message.AddInt(Bets.Count);
            foreach (var bet in Bets)
            {
                message.AddInt(bet.Key);
                message.AddInt(bet.Value);
            }

            message.AddInt(TableCommission);

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
            
            int tableCommission = message.GetInt();

            betsData.Bets = bets;
            betsData.TableCommission = tableCommission;

            return betsData;
        }
    }
}