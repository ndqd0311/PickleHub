using FluentValidation;

namespace PickleHub.Catalog.Application.Features.Brands.UpdateBrand
{
    public class UpdateBrandValidator : AbstractValidator<UpdateBrandCommand>
    {
        public UpdateBrandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Id thương hiệu không được để trống.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Tên thương hiệu không được để trống.")
                .MaximumLength(100).WithMessage("Tên thương hiệu tối đa 100 kí tự.");
        }
    }
}
