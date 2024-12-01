using System;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Kwicot.Server.ClientLibrary.Models.Enums;
using Riptide;
using Message = Riptide.Message;
using Timer = System.Threading.Timer;

namespace WindowsFormsApp1
{
    public static class Server
    {
        private const string Tag = "Server";
        
        private static Timer _timer;
        private static Riptide.Server _riptideServer;
        
        public static bool IsRunning { get; private set; }
        public static Action OnStatusChanged;

        public static void Initialize()
        {
            _timer = new Timer(TimerTick, DateTime.Now, 0, 10);
            
            _riptideServer = new Riptide.Server();
            _riptideServer.ClientConnected += ClientManager.OnClientConnected;
            _riptideServer.ClientDisconnected += ClientManager.OnClientDisconnected;
            
            Logger.LogInfo(Tag, "Initialized");
        }
        

        public static async Task Start(ushort port, ushort maxClientCount)
        {
            if(IsRunning)
                await Stop();
            
            _riptideServer.Start(port, maxClientCount);
            IsRunning = true;
            OnStatusChanged?.Invoke();
            
            Logger.LogInfo(Tag,"Server started");
        }
        public static async Task Stop()
        {
            if (IsRunning)
            {
                _riptideServer.Stop();

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