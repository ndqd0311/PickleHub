using Microsoft.EntityFrameworkCore;
using PickleHub.Catalog.Domain;
using PickleHub.Catalog.Domain.Entities;

namespace PickleHub.Catalog.Infrastructure.Persistence;

public class CatalogDbContext : DbContext
{
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options) { }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(e =>
        {
            e.ToTable("category");
            e.HasKey(c => c.Id);
            e.Property(c => c.Name).IsRequired().HasMaxLength(200);
            e.Property(c => c.CreatedAt).HasColumnName("created_at");
            e.Property(c => c.UpdatedAt).HasColumnName("updated_at");
            e.HasMany(c => c.Children)
                .WithOne()
                .HasForeignKey(c => c.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Brand>(e =>
        {
            e.ToTable("brand");
            e.HasKey(b => b.Id);
            e.Property(b => b.Name).IsRequired().HasMaxLength(200);
            e.Property(c => c.CreatedAt).HasColumnName("created_at");
            e.Property(c => c.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.Entity<Product>(e =>
        {
            e.ToTable("product");
            e.HasKey(p => p.Id);
            e.Property(p => p.Name).IsRequired().HasMaxLength(300);
            e.Property(p => p.BasePrice).HasColumnType("decimal(12,2)");
            e.Property(p => p.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(p => p.SpecsJson).HasColumnName("specs").HasColumnType("jsonb");
            e.Property(c => c.CreatedAt).HasColumnName("created_at");
            e.Property(c => c.UpdatedAt).HasColumnName("updated_at");

            e.HasOne(p => p.Category)
                .WithMany()
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(p => p.Brand)
                .WithMany()
                .HasForeignKey(p => p.BrandId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ProductVariant>(e =>
        {
            e.ToTable("product_variant");
            e.HasKey(v => v.Id);
            e.Property(v => v.Sku).IsRequired().HasMaxLength(100);
            e.HasIndex(v => v.Sku).IsUnique();
            e.Property(v => v.AttributesJson).HasColumnName("attributes").HasColumnType("jsonb");
            e.Property(v => v.Price).HasColumnType("decimal(12,2)");
            e.Property(c => c.CreatedAt).HasColumnName("created_at");
            e.Property(c => c.UpdatedAt).HasColumnName("updated_at");

            e.HasOne(v => v.Product)
                .WithMany(p => p.Variants)
                .HasForeignKey(v => v.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProductImage>(e =>
        {
            e.ToTable("product_image");
            e.HasKey(i => i.Id);
            e.Property(i => i.Url).IsRequired();
            e.Property(c => c.CreatedAt).HasColumnName("created_at");
            e.Property(c => c.UpdatedAt).HasColumnName("updated_at");

            e.HasOne(i => i.Product)
                .WithMany(p => p.Images)
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}