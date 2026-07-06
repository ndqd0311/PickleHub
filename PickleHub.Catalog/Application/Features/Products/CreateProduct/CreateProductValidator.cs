using FluentValidation;

namespace PickleHub.Catalog.Application.Features.Products.CreateProduct
{
    public class CreateProductValidator : AbstractValidator<CreateProductCommand>
    {
        public CreateProductValidator() 
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Tên sản phẩm không được để trống.")
                .MaximumLength(100).WithMessage("Tên sản phẩm tối đa 100 kí tự.");

            RuleFor(x => x.BasePrice)
                .GreaterThan(0).WithMessage("Giá sản phẩm phải lớn hơn ).");

            RuleFor(x => x.BrandId)
                .NotEmpty().WithMessage("Phải chọn thương hiệu.");

            RuleFor(x => x.CategoryId)
                .NotEmpty().WithMessage("Phải chọn danh mục.");
        }
    }
}
