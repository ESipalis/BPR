using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ApplicationServer.BackgroundServices
{
    public abstract class TimedBackgroundService : IHostedService, IDisposable
    {
        protected IServiceProvider ServiceProvider { get; }
        protected ILogger _logger { get; }
        private readonly TimeSpan _period;
        private readonly string _serviceName;
        private Timer _timer;

        protected TimedBackgroundService(IServiceProvider serviceProvider, TimeSpan period, string serviceName, ILogger logger)
        {
            ServiceProvider = serviceProvider;
            _period = period;
            _serviceName = serviceName;
            _logger = logger;
        }
        
        protected abstract Task DoWorkAsync();
        
        public Task StartAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation($"{_serviceName} service started");

            _timer = new Timer(DoWork, null, TimeSpan.Zero, _period);

            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            DoWorkAsync().Wait();
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation($"{_serviceName} service is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }

    }
}