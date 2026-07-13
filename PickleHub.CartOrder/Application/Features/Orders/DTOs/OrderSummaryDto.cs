namespace PickleHub.CartOrder.Application.Features.Orders.DTOs;

public class OrderSummaryDto
{
    // DTO rút gọn — dùng cho danh sách đơn hàng (GetMyOrders)
    // Không cần Items vì hiển thị dạng list
    public Guid OrderId { get; set; }
    public decimal TotalPrice { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int ItemCount { get; set; }  
}