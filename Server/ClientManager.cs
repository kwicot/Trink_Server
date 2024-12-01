using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Riptide;

namespace WindowsFormsApp1
{
    public static class ClientManager
    {
        public static Dictionary<ushort, ClientData> List;
        
        public static string Tag { get; } = "ClientManager";

        public static async Task Initialize()
        {
            List = new Dictionary<ushort, ClientData>();
        }
        
        public static void OnClientConnected(object sender, ServerConnectedEventArgs e)
        {
            Logger.LogInfo(Tag,$"Client with id [{e.Client.Id}] connected");
            
            var clientId = e.Client.Id;
            
            List[clientId] = new ClientData()
            {
                ClientID = clientId,
                FirebaseId = string.Empty,
                Balance = 0,
                IsConnectedToServer = true,
                
                ConnectionTime = DateTime.Now,
                DisconnectionTime = DateTime.Now,
            };
        }
        public static void OnClientDisconnected(object sender, ServerDisconnectedEventArgs e)
        {
            Logger.LogInfo(Tag,$"Client with id [{e.Client.Id}] disconnected via {e.Reason}");

            if (List.TryGetValue(e.Client.Id, out ClientData clientData))
            {
                clientData.FirebaseId = string.Empty;
                clientData.IsConnectedToServer = false;
                
                clientData.Balance = 0;
                clientData.DisconnectionTime = DateTime.Now;
            }
        }
    }
}