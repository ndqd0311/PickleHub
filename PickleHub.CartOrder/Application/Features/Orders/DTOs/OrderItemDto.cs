namespace PickleHub.CartOrder.Application.Features.Orders.DTOs;


// DTO cho từng dòng sản phẩm trong đơn hàng
public class OrderItemDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;  // snapshot
    public decimal UnitPrice { get; set; }                   // snapshot
    public int Quantity { get; set; }
    public decimal SubTotal => UnitPrice * Quantity;         // tính toán, không lưu DB
}