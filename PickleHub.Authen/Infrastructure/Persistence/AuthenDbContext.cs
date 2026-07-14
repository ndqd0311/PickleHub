using Microsoft.EntityFrameworkCore;
using PickleHub.Authen.Domain.Entities;
using PickleHub.Authen.Domain.Interfaces.Repositories;

namespace PickleHub.Authen.Infrastructure.Persistence
{
    public class AuthenDbContext : DbContext, IUnitOfWork
    {
        public AuthenDbContext(DbContextOptions<AuthenDbContext> options) : base(options) { }
        public DbSet<User> Users => Set<User>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(e =>
            {
                e.ToTable("user");
                e.HasKey(u => u.Id);
                e.Property(u => u.Email).IsRequired().HasMaxLength(255);
                e.HasIndex(u => u.Email).IsUnique();
                e.Property(u => u.PasswordHash).IsRequired();
                e.Property(u => u.Role).HasConversion<string>().HasMaxLength(20);
                e.Property(u => u.IsBlocked).HasDefaultValue(false);
                e.Property(u => u.CreatedAt).HasColumnName("created_at");
                e.Property(u => u.UpdatedAt).HasColumnName("updated_at");
                e.Navigation(u => u.RefreshTokens).HasField("_refreshTokens");
                e.Navigation(u => u.PasswordResetTokens).HasField("_passwordResetTokens");
            });

            modelBuilder.Entity<RefreshToken>(e =>
            {
                e.ToTable("refresh_token");
                e.HasKey(r => r.Id);
                e.Property(r => r.Token).IsRequired();
                e.HasIndex(r => r.Token).IsUnique();
                e.Property(r => r.CreatedAt).HasColumnName("created_at");
                e.Property(r => r.UpdatedAt).HasColumnName("updated_at");

                e.HasOne(r => r.User)
                    .WithMany(u => u.RefreshTokens)
                    .HasForeignKey(r => r.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<PasswordResetToken>(e =>
            {
                e.ToTable("password_reset_token");
                e.HasKey(p => p.Id);
                e.Property(p => p.Token).IsRequired();
                e.HasIndex(p => p.Token).IsUnique();
                e.Property(p => p.CreatedAt).HasColumnName("created_at");
                e.Property(p => p.UpdatedAt).HasColumnName("updated_at");

                e.HasOne(p => p.User)
                    .WithMany(u => u.PasswordResetTokens)
                    .HasForeignKey(p => p.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
