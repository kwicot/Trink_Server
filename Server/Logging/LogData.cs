using System;

namespace WindowsFormsApp1
{
    [Serializable]
    public class LogData
    {
        public DateTime DateTime;
        public LogType Type;
        public string Tag;
        public string Message;

        public override string ToString()
        {
           return $"{DateTime:yyyy-MM-dd HH:mm:ss} : {Type} : {Tag.ToUpper()} : {Message}";
        }
    }
}