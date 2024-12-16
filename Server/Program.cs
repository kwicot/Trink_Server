using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Model;
using Server.Core;
using Server.Core.Rooms;
using WindowsFormsApp1.Database;

namespace WindowsFormsApp1
{
    static class Program
    {
        public static Config Config { get; private set; }
        
        [STAThread]
        static async Task Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Config = LoadConfig();
            
            Logger.Initialize();
            Logger.IsDebug = true;
            
            await FirebaseService.Initialize();
            await FirebaseDatabase.Initialize();
            await UsersDatabase.Initialize();
            
            await ClientManager.Initialize();
            await Master.Initialize();
            await RoomManager.Initiailize();
            await Lobby.Initialize();
            
            Server.Core.Server.Initialize();
            
            Application.Run(new MainForm());
        }


        static Config LoadConfig()
        {
            var config = FilesHelper.Load<Config>(Constants.ConfigPath);
            if (config == null)
            {
                config = new Config()
                {
                    RegisterBalance = 1000,
                    WriteLogToFile = false,
                    StateMachineConfig = new StateMachineConfig()
                };
                SaveConfig(config);
            }
        
            return config;
        }
        
        public static void SaveConfig(Config config = null)
        {
            if (config == null)
                config = Config;
            
            FilesHelper.Save(Constants.ConfigPath, config);
        }
    }
}