using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Server.Core;

namespace WindowsFormsApp1.Database
{
    public static class FirebaseDatabase
    {
        private static string _databaseUrl;
        private static string _accessToken;

        public static async Task Initialize()
        {
            _databaseUrl = Constants.FirebaseUrl;
            _accessToken = FirebaseService.AccessToken;
        }

        public static async Task WriteDataAsync<T>(string path, T data)
        {
            var jsonData = JsonConvert.SerializeObject(data);

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _accessToken);

                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                var response = await client.PutAsync($"{_databaseUrl}/{path}.json", content);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Ошибка записи данных: {response.StatusCode}, {await response.Content.ReadAsStringAsync()}");
                }
            }
        }
        
        public static async Task<T> GetDataAsync<T>(string path)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _accessToken);

                var response = await client.GetAsync($"{_databaseUrl}/{path}.json");

                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"Ошибка при получении данных: {response.StatusCode}, {await response.Content.ReadAsStringAsync()}");
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(jsonResponse);
            }
        }
    }
}