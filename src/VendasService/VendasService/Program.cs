using Microsoft.EntityFrameworkCore;
using VendasService.Data;
using Serilog;

// Configurar Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Serilog
builder.Host.UseSerilog();

// Adicionar services ao container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "API Gerenciamento de Vendas",
        Version = "v1",
        Description = "API para gerenciamento de pedidos e vendas"
    });
});

// Entity Framework
builder.Services.AddDbContext<VendasDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Health checks básico
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configurar o pipeline HTTP da requisição
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Health checks endpoint
app.MapHealthChecks("/health");

// Migração de banco de dados na inicialização
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<VendasDbContext>();
    context.Database.EnsureCreated();
}

Console.WriteLine("Vendas Service iniciado - Portas: 5004 (HTTP) e 5005 (HTTPS)");

app.Run();