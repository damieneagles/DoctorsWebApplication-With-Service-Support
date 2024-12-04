using App.QueueService;
using App.ScopedService;

namespace DoctorsWebApplication.DoctorsServices
{
        public class ConsumeQueueServiceHostedService : BackgroundService
        {
            private readonly ILogger<ConsumeQueueServiceHostedService> _logger;

            public ConsumeQueueServiceHostedService(IServiceProvider services,
                ILogger<ConsumeQueueServiceHostedService> logger)
            {
                Services = services;
                _logger = logger;
            }

            public IServiceProvider Services { get; }

            protected override async Task ExecuteAsync(CancellationToken stoppingToken)
            {
                _logger.LogInformation(
                    "Consume Scoped Service Hosted Service running.");

                await DoWork(stoppingToken);
            }

            private async Task DoWork(CancellationToken stoppingToken)
            {
                _logger.LogInformation(
                    "Consume Scoped Service Hosted Service is working.");

                using (var scope = Services.CreateScope())
                {
                    var scopedProcessingService =
                        scope.ServiceProvider
                            .GetRequiredService<IBackgroundTaskQueue>();

                    await scopedProcessingService.DoWorkAsync(stoppingToken);
                }
            }

            public override async Task StopAsync(CancellationToken stoppingToken)
            {
                _logger.LogInformation(
                    "Consume Scoped Service Hosted Service is stopping.");

                await base.StopAsync(stoppingToken);
            }
        }
    }
}
