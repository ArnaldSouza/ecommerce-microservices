using Microsoft.EntityFrameworkCore;
using EstoqueService.Data;
using EstoqueService.Repositories;
using EstoqueService.Services;
using Serilog;

// Configurar Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Serilog
builder.Host.UseSerilog();

// Adiconar services ao container. 
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "API Gerenciamento de Estoque",
        Version = "v1",
        Description = "API para gerenciamento de estoque de produtos"
    });
});

// Entity Framework
builder.Services.AddDbContext<EstoqueDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Injeção de dependência
builder.Services.AddScoped<IProdutoRepository, ProdutoRepository>();
builder.Services.AddScoped<IProdutoService, ProdutoService>();

// Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<EstoqueDbContext>();

var app = builder.Build();

// Configura o pipeline HTTP da requisição
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
    var context = scope.ServiceProvider.GetRequiredService<EstoqueDbContext>();
    context.Database.EnsureCreated();
}

Console.WriteLine("Estoque Service iniciado - Portas: 5002 (HTTP) e 5003 (HTTPS)");

app.Run();