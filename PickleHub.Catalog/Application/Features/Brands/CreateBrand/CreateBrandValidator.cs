using FluentValidation;

namespace PickleHub.Catalog.Application.Features.Brands.CreateBrand
{
    public class CreateBrandValidator : AbstractValidator<CreateBrandCommand>
    {
        public CreateBrandValidator() 
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Tên thương hiệu không được để trống.")
                .MaximumLength(100).WithMessage("Tên thương hiệu tối đa 100 kí tự.");
        }
    }
}
