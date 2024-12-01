using System;
using System.Threading.Tasks;
using Kwicot.Server.ClientLibrary.Models.Enums;
using Model;
using Riptide;

namespace WindowsFormsApp1
{
    public static class Master
    {
        public static string Tag { get; } = "Master";
        
        public static Action<ClientData> OnConnected;
        public static Action<ClientData> OnDisconnected;
        
        public static async Task Initialize()
        {
        }
        
        [MessageHandler((ushort)ClientToServerId.connectToMaster)]
        public static async void MessageHandler_ConnectToMaster(ushort fromClientId, Message message)
        {
            if (ClientManager.List.TryGetValue(fromClientId, out ClientData clientData))
            {
                if (clientData.IsConnectedToMaster)
                {
                    SendMessage(CreateMessage(ServerToClientId.connectionToMasterFail)
                        .AddInt((int)ErrorType.ALREADY_CONNECTED)
                        , fromClientId);
                }
                else
                {
                    var firebaseId = message.GetString();
                    if (!string.IsNullOrWhiteSpace(firebaseId))
                    {
                        clientData.FirebaseId = firebaseId;

                        OnConnected?.Invoke(clientData);

                        var userData = await UsersDatabase.GetUserData(firebaseId);
                        SendMessage(CreateMessage(ServerToClientId.connectedToMaster)
                            .AddUserData(userData)
                            , fromClientId);
                        
                        Logger.LogInfo(Tag, $"Client [{fromClientId}] [{clientData.FirebaseId}] connected");
                    }
                    else
                    {
                        SendMessage(CreateMessage(ServerToClientId.connectionToMasterFail)
                            .AddInt((int)ErrorType.NEED_LOGIN)
                            , fromClientId);
                    }
                }
            }
        }
        
        [MessageHandler((ushort)ClientToServerId.disconnectFromMaster)]
        public static async void MessageHandler_DisconnectFromMaster(ushort fromClientId, Message message)
        {
            if (ClientManager.List.TryGetValue(fromClientId, out ClientData clientData))
            {
                if (clientData.IsConnectedToMaster)
                {
                    clientData.FirebaseId = null;
                    OnDisconnected?.Invoke(clientData);
                    
                    SendMessage(CreateMessage(ServerToClientId.disconnectedFromMaster)
                    , fromClientId);
                    
                    Logger.LogInfo(Tag, $"Client [{fromClientId}] [{clientData.FirebaseId}] disconnected");
                }
                else
                {
                    SendMessage(CreateMessage(ServerToClientId.disconnectFromMasterFail)
                        .AddInt((int)ErrorType.NOT_CONNECTED)
                        , fromClientId);
                    
                }
            }
        }
        
        static Message CreateMessage(ServerToClientId id) => Message.Create(MessageSendMode.Reliable, id);
        static void SendMessage(Message message, ushort clientId) => Server.SendMessage(message, clientId);
    }
}