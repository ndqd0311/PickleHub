using MassTransit;
using Microsoft.EntityFrameworkCore;
using PickleHub.Customers.Domain.Entities;
using PickleHub.Customers.Domain.Repositories;
using System.Collections.Generic;
using System.Reflection.Emit;


namespace PickleHub.Customers.Infrastructure.Persistence
{
    public class CustomerDbContext : DbContext, IUnitOfWork
    {
        public CustomerDbContext(DbContextOptions<CustomerDbContext> options) : base(options) { }

        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<CustomerAddress> CustomerAddresses => Set<CustomerAddress>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>(e =>
            {
                e.ToTable("customer");
                e.HasKey(c => c.Id);
                e.Property(c => c.UserId).IsRequired();
                e.HasIndex(c => c.UserId).IsUnique();
                e.Property(c => c.Email).IsRequired().HasMaxLength(255);
                e.HasIndex(c => c.Email).IsUnique();
                e.Property(c => c.FullName).IsRequired().HasMaxLength(200);
                e.Property(c => c.PhoneNumber).HasMaxLength(20);
                e.Property(c => c.IsBlocked).HasDefaultValue(false);
                e.Property(c => c.CreatedAt).HasColumnName("created_at");
                e.Property(c => c.UpdatedAt).HasColumnName("updated_at");

                e.Navigation(c => c.Addresses).HasField("_addresses");

                e.HasMany(c => c.Addresses)
                    .WithOne()
                    .HasForeignKey(a => a.CustomerId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<CustomerAddress>(e =>
            {
                e.ToTable("customer_address");
                e.HasKey(a => a.Id);
                e.Property(a => a.FullName).IsRequired().HasMaxLength(200);
                e.Property(a => a.PhoneNumber).IsRequired().HasMaxLength(20);
                e.Property(a => a.Province).IsRequired().HasMaxLength(100);
                e.Property(a => a.District).IsRequired().HasMaxLength(100);
                e.Property(a => a.Ward).IsRequired().HasMaxLength(100);
                e.Property(a => a.StreetAddress).IsRequired().HasMaxLength(300);
                e.Property(a => a.IsDefault).HasDefaultValue(false);
                e.Property(a => a.CreatedAt).HasColumnName("created_at");
                e.Property(a => a.UpdatedAt).HasColumnName("updated_at");
            });
        }
    }
}
