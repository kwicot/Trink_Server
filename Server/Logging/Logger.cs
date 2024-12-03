using System;
using System.Collections.Generic;
using System.IO;
using Server.Core;

namespace WindowsFormsApp1
{
    public static class Logger
    {
        private static string _savePath;
        public static List<LogData> Logs { get; private set; }
        public static bool IsDebug { get; set; }
        
        public static Action OnLogsChanged;

        private static bool _initialized;
        
        public static void Initialize()
        {
            Logs = new List<LogData>();
            
            var saveDirectory = Path.Combine(Constants.RootPath, "Logs");
            _savePath = Path.Combine(saveDirectory, $"Log_{DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")}.txt");
            
            if (!Directory.Exists(saveDirectory))
                Directory.CreateDirectory(saveDirectory);
            
            if(Program.Config.WriteLogToFile)
                File.Create(_savePath).Dispose();

            _initialized = true;
        }
        
        #region Log

        
        public static void Log(string tag, string message)
        {
            if(!_initialized)
                return;
            
            
            LogData logData = new LogData()
            {
                DateTime = DateTime.Now,
                Type = LogType.Log,
                Tag = tag,
                Message = message
            };
            
            Logs.Add(logData);
            WriteToFile(logData);
            
            OnLogsChanged?.Invoke();
            
            if(IsDebug)
                Console.WriteLine(logData);
        }
        
        public static void LogInfo(string tag, string message)
        {
            if(!_initialized)
                return;
            
            LogData logData = new LogData()
            {
                DateTime = DateTime.Now,
                Type = LogType.Info,
                Tag = tag,
                Message = message
            };
            
            Logs.Add(logData);
            WriteToFile(logData);
            
            OnLogsChanged?.Invoke();
            
            if(IsDebug)
                Console.WriteLine(logData);
        }
        
        public static void LogWarning(string tag, string message)
        {
            if(!_initialized)
                return;
            
            LogData logData = new LogData()
            {
                DateTime = DateTime.Now,
                Type = LogType.Warning,
                Tag = tag,
                Message = message
            };
            
            Logs.Add(logData);
            
            WriteToFile(logData);
            
            OnLogsChanged?.Invoke();
            
            if(IsDebug)
                Console.WriteLine(logData);
        }
        
        public static void LogError(string tag, string message)
        {
            if(!_initialized)
                return;
            
            LogData logData = new LogData()
            {
                DateTime = DateTime.Now,
                Type = LogType.Error,
                Tag = tag,
                Message = message
            };
            
            Logs.Add(logData);

            WriteToFile(logData);
            
            OnLogsChanged?.Invoke();
            
            if(IsDebug)
                Console.WriteLine(logData);
        }

        static void WriteToFile(LogData logData)
        {
            if(Program.Config.WriteLogToFile)
                File.AppendAllText(_savePath, logData + Environment.NewLine);
        }
        
        #endregion

    }
}