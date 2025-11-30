namespace EstoqueService.Models.DTOs
{
    public class ProdutoResponseDto
    {
        public int Id { get; set;}
        public string Nome { get; set;} = string.Empty;
        public string? Descricao { get; set;}
        public decimal Preco { get; set;}
        public int Quantidade { get; set;}
        public bool Ativo { get; set;}
        public bool EmEstoque { get; set;}
        public DateTime DataCriacao { get; set;}
        public DateTime DataAtualizacao { get; set;}

    }
}