using System;

namespace Kwicot.Server.ClientLibrary.Models.Enums
{
    [Serializable]
    public enum ServerToClientId : ushort
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
        updateUserProfileResult = 15,
        
        playerJoinedRoom = 16,
        playerLeftRoom = 17,
        
        updateSeatData = 18,
        updateStateMachineData = 19,
        updateRoomData = 20,
        seatRequestResult = 21,
        
        getUserDataResult = 22,
        
        offerToTopUpBalance = 23,
        topUpBalanceResult = 24,
        withdraw = 25,

        addCard = 26,
        turn = 27,
        endTurn = 28,
        showCards = 29,
        onReturn = 30,
        onWin = 31,
        onEndGame = 32,
        
        
    }
}