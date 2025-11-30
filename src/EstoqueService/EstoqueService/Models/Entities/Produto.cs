using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EstoqueService.Models.Entities
{
    public class Produto
    {
        [Key]
        public int Id { get; set; } 

        [Required]
        [MaxLength(200)]
        public string Nome { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Descricao {get; set;}

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Preco{ get; set;}

        [Required]
        public int Quantidade {get; set;}

        public DateTime DataCriacao {get; set;} = DateTime.UtcNow;

        public DateTime DataAtualizacao {get; set;} = DateTime.UtcNow;

        [Required]
        public bool Ativo {get; set;} = true;

        // Produto em estoque
        public bool EmEstoque => Quantidade > 0;        
    }
}