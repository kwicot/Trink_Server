using Server.Core;
using Server.Core.Rooms;
using WindowsFormsApp1;
using WindowsFormsApp1.Database;

namespace ServerLibrary;

public class Program
{
    public static async Task Main(string[] args)
    {
        Logger.Initialize();
        Logger.IsDebug = true;

        //Config = LoadConfig();
            
        await FirebaseService.Initialize();
        await FirebaseDatabase.Initialize();
        await UsersDatabase.Initialize();

        await ClientManager.Initialize();
        await Master.Initialize();
        await RoomManager.Initiailize();
        await Lobby.Initialize();
            
        Server.Core.Server.Initialize();

        Server.Core.Server.Start(80, 100);
    }
}