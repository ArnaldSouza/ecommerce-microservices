using System.Text.Json;
using Microsoft.Extensions.Logging;
using VendasService.Models.DTOs;
using VendasService.Models.Entities;
using VendasService.Repositories;

namespace VendasService.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _repo;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<OrderService> _logger;

        // Nome do client configurado em Program.cs
        private const string InventoryClientName = "inventory";

        public OrderService(IOrderRepository repo, IHttpClientFactory httpClientFactory, ILogger<OrderService> logger)
        {
            _repo = repo;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<PedidoResponseDto> CreateOrderAsync(PedidoCreateDto dto)
        {
            // 1. Validar itens
            if (dto.Items == null || !dto.Items.Any())
                throw new ArgumentException("Pedido deve possuir ao menos um item");

            // 2. Consultar estoque de cada item (síncrono)
            var client = _httpClientFactory.CreateClient(InventoryClientName);
            var insuficientes = new List<int>();

            foreach (var item in dto.Items)
            {
                // chama: GET /api/produtos/{id}/estoque/{quantidade}
                var url = $"/api/produtos/{item.ProdutoId}/estoque/{item.Quantidade}";
                var resp = await client.GetAsync(url);
                if (!resp.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Falha ao consultar estoque do produto {ProdutoId}. Status: {Status}", item.ProdutoId, resp.StatusCode);
                    throw new InvalidOperationException($"Falha ao consultar estoque do produto {item.ProdutoId}");
                }

                var json = await resp.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var temEstoque = doc.RootElement.GetProperty("TemEstoque").GetBoolean();
                if (!temEstoque) insuficientes.Add(item.ProdutoId);
            }

            if (insuficientes.Any())
                throw new InvalidOperationException($"Estoque insuficiente para produtos: {string.Join(',', insuficientes)}");

            // 3. Montar Pedido e calcular total (buscando preco unitario do Inventory ou assumindo que Inventory não oferece preço => simplificação: preço fake via consulta externa ou cálculo)
            // Para simplificar: assumimos que o preço será obtido via Inventory: GET /api/produtos/{id}
            decimal total = 0m;
            var pedido = new Pedido
            {
                ClienteId = dto.ClienteId,
                CreatedAt = DateTime.UtcNow,
                Items = new List<PedidoItem>()
            };

            foreach (var item in dto.Items)
            {
                var resp = await client.GetAsync($"/api/produtos/{item.ProdutoId}");
                if (!resp.IsSuccessStatusCode) throw new InvalidOperationException($"Não foi possível obter dados do produto {item.ProdutoId}");
                var prodJson = await resp.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(prodJson);
                var preco = doc.RootElement.GetProperty("Preco").GetDecimal();

                var pedidoItem = new PedidoItem
                {
                    ProdutoId = item.ProdutoId,
                    Quantidade = item.Quantidade,
                    PrecoUnitario = preco
                };
                pedido.Items.Add(pedidoItem);
                total += preco * item.Quantidade;
            }

            pedido.Total = total;

            // 4. Persistir Pedido e Outbox message na mesma operação (simple approach: criar pedido, em seguida criar outbox)
            // Observação: se desejar transação explícita usar context.Database.BeginTransaction() no repositório; aqui usamos repositório simples.
            var created = await _repo.CreateAsync(pedido);

            // 5. Criar mensagem outbox para publicar sale.confirmed
            var saleEvent = new
            {
                OrderId = created.Id,
                ClienteId = created.ClienteId,
                Total = created.Total,
                Items = created.Items.Select(i => new { ProdutoId = i.ProdutoId, Quantidade = i.Quantidade, PrecoUnitario = i.PrecoUnitario }),
                Timestamp = DateTime.UtcNow
            };

            var payload = JsonSerializer.Serialize(saleEvent);

            var outbox = new OutboxMessage
            {
                AggregateType = "Pedido",
                EventType = "sale.confirmed",
                Payload = payload,
                CreatedAt = DateTime.UtcNow
            };

            await _repo.AddOutboxAsync(outbox);

            _logger.LogInformation("Pedido criado {PedidoId} e mensagem outbox {OutboxId} criada", created.Id, outbox.Id);

            // Retornar DTO
            return MapToResponseDto(created);
        }

        public async Task<PedidoResponseDto?> GetByIdAsync(int id)
        {
            var p = await _repo.GetByIdAsync(id);
            if (p == null) return null;
            return MapToResponseDto(p);
        }

        public async Task<IEnumerable<PedidoResponseDto>> GetAllAsync()
        {
            var list = await _repo.GetAllAsync();
            return list.Select(MapToResponseDto);
        }

        private static PedidoResponseDto MapToResponseDto(Pedido p)
        {
            return new PedidoResponseDto
            {
                Id = p.Id,
                ClienteId = p.ClienteId,
                Total = p.Total,
                IsConfirmed = p.IsConfirmed,
                CreatedAt = p.CreatedAt,
                ConfirmedAt = p.ConfirmedAt,
                Items = p.Items.Select(i => new PedidoItemResponseDto
                {
                    ProdutoId = i.ProdutoId,
                    Quantidade = i.Quantidade,
                    PrecoUnitario = i.PrecoUnitario
                }).ToList()
            };
        }
    }
}