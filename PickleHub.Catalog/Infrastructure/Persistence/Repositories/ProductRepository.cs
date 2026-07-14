using Microsoft.EntityFrameworkCore;
using PickleHub.Catalog.Domain.Entities;
using PickleHub.Catalog.Domain.Enums;
using PickleHub.Catalog.Domain.Repositories;
using PickleHub.Common.ValueObjects;

namespace PickleHub.Catalog.Infrastructure.Persistence.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly CatalogDbContext _db;
        public ProductRepository(CatalogDbContext db) 
        {
            _db = db;
        }
        public void Add(Product product)
        {
            _db.Products.Add(product);
        }

        public async Task<bool> ExistsBySlugAsync(string slug, Guid? excludeId = null, CancellationToken ct = default)
        {
            var s = Slug.FromPersistedValue(slug);
            var query = _db.Products.Where(p => p.Slug == s);
            if(excludeId.HasValue)
                query = query.Where(p=> p.Id != excludeId.Value);
            return await query.AnyAsync(ct);
        }

        public async Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _db.Products.FindAsync([id], ct);
        }

        public async Task<Product?> GetByIdWithDetailAsync(Guid id, CancellationToken ct = default)
        {
            return await _db.Products
                .Include(p=> p.Category)
                .Include(p=> p.Brand)
                .Include(p=> p.Images)
                .Include(p=> p.Variants)
                .FirstOrDefaultAsync(p=> p.Id == id, ct);
        }

        public async Task<Product?> GetBySlugAsync(string slug, CancellationToken ct = default)
        {
            var s = Slug.FromPersistedValue(slug);
            return await _db.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Images)
                .Include(p => p.Variants)
                .FirstOrDefaultAsync(p => p.Slug == s, ct);
        }

        public async Task<(List<Product> Items, int TotalCount)> GetPagedAsync(
            string? keyword, Guid? categoryId, Guid? brandId, decimal? minPrice, 
            decimal? maxPrice, SortBy sortBy, int page, int pageSize, CancellationToken ct = default)
        {
            var query = _db.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Images)
                .Include(p => p.Variants)
                .Where(p => p.Status == ProductStatus.Active);

            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(p => p.Name.Contains(keyword));

            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId.Value);

            if (brandId.HasValue)
                query = query.Where(p => p.BrandId == brandId.Value);

            if (minPrice.HasValue)
                query = query.Where(p => p.BasePrice >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(p => p.BasePrice <= maxPrice.Value);

            query = sortBy switch
            {
                SortBy.Newest => query.OrderByDescending(p => p.CreatedAt),
                SortBy.PriceAsc => query.OrderBy(p => p.BasePrice),
                SortBy.PriceDesc => query.OrderByDescending(p => p.BasePrice),
                SortBy.BestSelling => query.OrderByDescending(p => p.SoldCount),
                _ => query.OrderByDescending(p => p.CreatedAt)
            };

            var totalCount = await query.CountAsync(ct);
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (items, totalCount);
        }

        public async Task<(List<Product> Items, int TotalCount)> GetAdminPagedAsync(
            string? keyword, Guid? categoryId, Guid? brandId,
            decimal? minPrice, decimal? maxPrice, ProductStatus? status,
            SortBy sortBy, int page, int pageSize, CancellationToken ct = default)
        {
            var query = _db.Products
                .AsNoTracking()
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Include(p => p.Images)
                .AsQueryable(); 

            if (status.HasValue)
                query = query.Where(p => p.Status == status.Value);

            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(p => p.Name.Contains(keyword));

            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId.Value);

            if (brandId.HasValue)
                query = query.Where(p => p.BrandId == brandId.Value);

            if (minPrice.HasValue)
                query = query.Where(p => p.BasePrice >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(p => p.BasePrice <= maxPrice.Value);

            query = sortBy switch
            {
                SortBy.Newest => query.OrderByDescending(p => p.CreatedAt),
                SortBy.PriceAsc => query.OrderBy(p => p.BasePrice),
                SortBy.PriceDesc => query.OrderByDescending(p => p.BasePrice),
                SortBy.BestSelling => query.OrderByDescending(p => p.SoldCount),
                _ => query.OrderByDescending(p => p.CreatedAt)
            };

            var totalCount = await query.CountAsync(ct);
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (items, totalCount);
        }

        public void Update(Product product)
        {
            _db.Products.Update(product);
        }
    }
}
