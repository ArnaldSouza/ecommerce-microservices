using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using EstoqueService.Data;
using EstoqueService.Models.Entities;
using EstoqueService.Models.Events;
using EstoqueService.Services;

namespace EstoqueService.Services
{
    public class RabbitMqOptions
    {
        public string HostName { get; set; } = "localhost";
        public int Port { get; set; } = 5672;
        public string UserName { get; set; } = "guest";
        public string Password { get; set; } = "guest";
        public string Exchange { get; set; } = "ecommerce.events";
        public string QueueName { get; set; } = "inventory.stock.update";
        public string RoutingKey { get; set; } = "sale.confirmed";
    }

    public class RabbitMqService : IRabbitMqService, IDisposable
    {
        private readonly ILogger<RabbitMqService> _logger;
        private readonly RabbitMqOptions _options;
        private readonly IServiceScopeFactory _scopeFactory;
        private IConnection? _connection;
        private IModel? _channel;
        private bool _disposed = false;

        public RabbitMqService(
            ILogger<RabbitMqService> logger,
            IOptions<RabbitMqOptions> options,
            IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _options = options.Value;
            _scopeFactory = scopeFactory;
        }

        public async Task StartConsumingAsync(CancellationToken cancellationToken)
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = _options.HostName,
                    Port = _options.Port,
                    UserName = _options.UserName,
                    Password = _options.Password,
                    DispatchConsumersAsync = true
                };

                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                // Declarar exchange e queue (idempotente)
                _channel.ExchangeDeclare(_options.Exchange, ExchangeType.Topic, durable: true);
                _channel.QueueDeclare(_options.QueueName, durable: true, exclusive: false, autoDelete: false);
                _channel.QueueBind(_options.QueueName, _options.Exchange, _options.RoutingKey);

                // Configurar QoS para processar uma mensagem por vez
                _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

                var consumer = new AsyncEventingBasicConsumer(_channel);
                consumer.Received += async (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var deliveryTag = ea.DeliveryTag;

                    _logger.LogInformation("Mensagem recebida: {Message}", message);

                    try
                    {
                        await ProcessSaleConfirmedEvent(message);
                        _channel.BasicAck(deliveryTag, multiple: false);
                        _logger.LogInformation("Mensagem processada e confirmada: {DeliveryTag}", deliveryTag);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Erro ao processar mensagem: {Message}", message);

                        // Rejeitar mensagem e enviar para DLQ (se configurado) ou descartar
                        _channel.BasicNack(deliveryTag, multiple: false, requeue: false);
                    }
                };

                _channel.BasicConsume(queue: _options.QueueName, autoAck: false, consumer: consumer);

                _logger.LogInformation("RabbitMQ Consumer iniciado. Queue: {Queue}", _options.QueueName);

                // Manter o consumer rodando
                while (!cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(1000, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao inicializar RabbitMQ Consumer");
                throw;
            }
        }

        private async Task ProcessSaleConfirmedEvent(string message)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<EstoqueDbContext>();
            var produtoService = scope.ServiceProvider.GetRequiredService<IProdutoService>();

