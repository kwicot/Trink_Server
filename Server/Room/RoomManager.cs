using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kwicot.Server.ClientLibrary.Models.Enums;
using Model;
using Riptide;
using WindowsFormsApp1.Room;

namespace WindowsFormsApp1
{
    public static class RoomManager
    {
        private const string Tag = "RoomManager";

        private static Dictionary<string, RoomController> _roomsMap;

        public static async Task Initiailize()
        {
            _roomsMap = new Dictionary<string, RoomController>();
            Lobby.OnConnected += OnConnectedToLobby;
        }
        
        [MessageHandler((ushort)ClientToServerId.createRoom)]
        public static async void MessageHandler_CreateRoom(ushort fromClientId, Message message)
        {
            var roomSettings = message.GetRoomSettings();

            if (ClientManager.List.TryGetValue(fromClientId, out ClientData clientData))
            {
                if (clientData.CurrentRoom != null) // Already in room
                {
                    SendMessage(CreateMessage(ServerToClientId.createRoomFail)
                        .AddInt((int)ErrorType.IN_ROOM)
                        , fromClientId);
                    return;
                }
                if (_roomsMap.ContainsKey(roomSettings.RoomName)) // Room name exist
                {
                    SendMessage(CreateMessage(ServerToClientId.createRoomFail)
                        .AddInt((int)ErrorType.NAME_EXISTS)
                        , fromClientId);
                    
                    return;
                }
                
                var roomController = new RoomController(roomSettings);
                _roomsMap.Add(roomController.RoomInfo.RoomSettings.RoomName, roomController);
                
                SendMessage(CreateMessage(ServerToClientId.createdRoom)
                    , fromClientId);

                OnNewRoomCreated(roomController);
                
                Logger.LogInfo(Tag, $"Client [{fromClientId}] [{clientData.FirebaseId}] Created new room");
            }
        }       
        
        [MessageHandler((ushort)ClientToServerId.joinRoom)]
        public static async void MessageHandler_JoinRoom(ushort fromClientId, Message message)
        {
            string roomName = message.GetString();

            if (ClientManager.List.TryGetValue(fromClientId, out ClientData clientData))
            {
                if (clientData.CurrentRoom != null) // Already in room
                {
                    SendMessage(CreateMessage(ServerToClientId.joinRoomFail)
                            .AddInt((int)ErrorType.IN_ROOM)
                        , fromClientId);
                    return;
                }
                if (!_roomsMap.ContainsKey(roomName))
                {
                    SendMessage(CreateMessage(ServerToClientId.joinRoomFail)
                            .AddInt((int)ErrorType.DOES_NOT_EXIST)
                        , fromClientId);
                    
                    return;
                }

                if (_roomsMap.TryGetValue(roomName, out RoomController roomController))
                {
                    if (roomController.RoomInfo.PlayersCount >= roomController.RoomInfo.RoomSettings.MaxPlayers)
                    {
                        SendMessage(CreateMessage(ServerToClientId.joinRoomFail)
                            .AddInt((int)ErrorType.NO_FREE_SPACE)
                            , fromClientId);
                        return;
                    }
                    
                    else if (!roomController.AddClient(clientData))
                    {
                        SendMessage(CreateMessage(ServerToClientId.joinRoomFail)
                                .AddInt((int)ErrorType.BLOCKED)
                            , fromClientId);
                        return;
                    }
                    
                    SendMessage(CreateMessage(ServerToClientId.joinedRoom)
                        , fromClientId);

                    OnRoomStateChanged(roomController);
                    
                    Logger.LogInfo(Tag, $"Client [{fromClientId}] [{clientData.FirebaseId}] joined to room [{roomController.RoomInfo.RoomSettings.RoomName}]");
                }
            }
        }
        
        [MessageHandler((ushort)ClientToServerId.leftRoom)]
        public static async void MessageHandler_LeftRoom(ushort fromClientId, Message message)
        {
            if (ClientManager.List.TryGetValue(fromClientId, out ClientData clientData))
            {
                if (clientData.CurrentRoom == null) 
                {
                    SendMessage(CreateMessage(ServerToClientId.joinRoomFail)
                            .AddInt((int)ErrorType.NOT_IN_ROOM)
                        , fromClientId);
                    return;
                }

                var room = clientData.CurrentRoom;

                clientData.CurrentRoom = null;
                clientData.Balance = 0;
                
                room.RemoveClient(clientData);

                OnRoomStateChanged(room);
                
                Logger.LogInfo(Tag, $"Client [{fromClientId}] [{clientData.FirebaseId}] left from room [{room.RoomInfo.RoomSettings.RoomName}]");
            }
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
            foreach (var roomController in _roomsMap)
            {
                roomController.Value.OnServerTick();
            }
        }
        
        
        static Message CreateMessage(ServerToClientId id) => Message.Create(MessageSendMode.Reliable, id);
        static void SendMessage(Message message, ushort clientId) => Server.SendMessage(message, clientId);
    }
}