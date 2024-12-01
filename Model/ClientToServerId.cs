using System;

namespace Kwicot.Server.ClientLibrary.Models.Enums
{
    [Serializable]
    public enum ClientToServerId
    {
        connectToMaster = 0,
        disconnectFromMaster = 1,
        
        connectToLobby = 2,
        disconnectFromLobby = 3,
        
        createRoom = 4,
        joinRoom = 5,
        leftRoom = 6,
        
    }
}