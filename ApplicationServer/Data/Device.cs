using System;

namespace Data
{

    public class Device
    {
        public int DeviceId { get; set; }
        public string DeviceEui { get; set; }
        public string Address { get; set; }
        
        public DeviceStatus Status { get; set; }
        public DeviceConfiguration Configuration { get; set; }
    }

    public class DeviceStatus
    {
        public bool DeviceWorking { get; set; }
        public bool SentToKommune { get; set; }

        public int DeviceId { get; set; }
        public Device Device { get; set; }
    }
    
    public class DeviceConfiguration
    {
        public ConfigurationStatus Status { get; set; }
        public short ScanMinuteOfTheDay { get; set; }
        public byte HeartbeatPeriodDays { get; set; }
        
        public int DeviceId { get; set; }
        public Device Device { get; set; }
    }
    
    public enum ConfigurationStatus
    {
        NotSent,
        SentToNetwork,
        AcknowledgedByNetwork,
        ErrorByNetwork,
        SentToGateway,
        SentToDevice,
        Acknowledged
    }
}