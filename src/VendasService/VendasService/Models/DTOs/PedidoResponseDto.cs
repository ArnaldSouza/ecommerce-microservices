using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VendasService.Models.DTOs
{
    public class PedidoItemResponseDto
    {
        public int ProdutoId { get; set; }
        public int Quantidade { get; set; }
        public decimal PrecoUnitario { get; set; }
    }

    public class PedidoResponseDto
    {
        public int Id { get; set; }
        public string ClienteId { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public bool IsConfirmed { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ConfirmedAt { get; set; }
        public List<PedidoItemResponseDto> Items { get; set; } = new();
    }
}