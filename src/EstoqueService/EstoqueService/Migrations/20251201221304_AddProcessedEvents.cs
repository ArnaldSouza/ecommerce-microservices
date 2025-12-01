using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EstoqueService.Migrations
{
    /// <inheritdoc />
    public partial class AddProcessedEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProcessedEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EventId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RetryCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessedEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Produtos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Preco = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Quantidade = table.Column<int>(type: "int", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    DataAtualizacao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    Ativo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Produtos", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Produtos",
                columns: new[] { "Id", "Ativo", "DataAtualizacao", "DataCriacao", "Descricao", "Nome", "Preco", "Quantidade" },
                values: new object[,]
                {
                    { 1, true, new DateTime(2025, 12, 1, 22, 13, 4, 155, DateTimeKind.Utc).AddTicks(4170), new DateTime(2025, 12, 1, 22, 13, 4, 155, DateTimeKind.Utc).AddTicks(4168), "Notebook Dell Inspiron 15 3000, Intel Core i5, 8GB RAM, SSD 256GB", "Notebook Dell Inspiron 15", 2499.99m, 10 },
                    { 2, true, new DateTime(2025, 12, 1, 22, 13, 4, 155, DateTimeKind.Utc).AddTicks(4177), new DateTime(2025, 12, 1, 22, 13, 4, 155, DateTimeKind.Utc).AddTicks(4176), "Mouse sem fio Logitech MX Master 3, sensor de alta precisão", "Mouse Logitech MX Master 3", 349.90m, 25 },
                    { 3, true, new DateTime(2025, 12, 1, 22, 13, 4, 155, DateTimeKind.Utc).AddTicks(4181), new DateTime(2025, 12, 1, 22, 13, 4, 155, DateTimeKind.Utc).AddTicks(4181), "Teclado mecânico sem fio Keychron K2, switch Blue, layout ABNT2", "Teclado Mecânico Keychron K2", 799.99m, 15 },
                    { 4, true, new DateTime(2025, 12, 1, 22, 13, 4, 155, DateTimeKind.Utc).AddTicks(4186), new DateTime(2025, 12, 1, 22, 13, 4, 155, DateTimeKind.Utc).AddTicks(4185), "Monitor Samsung 27 polegadas, resolução 4K, painel IPS", "Monitor Samsung 27\" 4K", 1899.90m, 5 },
                    { 5, true, new DateTime(2025, 12, 1, 22, 13, 4, 155, DateTimeKind.Utc).AddTicks(4190), new DateTime(2025, 12, 1, 22, 13, 4, 155, DateTimeKind.Utc).AddTicks(4190), "SSD NVMe Kingston NV2 1TB, interface PCIe 4.0", "SSD Kingston NV2 1TB", 399.90m, 0 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProcessedEvents_ProcessedAt",
                table: "ProcessedEvents",
                column: "ProcessedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessedEvents_Unique",
                table: "ProcessedEvents",
                columns: new[] { "EventType", "EventId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Produtos_Nome",
                table: "Produtos",
                column: "Nome");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProcessedEvents");

            migrationBuilder.DropTable(
                name: "Produtos");
        }
    }
}