            try
            {
                var saleEvent = JsonSerializer.Deserialize<SaleConfirmedEvent>(message);
                if (saleEvent == null)
                {
                    _logger.LogWarning("Não foi possível deserializar evento: {Message}", message);
                    return;
                }

                var eventId = saleEvent.OrderId.ToString();
                var eventType = "sale.confirmed";

                // Verificar se já foi processado (idempotência)
                var existingEvent = await context.ProcessedEvents
                    .FirstOrDefaultAsync(e => e.EventType == eventType && e.EventId == eventId);

                if (existingEvent != null)
                {
                    if (existingEvent.Status == "SUCCESS")
                    {
                        _logger.LogInformation("Evento já processado com sucesso: OrderId {OrderId}", saleEvent.OrderId);
                        return;
                    }

                    if (existingEvent.Status == "PROCESSING")
                    {
                        _logger.LogWarning("Evento já está sendo processado: OrderId {OrderId}", saleEvent.OrderId);
                        return;
                    }

                    // Se falhou antes, incrementar retry count
                    existingEvent.RetryCount++;
                    existingEvent.Status = "PROCESSING";
                    existingEvent.ProcessedAt = DateTime.UtcNow;
                }
                else
                {
                    // Criar novo registro de evento
                    var processedEvent = new ProcessedEvent
                    {
                        Id = Guid.NewGuid(),
                        EventType = eventType,
                        EventId = eventId,
                        Status = "PROCESSING",
                        ProcessedAt = DateTime.UtcNow,
                        RetryCount = 0
                    };

                    context.ProcessedEvents.Add(processedEvent);
                }

                await context.SaveChangesAsync();

                // Processar redução de estoque para cada item
                var errors = new List<string>();

                using var transaction = await context.Database.BeginTransactionAsync();

                try
                {
                    foreach (var item in saleEvent.Items)
                    {
                        var produto = await context.Produtos.FindAsync(item.ProdutoId);
                        if (produto == null)
                        {
                            var error = $"Produto {item.ProdutoId} não encontrado";
                            errors.Add(error);
                            _logger.LogWarning(error);
                            continue;
                        }

                        if (produto.Quantidade < item.Quantidade)
                        {
                            var error = $"Estoque insuficiente para produto {item.ProdutoId}.  Disponível: {produto.Quantidade}, Solicitado: {item.Quantidade}";
                            errors.Add(error);
                            _logger.LogWarning(error);
                            continue;
                        }

                        // Reduzir estoque
                        produto.Quantidade -= item.Quantidade;
                        produto.DataAtualizacao = DateTime.UtcNow;

                        _logger.LogInformation("Estoque reduzido: Produto {ProdutoId}, Quantidade: -{Quantidade}, Novo Estoque: {NovoEstoque}",
                            item.ProdutoId, item.Quantidade, produto.Quantidade);
                    }

                    if (errors.Any())
                    {
                        throw new InvalidOperationException($"Erros ao processar estoque: {string.Join("; ", errors)}");
                    }

                    await context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    // Marcar como sucesso
                    var processedEventSuccess = await context.ProcessedEvents
                        .FirstOrDefaultAsync(e => e.EventType == eventType && e.EventId == eventId);

                    if (processedEventSuccess != null)
                    {
                        processedEventSuccess.Status = "SUCCESS";
                        processedEventSuccess.ErrorMessage = null;
                        await context.SaveChangesAsync();
                    }

                    _logger.LogInformation("Evento processado com sucesso: OrderId {OrderId}, Itens: {ItemCount}",
                        saleEvent.OrderId, saleEvent.Items.Count);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar evento sale.confirmed");

                // Marcar como falha
                try
                {
                    var saleEvent = JsonSerializer.Deserialize<SaleConfirmedEvent>(message);
                    if (saleEvent != null)
                    {
                        var eventId = saleEvent.OrderId.ToString();
                        var eventType = "sale.confirmed";

                        var processedEventError = await context.ProcessedEvents
                            .FirstOrDefaultAsync(e => e.EventType == eventType && e.EventId == eventId);

                        if (processedEventError != null)
                        {
                            processedEventError.Status = "FAILED";
                            processedEventError.ErrorMessage = ex.Message;
                            await context.SaveChangesAsync();
                        }
                    }
                }
                catch (Exception saveEx)
                {
                    _logger.LogError(saveEx, "Erro ao salvar falha do evento");
                }

                throw;
            }
        }

        public void StopConsuming()
        {
            try
            {
                _channel?.Close();
                _connection?.Close();
                _logger.LogInformation("RabbitMQ Consumer parado");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao parar RabbitMQ Consumer");
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                StopConsuming();
                _channel?.Dispose();
                _connection?.Dispose();
                _disposed = true;
            }
        }
    }
}