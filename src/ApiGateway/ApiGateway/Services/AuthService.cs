using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using ApiGateway.Models.DTOs;

namespace ApiGateway.Services;

public class AuthService : IAuthService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    // Simulação de usuários (em produção usar banco de dados)
    private readonly Dictionary<string, (string Password, string Name)> _users = new()
    {
        { "admin@ecommerce.com", ("admin123", "Administrador") },
        { "user@ecommerce.com", ("user123", "Usuário Teste") },
        { "cliente@ecommerce. com", ("cliente123", "Cliente Demo") }
    };

    public AuthService(IConfiguration configuration, ILogger<AuthService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<TokenResponseDto?> LoginAsync(LoginDto loginDto)
    {
        await Task.Delay(100); // Simular operação assíncrona

        if (!_users.TryGetValue(loginDto.Email, out var userData))
        {
            _logger.LogWarning("Tentativa de login com email não encontrado: {Email}", loginDto.Email);
            return null;
        }

        if (userData.Password != loginDto.Password)
        {
            _logger.LogWarning("Tentativa de login com senha incorreta: {Email}", loginDto.Email);
            return null;
        }

        var userId = Guid.NewGuid().ToString();
        var token = GenerateJwtToken(userId, loginDto.Email, userData.Name);

        _logger.LogInformation("Login realizado com sucesso: {Email}", loginDto.Email);

        return new TokenResponseDto
        {
            AccessToken = token,
            TokenType = "Bearer",
            ExpiresIn = 3600, // 1 hora
            RefreshToken = Guid.NewGuid().ToString(), // Simulação
            User = new UserInfoDto
            {
                Id = userId,
                Email = loginDto.Email,
                Name = userData.Name
            }
        };
    }

    public async Task<TokenResponseDto?> RegisterAsync(RegisterDto registerDto)
    {
        await Task.Delay(100); // Simular operação assíncrona

        if (_users.ContainsKey(registerDto.Email))
        {
            _logger.LogWarning("Tentativa de registro com email já existente: {Email}", registerDto.Email);
            return null;
        }

        // Simular registro (em produção salvar no banco)
        _users[registerDto.Email] = (registerDto.Password, registerDto.Name);

        var userId = Guid.NewGuid().ToString();
        var token = GenerateJwtToken(userId, registerDto.Email, registerDto.Name);

        _logger.LogInformation("Usuário registrado com sucesso: {Email}", registerDto.Email);

        return new TokenResponseDto
        {
            AccessToken = token,
            TokenType = "Bearer",
            ExpiresIn = 3600,
            RefreshToken = Guid.NewGuid().ToString(),
            User = new UserInfoDto
            {
                Id = userId,
                Email = registerDto.Email,
                Name = registerDto.Name
            }
        };
    }

    public Task<bool> ValidateTokenAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "");

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Token inválido: {Token}", token);
            return Task.FromResult(false);
        }
    }

    public string GenerateJwtToken(string userId, string email, string name)
    {
        var key = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key não configurada");
        var issuer = _configuration["Jwt:Issuer"] ?? "ecommerce";

        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenKey = Encoding.UTF8.GetBytes(key);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Name, name),
                new Claim("userId", userId),
                new Claim("email", email)
            }),
            Expires = DateTime.UtcNow.AddHours(1),
            Issuer = issuer,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(tokenKey),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}