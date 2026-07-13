namespace PickleHub.CartOrder.Application.Features.Cart.DTOs;

// DTO trả về toàn bộ giỏ hàng
public class CartDto
{
    public Guid CartId { get; set; }
    public List<CartItemDto> Items { get; set; } = new();
    public decimal GrandTotal => Items.Sum(i => i.SubTotal);  // tổng tiền toàn giỏ
}
