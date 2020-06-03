using CommonServices.DetectionSystemServices;
using Xunit;

namespace CommonServices.Test
{
    public class DetectionSystemServiceUtilTest
    {
        
        [Fact]
        public void ConfigurationToDataStringTest()
        {
            Assert.Equal("1F05", DetectionSystemServiceUtil.ConfigurationToDataString(31, 5));
            Assert.Equal("0305", DetectionSystemServiceUtil.ConfigurationToDataString(3, 5));
            Assert.Equal("001C", DetectionSystemServiceUtil.ConfigurationToDataString(0, 28));
        }
    }
}