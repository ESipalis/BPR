using Data;

namespace CommonServices.DetectionSystemServices.Storage
{
    public class ConfigureDevice
    {
        public string DeviceEui { get; set; }
        public DeviceConfiguration Configuration { get; set; }
    }
}