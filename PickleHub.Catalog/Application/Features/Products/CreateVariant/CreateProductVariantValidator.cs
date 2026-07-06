using FluentValidation;

namespace PickleHub.Catalog.Application.Features.Products.CreateVariant
{
    public class CreateProductVariantValidator : AbstractValidator<CreateProductVariantCommand>
    {
        public CreateProductVariantValidator()
        {
            RuleFor(x => x.ProductId)
                .NotEmpty().WithMessage("thiếu Id sản phẩm");

            RuleFor(x => x.Sku)
                .NotEmpty().WithMessage("SKU không được để trống.")
                .MaximumLength(100).WithMessage("SKU tối đa 100 kí tự.");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Giá phải lớn hơn 0.");
        }

    }
}
