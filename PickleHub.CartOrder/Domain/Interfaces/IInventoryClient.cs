namespace PickleHub.CartOrder.Domain.Interfaces;

//Contract để gọi Inventory Service để kiểm tra tồn kho.
public interface IInventoryClient
{
    Task<bool> CheckStockAsync(Guid productId, int quantity, CancellationToken ct = default);
}
