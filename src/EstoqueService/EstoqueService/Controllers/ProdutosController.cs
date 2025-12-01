using Microsoft.AspNetCore.Mvc;
using EstoqueService.Models.DTOs;
using EstoqueService.Services;

namespace EstoqueService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ProdutosController : ControllerBase
{
    private readonly IProdutoService _produtoService;
    private readonly ILogger<ProdutosController> _logger;

    public ProdutosController(IProdutoService produtoService, ILogger<ProdutosController> logger)
    {
        _produtoService = produtoService;
        _logger = logger;
    }

    /// Obter todos os produtos    
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProdutoResponseDto>), 200)]
    public async Task<ActionResult<IEnumerable<ProdutoResponseDto>>> GetAll()
    {
        var produtos = await _produtoService.GetAllProdutosAsync();
        return Ok(produtos);
    }

    /// Obter produto por ID    
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProdutoResponseDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<ProdutoResponseDto>> GetById(int id)
    {
        var produto = await _produtoService.GetProdutoByIdAsync(id);

        if (produto == null)
        {
            _logger.LogWarning("Produto não encontrado: {ProdutoId}", id);
            return NotFound($"Produto com ID {id} não encontrado");
        }

        return Ok(produto);
    }

    /// Criar novo produto    
    [HttpPost]
    [ProducesResponseType(typeof(ProdutoResponseDto), 201)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<ProdutoResponseDto>> Create([FromBody] ProdutoCreateDto produtoDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var produto = await _produtoService.CreateProdutoAsync(produtoDto);

        return CreatedAtAction(
            nameof(GetById),
            new { id = produto.Id },
            produto);
    }

    /// Atualizar produto
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ProdutoResponseDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<ProdutoResponseDto>> Update(int id, [FromBody] ProdutoUpdateDto produtoDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var produto = await _produtoService.UpdateProdutoAsync(id, produtoDto);

        if (produto == null)
        {
            _logger.LogWarning("Tentativa de atualizar produto inexistente: {ProdutoId}", id);
            return NotFound($"Produto com ID {id} não encontrado");
        }

        return Ok(produto);
    }

    /// Remover produto (soft delete)    
    [HttpDelete("{id:int}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(int id)
    {
        var sucesso = await _produtoService.DeleteProdutoAsync(id);

        if (!sucesso)
        {
            _logger.LogWarning("Tentativa de remover produto inexistente: {ProdutoId}", id);
            return NotFound($"Produto com ID {id} não encontrado");
        }

        return NoContent();
    }

    /// Verificar disponibilidade de estoque    
    [HttpGet("{id:int}/estoque/{quantidade:int}")]
    [ProducesResponseType(typeof(object), 200)]
    public async Task<ActionResult> VerificarEstoque(int id, int quantidade)
    {
        var produto = await _produtoService.GetProdutoByIdAsync(id);
        if (produto == null)
            return NotFound($"Produto com ID {id} não encontrado");

        var temEstoque = await _produtoService.VerificarEstoqueAsync(id, quantidade);

        return Ok(new
        {
            ProdutoId = id,
            QuantidadeDisponivel = produto.Quantidade,
            QuantidadeSolicitada = quantidade,
            TemEstoque = temEstoque,
            Timestamp = DateTime.UtcNow
        });
    }


    /// Atualizar estoque
    [HttpPatch("{id:int}/estoque")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult> AtualizarEstoque(int id, [FromBody] EstoqueUpdateDto estoqueDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var produto = await _produtoService.GetProdutoByIdAsync(id);
        if (produto == null)
            return NotFound($"Produto com ID {id} não encontrado");

        var sucesso = await _produtoService.AtualizarEstoqueAsync(id, estoqueDto);

        if (!sucesso)
            return BadRequest("Não foi possível atualizar o estoque.  Verifique se há quantidade suficiente.");

        var produtoAtualizado = await _produtoService.GetProdutoByIdAsync(id);

        return Ok(new
        {
            Message = "Estoque atualizado com sucesso",
            ProdutoId = id,
            QuantidadeAnterior = produto.Quantidade,
            QuantidadeAtual = produtoAtualizado!.Quantidade,
            Operacao = estoqueDto.Operacao,
            Timestamp = DateTime.UtcNow
        });
    }
}