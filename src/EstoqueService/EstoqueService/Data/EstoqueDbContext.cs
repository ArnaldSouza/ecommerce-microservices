using Microsoft. EntityFrameworkCore;
using EstoqueService.Models. Entities;

namespace EstoqueService.Data;

public class EstoqueDbContext : DbContext
{
    public EstoqueDbContext(DbContextOptions<EstoqueDbContext> options) : base(options)
    {
    }

    public DbSet<Produto> Produtos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<Produto>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Nome)
                .IsRequired()
                . HasMaxLength(200);
                
            entity.Property(e => e. Descricao)
                .HasMaxLength(1000);
                
            entity.Property(e => e.Preco)
                . HasColumnType("decimal(18,2)")
                . IsRequired();
                
            entity.Property(e => e. Quantidade)
                . IsRequired();
                
            entity.Property(e => e.DataCriacao)
                . IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");
                
            entity.Property(e => e. DataAtualizacao)
                .IsRequired()
                . HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(e => e. Nome)
                .HasDatabaseName("IX_Produtos_Nome");
        });

        // Seed de dados
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Produto>(). HasData(
            new Produto
            {
                Id = 1,
                Nome = "Notebook ",
                Descricao = "Notebook ASUS, Intel Core i5, 8GB RAM, SSD 256GB",
                Preco = 2499.99m,
                Quantidade = 10,
                DataCriacao = DateTime.UtcNow,
                DataAtualizacao = DateTime.UtcNow,
                Ativo = true
            },
            new Produto
            {
                Id = 2,
                Nome = "Mouse",
                Descricao = "Mouse sem fio Logitech MX Master 3, sensor de alta precisão",
                Preco = 349.90m,
                Quantidade = 25,
                DataCriacao = DateTime. UtcNow,
                DataAtualizacao = DateTime.UtcNow,
                Ativo = true
            },
            new Produto
            {
                Id = 3,
                Nome = "Teclado",
                Descricao = "Teclado mecânico sem fio Keychron K2, switch Blue, layout ABNT2",
                Preco = 799.99m,
                Quantidade = 15,
                DataCriacao = DateTime. UtcNow,
                DataAtualizacao = DateTime. UtcNow,
                Ativo = true
            },
            new Produto
            {
                Id = 4,
                Nome = "Monitor Samsung 4K",
                Descricao = "Monitor Samsung 27 polegadas, resolução 4K, painel IPS",
                Preco = 1899.90m,
                Quantidade = 5,
                DataCriacao = DateTime. UtcNow,
                DataAtualizacao = DateTime. UtcNow,
                Ativo = true
            },
            new Produto
            {
                Id = 5,
                Nome = "SSD Kingston NV2 1TB",
                Descricao = "SSD NVMe Kingston NV2 1TB, interface PCIe 4.0",
                Preco = 399.90m,
                Quantidade = 0, // Sem estoque do produto
                DataCriacao = DateTime. UtcNow,
                DataAtualizacao = DateTime.UtcNow,
                Ativo = true
            }
        );
    }
}