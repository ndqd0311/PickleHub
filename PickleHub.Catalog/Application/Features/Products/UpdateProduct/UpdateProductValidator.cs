using FluentValidation;

namespace PickleHub.Catalog.Application.Features.Products.UpdateProduct
{
    public class UpdateProductValidator : AbstractValidator<UpdateProductCommand>
    {
        public UpdateProductValidator() 
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Tên sản phẩm không được để trống.")
                .MaximumLength(200).WithMessage("Tên sản phẩm không được vượt quá 200 ký tự.");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Mô tả sản phẩm không được vượt quá 500 ký tự.");

            RuleFor(x => x.BasePrice)
                .GreaterThan(0).WithMessage("Giá cơ bản phải lớn hơn 0.");

            //RuleFor(x=> x.BrandId)
            //    .NotEmpty().WithMessage("Phải chọn thương hiệu.");

            //RuleFor(x => x.CategoryId)
            //    .NotEmpty().WithMessage("Phải chọn danh mục sản phẩm.");

            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("Trạng thái sản phẩm không được để trống.")
                .Must(s=> new[] { "Draft", "Active", "Hidden" }.Contains(s))
                .WithMessage("Trạng thái sản phẩm không hợp lệ. Trạng thái phải là Draft, Active hoặc Hidden.");
        }
    }
}
