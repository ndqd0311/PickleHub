using FluentValidation;

namespace PickleHub.CartOrder.Application.Features.Orders.Checkout;

public class CheckoutValidator : AbstractValidator<CheckoutCommand>
{
    public CheckoutValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId không được để trống.");

        RuleFor(x => x.ShippingFullName)
            .NotEmpty().WithMessage("Tên người nhận hàng không được để trống.")
            .MaximumLength(150).WithMessage("Tên người nhận tối đa 150 ký tự.");

        RuleFor(x => x.ShippingPhone)
            .NotEmpty().WithMessage("Số điện thoại nhận hàng không được để trống.")
            .Matches(@"^[0-9]{9,11}$").WithMessage("Số điện thoại không hợp lệ (phải gồm 9-11 chữ số).");

        RuleFor(x => x.ShippingAddress)
            .NotEmpty().WithMessage("Địa chỉ nhận hàng không được để trống.");

        RuleFor(x => x.ShippingCity)
            .NotEmpty().WithMessage("Tỉnh/Thành phố nhận hàng không được để trống.");
    }
}
