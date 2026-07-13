using FluentValidation;

namespace PickleHub.CartOrder.Application.Features.Cart.UpdateItem;

public class UpdateCartItemValidator : AbstractValidator<UpdateCartItemCommand>
{
    public UpdateCartItemValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId không được để trống.");

        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("ProductId không được để trống.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Số lượng sản phẩm trong giỏ hàng phải lớn hơn 0.");
    }
}
