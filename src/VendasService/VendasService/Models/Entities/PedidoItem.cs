using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VendasService.Models.Entities;

[Table("PedidoItems")]
public class PedidoItem
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int ProdutoId { get; set; }

    [Required]
    public int Quantidade { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal PrecoUnitario { get; set; }

    [ForeignKey("Pedido")]
    public int PedidoId { get; set; }

    public Pedido? Pedido { get; set; }
}