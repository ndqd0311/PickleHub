using MassTransit;
using Microsoft.EntityFrameworkCore;
using PayOS;
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

app.UseAuthorization();
app.MapControllers();

app.Run();
