using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KommuneNotificationModels;

namespace CommonServices.DetectionSystemServices.KommuneService
{
    public class KommuneServiceHttp : IKommuneService
    {
        private readonly KommuneHttpClient _httpClient;

        public KommuneServiceHttp(KommuneHttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task SendNotifications(IEnumerable<NotificationToKommune> notifications)
        {
            try
            {
                await _httpClient.SendNotifications(notifications);
            }
            catch (Exception e)
            {
                throw new KommuneCommunicationException("Error communicating with kommune service", e);
            }
        }
    }
}