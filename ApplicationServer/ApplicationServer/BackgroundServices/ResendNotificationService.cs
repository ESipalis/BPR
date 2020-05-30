using System;
using System.Threading;
using System.Threading.Tasks;
using CommonServices.DetectionSystemServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;


namespace ApplicationServer.BackgroundServices
{
    public class ResendNotificationService : IHostedService, IDisposable
    {
        private readonly ILogger<ResendNotificationService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private Timer _timer;

        public ResendNotificationService(ILogger<ResendNotificationService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Resend notifications service started");

            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));

            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            DoWorkAsync().Wait();
        }

        private async Task DoWorkAsync()
        {
            using IServiceScope scope = _serviceProvider.CreateScope();
            var detectionSystemService = scope.ServiceProvider.GetRequiredService<DetectionSystemService>();
            await detectionSystemService.ResendFailedNotifications();
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Resend notifications service is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}