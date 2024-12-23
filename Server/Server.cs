using System;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using Riptide;
using Riptide.Utils;
using Server.Core.Models;
using Server.Core.Rooms;
using WindowsFormsApp1;

namespace Server.Core
{
    public static class Server
    {
        private const string Tag = "Server";
        
        private static Timer _timer;
        private static Riptide.Server _riptideServer;
        
        public static bool IsRunning { get; private set; }
        public static bool IsStopping { get; private set; }
        
        public static Action OnStatusChanged;
        
        public static DateTime ServerTime => DateTime.Now;

        public static void Initialize()
        {
            _timer = new Timer(TimerTick, DateTime.Now, 0, 50);
            
            _riptideServer = new Riptide.Server();
            
            RiptideLogger.Initialize(Log, Log, LogWarning, LogError, false);
            
            _riptideServer.ClientConnected += ClientManager.OnClientConnected;
            _riptideServer.ClientDisconnected += ClientManager.OnClientDisconnected;
            
            _riptideServer.TimeoutTime = 5000;
            
            Logger.LogInfo(Tag, "Initialized");
        }

        static void Log(string message)
        {
            //Logger.LogInfo(Tag, message.ToString());
        }
        
        static void LogWarning(string message)
        {
            //Logger.LogWarning(Tag, message.ToString());
        }
        static void LogError(string message)
        {
            //Logger.LogError(Tag, message.ToString());
        }
        

        public static async Task Start(ushort port, ushort maxClientCount)
        {
            if(IsRunning)
                await Stop();
            
            _riptideServer.Start(port, maxClientCount);
            IsRunning = true;
            OnStatusChanged?.Invoke();
            
            Logger.LogInfo(Tag,$"Server started on port [{port}]");
        }
        public static async Task Stop()
        {
            if (IsRunning)
            {
                IsStopping = true;
                OnStatusChanged?.Invoke();

                await RoomManager.OnServerStopping();

                _riptideServer.Stop();
                IsStopping = false;
                IsRunning = false;

                OnStatusChanged?.Invoke();
                
                Logger.LogInfo(Tag,"Server stoped");
            }
        }

        public static void SendMessage(Message message, ushort clientId)
        {
            if (ClientManager.List.TryGetValue(clientId, out ClientData client) && client.IsConnectedToServer)
            {
                _riptideServer.Send(message, clientId);
            }
        }

        public static void SendMessageToAll(Message message)
        {
            _riptideServer.SendToAll(message);
        }

        
        public static bool CheckInternetConnection()
        {
            try
            {
                using (Ping ping = new Ping())
                {
                    PingReply reply = ping.Send("8.8.8.8", 3000); // Таймаут 3 секунды
                    return reply.Status == IPStatus.Success;
                }
            }
            catch
            {
                return false; // Если произошла ошибка, интернета, скорее всего, нет
            }
        }

        static void TimerTick(object state)
        {
            if (!CheckInternetConnection())
            {
                Logger.LogError(Tag, "Cant access internet connection");
            }
            
            _riptideServer?.Update();
            
            RoomManager.OnServerTick();
        }
    }
}