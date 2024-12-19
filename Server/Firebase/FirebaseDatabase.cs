using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Server.Core;

namespace WindowsFormsApp1.Database
{
    public static class FirebaseDatabase
    {
        public static string Tag => "Firebase_Database";
        private static readonly HttpClient HttpClient = new HttpClient();

        private static readonly SemaphoreSlim
            QueueSemaphore = new SemaphoreSlim(1, 1); // Ограничение на одновременное выполнение

        private static readonly Queue<Func<Task>> RequestQueue = new Queue<Func<Task>>();

        public static async Task Initialize()
        {
        }

        private static void ConfigureHttpClient()
        {
            HttpClient.DefaultRequestHeaders.Clear();
            HttpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", FirebaseService.AccessToken);
        }

        private static async Task ProcessQueueAsync()
        {
            while (true)
            {
                Func<Task> request;

                // Получить следующий запрос из очереди
                await QueueSemaphore.WaitAsync();
                try
                {
                    if (RequestQueue.Count == 0)
                        break; // Выход, если очередь пуста

                    request = RequestQueue.Dequeue();
                }
                finally
                {
                    QueueSemaphore.Release();
                }

                // Выполнить запрос
                await request();
            }
        }

        private static async Task EnqueueRequestAsync(Func<Task> request)
        {
            await QueueSemaphore.WaitAsync();
            try
            {
                RequestQueue.Enqueue(request);
            }
            finally
            {
                QueueSemaphore.Release();
            }

            _ = Task.Run(ProcessQueueAsync); // Запустить обработку очереди
        }

        public static async Task WriteDataAsync<T>(string path, T data)
        {
            await EnqueueRequestAsync(async () =>
            {
                await ExecuteRequestWithRetryAsync(async () =>
                {
                    ConfigureHttpClient();
                    var jsonData = JsonConvert.SerializeObject(data);
                    var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                    var response = await HttpClient.PutAsync($"{Constants.FirebaseUrl}/{path}.json", content);

                    if (!response.IsSuccessStatusCode)
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        Logger.LogError(Tag, $"Error writing data: {response.StatusCode}, {errorContent}");
                    }
                });
            });
        }

        public static async Task<T> GetDataAsync<T>(string path)
        {
            T result = default;
            await EnqueueRequestAsync(async () =>
            {
                await ExecuteRequestWithRetryAsync(async () =>
                {
                    ConfigureHttpClient();
                    var response = await HttpClient.GetAsync($"{Constants.FirebaseUrl}/{path}.json");

                    if (!response.IsSuccessStatusCode)
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        Logger.LogError(Tag, $"Error reading data: {response.StatusCode}, {errorContent}");
                    }
                    else
                    {
                        var jsonResponse = await response.Content.ReadAsStringAsync();
                        result = JsonConvert.DeserializeObject<T>(jsonResponse);
                    }
                });
            });

            return result;
        }

        private static async Task ExecuteRequestWithRetryAsync(Func<Task> request)
        {
            bool retry = false;
            try
            {
                await request();
            }
            catch (HttpRequestException e) when (e.Message.Contains("401") || e.Message.Contains("Unauthorized"))
            {
                Logger.LogError(Tag, "Unauthorized request. Refreshing token and retrying...");
                retry = true;
                await RefreshAccessTokenAsync();
            }

            if (retry)
            {
                await request();
            }
        }

        private static async Task RefreshAccessTokenAsync()
        {
            await FirebaseService.RefreshAccessTokenAsync();
        }
    }
}