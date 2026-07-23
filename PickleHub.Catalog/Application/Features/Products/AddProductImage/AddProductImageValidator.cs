using FluentValidation;

namespace PickleHub.Catalog.Application.Features.Products.AddProductImage
{
    public class AddProductImageValidator : AbstractValidator<AddProductImageCommand>
    {
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
        private const long MaxFileSizeBytes = 5 * 1024 * 1024;
        public AddProductImageValidator()
        {
            RuleFor(x => x.ProductId)
                .NotEmpty().WithMessage("Thiếu Id sản phẩm.");

            RuleFor(x => x.File)
                .NotNull().WithMessage("File ảnh không được để trống.")
                .Must(f => f.Length > 0).WithMessage("File ảnh không được rỗng.")
                .Must(f => f.Length <= MaxFileSizeBytes).WithMessage("File ảnh không được vượt quá 5MB.")
                .Must(f => AllowedExtensions.Contains(
                    Path.GetExtension(f.FileName).ToLowerInvariant()))
                .WithMessage("Chỉ chấp nhận file .jpg, .jpeg, .png, .webp.");

            RuleFor(x => x.SortOrder)
                .GreaterThanOrEqualTo(0).WithMessage("Thứ tự sắp xếp không được âm.");
        }
    }
}
