using Microsoft.AspNetCore. Mvc;
using VendasService.Models.DTOs;
using VendasService.Services;

namespace VendasService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class PedidosController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<PedidosController> _logger;

        public PedidosController(IOrderService orderService, ILogger<PedidosController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        /// <summary>
        /// Obter todos os pedidos
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _orderService.GetAllAsync();
            return Ok(list);
        }

        /// <summary>
        /// Obter pedido por ID
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var p = await _orderService.GetByIdAsync(id);
            if (p == null) return NotFound();
            return Ok(p);
        }

        /// <summary>
        /// Criar novo pedido
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PedidoCreateDto dto)
        {
            try
            {
                var created = await _orderService.CreateOrderAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Erro de validação ao criar pedido");
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Erro de requisição");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar pedido");
                return StatusCode(500, new { message = "Erro interno ao processar pedido" });
            }
        }
    }
}