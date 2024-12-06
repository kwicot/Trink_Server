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
    }
}