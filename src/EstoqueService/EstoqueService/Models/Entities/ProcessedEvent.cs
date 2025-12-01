using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
    
[Table("ProcessedEvents")]
public class ProcessedEvent
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string EventType { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string EventId { get; set; } = string.Empty; // OrderId para sale.confirmed

    [Required]
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;

    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = string.Empty; // "SUCCESS", "FAILED", "PROCESSING"

    public string? ErrorMessage { get; set; }

    public int RetryCount { get; set; } = 0;


    public string UniqueKey => $"{EventType}_{EventId}";
}