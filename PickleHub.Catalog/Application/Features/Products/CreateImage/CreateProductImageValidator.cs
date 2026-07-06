using FluentValidation;

namespace PickleHub.Catalog.Application.Features.Products.CreateImage
{
    public class CreateProductImageValidator : AbstractValidator<CreateProductImageCommand>
    {
        public CreateProductImageValidator() 
        {
            RuleFor(x => x.ProductId)
                .NotEmpty().WithMessage("thiếu Id sản phẩm");

            RuleFor(x => x.Url)
                .NotEmpty().WithMessage("Url không được để trống.")
                .Must(BeAValidUrl).WithMessage("URL ảnh không đúng định dạng.");

            RuleFor(x => x.sortOrder)
                .GreaterThanOrEqualTo(0).WithMessage("SortOrder phải lớn hơn hoặc bằng 0.");
        }

        private bool BeAValidUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out var result)
             && (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
        }
    }
}
