namespace CommonServices.DetectionSystemServices.Storage
{
    public class UnsentDeviceStatus
    {
        public int NotificationId { get; set; }
        public long Timestamp { get; set; }
        public string Address { get; set; }
        public string DeviceEui { get; set; }
        public bool DeviceUnresponsive { get; set; }
    }
}