using Microsoft.EntityFrameworkCore;
using VendasService.Models.Entities;

namespace VendasService.Data
{
    public class VendasDbContext : DbContext
    {
        public VendasDbContext(DbContextOptions<VendasDbContext> options) : base(options) { }

        public DbSet<Pedido> Pedidos { get; set; } = null!;
        public DbSet<PedidoItem> PedidoItems { get; set; } = null!;
        public DbSet<OutboxMessage> OutboxMessages { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Pedido>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Total).HasColumnType("decimal(18,2)");
                entity.HasMany(e => e.Items).WithOne(i => i.Pedido).HasForeignKey(i => i.PedidoId);
            });

            modelBuilder.Entity<PedidoItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.PrecoUnitario).HasColumnType("decimal(18,2)");
            });

            modelBuilder.Entity<OutboxMessage>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Payload).IsRequired();
                entity.HasIndex(e => e.SentAt);
            });
        }
    }
}