﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kwicot.Server.ClientLibrary.Models.Enums;
using Model;
using Model.Room;
using Riptide;
using Server.Core.Models;
using WindowsFormsApp1;
using WindowsFormsApp1.Room;

namespace Server.Core.Rooms
{
    public static class RoomManager
    {
        private const string Tag = "Room_Manager";

        private static Dictionary<string, RoomController> _roomsMap;

        public static RoomSettingPreset[] RoomSettingPresets { get; private set; }

        public static async Task Initiailize()
        {
            _roomsMap = new Dictionary<string, RoomController>();
            RoomSettingPresets = new[]
            {
                new RoomSettingPreset() { Type = RoomType.Noob, MinBalance = 20, MaxBalance = 200, StartBet = 2 },
                new RoomSettingPreset() { Type = RoomType.Amator, MinBalance = 50, MaxBalance = 500, StartBet = 5 },
                new RoomSettingPreset() { Type = RoomType.Profesional, MinBalance = 100, MaxBalance = 1000, StartBet = 10 },
                new RoomSettingPreset() { Type = RoomType.Vip, MinBalance = 200, MaxBalance = 2000, StartBet = 20 },
                new RoomSettingPreset() { Type = RoomType.Custom, MinBalance = -1, MaxBalance = -1, StartBet = -1 },
            };
            Lobby.OnConnected += OnConnectedToLobby;
        }
        
        [MessageHandler((ushort)ClientToServerId.createRoom)]
        public static async void MessageHandler_CreateRoom(ushort fromClientId, Message message)
        {
            if (ClientManager.List.TryGetValue(fromClientId, out ClientData clientData))
            {
                var roomSettings = message.GetRoomSettings();
                OnRequestCreateRoom(clientData, roomSettings);
            }
        }
        static void OnRequestCreateRoom(ClientData clientData, RoomSettings settings)
        {
            Logger.LogInfo(Tag, $"Client [{clientData.ClientID}] request create room");
            if (clientData.CurrentRoom != null) // Already in room
            {
                SendMessage(CreateMessage(ServerToClientId.createRoomFail)
                        .AddInt((int)ErrorType.IN_ROOM)
                    , clientData.ClientID);
                    
                Logger.LogInfo(Tag, $"Request denied from client [{clientData.ClientID}] via [IN_ROOM]]");

                return;
            }
            if (_roomsMap.ContainsKey(settings.RoomName)) // Room name exist
            {
                SendMessage(CreateMessage(ServerToClientId.createRoomFail)
                        .AddInt((int)ErrorType.NAME_EXISTS)
                    , clientData.ClientID);
                    
                Logger.LogInfo(Tag, $"Request denied from client [{clientData.ClientID}] via [NAME_EXISTS]]");
                    
                return;
            }

            var room = CreateNewRoom(settings);
                
            SendMessage(CreateMessage(ServerToClientId.createdRoom)
                , clientData.ClientID);
                
            Logger.LogInfo(Tag, $"Client [{clientData.ClientID}] [{clientData.FirebaseId}] Created new room with name [{room.RoomInfo.RoomSettings.RoomName}]");

            OnRequestJoinRoom(clientData, settings.RoomName);
        }
        
        
        
        [MessageHandler((ushort)ClientToServerId.joinRoom)]
        public static async void MessageHandler_JoinRoom(ushort fromClientId, Message message)
        {
            if (ClientManager.List.TryGetValue(fromClientId, out ClientData clientData))
            {
                string roomName = message.GetString();
                OnRequestJoinRoom(clientData, roomName);
            }
        }
        static async void OnRequestJoinRoom(ClientData clientData, string roomName)
        {
            Logger.LogInfo(Tag, $"Client [{clientData.ClientID}] request join room [{roomName}]");
                if (clientData.CurrentRoom != null) // Already in room
                {
                    SendMessage(CreateMessage(ServerToClientId.joinRoomFail)
                            .AddInt((int)ErrorType.IN_ROOM)
                        , clientData.ClientID);
                    
                    Logger.LogInfo(Tag, $"Request denied from client [{clientData.ClientID}] via [IN_ROOM]");
                    return;
                }
                if (!_roomsMap.ContainsKey(roomName))
                {
                    SendMessage(CreateMessage(ServerToClientId.joinRoomFail)
                            .AddInt((int)ErrorType.DOES_NOT_EXIST)
                        , clientData.ClientID);
                    
                    Logger.LogInfo(Tag, $"Request denied from client [{clientData.ClientID}] via [DOES_NOT_EXIST]. Rooms count {_roomsMap.Count}");
                    return;
                }

                if (_roomsMap.TryGetValue(roomName, out RoomController roomController))
                {
                    if (roomController.RoomInfo.PlayersCount >= roomController.RoomInfo.RoomSettings.MaxPlayers)
                    {
                        SendMessage(CreateMessage(ServerToClientId.joinRoomFail)
                            .AddInt((int)ErrorType.NO_FREE_SPACE)
                            , clientData.ClientID);
                        
                        Logger.LogInfo(Tag, $"Request denied from client [{clientData.ClientID}] via [NO_FREE_SPACE]");
                        return;
                    }
                    
                    else if (!await roomController.AddClient(clientData))
                    {
                        SendMessage(CreateMessage(ServerToClientId.joinRoomFail)
                                .AddInt((int)ErrorType.BLOCKED)
                            , clientData.ClientID);
                        
                        Logger.LogInfo(Tag, $"Request denied from client [{clientData.ClientID}] via [BLOCKED]]");
                        return;
                    }
                    
                    SendMessage(CreateMessage(ServerToClientId.joinedRoom)
                        .AddRoomInfo(roomController.RoomInfo)
                        , clientData.ClientID);

                    OnRoomStateChanged(roomController);
                    
                    Logger.LogInfo(Tag, $"Client [{clientData.ClientID}] [{clientData.FirebaseId}] joined to room [{roomController.RoomInfo.RoomSettings.RoomName}]");
                }
        }
        
        
        
