using System.ComponentModel.DataAnnotations;

namespace VendasService.Models.DTOs;

public class PedidoCreateDto
{
    [Required]
    public string ClienteId { get; set; } = string.Empty;

    [Required]
    [MinLength(1)]
    public List<PedidoItemDto> Items { get; set; } = new();
}