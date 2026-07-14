using Microsoft.EntityFrameworkCore;
using PickleHub.Authen.Domain.Entities;
using PickleHub.Authen.Domain.Enums;
using PickleHub.Authen.Domain.Interfaces.Repositories;
using PickleHub.Authen.Infrastructure.Persistence;
using PickleHub.Common.Interfaces;

namespace PickleHub.Authen.Extensions
{
    public static class WebApplicationExtensions
    {
        public static async Task SeedAdminAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();

            var userRepo = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            if (await userRepo.AnyAdminAsync())
                return;

            var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var email = config["SeedAdmin:Email"]!;
            var password = config["SeedAdmin:Password"]!;

            var admin = User.Create(email, BCrypt.Net.BCrypt.HashPassword(password), UserRole.Admin);

            userRepo.Add(admin);
            await uow.SaveChangesAsync();
        }

        //public static async Task SeedAdminAsync(this WebApplication app)
        //{
        //    using var scope = app.Services.CreateScope();

        //    var db = scope.ServiceProvider.GetRequiredService<AuthenDbContext>();
        //    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        //    if (!await db.Database.CanConnectAsync())
        //        throw new InvalidOperationException("Không th? k?t n?i t?i database.");

        //    if (await db.Users.AnyAsync())
        //        return;

        //    var admin = User.Create(
        //        config["SeedAdmin:Email"]!,
        //        BCrypt.Net.BCrypt.HashPassword(config["SeedAdmin:Password"]!),
        //        UserRole.Admin);

        //    db.Users.Add(admin);
        //    await db.SaveChangesAsync();
        //}
    }
}
