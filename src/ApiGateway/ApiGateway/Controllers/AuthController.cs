using Microsoft.AspNetCore.Mvc;
using ApiGateway.Models.DTOs;
using ApiGateway.Services;

namespace ApiGateway.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// Realizar login
        [HttpPost("login")]
        [ProducesResponseType(typeof(TokenResponseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.LoginAsync(loginDto);

            if (result == null)
            {
                return Unauthorized(new { message = "Email ou senha inválidos" });
            }

            return Ok(result);
        }

        // Registrar novo usuário
        [HttpPost("register")]
        [ProducesResponseType(typeof(TokenResponseDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(409)]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.RegisterAsync(registerDto);

            if (result == null)
            {
                return Conflict(new { message = "Email já está em uso" });
            }

            return CreatedAtAction(nameof(Login), result);
        }

        // Validar token        
        [HttpPost("validate")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> ValidateToken([FromBody] string token)
        {
            var isValid = await _authService.ValidateTokenAsync(token);

            if (!isValid)
                return Unauthorized(new { message = "Token inválido" });

            return Ok(new { message = "Token válido" });
        }


        // Usuários de exemplo para teste        
        [HttpGet("demo-users")]
        public IActionResult GetDemoUsers()
        {
            var demoUsers = new[]
            {
            new { Email = "admin@ecommerce.com", Password = "admin123", Role = "Admin" },
            new { Email = "user@ecommerce. com", Password = "user123", Role = "User" },
            new { Email = "cliente@ecommerce.com", Password = "cliente123", Role = "Cliente" }
        };

            return Ok(new
            {
                message = "Usuários demo disponíveis",
                users = demoUsers,
                note = "Use estes usuários para fazer login e obter o token JWT"
            });
        }
    }
}