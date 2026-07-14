using PickleHub.Catalog.Domain.Enums;
using PickleHub.Common.Domain;
using PickleHub.Common.Exceptions;
using PickleHub.Common.ValueObjects;

namespace PickleHub.Catalog.Domain.Entities
{
    public class Product : BaseEntity
    {
        public Guid CategoryId { get; private set; }
        public Guid BrandId { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public Slug Slug { get; private set; } = null!;
        public string Description { get; private set; } = string.Empty;
        public decimal BasePrice { get; private set; }
        public ProductStatus Status { get; private set; } = ProductStatus.Draft;
        public string SpecsJson { get; private set; } = "{}";
        public int SoldCount { get; private set; }

        public Category? Category { get; private set; }
        public Brand? Brand { get; private set; }

        private readonly List<ProductImage> _images = new();
        public IReadOnlyCollection<ProductImage> Images => _images.AsReadOnly();

        private readonly List<ProductVariant> _variants = new();
        public IReadOnlyCollection<ProductVariant> Variants => _variants.AsReadOnly();

        private Product() { }

        // --- Factory method ---
        public static Product Create(string name, Slug slug, string description,
            Guid categoryId, Guid brandId, decimal basePrice, string specsJson)
        {
            if (basePrice <= 0)
                throw new DomainException("Giá sản phẩm phải lớn hơn 0.");

            return new Product
            {
                Name = name,
                Slug = slug,
                Description = description,
                CategoryId = categoryId,
                BrandId = brandId,
                BasePrice = basePrice,
                SpecsJson = string.IsNullOrWhiteSpace(specsJson) ? "{}" : specsJson,
                Status = ProductStatus.Draft 
            };
        }

        public void Update(string name, Slug slug, string description,
            Guid categoryId, Guid brandId, decimal basePrice, string specsJson)
        {
            if (basePrice <= 0)
                throw new DomainException("Giá sản phẩm phải lớn hơn 0.");

            Name = name;
            Slug = slug;
            Description = description;
            CategoryId = categoryId;
            BrandId = brandId;
            BasePrice = basePrice;
            SpecsJson = string.IsNullOrWhiteSpace(specsJson) ? "{}" : specsJson;
            UpdatedAt = DateTime.UtcNow;
        }

        // --- Hành vi thay đổi trạng thái ---
        public void Publish()
        {
            if (Status == ProductStatus.Hidden)
                throw new DomainException("Không thể publish sản phẩm đã bị ẩn. Vui lòng khôi phục trước.");

            if (!_images.Any())
                throw new DomainException("Sản phẩm phải có ít nhất 1 ảnh trước khi publish.");

            if (!_variants.Any())
                throw new DomainException("Sản phẩm phải có ít nhất 1 biến thể trước khi publish.");

            Status = ProductStatus.Active;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Hide()
        {
            if (Status == ProductStatus.Draft)
                throw new DomainException("Sản phẩm đang ở trạng thái Draft, không cần ẩn.");

            Status = ProductStatus.Hidden;
            SetUpdated();
        }

        public void Restore()
        {
            if (Status != ProductStatus.Hidden)
                throw new DomainException("Chỉ có thể khôi phục sản phẩm đang bị ẩn.");

            Status = ProductStatus.Draft;
            SetUpdated();
        }

        public ProductImage AddImage(
            string publicId,
            string url,
            int sortOrder,
            Guid? variantId = null,
            bool isSizeChart = false)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new DomainException("URL ảnh không được để trống.");

            if (string.IsNullOrWhiteSpace(publicId))
                throw new DomainException("PublicId ảnh không được để trống.");

            var image = ProductImage.Create(Id, publicId, url, sortOrder, variantId, isSizeChart);
            _images.Add(image);
            UpdatedAt = DateTime.UtcNow;
            return image;
        }

        public void RemoveImage(Guid imageId)
        {
            var image = _images.FirstOrDefault(i => i.Id == imageId);
            if (image is null)
                throw new NotFoundException("Ảnh không tồn tại trong sản phẩm này.");

            _images.Remove(image);
            SetUpdated();
        }

        public ProductVariant AddVariant(string sku, string attributesJson, decimal price)
        {
            if (_variants.Any(v => v.Sku == sku))
                throw new ConflictException($"SKU '{sku}' đã tồn tại trong sản phẩm này.");

            var variant = ProductVariant.Create(Id, sku, attributesJson, price);
            _variants.Add(variant);
            SetUpdated();
            return variant;
        }

        public void UpdateVariant(Guid variantId, string sku, string attributesJson, decimal price)
        {
            var variant = _variants.FirstOrDefault(v => v.Id == variantId);
            if (variant is null)
                throw new NotFoundException("Biến thể không tồn tại trong sản phẩm này.");

            if (_variants.Any(v => v.Sku == sku && v.Id != variantId))
                throw new ConflictException($"SKU '{sku}' đã tồn tại trong sản phẩm này.");

            variant.Update(sku, attributesJson, price);
            UpdatedAt = DateTime.UtcNow;
        }

        public void RemoveVariant(Guid variantId)
        {
            var variant = _variants.FirstOrDefault(v => v.Id == variantId);
            if (variant == null)
                throw new NotFoundException("Biến thể không tồn tại trong sản phẩm này.");

            _variants.Remove(variant);
            SetUpdated();
        }

        public void IncreaseSoldCount(int quantity)
        {
            if (quantity <= 0)
                throw new DomainException("Số lượng bán phải lớn hơn 0.");

            SoldCount += quantity;
            SetUpdated();
        }

    }
    }
