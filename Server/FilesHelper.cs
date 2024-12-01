using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Model;
using Newtonsoft.Json;

namespace WindowsFormsApp1
{
    public static class FilesHelper
    {
        public static string Tag { get; private set; } = "FilesHelper";
        
        
        public static T Load<T>(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    var bytes = File.ReadAllBytes(filePath);
                    var json = Encoding.UTF8.GetString(bytes);
                    var data = JsonConvert.DeserializeObject<T>(json);
                    
                    Logger.LogInfo(Tag, $"Loaded file on path: {filePath}");
                    return data;
                }
                else
                {
                    Logger.LogInfo(Tag, $"Cant load file on path: {filePath}.  File not found.");
                    return default;
                }
            }
            catch (Exception e)
            {
                Logger.LogError(Tag, e.ToString());
                return default;
            }
        }

        public static bool Save<T>(string filePath, T data)
        {
            try
            {
                var json = JsonConvert.SerializeObject(data, Formatting.Indented);
                var bytes = Encoding.UTF8.GetBytes(json);
                
                File.WriteAllBytes(filePath, bytes);                

                Logger.LogInfo(Tag, $"File: {filePath} saved successfully");
                return true;
            }
            catch (Exception e)
            {
                Logger.LogError(Tag, $"Failed to save file: {filePath}. {e}");
                return false;
            }
        }
    }
}