﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kwicot.Server.ClientLibrary.Models.Enums;
using Model;
using Riptide;
using Server.Core.Models;
using Trink_RiptideServer.Library.StateMachine;
using WindowsFormsApp1;
using WindowsFormsApp1.Room;

namespace Server.Core.Rooms
{
    public class RoomController
    {
        public string Tag => $"Room_{RoomInfo.RoomSettings.RoomName}";
        public RoomInfo RoomInfo { get; }

        private bool _removeTimerEnable;
        private DateTime _removeTimerStart;
        
        public SeatController[] Seats { get; }
        public StateMachine StateMachine { get; private set; }
        
        public int Balance;

        public bool IsWaitingPlayer(string firebaseId)
        {
            foreach (var seatController in Seats)
            {
                if (!seatController.IsFree && seatController.SeatData.FirebaseId == firebaseId &&
                    seatController.Waiting)
                    return true;
            }

            return false;
        }
        
        SeatController SeatOfPlayer(ClientData clientData)
        {
            foreach (var seatController in Seats)
            {
                if (seatController.ClientData != null && seatController.ClientData.ClientID == clientData.ClientID)
                    return seatController;
            }

            return null;
        }
        
        public RoomController(RoomSettings roomSettings)
        {
            RoomInfo = new RoomInfo()
            {
                RoomSettings = roomSettings,
                Players = new List<UserData>()
            };

            Seats = new[]
            {
                new SeatController() { Index = 0, RoomController = this},
                new SeatController() { Index = 1, RoomController = this },
                new SeatController() { Index = 2, RoomController = this },
                new SeatController() { Index = 3, RoomController = this },
                new SeatController() { Index = 4, RoomController = this },
                new SeatController() { Index = 5, RoomController = this },
                new SeatController() { Index = 6, RoomController = this },
                new SeatController() { Index = 7, RoomController = this }
            };

            StateMachine = new StateMachine(this);

            Logger.LogInfo(Tag, $"Room created");

            _removeTimerEnable = true;
            _removeTimerStart = Server.ServerTime;
        }

        public void OnServerTick()
        {
            if (_removeTimerEnable)
            {
                var timeLeft = Server.ServerTime - _removeTimerStart;
                if (timeLeft.TotalMilliseconds >= RoomInfo.RoomSettings.RoomTtl)
                {
                    Logger.LogInfo(Tag, $"Remove room via TTL: {RoomInfo.RoomSettings.RoomTtl}");

                    _removeTimerEnable = false;
                    RoomManager.RemoveRoom(this);
                }
            }
        }
        
        public async Task<bool> AddClient(ClientData clientData)
        {
            if (RoomInfo.Removed)
                return false;
            
            var userData = await UsersDatabase.GetUserData(clientData.FirebaseId);
            RoomInfo.Players.Add(userData);
            
            foreach (var seatController in Seats)
            {
                if (!seatController.IsFree)
                {
                    if (seatController.SeatData.FirebaseId == clientData.FirebaseId)
                    { 
                        seatController.ReturnPlayer(clientData);
                    }
                }
            }

            _removeTimerEnable = false;
            clientData.CurrentRoom = this;

            return true;
        }

        public async Task RemoveClient(ClientData clientData, bool waiting)
        {
            clientData.InGameScene = false;
            
            var userData = await UsersDatabase.GetUserData(clientData.FirebaseId);
            var seat = SeatOfPlayer(clientData);

            if (seat != null)
            {
                if(!waiting)
                    StateMachine.OnPlayerRemove(seat.Index);
                
                await seat.RemovePlayer(waiting);
            }

            RoomInfo.Players.Remove(userData);
            clientData.CurrentRoom = null;

            Logger.LogInfo(Tag, $"{userData.UserProfile.NickName} left");

            if (RoomInfo.PlayersCount == 0)
            {
                _removeTimerStart = Server.ServerTime;
                _removeTimerEnable = true;
            }
        }


        public void OnPlayerLoadedScene(ClientData clientData)
        {
            clientData.InGameScene = true;
            SendDataToNewPlayer(clientData);

            StateMachine.OnPlayerLoaded(clientData.ClientID);
        }

        public async Task OnServerStopping()
        {
            await StateMachine.OnServerStopping();
        }


        void SendDataToNewPlayer(ClientData clientData)
        {
            SendRoomData();
            StateMachine.SendData();
            
            foreach (var seat in Seats)
                seat.SendData(clientData.ClientID);
        }
        public void SendRoomData()
        {
            SendToAll(CreateMessage(ServerToClientId.updateRoomData)
                .AddRoomInfo(RoomInfo));
        }
        
        static Message CreateMessage(ServerToClientId id) => Message.Create(MessageSendMode.Reliable, id);
        static void SendMessage(Message message, ushort clientId)
        {
            if(ClientManager.List.TryGetValue(clientId, out var client) && client.InGameScene)
                Server.SendMessage(message, clientId);
        }

        public void SendToAll(Message message, params ushort[] excludeClients)
        {
            foreach (var infoPlayer in RoomInfo.Players)
            {
                if(!excludeClients.Contains(infoPlayer.ClientId))
                    SendMessage(message, infoPlayer.ClientId);
            }
        }
    }
}