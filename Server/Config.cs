using System;
using Model;

namespace Server.Core
{
    [Serializable]
    public class Config
    {
        public int RegisterBalance = 1000;
        public bool WriteLogToFile = true;
        public bool StartOnLaunch = true;
        
        public int TablePercent = 3;
        public int MinBalanceForCommission = 20;

        public int PlayersCount = 100;
        public int Port = 80;
        
        public StateMachineConfig StateMachineConfig;

        public string AdminPassword = "Test123";
    }
}