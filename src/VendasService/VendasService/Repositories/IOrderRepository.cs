using VendasService.Models.Entities;

namespace VendasService.Repositories
{
    public interface IOrderRepository
    {
        Task<Pedido> CreateAsync(Pedido pedido);
        Task<Pedido?> GetByIdAsync(int id);
        Task<IEnumerable<Pedido>> GetAllAsync();
        Task AddOutboxAsync(OutboxMessage outbox);
        Task<IEnumerable<OutboxMessage>> GetPendingOutboxAsync(int max = 50);
        Task MarkOutboxSentAsync(Guid outboxId);
    }
}