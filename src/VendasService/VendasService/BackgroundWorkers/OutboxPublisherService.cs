using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using VendasService.Repositories;
using VendasService.Models.Entities;

namespace VendasService.BackgroundWorkers
{
    public class RabbitMqOptions
    {
        public string HostName { get; set; } = "rabbitmq";
        public int Port { get; set; } = 5672;
        public string UserName { get; set; } = "guest";
        public string Password { get; set; } = "guest";
        public string Exchange { get; set; } = "ecommerce.events";
    }

    public class OutboxPublisherService : BackgroundService
    {
        private readonly ILogger<OutboxPublisherService> _logger;
        private readonly IOrderRepository _repo;
        private readonly RabbitMqOptions _options;

        public OutboxPublisherService(ILogger<OutboxPublisherService> logger, IOrderRepository repo, IOptions<RabbitMqOptions> options)
        {
            _logger = logger;
            _repo = repo;
            _options = options.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("OutboxPublisherService iniciado");

            var factory = new ConnectionFactory()
            {
                HostName = _options.HostName,
                Port = _options.Port,
                UserName = _options.UserName,
                Password = _options.Password
            };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            // Exchange do tipo topic
            channel.ExchangeDeclare(_options.Exchange, ExchangeType.Topic, durable: true);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var pendentes = await _repo.GetPendingOutboxAsync(50);
                    foreach (var msg in pendentes)
                    {
                        var routingKey = msg.EventType; // Venda Confirmada
                        var body = Encoding.UTF8.GetBytes(msg.Payload);

                        var props = channel.CreateBasicProperties();
                        props.Persistent = true;
                        props.MessageId = msg.Id.ToString();

                        channel.BasicPublish(exchange: _options.Exchange,
                                             routingKey: routingKey,
                                             basicProperties: props,
                                             body: body);

                        await _repo.MarkOutboxSentAsync(msg.Id);
                        _logger.LogInformation("Publicada mensagem outbox {OutboxId} com routingKey {RoutingKey}", msg.Id, routingKey);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao publicar outbox, aguardando retry...");
                    // espera com backoff mínimo
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }

                await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
            }
        }
    }
}