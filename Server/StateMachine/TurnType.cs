using System;

namespace Trink_RiptideServer.Library.StateMachine
{
    public enum TurnType
    {
        No = 0,
        Hide = 10,
        Normal = 11,
        AllIn = 12,
        Min = 13,
        Increase = 14,
        Pass = 99
    }
}