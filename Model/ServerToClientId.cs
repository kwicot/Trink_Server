﻿using System;

namespace Kwicot.Server.ClientLibrary.Models.Enums
{
    [Serializable]
    public enum ServerToClientId 
    {
        connectedToMaster = 0,
        connectionToMasterFail = 1,
        
        connectedToLobby = 2,
        connectionToLobbyFail = 3,
        
        disconnectedFromMaster = 4,
        disconnectFromMasterFail = 5,
        
        disconnectedFromLobby = 6,
        disconnectFromLobbyFail = 7,
        
        createdRoom = 8,
        createRoomFail = 9,
        
        joinedRoom = 10,
        joinRoomFail = 11,
        
        leftRoom = 12,
        leftRoomFail = 13,
        
        roomListUpdate = 14,
    }
}