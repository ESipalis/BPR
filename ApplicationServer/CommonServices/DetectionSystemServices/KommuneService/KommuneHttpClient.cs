using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using KommuneNotificationModels;

namespace CommonServices.DetectionSystemServices.KommuneService
{
    public class KommuneHttpClient
    {

        private readonly HttpClient _httpClient;

        public KommuneHttpClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task SendNotifications(IEnumerable<NotificationToKommune> notifications)
        {
            await _httpClient.PostAsync("Notifications", new StringContent(JsonSerializer.Serialize(notifications), Encoding.UTF8, "application/json"));
        }
    }
}