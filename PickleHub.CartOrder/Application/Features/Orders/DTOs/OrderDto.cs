using System;
using System.Collections.Generic;

namespace PickleHub.CartOrder.Application.Features.Orders.DTOs;

public class OrderDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    
    public string Status { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    
    public string ShippingFullName { get; set; } = string.Empty;
    public string ShippingPhone { get; set; } = string.Empty;
    public string ShippingProvince { get; set; } = string.Empty;
    public string ShippingDistrict { get; set; } = string.Empty;
    public string ShippingWard { get; set; } = string.Empty;
    public string ShippingStreetAddress { get; set; } = string.Empty;
    
    public string? ShippingProvider { get; set; }
    public string? TrackingNumber { get; set; }
    public string? TrackingUrl { get; set; }
    
    public List<OrderItemDto> Items { get; set; } = new();
    
    public decimal Subtotal { get; set; }
    public decimal ShippingFee { get; set; }
    public decimal TotalAmount { get; set; }
    
    public string? CancelledBy { get; set; }
    public string? CancelReason { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
