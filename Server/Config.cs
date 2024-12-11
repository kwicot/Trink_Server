using System;
using Model;

namespace Server.Core
{
    [Serializable]
    public class Config
    {
        public int RegisterBalance = 1000;
        public bool WriteLogToFile = true;
        
        public StateMachineConfig StateMachineConfig;
    }
}