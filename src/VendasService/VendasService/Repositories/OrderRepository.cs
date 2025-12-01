using Microsoft.EntityFrameworkCore;
using VendasService.Data;
using VendasService.Models.Entities;

namespace VendasService.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly VendasDbContext _context;

        public OrderRepository(VendasDbContext context)
        {
            _context = context;
        }

        public async Task<Pedido> CreateAsync(Pedido pedido)
        {
            _context.Pedidos.Add(pedido);
            await _context.SaveChangesAsync();
            return pedido;
        }

        public async Task<Pedido?> GetByIdAsync(int id)
        {
            return await _context.Pedidos.Include(p => p.Items).FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<Pedido>> GetAllAsync()
        {
            return await _context.Pedidos.Include(p => p.Items).OrderByDescending(p => p.CreatedAt).ToListAsync();
        }

        public async Task AddOutboxAsync(OutboxMessage outbox)
        {
            _context.OutboxMessages.Add(outbox);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<OutboxMessage>> GetPendingOutboxAsync(int max = 50)
        {
            return await _context.OutboxMessages
                .Where(o => o.SentAt == null)
                .OrderBy(o => o.CreatedAt)
                .Take(max)
                .ToListAsync();
        }

        public async Task MarkOutboxSentAsync(Guid outboxId)
        {
            var msg = await _context.OutboxMessages.FindAsync(outboxId);
            if (msg == null) return;
            msg.SentAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}