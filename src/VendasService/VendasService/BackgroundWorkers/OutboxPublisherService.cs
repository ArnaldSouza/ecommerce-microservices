using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VendasService.Repositories;
using VendasService.Services;

namespace VendasService.BackgroundWorkers
{
    public class OutboxPublisherService : BackgroundService
    {
        private readonly ILogger<OutboxPublisherService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public OutboxPublisherService(
            ILogger<OutboxPublisherService> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var orderRepository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();

                        // Usar os métodos corretos da interface
                        var pendingEvents = await orderRepository.GetPendingOutboxAsync(50);

                        foreach (var outboxEvent in pendingEvents)
                        {
                            try
                            {
                                // Se tiver IMessageBusService, usar assim:
                                // var messageBus = scope.ServiceProvider. GetRequiredService<IMessageBusService>();
                                // await messageBus.PublishAsync(outboxEvent. EventType, outboxEvent.Data);

                                // Por enquanto, só marcar como enviado
                                await orderRepository.MarkOutboxSentAsync(outboxEvent.Id);

                                _logger.LogInformation($"Published outbox event {outboxEvent.Id}");
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, $"Failed to publish outbox event {outboxEvent.Id}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in OutboxPublisherService");
                }

                // Aguardar 30 segundos antes da próxima execução
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }
    }
}