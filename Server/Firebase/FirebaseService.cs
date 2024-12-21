﻿using System;
using System.Threading.Tasks;
using Server.Core;

namespace WindowsFormsApp1.Database
{
    public static class FirebaseService
    {
        public const string Tag = "Firebase_Service";
        public static string AccessToken { get; private set; }
        public static DateTime TokenExpiration { get; private set; }
        public static bool Initialized { get; private set; }
        
        static TokenGenerator _tokenGenerator;
        
        public static async Task Initialize()
        {
            _tokenGenerator = new TokenGenerator(Constants.ServiceAccountKeyPath);
            
            AccessToken = await _tokenGenerator.GetAccessTokenAsync();
            TokenExpiration = DateTime.Now + TimeSpan.FromHours(1);
            
            Logger.LogInfo(Tag, $"Initialized. Access token [{AccessToken}]");
            Initialized = true;
        }

        public static async Task RefreshAccessTokenAsync()
        {
            AccessToken = await _tokenGenerator.GetAccessTokenAsync();
        }
        
    }
}