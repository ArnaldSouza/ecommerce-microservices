using EstoqueService.Models.DTOs;
using EstoqueService.Models.Entities;
using EstoqueService.Repositories;

namespace EstoqueService.Services;

public class ProdutoService : IProdutoService
{
    private readonly IProdutoRepository _produtoRepository;
    private readonly ILogger<ProdutoService> _logger;

    public ProdutoService(IProdutoRepository produtoRepository, ILogger<ProdutoService> logger)
    {
        _produtoRepository = produtoRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<ProdutoResponseDto>> GetAllProdutosAsync()
    {
        var produtos = await _produtoRepository.GetAllAsync();
        return produtos.Select(MapToResponseDto);
    }

    public async Task<ProdutoResponseDto?> GetProdutoByIdAsync(int id)
    {
        var produto = await _produtoRepository.GetByIdAsync(id);
        return produto == null ? null : MapToResponseDto(produto);
    }

    public async Task<ProdutoResponseDto> CreateProdutoAsync(ProdutoCreateDto produtoDto)
    {
        var produto = new Produto
        {
            Nome = produtoDto.Nome,
            Descricao = produtoDto.Descricao,
            Preco = produtoDto.Preco,
            Quantidade = produtoDto.Quantidade,
            DataCriacao = DateTime.UtcNow,
            DataAtualizacao = DateTime.UtcNow
        };

        var produtoCriado = await _produtoRepository.CreateAsync(produto);
        _logger.LogInformation("Produto criado: {ProdutoId} - {Nome}", produtoCriado.Id, produtoCriado.Nome);

        return MapToResponseDto(produtoCriado);
    }

    public async Task<ProdutoResponseDto?> UpdateProdutoAsync(int id, ProdutoUpdateDto produtoDto)
    {
        var produto = await _produtoRepository.GetByIdAsync(id);
        if (produto == null) return null;

        if (!string.IsNullOrWhiteSpace(produtoDto.Nome))
            produto.Nome = produtoDto.Nome;

        if (produtoDto.Descricao != null)
            produto.Descricao = produtoDto.Descricao;

        if (produtoDto.Preco.HasValue)
            produto.Preco = produtoDto.Preco.Value;

        if (produtoDto.Quantidade.HasValue)
            produto.Quantidade = (int)produtoDto.Quantidade.Value;

        if (produtoDto.Ativo.HasValue)
            produto.Ativo = produtoDto.Ativo.Value;

        produto.DataAtualizacao = DateTime.UtcNow;

        var produtoAtualizado = await _produtoRepository.UpdateAsync(produto);
        _logger.LogInformation("Produto atualizado: {ProdutoId} - {Nome}", produtoAtualizado.Id, produtoAtualizado.Nome);

        return MapToResponseDto(produtoAtualizado);
    }

    public async Task<bool> DeleteProdutoAsync(int id)
    {
        if (!await _produtoRepository.ExistsAsync(id))
            return false;

        await _produtoRepository.DeleteAsync(id);
        _logger.LogInformation("Produto removido: {ProdutoId}", id);
        return true;
    }

    public async Task<bool> VerificarEstoqueAsync(int produtoId, int quantidade)
    {
        return await _produtoRepository.TemEstoqueAsync(produtoId, quantidade);
    }

    public async Task<bool> AtualizarEstoqueAsync(int produtoId, int quantidade, string operacao)
    {
        var sucesso = await _produtoRepository.AtualizarEstoqueAsync(produtoId, quantidade, operacao);

        if (sucesso)
        {
            _logger.LogInformation("Estoque atualizado: Produto {ProdutoId}, Operação: {Operacao}, Quantidade: {Quantidade}",
                produtoId, operacao, quantidade);
        }

        return sucesso;
    }

    public async Task<bool> AtualizarEstoqueAsync(int produtoId, EstoqueUpdateDto estoqueDto)
    {
        return await AtualizarEstoqueAsync(produtoId, estoqueDto.Quantidade, estoqueDto.Operacao);
    }

    private static ProdutoResponseDto MapToResponseDto(Produto produto)
    {
        return new ProdutoResponseDto
        {
            Id = produto.Id,
            Nome = produto.Nome,
            Descricao = produto.Descricao,
            Preco = produto.Preco,
            Quantidade = produto.Quantidade,
            Ativo = produto.Ativo,
            EmEstoque = produto.Quantidade > 0,
            DataCriacao = produto.DataCriacao,
            DataAtualizacao = produto.DataAtualizacao
        };
    }
}