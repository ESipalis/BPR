namespace CommonServices.DetectionSystemServices
{
    public class UplinkMessage
    {
        public string DeviceEui { get; set; }
        public long Timestamp { get; set; }
        public string Data { get; set; }
    }
}