using System.ComponentModel.DataAnnotations;

namespace EstoqueService.Models.DTOs
{
    public class ProdutoCreateDto
    {
        [Required(ErrorMessage = "Nome é obrigatório!")]
        [StringLength(200, ErrorMessage = "Nome deve ter o tamanho máximo de 200 caracteres!")]
        public string Nome { get; set;} = string.Empty;

        [StringLength(1000, ErrorMessage = "Descrição deve ter o tamanho máximo de 1000 caracteres!")]
        public string? Descricao { get; set;}

        [Required(ErrorMessage = "Preço é obrigatório!")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Preço deve ser maior que zero")]
        public decimal Preco { get; set;}

        [Required(ErrorMessage = "Quantidade é obrigatória!")]
        [Range(0, int.MaxValue, ErrorMessage = "Quantidade não pode ser negativa!")]
        public int Quantidade { get; set;}
    }
}