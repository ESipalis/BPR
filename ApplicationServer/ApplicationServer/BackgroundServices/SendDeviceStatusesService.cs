using System;
using System.Threading.Tasks;
using CommonServices.DetectionSystemServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ApplicationServer.BackgroundServices
{
    public class SendDeviceStatusesService : TimedBackgroundService
    {
        
        public SendDeviceStatusesService(ILogger<SendDeviceStatusesService> logger, IServiceProvider serviceProvider)
            : base(serviceProvider, TimeSpan.FromMinutes(1), "Resend notifications", logger)
        {
        }

        protected override async Task DoWorkAsync()
        {
            using IServiceScope scope = ServiceProvider.CreateScope();
            var detectionSystemService = scope.ServiceProvider.GetRequiredService<DetectionSystemService>();
            await detectionSystemService.RefreshAndSendDeviceStatuses();
        }
        
    }
}