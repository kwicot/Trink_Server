using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kwicot.Server.ClientLibrary.Models.Enums;
using Model;
using Riptide;
using Server.Core.Models;
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

        public int Balance;
        
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

            _removeTimerEnable = false;
            clientData.CurrentRoom = this;

            Logger.LogInfo(Tag, $"{userData.UserProfile.NickName} joined");
            return true;
        }

        public async void RemoveClient(ClientData clientData)
        {
            var userData = await UsersDatabase.GetUserData(clientData.FirebaseId);

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
            
        }
        
        
        static Message CreateMessage(ServerToClientId id) => Message.Create(MessageSendMode.Reliable, id);
        static void SendMessage(Message message, ushort clientId) => Server.SendMessage(message, clientId);

        public void SendToAll(Message message)
        {
            foreach (var infoPlayer in RoomInfo.Players)
            {
                SendMessage(message, infoPlayer.ClientId);
            }
        }
    }
}