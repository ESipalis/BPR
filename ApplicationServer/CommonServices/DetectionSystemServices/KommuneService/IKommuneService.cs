using System.Collections.Generic;
using System.Threading.Tasks;
using KommuneNotificationModels;

namespace CommonServices.DetectionSystemServices.KommuneService
{
    public interface IKommuneService
    {
        public Task SendNotifications(IEnumerable<NotificationToKommune> notifications);
    }
}