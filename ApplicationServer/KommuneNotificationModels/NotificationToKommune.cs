using System;
using Data;

namespace KommuneNotificationModels
{
    public abstract class NotificationToKommune
    {
        public int NotificationId { get; set; }
        public long Timestamp { get; set; }
        public string Address { get; set; }
        public string DeviceEui { get; set; }
    }

    public class ObjectNotification : NotificationToKommune
    {
        public ObjectDetection ObjectDetection { get; set; }
    }

    public class ObjectNotificationDetectedWithSize : ObjectNotification
    {
        public int WidthCentimeters { get; set; }
    }

    public class DeviceStatusNotification : NotificationToKommune
    {
        public bool DeviceUnresponsive { get; set; }
    }
}