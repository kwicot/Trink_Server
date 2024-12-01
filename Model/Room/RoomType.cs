using System;

namespace WindowsFormsApp1.Room
{
    [Serializable]
    public enum RoomType : int
    {
        Noob = 0,
        Amator = 5,
        Profesional = 10,
        Vip = 15,
        Custom = 100
    }
}