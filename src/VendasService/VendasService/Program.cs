using Microsoft.EntityFrameworkCore;
using VendasService.BackgroundWorkers;
using VendasService.Data;
using VendasService.Repositories;
using VendasService.Services;
using Serilog;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Vendas Service API",
        Version = "v1",
        Description = "API para gerenciamento de vendas e pedidos"
    });
});

// Configs
var configuration = builder.Configuration;

// EF Core
builder.Services.AddDbContext<VendasDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

// Repositories & Services
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();

// HttpClient para Inventory (base address via env/ appsettings)
builder.Services.AddHttpClient("inventory", client =>
{
    var baseUrl = configuration["Inventory:BaseUrl"] ?? "http://localhost:5002";
    client.BaseAddress = new Uri(baseUrl);
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler()
{
    ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
});

// RabbitMQ options
builder.Services.Configure<RabbitMqOptions>(configuration.GetSection("RabbitMq"));
builder.Services.AddHostedService<OutboxPublisherService>();

// Health checks
builder.Services.AddHealthChecks();

// JWT TEMPORARIAMENTE DESABILITADO PARA TESTES
/*
var jwtKey = configuration["Jwt:Key"];
var jwtIssuer = configuration["Jwt:Issuer"];

if (!string.IsNullOrEmpty(jwtKey))
{
    builder.Services.AddAuthentication("Bearer")
        . AddJwtBearer("Bearer", options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options. TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = false,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtIssuer,
                IssuerSigningKey = new Microsoft.IdentityModel.Tokens. SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtKey))
            };
        });

    builder.Services. AddAuthorization();
}
*/

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// ❌ JWT DESABILITADO
/*
if (!string.IsNullOrEmpty(jwtKey))
{
    app.UseAuthentication();
    app.UseAuthorization();
}
*/

app.MapControllers();
app.MapHealthChecks("/health");

// Ensure DB created / migrations to be applied manually
using (var scope = app.Services.CreateScope())
{
    var ctx = scope.ServiceProvider.GetRequiredService<VendasDbContext>();
    ctx.Database.EnsureCreated();
}

Console.WriteLine("Vendas Service iniciado - Portas: 5004 (HTTP) e 5005 (HTTPS)");

app.Run();