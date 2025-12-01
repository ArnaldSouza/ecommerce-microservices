using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VendasService.Models.Entities
{
    [Table("OutboxMessages")]
    public class OutboxMessage
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string AggregateType { get; set; } = string.Empty; // "Pedido"

        [Required]
        public string EventType { get; set; } = string.Empty; // "venda.confirmada"

        [Required]
        public string Payload { get; set; } = string.Empty; // JSON

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? SentAt { get; set; }

        public int RetryCount { get; set; } = 0;
    }
}

