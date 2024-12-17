using System;
using System.Threading.Tasks;
using Kwicot.Server.ClientLibrary.Models.Enums;
using Model;
using Riptide;
using Server.Core.Models;
using Server.Core.Rooms;
using WindowsFormsApp1;

namespace Server.Core
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
            Logger.LogInfo(Tag, $"Receive message [ConnectToMaster] from [{fromClientId}]");
            if (ClientManager.List.TryGetValue(fromClientId, out ClientData clientData))
            {
                if (clientData.IsConnectedToMaster)
                {
                    Logger.LogInfo(Tag,"Connection fail via already connected");
                    SendMessage(CreateMessage(ServerToClientId.connectionToMasterFail)
                        .AddInt((int)ErrorType.ALREADY_CONNECTED)
                        , fromClientId);
                }
                else
                {
                    var firebaseId = message.GetString();
                    Logger.LogInfo(Tag, $"Request firebase id : [{firebaseId}]");
                    if (!string.IsNullOrWhiteSpace(firebaseId))
                    {
                        clientData.FirebaseId = firebaseId;

                        OnConnected?.Invoke(clientData);

                        var userData = await UsersDatabase.GetUserData(firebaseId);
                        userData.FirebaseId = firebaseId;
                        userData.ClientId = fromClientId;
                        
                        SendMessage(CreateMessage(ServerToClientId.connectedToMaster)
                            .AddUserData(userData)
                            .AddRoomSettingsPresets(RoomManager.RoomSettingPresets)
                            , fromClientId);


                        if (RoomManager.IsWaitingReturn(firebaseId, out var room))
                        {
                            await Task.Delay(2000);
                           await room.AddClient(clientData);
                           
                           SendMessage(CreateMessage(ServerToClientId.joinedRoom)
                                   .AddRoomInfo(room.RoomInfo)
                               , clientData.ClientID);
                        }

                        Logger.LogInfo(Tag, $"Client [{fromClientId}] [{clientData.FirebaseId}] connected");
                    }
                    else
                    {
                        Logger.LogInfo(Tag,"Connection fail via NEED_LOGIN");
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
            Logger.LogInfo(Tag, $"Receive message [DisconnectFromMaster] from [{fromClientId}]");

            if (ClientManager.List.TryGetValue(fromClientId, out ClientData clientData))
            {
                DisconnectClient(clientData, true);
            }
        }

        public static async void DisconnectClient(ClientData clientData, bool senDisconnectMessage)
        {
            try
            {
                var userData = await UsersDatabase.GetUserData(clientData.FirebaseId);
                await UsersDatabase.UpdateUserData(userData);
            
                if (clientData.IsConnectedToMaster)
                {
                    clientData.FirebaseId = null;
                    OnDisconnected?.Invoke(clientData);

                    if (senDisconnectMessage)
                    {
                        SendMessage(CreateMessage(ServerToClientId.disconnectedFromMaster)
                            , clientData.ClientID);
                    }

                    Logger.LogInfo(Tag, $"Client [{clientData.ClientID}] [{clientData.FirebaseId}] disconnected");
                }
                else
                {
                    if (senDisconnectMessage)
                    {
                        SendMessage(CreateMessage(ServerToClientId.disconnectFromMasterFail)
                                .AddInt((int)ErrorType.NOT_CONNECTED)
                            , clientData.ClientID);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.LogInfo(Tag, e.Message);
            }
        }
        
        static Message CreateMessage(ServerToClientId id) => Message.Create(MessageSendMode.Reliable, id);
        static void SendMessage(Message message, ushort clientId) => Server.SendMessage(message, clientId);
    }
}