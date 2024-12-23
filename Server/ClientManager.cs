﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kwicot.Server.ClientLibrary.Models.Enums;
using Model;
using Riptide;
using Server.Core.Models;
using WindowsFormsApp1;

namespace Server.Core
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
                IsConnectedToServer = true,
                
                ConnectionTime = DateTime.Now,
                DisconnectionTime = DateTime.Now,
            };
        }
        public static async void OnClientDisconnected(object sender, ServerDisconnectedEventArgs e)
        {
            try
            {
                Logger.LogInfo(Tag,$"Client with id [{e.Client.Id}] disconnected via {e.Reason}");

                if (List.TryGetValue(e.Client.Id, out ClientData clientData))
                {
                    if (clientData.CurrentRoom != null)
                        await clientData.CurrentRoom.RemoveClient(clientData, true);
                
                    if(clientData.IsConnectedToLobby)
                        Master.DisconnectClient(clientData, false);
                
                    clientData.IsConnectedToServer = false;
                    clientData.DisconnectionTime = DateTime.Now;
                }
            }
            catch (Exception exception)
            {
                Logger.LogInfo(Tag, exception.Message);
            }
        }

        
        [MessageHandler((ushort)ClientToServerId.updateUserProfile)]
        public static async void MessageHandler_DisconnectFromMaster(ushort fromClientId, Message message)
        {
            if (List.TryGetValue(fromClientId, out ClientData clientData))
            {
                var userProfile = message.GetUserProfile();
                var userData = await UsersDatabase.GetUserData(clientData.FirebaseId);
                userData.UserProfile = userProfile;
                await UsersDatabase.UpdateUserData(clientData.FirebaseId, userData);
                
                Logger.LogInfo(Tag, $"Success update user profile from [{fromClientId}] new NickName: {userProfile.NickName}");
                
                SendMessage(CreateMessage(ServerToClientId.updateUserProfileResult)
                    .AddBool(true)
                    ,fromClientId);
            }
            else
            {
                
                Logger.LogInfo(Tag, $"Success update user profile from [{fromClientId}]");

                SendMessage(CreateMessage(ServerToClientId.updateUserProfileResult)
                    .AddBool(false)
                    .AddInt((int)ErrorType.NEED_LOGIN)
                    , fromClientId);
            }
        }
        
        static Message CreateMessage(ServerToClientId id) => Message.Create(MessageSendMode.Reliable, id);
        static void SendMessage(Message message, ushort clientId) => Server.SendMessage(message, clientId);
    }
}