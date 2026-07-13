namespace PickleHub.CartOrder.Application.Features.Cart.DTOs;

// DTO cho từng sản phẩm trong giỏ
public class CartItemDto
{
    public Guid CartItemId { get; set; }        
    public Guid ProductId { get; set; }

    // Lấy live từ Catalog Service khi query giỏ hàng
    // (không lưu DB — Cart chỉ lưu ProductId + Quantity)
    public string ProductName { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }
    public decimal SubTotal => UnitPrice * Quantity;
}
