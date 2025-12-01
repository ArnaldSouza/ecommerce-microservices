using EstoqueService.Services;

namespace EstoqueService.BackgroundServices
{
    public class SaleConfirmedConsumer : BackgroundService
    {
        private readonly IRabbitMqService _rabbitMqService;
        private readonly ILogger<SaleConfirmedConsumer> _logger;

        public SaleConfirmedConsumer(IRabbitMqService rabbitMqService, ILogger<SaleConfirmedConsumer> logger)
        {
            _rabbitMqService = rabbitMqService;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("SaleConfirmedConsumer iniciado");

            try
            {
                await _rabbitMqService.StartConsumingAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no SaleConfirmedConsumer");
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("SaleConfirmedConsumer parando...");
            _rabbitMqService.StopConsuming();
            await base.StopAsync(cancellationToken);
        }
    }
}