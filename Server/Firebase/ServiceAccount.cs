using System;
using Newtonsoft.Json;

namespace WindowsFormsApp1.Database
{
    [Serializable]
    public class ServiceAccount
    {
        [JsonProperty("private_key")]
        public string PrivateKey { get; set; }

        [JsonProperty("client_email")]
        public string ClientEmail { get; set; }
        
        [JsonProperty("private_key_id")]
        public string PrivateKeyId { get; set; }
    }
}