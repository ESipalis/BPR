using System;
using System.Threading;
using System.Threading.Tasks;
using CommonServices.DetectionSystemServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;


namespace ApplicationServer.BackgroundServices
{
    public class ResendNotificationService : TimedBackgroundService
    {

        public ResendNotificationService(ILogger<ResendNotificationService> logger, IServiceProvider serviceProvider)
            : base(serviceProvider, TimeSpan.FromMinutes(1), "Resend notifications", logger)
        {
        }

        protected override async Task DoWorkAsync()
        {
            using IServiceScope scope = ServiceProvider.CreateScope();
            var detectionSystemService = scope.ServiceProvider.GetRequiredService<DetectionSystemService>();
            _logger.LogInformation("Resending failed notifications...");
            await detectionSystemService.ResendFailedNotifications();
        }

    }
}