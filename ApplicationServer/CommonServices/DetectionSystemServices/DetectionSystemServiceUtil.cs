using System.Collections.Generic;
using System.Linq;
using Data;
using KommuneNotificationModels;

namespace CommonServices.DetectionSystemServices
{
    public static class DetectionSystemServiceUtil
    {
        public static List<NotificationToKommune> NotificationsToKommuneNotifications(IEnumerable<Notification> notifications)
        {
            IEnumerable<NotificationToKommune> notificationsToKommune = notifications.Select(notification =>
            {
                if (notification.ObjectDetectionNotification.ObjectDetection == ObjectDetection.DetectedWithSize)
                {
                    return new ObjectNotificationDetectedWithSize
                    {
                        NotificationId = notification.NotificationId,
                        Address = notification.Address,
                        DeviceEui = notification.Address,
                        Timestamp = notification.Timestamp,
                        ObjectDetection = notification.ObjectDetectionNotification.ObjectDetection,
                        WidthCentimeters = notification.ObjectDetectionNotification.WidthCentimeters.Value
                    };
                }
                else
                {
                    return new ObjectNotification
                    {
                        NotificationId = notification.NotificationId,
                        Address = notification.Address,
                        DeviceEui = notification.Address,
                        Timestamp = notification.Timestamp,
                        ObjectDetection = notification.ObjectDetectionNotification.ObjectDetection
                    };
                }
            });
            return notificationsToKommune.ToList();
        }
        
    }
}