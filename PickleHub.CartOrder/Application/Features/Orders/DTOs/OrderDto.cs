namespace PickleHub.CartOrder.Application.Features.Orders.DTOs;

// DTO trả về chi tiết 1 đơn hàng
public class OrderDto
{
    public Guid OrderId { get; set; }
    public decimal TotalPrice { get; set; }
    public string Status { get; set; } = string.Empty;  
    public DateTime CreatedAt { get; set; }

    // Thông tin giao hàng
    public string ShippingFullName { get; set; } = string.Empty;
    public string ShippingPhone { get; set; } = string.Empty;
    public string ShippingAddress { get; set; } = string.Empty;
    public string ShippingCity { get; set; } = string.Empty;

    // Danh sách sản phẩm trong đơn
    public List<OrderItemDto> Items { get; set; } = new();
}
