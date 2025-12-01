using ApiGateway.Models.DTOs;

namespace ApiGateway.Services
{
    public interface IAuthService
    {
        Task<TokenResponseDto?> LoginAsync(LoginDto loginDto);
        Task<TokenResponseDto?> RegisterAsync(RegisterDto registerDto);
        Task<bool> ValidateTokenAsync(string token);
        string GenerateJwtToken(string userId, string email, string name);
    }
}