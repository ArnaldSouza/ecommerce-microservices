using Microsoft.EntityFrameworkCore;
using EstoqueService.Data;
using EstoqueService.Repositories;
using EstoqueService.Services;
using EstoqueService.BackgroundServices;
using Serilog;

// Configurar Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Serilog
builder.Host.UseSerilog();

// Add services to the container. 
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Estoque Service API",
        Version = "v1",
        Description = "API para gerenciamento de estoque de produtos com consumer RabbitMQ"
    });
});

// Entity Framework
builder.Services.AddDbContext<EstoqueDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Dependency Injection
builder.Services.AddScoped<IProdutoRepository, ProdutoRepository>();
builder.Services.AddScoped<IProdutoService, ProdutoService>();

// RabbitMQ Configuration
builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection("RabbitMq"));
builder.Services.AddSingleton<IRabbitMqService, RabbitMqService>();

// Background Services
builder.Services.AddHostedService<SaleConfirmedConsumer>();

// Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<EstoqueDbContext>();

var app = builder.Build();

// Configure the HTTP request pipeline.
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

// Database migration on startup
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<EstoqueDbContext>();

    try
    {
        // Aplicar migrações pendentes
        context.Database.Migrate();
        Console.WriteLine("✅ Migrações aplicadas com sucesso");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Erro ao aplicar migrações: {ex.Message}");
        // Em produção, considere falhar a aplicação se as migrações falharem
    }
}

Console.WriteLine("Estoque Service iniciado - Portas: 5002 (HTTP) e 5003 (HTTPS)");
Console.WriteLine("RabbitMQ Consumer ativo para eventos sale.confirmed");

app.Run();