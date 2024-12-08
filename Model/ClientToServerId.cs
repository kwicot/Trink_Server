using System;

namespace Kwicot.Server.ClientLibrary.Models.Enums
{
    [Serializable]
    public enum ClientToServerId : ushort
    {
        connectToMaster = 0,
        disconnectFromMaster = 1,
        
        connectToLobby = 2,
        disconnectFromLobby = 3,
        
        createRoom = 4,
        joinRoom = 5,
        leftRoom = 6,
        
        updateUserProfile = 7,
        sceneLoaded = 8,
        
        requestSeat = 9,
        getUserData = 10,
        
        requestTopUpBalance = 11,
        turn = 12,
        checkCards = 13,
    }
}