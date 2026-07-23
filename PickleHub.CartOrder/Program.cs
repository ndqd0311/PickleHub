using System.Text;
using FluentValidation;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PickleHub.CartOrder.Domain.Interfaces;
using PickleHub.CartOrder.Infrastructure.Consumers;
using PickleHub.CartOrder.Infrastructure.HttpClients;
using PickleHub.CartOrder.Infrastructure.Persistence;
using PickleHub.CartOrder.Middleware;
using PickleHub.Common.Behaviors;

var builder = WebApplication.CreateBuilder(args);

// 1. Database Connection (EF Core + PostgreSQL)
builder.Services.AddDbContext<CartOrderDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("CartOrderDb")));

// 2. MediatR + FluentValidation
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

// 3. HTTP Clients (Giao tiếp Synchronous với Catalog, Inventory & Customer Service)
builder.Services.AddHttpClient<ICatalogClient, CatalogHttpClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:CatalogUrl"] ?? "http://localhost:5001/");
});

builder.Services.AddHttpClient<IInventoryClient, InventoryHttpClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:InventoryUrl"] ?? "http://localhost:5002/");
});

builder.Services.AddHttpClient<ICustomerClient, CustomerHttpClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:CustomerUrl"] ?? "http://localhost:5003/");
});

builder.Services.AddHttpClient<ISystemClient, SystemHttpClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:SystemUrl"] ?? "http://localhost:5009/");
});

builder.Services.AddHttpClient<IPaymentClient, PaymentHttpClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:PaymentUrl"] ?? "http://localhost:5006/");
});

// 4. MassTransit + RabbitMQ (Giao tiếp Asynchronous)
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<PaymentCompletedConsumer>();
    x.AddConsumer<PaymentFailedConsumer>();

    x.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ:Host"] ?? "localhost");

        cfg.ReceiveEndpoint("cartorder-payment-completed", e =>
        {
            e.ConfigureConsumer<PaymentCompletedConsumer>(ctx);
        });

        cfg.ReceiveEndpoint("cartorder-payment-failed", e =>
        {
            e.ConfigureConsumer<PaymentFailedConsumer>(ctx);
        });
    });
});

// 5. JWT Authentication
var jwtSecretKey = builder.Configuration["Jwt:SecretKey"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];

if (!string.IsNullOrEmpty(jwtSecretKey))
{
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtIssuer,
                ValidateAudience = true,
                ValidAudience = builder.Configuration["Jwt:Audience"] ?? "PickleHub.Clients",
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey))
            };
        });
}

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 6. Request Pipeline Configuration
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (!string.IsNullOrEmpty(jwtSecretKey))
{
    app.UseAuthentication();
}

app.UseAuthorization();
app.MapControllers();

app.Run();