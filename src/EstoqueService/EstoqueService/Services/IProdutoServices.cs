using EstoqueService.Models.DTOs;

namespace EstoqueService.Services;

public interface IProdutoService
{
    Task<IEnumerable<ProdutoResponseDto>> GetAllProdutosAsync();
    Task<ProdutoResponseDto? > GetProdutoByIdAsync(int id);
    Task<ProdutoResponseDto> CreateProdutoAsync(ProdutoCreateDto produtoDto);
    Task<ProdutoResponseDto? > UpdateProdutoAsync(int id, ProdutoUpdateDto produtoDto);
    Task<bool> DeleteProdutoAsync(int id);
    Task<bool> VerificarEstoqueAsync(int produtoId, int quantidade);
    Task<bool> AtualizarEstoqueAsync(int produtoId, EstoqueUpdateDto estoqueDto);
}