using System;
using System.Threading.Tasks;
using System.Windows.Forms;
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

            Logger.Initialize();
            Logger.IsDebug = true;

            Config = LoadConfig();
            
            await FirebaseService.Initialize();
            await FirebaseDatabase.Initialize();
            await UsersDatabase.Initialize();

            await ClientManager.Initialize();
            await Master.Initialize();
            await RoomManager.Initiailize();
            await Lobby.Initialize();
            
            Server.Initialize();
            
            Application.Run(new MainForm());
        }


        static Config LoadConfig()
        {
            var config = FilesHelper.Load<Config>(Constants.ConfigPath);
            if (config == null)
            {
                config = new Config();
                SaveConfig(config);
            }

            return config;
        }

        static void SaveConfig(Config config)
        {
            FilesHelper.Save(Constants.ConfigPath, config);
        }
    }
}