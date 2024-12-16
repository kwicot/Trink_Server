using System;
using System.Linq;
using Admin;
using FirebaseAdmin.Auth;
using Model;
using Riptide;

namespace WindowsFormsApp1;

public static class AdminController
{
    public static string Tag => "Admin_controller";
    
    [MessageHandler((ushort)AdminToServerId.updateUserBalance)]
    public static async void MessageHandler_UpdateUserBalance(ushort fromClientId, Message message)
    {
        string password = message.GetString();
        string firebaseId = message.GetString();
        int newBalance = message.GetInt();
        
        if (IsCorrectPassword(password))
        {
            Logger.LogWarning(Tag,$"Request to update user balance with id {firebaseId} to {newBalance}");
            
            var userData = await UsersDatabase.GetUserData(firebaseId);
            if (userData != null)
            {
                userData.Balance = newBalance;
                await UsersDatabase.UpdateUserData(userData);
                
                SendMessage(CreateMessage(ServerToAdminId.updateUserBalanceResult)
                    .AddBool(true)
                    , fromClientId);
            }
            else
            {
                SendMessage(CreateMessage(ServerToAdminId.updateUserBalanceResult)
                    .AddBool(false)
                    , fromClientId);
            }
        }
        else
        {
            Logger.LogWarning(Tag,"Invalid password");
            SendMessage(CreateMessage(ServerToAdminId.updateUserBalanceResult)
                    .AddBool(false)
                , fromClientId);
        }
    }
    
    [MessageHandler((ushort)AdminToServerId.deleteUser)]
    public static async void MessageHandler_DeleteUsers(ushort fromClientId, Message message)
    {
        string password = message.GetString();
        string id = message.GetString();
        
        if (IsCorrectPassword(password))
        {
            Logger.LogWarning(Tag,$"Request to delete user [{id}]");

            var pagedEnumerable = FirebaseAuth.DefaultInstance.ListUsersAsync(null);

            await foreach (var userRecord in pagedEnumerable)
            {
                if (id == userRecord.Uid)
                {
                    try
                    {
                        Logger.LogInfo(Tag, $"Deleting account {userRecord.Uid}");
                        await FirebaseAuth.DefaultInstance.DeleteUserAsync(userRecord.Uid);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogInfo(Tag, ex.Message);
                    }
                    
                    SendMessage(CreateMessage(ServerToAdminId.deleteUserResult)
                        .AddBool(true)
                        .AddString(id)
                        , fromClientId);
                }
            }
        }
        else
        {
            Logger.LogWarning(Tag,"Invalid password");
            SendMessage(CreateMessage(ServerToAdminId.deleteUserResult)
                    .AddBool(false)
                    .AddInt((int)ErrorType.PASSWORD_INCORECT)
                , fromClientId);
        }
    }
    
    [MessageHandler((ushort)AdminToServerId.deleteAllUsers)]
    public static async void MessageHandler_DeleteAllUsers(ushort fromClientId, Message message)
    {
        string password = message.GetString();
        
        if (IsCorrectPassword(password))
        {
            Logger.LogWarning(Tag,$"Request to delete all user");

            var pagedEnumerable = FirebaseAuth.DefaultInstance.ListUsersAsync(null);

            await foreach (var userRecord in pagedEnumerable)
            {
                string id = userRecord.Uid;
                    try
                    {
                        Logger.LogInfo(Tag, $"Deleting account {userRecord.Uid}");
                        await FirebaseAuth.DefaultInstance.DeleteUserAsync(userRecord.Uid);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogInfo(Tag, ex.Message);
                    }
                    
                    SendMessage(CreateMessage(ServerToAdminId.deleteUserResult)
                            .AddBool(true)
                            .AddString(id)
                        , fromClientId);
            }
        }
        else
        {
            Logger.LogWarning(Tag, "Invalid password");
            SendMessage(CreateMessage(ServerToAdminId.deleteUserResult)
                    .AddBool(false)
                , fromClientId);
        }
    }
    

    static bool IsCorrectPassword(string password)
    {
        if (password == Program.Config.AdminPassword)
            return true;

        return false;
    }
    
    static Message CreateMessage(ServerToAdminId id) => Message.Create(MessageSendMode.Reliable, id);
    static void SendMessage(Message message, ushort clientId) => Server.Core.Server.SendMessage(message, clientId);
}