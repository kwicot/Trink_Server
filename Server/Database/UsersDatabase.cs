using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Model;
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
                data = CreateNewUserData();
                
                await FirebaseDatabase.WriteDataAsync(Constants.FirebaseDatabaseUsersPath + firebaseId, data);
            }
            
            _usersMap.Add(firebaseId, data);
            
            return _usersMap[firebaseId];
        }

        static UserData CreateNewUserData()
        {
            return new UserData()
            {
                Balance = Program.Config.RegisterBalance,
                UserProfile = new UserProfile()
                {
                    NickName = "Player",
                    Picture = Array.Empty<byte>()
                }
            };
        }

        public static async Task UpdateUserData(string firebaseId, UserData userData)
        {
            SaveUsers();
            
            
            
            //await FirebaseDatabase.WriteDataAsync(Constants.FirebaseDatabaseUsersPath + firebaseId, userData);
        }

    }
}