namespace Data
{
    public class Notification
    {
        public int NotificationId { get; set; }
        public NotificationType Type { get; set; }
        public long Timestamp { get; set; }
        public string DeviceEui { get; set; }
        public string Address { get; set; }
        public ObjectDetectionNotification ObjectDetectionNotification { get; set; }
    }

    public class ObjectDetectionNotification
    {
        public int NotificationId { get; set; }
        public Notification Notification { get; set; }
        
        public bool SentToKommune { get; set; }
        public int? WidthCentimeters { get; set; }
        public ObjectDetection ObjectDetection { get; set; }
    }

    public enum NotificationType
    {
        ObjectDetection,
        Heartbeat
    }
    
    public enum ObjectDetection
    {
        DetectedWithSize,
        Detected,
        Removed
    }
}