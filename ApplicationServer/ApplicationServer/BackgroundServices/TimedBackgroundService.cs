using System;

namespace ApplicationServer.BackgroundServices
{
    public abstract class TimedBackgroundService
    {
        protected IServiceProvider ServiceProvider { get; }
        private readonly TimeSpan _period;

        protected TimedBackgroundService(IServiceProvider serviceProvider, TimeSpan period)
        {
            ServiceProvider = serviceProvider;
            _period = period;
        }
        
        
    }
}