        [MessageHandler((ushort)ClientToServerId.leftRoom)]
        public static async void MessageHandler_LeftRoom(ushort fromClientId, Message message)
        {
            if (ClientManager.List.TryGetValue(fromClientId, out ClientData clientData))
            {
                OnRequestLeftRoom(clientData);
            }
        }
        static void OnRequestLeftRoom(ClientData clientData)
        {
            if (clientData.CurrentRoom == null) 
            {
                SendMessage(CreateMessage(ServerToClientId.leftRoomFail)
                        .AddInt((int)ErrorType.NOT_IN_ROOM)
                    , clientData.ClientID);
                return;
            }

            var room = clientData.CurrentRoom;

            clientData.CurrentRoom = null;
                
            room.RemoveClient(clientData);
                
            SendMessage(CreateMessage(ServerToClientId.leftRoom)
                , clientData.ClientID);

            OnRoomStateChanged(room);
                
            Logger.LogInfo(Tag, $"Client [{clientData.ClientID}] [{clientData.FirebaseId}] left from room [{room.RoomInfo.RoomSettings.RoomName}]");
        }
        
        
        [MessageHandler((ushort)ClientToServerId.sceneLoaded)]
        public static async void MessageHandler_SceneLoaded(ushort fromClientId, Message message)
        {
            if (ClientManager.List.TryGetValue(fromClientId, out ClientData clientData))
            {
                if (clientData.CurrentRoom == null) 
                {
                    SendMessage(CreateMessage(ServerToClientId.leftRoomFail)
                            .AddInt((int)ErrorType.NOT_IN_ROOM)
                        , fromClientId);
                    return;
                }

                var room = clientData.CurrentRoom;
                room.OnPlayerLoadedScene(clientData);

                Logger.LogInfo(Tag, $"Client [{fromClientId}] [{clientData.FirebaseId}] loaded scene on room [{room.RoomInfo.RoomSettings.RoomName}]");
            }
        }
        
        
        public static RoomController CreateNewRoom(RoomSettings roomSettings)
        {
            var roomController = new RoomController(roomSettings);
            _roomsMap.Add(roomSettings.RoomName, roomController);
            
            OnNewRoomCreated(roomController);
            return roomController;
        }

        public static void RemoveRoom(RoomController roomController)
        {
            roomController.RoomInfo.Removed = true;
            OnRoomStateChanged(roomController);
            
            _roomsMap.Remove(roomController.RoomInfo.RoomSettings.RoomName);
        }
        
        
        private static void OnNewRoomCreated(RoomController roomController)
        {
            foreach (var clientData in ClientManager.List.Values)
            {
                if (clientData.IsConnectedToMaster && clientData.IsConnectedToLobby)
                {
                    SendMessage(CreateMessage(ServerToClientId.roomListUpdate)
                            .AddRoomInfo(roomController.RoomInfo)
                        , clientData.ClientID);
                }
            }
        }
        
        private static void OnRoomStateChanged(RoomController roomController)
        {
            foreach (var clientData in ClientManager.List.Values)
            {
                if (clientData.IsConnectedToMaster && clientData.IsConnectedToLobby)
                {
                    SendMessage(CreateMessage(ServerToClientId.roomListUpdate)
                            .AddRoomInfo(roomController.RoomInfo)
                        , clientData.ClientID);
                }
            }
        }
        
        private static void OnConnectedToLobby(ClientData clientData)
        {
            var message = CreateMessage(ServerToClientId.roomListUpdate);
            foreach (var roomController in _roomsMap.Values)
            {
                if (roomController.RoomInfo.IsVisible)
                    message.AddRoomInfo(roomController.RoomInfo);
            }    
            
            SendMessage(message, clientData.ClientID);
        }
        
        
        public static void OnServerTick()
        {
            foreach (var roomController in _roomsMap.Values.ToList())
            {
                roomController.OnServerTick();
            }
        }
        
        
        static Message CreateMessage(ServerToClientId id) => Message.Create(MessageSendMode.Reliable, id);
        static void SendMessage(Message message, ushort clientId) => Server.SendMessage(message, clientId);
    }
}