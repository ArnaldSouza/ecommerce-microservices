namespace EstoqueService.Models.Events
{
    public class SaleConfirmedEvent
    {
        public int OrderId { get; set; }
        public string ClienteId { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public List<SaleItemEvent> Items { get; set; } = new();
        public DateTime Timestamp { get; set; }
    }

    public class SaleItemEvent
    {
        public int ProdutoId { get; set; }
        public int Quantidade { get; set; }
        public decimal PrecoUnitario { get; set; }
    }
}