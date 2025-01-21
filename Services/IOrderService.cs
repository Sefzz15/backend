using backend.Models;

namespace backend.Services
{
    public interface IOrderService
    {
        Task<IEnumerable<object>> GetOrdersAsync();
        Task<Order?> GetOrderByIdAsync(int id);
        Task<Order> AddOrderAsync(Order order);
        Task<bool> UpdateOrderAsync(int id, Order order);
        Task<bool> DeleteOrderAsync(int id);
    }
}
