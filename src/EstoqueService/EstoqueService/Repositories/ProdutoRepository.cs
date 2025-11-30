using Microsoft.EntityFrameworkCore;
using EstoqueService.Data;
using EstoqueService.Models. Entities;

namespace EstoqueService.Repositories;

public class ProdutoRepository : IProdutoRepository
{
    private readonly EstoqueDbContext _context;

    public ProdutoRepository(EstoqueDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Produto>> GetAllAsync()
    {
        return await _context. Produtos
            .Where(p => p. Ativo)
            .OrderBy(p => p.Nome)
            .ToListAsync();
    }

    public async Task<Produto? > GetByIdAsync(int id)
    {
        return await _context. Produtos
            .FirstOrDefaultAsync(p => p.Id == id && p.Ativo);
    }

    public async Task<Produto> CreateAsync(Produto produto)
    {
        _context.Produtos.Add(produto);
        await _context.SaveChangesAsync();
        return produto;
    }

    public async Task<Produto> UpdateAsync(Produto produto)
    {
        produto.DataAtualizacao = DateTime. UtcNow;
        _context. Produtos.Update(produto);
        await _context.SaveChangesAsync();
        return produto;
    }

    public async Task DeleteAsync(int id)
    {
        var produto = await GetByIdAsync(id);
        if (produto != null)
        {
            produto.Ativo = false; // Soft delete
            produto.DataAtualizacao = DateTime. UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context. Produtos
            .AnyAsync(p => p. Id == id && p.Ativo);
    }

    public async Task<bool> TemEstoqueAsync(int produtoId, int quantidadeNecessaria)
    {
        var produto = await GetByIdAsync(produtoId);
        return produto != null && produto. Quantidade >= quantidadeNecessaria;
    }

    public async Task<bool> AtualizarEstoqueAsync(int produtoId, int quantidade, string operacao)
    {
        var produto = await GetByIdAsync(produtoId);
        if (produto == null) return false;

        switch (operacao. ToLower())
        {
            case "adicionar":
                produto.Quantidade += quantidade;
                break;
            case "remover":
                if (produto.Quantidade < quantidade) return false;
                produto.Quantidade -= quantidade;
                break;
            case "definir":
                produto.Quantidade = quantidade;
                break;
            default:
                return false;
        }

        await UpdateAsync(produto);
        return true;
    }
}