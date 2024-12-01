using System;
using System.Threading.Tasks;
using Kwicot.Server.ClientLibrary.Models.Enums;
using Model;
using Riptide;
using WindowsFormsApp1.Room;

namespace WindowsFormsApp1
{
    public static class Lobby
    {
        public static string Tag { get; } = "Lobby";

        public static Action<ClientData> OnConnected;
        
        public static async Task Initialize()
        {
            Master.OnDisconnected += OnDisconectedFromMaster;
        }

        

        private static void OnDisconectedFromMaster(ClientData clientData)
        {
            if (clientData.IsConnectedToLobby)
            {
                clientData.IsConnectedToLobby = false;
            }
        }

        [MessageHandler((ushort)ClientToServerId.connectToLobby)]
        public static async void MessageHandler_ConnectToLobby(ushort fromClientId, Message message)
        {
            if (ClientManager.List.TryGetValue(fromClientId, out ClientData clientData))
            {
                if (!clientData.IsConnectedToMaster)
                {
                    SendMessage(CreateMessage(ServerToClientId.connectionToLobbyFail)
                            .AddInt((int)ErrorType.NEED_CONNECT_TO_MASTER)
                        , fromClientId);
                }
                else if (clientData.IsConnectedToLobby)
                {
                    SendMessage(CreateMessage(ServerToClientId.connectionToLobbyFail)
                            .AddInt((int)ErrorType.ALREADY_CONNECTED)
                        , fromClientId);
                }
                else
                {
                    clientData.IsConnectedToLobby = true;
                    SendMessage(CreateMessage(ServerToClientId.connectedToLobby)
                        , fromClientId);
                    
                    OnConnected?.Invoke(clientData);
                    
                    Logger.LogInfo(Tag, $"Client [{fromClientId}] [{clientData.FirebaseId}] connected");
                }
            }
        }
        
        [MessageHandler((ushort)ClientToServerId.disconnectFromLobby)]
        public static async void MessageHandler_DisconnectFromLobby(ushort fromClientId, Message message)
        {
            if (ClientManager.List.TryGetValue(fromClientId, out ClientData clientData))
            {
                if (!clientData.IsConnectedToLobby)
                {
                    SendMessage(CreateMessage(ServerToClientId.disconnectFromLobbyFail)
                            .AddInt((int)ErrorType.NEED_CONNECT_TO_LOBBY)
                        , fromClientId);
                }
                else
                {
                    clientData.IsConnectedToLobby = false;
                    SendMessage(CreateMessage(ServerToClientId.disconnectedFromLobby)
                        , fromClientId);
                    
                    Logger.LogInfo(Tag, $"Client [{fromClientId}] [{clientData.FirebaseId}] disconnected");
                }
            }
        }

        
        static void SendRoomsInfoTo(ushort fromClientId)
        {
            
        }
        
        
        static Message CreateMessage(ServerToClientId id) => Message.Create(MessageSendMode.Reliable, id);
        static void SendMessage(Message message, ushort clientId) => Server.SendMessage(message, clientId);
    }
}