using VendasService.Models.DTOs;

namespace VendasService.Services
{
    public interface IOrderService
    {
        Task<PedidoResponseDto> CreateOrderAsync(PedidoCreateDto dto);
        Task<PedidoResponseDto?> GetByIdAsync(int id);
        Task<IEnumerable<PedidoResponseDto>> GetAllAsync();
    }
}