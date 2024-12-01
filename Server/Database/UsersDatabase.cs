using System.Collections.Generic;
using System.Threading.Tasks;
using Model;
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
            
            var data = FirebaseDatabase.GetDataAsync<UserData>(Constants.FirebaseDatabaseUsersPath + firebaseId);
        
            if (data == null)
            {
                var userData = new UserData()
                {
                    Balance = Program.Config.RegisterBalance,
                    NickName = $"Player"
                };
                
                await FirebaseDatabase.WriteDataAsync(Constants.FirebaseDatabaseUsersPath + firebaseId, userData);
                _usersMap.Add(firebaseId, userData);
            }
            
            return _usersMap[firebaseId];
        }

        public static async Task UpdateUserData(string firebaseId, UserData userData)
        {
            await FirebaseDatabase.WriteDataAsync(Constants.FirebaseDatabaseUsersPath + firebaseId, userData);
        }

    }
}