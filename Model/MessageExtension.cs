﻿using System;
using Kwicot.Server.ClientLibrary.Models.Enums;
using Model.Room;
using Riptide;
using WindowsFormsApp1.Room;

namespace Model
{
    public static class MessageExtension
    {
        #region UserData

        public static Message AddUserData(this Message message, UserData value) => value.GetMessageData(message);
        public static UserData GetUserData(this Message message) => UserData.GetDataFromMessage(message);

        #endregion
        
        #region UserProfile

        public static Message AddUserProfile(this Message message, UserProfile value) => value.GetMessageData(message);
        public static UserProfile GetUserProfile(this Message message) => UserProfile.GetDataFromMessage(message);

        #endregion
        
        #region DateTime

        public static Message AddDateTime(this Message message, DateTime value) => Add(message, value);
        public static Message Add(this Message message, DateTime value)
        {
            message.AddInt(value.Year);
            message.AddInt(value.Month);
            message.AddInt(value.Day);
            message.AddInt(value.Hour);
            message.AddInt(value.Minute);
            message.AddInt(value.Second);
            message.AddInt(value.Millisecond);
            
            return message;
        }

        public static DateTime GetDateTime(this Message message)
        {
            int year = message.GetInt();
            int month = message.GetInt();
            int day = message.GetInt();
            int hour = message.GetInt();
            int minute = message.GetInt();
            int second = message.GetInt();
            int millisecond = message.GetInt();
            
            return new DateTime(year, month, day, hour, minute, second, millisecond);
        }

        #endregion
        
        #region RoomSettings

        public static Message AddRoomSettings(this Message message, RoomSettings value) => value.GetMessageData(message);
        public static RoomSettings GetRoomSettings(this Message message) => RoomSettings.GetDataFromMessage(message);

        #endregion
        
        #region RoomInfo

        public static Message AddRoomInfo(this Message message, RoomInfo value) => value.GetMessageData(message);
        public static RoomInfo GetRoomInfo(this Message message) => RoomInfo.GetDataFromMessage(message);

        #endregion
        
        #region RoomSettingsPreset
        
        public static Message AddRoomSettingsPreset(this Message message, RoomSettingPreset value) => value.GetMessageData(message);
        public static RoomSettingPreset GetRoomSettingsPreset(this Message message) => RoomSettingPreset.GetDataFromMessage(message);
        
        #endregion
        
        #region RoomSettingsPresets
        
        public static Message AddRoomSettingsPresets(this Message message, RoomSettingPreset[] value)
        {
            message.AddInt(value.Length);
            foreach (var roomSettingPreset in value)
                message.AddRoomSettingsPreset(roomSettingPreset);

            return message;
        }
        public static RoomSettingPreset[] GetRoomSettingsPresets(this Message message)
        {
            int count = message.GetInt();
            RoomSettingPreset[] presets = new RoomSettingPreset[count];
            for (int i = 0; i < count; i++)
            {
                presets[i] = message.GetRoomSettingsPreset();
            }

            return presets;
        }
        
        #endregion
    }
}