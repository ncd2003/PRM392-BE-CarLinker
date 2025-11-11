using BusinessObjects.Models;
using BusinessObjects.Models.DTOs.Order;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public interface IOrderRepository
    {
        Task<Order> CreateOrderFromCart(int userId, CreateOrderDto orderDto);

        Task<List<Order>> GetOrdersByUserId(int userId);

        Task<Order?> GetOrderDetail(int orderId);

        Task<List<Order>> GetAllOrders();

        Task UpdateOrderStatus(int orderId, OrderStatus newStatus);

        Task<bool> CancelOrderAsync(int orderId, int? userId);

        Task<int> GetTotalOrderCountAsync();

        Task<int> GetPendingOrderCountAsync();

        Task<decimal> GetTotalRevenueAsync();

        //Task<bool> UpdateOrderStatusWithTransaction(int orderId, OrderStatus newStatus);
    }
}
