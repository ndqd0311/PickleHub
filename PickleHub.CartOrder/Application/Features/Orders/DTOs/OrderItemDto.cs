using System;

namespace PickleHub.CartOrder.Application.Features.Orders.DTOs;

public class OrderItemDto
{
    public Guid Id { get; set; }
    public Guid ProductVariantId { get; set; }
    public Guid ProductId { get; set; }
    
    public string ProductNameSnapshot { get; set; } = string.Empty;
    public string VariantAttributesSnapshot { get; set; } = string.Empty;
    public string? ImageUrlSnapshot { get; set; }
    
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal Subtotal { get; set; }
}