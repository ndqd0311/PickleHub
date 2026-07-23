using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using PayOS;
using PickleHub.Payment.Domain.Interfaces;
using PickleHub.Payment.Infrastructure.HttpClients;
using PickleHub.Payment.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

//Database Connection
builder.Services.AddDbContext<PaymentDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PaymentDb")));

// 2. Đăng ký PayOSClient Service dưới dạng Singleton
var payOsConfig = builder.Configuration.GetSection("PayOS");
var payOSOptions = new PayOSOptions
{
    ClientId = payOsConfig["ClientId"]!,
    ApiKey = payOsConfig["ApiKey"]!,
    ChecksumKey = payOsConfig["ChecksumKey"]!
};
builder.Services.AddSingleton(new PayOSClient(payOSOptions));

builder.Services.AddHttpClient<IOrderClient, OrderHttpClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:CartOrderUrl"] ?? "http://localhost:5005/");
});

// 3. Đăng ký MassTransit & RabbitMQ (Để publish các sự kiện thanh toán)
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ:Host"] ?? "localhost");
    });
});

// 4. Đăng ký MediatR cho dự án Payment
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

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

// 5. Request Pipeline Configuration
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!string.IsNullOrEmpty(jwtSecretKey))
{
    app.UseAuthentication();
}

app.UseAuthorization();
app.MapControllers();

app.Run();
