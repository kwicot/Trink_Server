using Kwicot.Server.ClientLibrary.Models.Enums;
using Riptide;
using Server.Core;

namespace WindowsFormsApp1
{
    public static class SeatMessageHandler
    {
        public static string Tag { get; } = "Seat_message_handler";
        
        [MessageHandler((ushort)ClientToServerId.requestSeat)]
        public static async void MessageHandler_RequestSeat(ushort fromClientId, Message message)
        {
            if (ClientManager.List.TryGetValue(fromClientId, out var clientData))
            {
                if (clientData.CurrentRoom != null)
                {
                    int seatIndex = message.GetInt();
                    clientData.CurrentRoom.Seats[seatIndex].OnRequestSeat(clientData);
                }
                else 
                    Logger.LogInfo(Tag, $"Request seat from [{fromClientId}] denied. Not in a room");
            }
        }
        
        [MessageHandler((ushort)ClientToServerId.requestTopUpBalance)]
        public static async void MessageHandler_RequestTopUpBalance(ushort fromClientId, Message message)
        {
            if (ClientManager.List.TryGetValue(fromClientId, out var clientData))
            {
                if (clientData.CurrentRoom != null)
                {
                    int seatIndex = message.GetInt();
                    int value = message.GetInt();
                    clientData.CurrentRoom.Seats[seatIndex].OnRequestTopUpBalance(value);
                }
                else 
                    Logger.LogInfo(Tag, $"Request TopOpBalance from [{fromClientId}] denied. Not in a room");
            }
        }

        [MessageHandler((ushort)ClientToServerId.turn)]
        public static async void MessageHandler_Turn(ushort fromClientId, Message message)
        {
            if (ClientManager.List.TryGetValue(fromClientId, out var clientData))
            {
                if (clientData.CurrentRoom != null)
                {
                    int seatIndex = message.GetInt();
                    int value = message.GetInt();
                    
                    Logger.LogInfo(Tag, $"Turn from Seat [{seatIndex}] Value [{value}]");
                    
                    clientData.CurrentRoom.StateMachine.OnSeatTurn(seatIndex, value);
                    clientData.CurrentRoom.Seats[seatIndex].OnTurn();
                }
                else 
                    Logger.LogInfo(Tag, $"Turn from [{fromClientId}] denied. Not in a room");
            }
        }
        
        
        [MessageHandler((ushort)ClientToServerId.checkCards)]
        public static async void MessageHandler_CheckCards(ushort fromClientId, Message message)
        {
            if (ClientManager.List.TryGetValue(fromClientId, out var clientData))
            {
                if (clientData.CurrentRoom != null)
                {
                    int seatIndex = message.GetInt();
                    clientData.CurrentRoom.StateMachine.OnSeatCheckCards(seatIndex);
                }
                else 
                    Logger.LogInfo(Tag, $"Turn from [{fromClientId}] denied. Not in a room");
            }
        }
    }
}