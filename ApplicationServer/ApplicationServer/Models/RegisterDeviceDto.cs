namespace ApplicationServer.Models
{
    public class RegisterDeviceDto
    {
        public string DeviceEui { get; set; }
        public string Address { get; set; }
        public Configuration Configuration { get; set; }
    }
}