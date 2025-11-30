using System.ComponentModel.DataAnnotations;

namespace EstoqueService.Models.DTOs
{
    public class ProdutoUpdateDto
    {
        [StringLength(200, ErrorMessage = "Nome deve ter o tamanho máximo de 200 caracteres!")]
        public string? Nome { get; set;}

        [StringLength(1000, ErrorMessage = "Descrição deve ter o tamanho máximo de 1000 caracteres!")]
        public string? Descricao { get; set;}

        [Range(0.01, double.MaxValue, ErrorMessage = "Preço deve ser maior que zero!")]
        public decimal? Preco { get; set;}

        [Range(0, int.MaxValue, ErrorMessage = "Quantidade não pode ser negativa")]
        public decimal? Quantidade { get; set;}

        public bool? Ativo {get; set;}
    }
}