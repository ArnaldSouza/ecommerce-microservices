using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VendasService.Models.Entities
{
    [Table("Pedidos")]
    public class Pedido
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string ClienteId { get; set; } = string.Empty;

        [Required]
        public decimal Total { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ConfirmedAt { get; set; }

        public bool IsConfirmed => ConfirmedAt.HasValue;

        public ICollection<PedidoItem> Items { get; set; } = new List<PedidoItem>();
    }
}
