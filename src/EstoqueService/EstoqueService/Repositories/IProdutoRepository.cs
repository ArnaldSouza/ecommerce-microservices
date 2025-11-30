using EstoqueService.Models.Entities;

namespace EstoqueService.Repositories
{
    public interface IProdutoRepository
    {
        Task<IEnumerable<Produto>> GetAllAsync();
        Task<Produto?> GetByIdAsync(int id);
        Task<Produto> CreateAsync(Produto produto);
        Task<Produto> UpdateAsync(Produto produto);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> TemEstoqueAsync(int produtoId, int quantidadeNecessaria);
        Task<bool> AtualizarEstoqueAsync(int produtoId, int quantidade, string operacao);
    }
}