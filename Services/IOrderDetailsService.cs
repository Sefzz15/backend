using backend.Models;

namespace backend.Services
{
    public interface IOrderDetailsService
    {
        Task<IEnumerable<object>> GetOrderDetailsAsync();
        Task<OrderDetail?> GetOrderDetailByIdAsync(int id);
        Task<OrderDetail> AddOrderDetailAsync(OrderDetail orderDetail);
        Task<bool> UpdateOrderDetailAsync(int id, OrderDetail orderDetail);
        Task<bool> DeleteOrderDetailAsync(int id);
    }
}
