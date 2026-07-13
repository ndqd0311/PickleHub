using FluentValidation;

namespace PickleHub.CartOrder.Application.Features.Cart.AddItem;

public class AddCartItemValidator : AbstractValidator<AddCartItemCommand>
{
    public AddCartItemValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId không được để trống.");

        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("ProductId không được để trống.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Số lượng sản phẩm thêm vào giỏ phải lớn hơn 0.");
    }
}
