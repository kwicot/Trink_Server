using System;
using System.IO;

namespace Server.Core
{
    public static class Constants
    {
        public static string RootPath = AppDomain.CurrentDomain.BaseDirectory;
        public static string UsersDatabasePath => Path.Combine(RootPath, "Data", "Users.db");
        public static string ConfigPath => Path.Combine(RootPath, "Data", "Config.cfg");
        
        public static string FirebaseUrl = "https://trink-3fd78-default-rtdb.firebaseio.com/";
        public static string FirebaseDatabaseUsersPath => "Users/";

        public static string ServiceAccountKeyPath = Path.Combine(RootPath, "Data", "trink-service.json");
    }
}