using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kwicot.Server.ClientLibrary.Models.Enums;
using Model;
using Riptide;
using Server.Core;
using WindowsFormsApp1.Database;

namespace WindowsFormsApp1
{
    public static class UsersDatabase
    {
        private const string Tag = "Users_Database";
        
        private static Dictionary<string, UserData> _usersMap;
        
        public static async Task Initialize()
        {
            LoadUsers();
        }

        static void LoadUsers()
        {
            _usersMap = FilesHelper.Load<Dictionary<string, UserData>>(Constants.UsersDatabasePath);
            if (_usersMap == null)
                _usersMap = new Dictionary<string, UserData>();
            
        }

        static void SaveUsers()
        {
            FilesHelper.Save(Constants.UsersDatabasePath, _usersMap);
        }


        public static async Task<UserData> GetUserData(string firebaseId)
        {
            if(_usersMap.ContainsKey(firebaseId))
                return _usersMap[firebaseId];
            
            var data = await FirebaseDatabase.GetDataAsync<UserData>(Constants.FirebaseDatabaseUsersPath + firebaseId);
        
            if (data == null)
            {
                data = CreateNewUserData(firebaseId);
                
                await FirebaseDatabase.WriteDataAsync(Constants.FirebaseDatabaseUsersPath + firebaseId, data);
            }
            
            _usersMap.Add(firebaseId, data);
            
            return _usersMap[firebaseId];
        }

        static UserData CreateNewUserData(string firebaseId)
        {
            return new UserData()
            {
                Balance = Program.Config.RegisterBalance,
                UserProfile = new UserProfile()
                {
                    NickName = "Player",
                    Picture = Array.Empty<byte>()
                },
                FirebaseId = firebaseId
            };
        }

        public static async Task UpdateUserData(string firebaseId, UserData userData)
        {
            SaveUsers();
            
            
            
            //await FirebaseDatabase.WriteDataAsync(Constants.FirebaseDatabaseUsersPath + firebaseId, userData);
        }

        public static async Task UpdateUserData(UserData userData) => UpdateUserData(userData.FirebaseId, userData);


        [MessageHandler((ushort)ClientToServerId.getUserData)]
        public static async void MessageHandler_GetUserData(ushort fromClientId, Message message)
        {
            string requestId = message.GetString();
            string firebaseId = message.GetString();
            
            var userData = await GetUserData(firebaseId);

            if (userData != null)
            {
                SendMessage(CreateMessage(ServerToClientId.getUserDataResult)
                    .AddString(requestId)
                    .AddBool(true)
                    .AddUserData(userData)
                    , fromClientId);
            }
            else
            {
                SendMessage(CreateMessage(ServerToClientId.getUserDataResult)
                        .AddString(requestId)
                        .AddBool(false)
                        .AddInt((int)ErrorType.DOES_NOT_EXIST)
                    , fromClientId);
                
                Logger.LogInfo(Tag, "No user data found");
            }
        }
        
        static Message CreateMessage(ServerToClientId id) => Message.Create(MessageSendMode.Reliable, id);
        static void SendMessage(Message message, ushort clientId) => Server.Core.Server.SendMessage(message, clientId);

    }
}