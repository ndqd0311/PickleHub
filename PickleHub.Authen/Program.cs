using MediatR;
using Microsoft.EntityFrameworkCore;
using PickleHub.Authen.Domain.Entities;
using PickleHub.Authen.Domain.Enums;
using PickleHub.Authen.Infrastructure.Persistence;
using PickleHub.Authen.Infrastructure.Service;
using PickleHub.Authen.Infrastructure.Service;
using PickleHub.Authen.Middleware;
using Resend;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AuthenDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("AuthenDb")));

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

builder.Services.AddSingleton<JwtTokenService>();
builder.Services.AddResend(options =>
    options.ApiToken = builder.Configuration["Resend:ApiKey"]!);
builder.Services.AddScoped<IEmailService, ResendEmailService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AuthenDbContext>();
    var hasAdmin = await db.Users.AnyAsync(u => u.Role == UserRole.Admin);

    if (!hasAdmin)
    {
        var adminEmail = builder.Configuration["SeedAdmin:Email"]!;
        var adminPassword = builder.Configuration["SeedAdmin:Password"]!;

        db.Users.Add(new User
        {
            Email = adminEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword),
            Role = UserRole.Admin
        });

        await db.SaveChangesAsync();
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseAuthorization();
app.MapControllers();

app.Run();