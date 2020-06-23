using CommonServices.DetectionSystemServices;
using Xunit;

namespace CommonServices.Test
{
    public class DetectionSystemServiceUtilTest
    {
        
        [Fact]
        public void ConfigurationToDataStringTest()
        {
            Assert.Equal("04C105", DetectionSystemServiceUtil.ConfigurationToDataString(1217, 5));
            Assert.Equal("001F05", DetectionSystemServiceUtil.ConfigurationToDataString(31, 5));
            Assert.Equal("000305", DetectionSystemServiceUtil.ConfigurationToDataString(3, 5));
            Assert.Equal("00001C", DetectionSystemServiceUtil.ConfigurationToDataString(0, 28));
        }
    }
}