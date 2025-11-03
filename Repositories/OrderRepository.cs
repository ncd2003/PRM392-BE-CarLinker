using BusinessObjects;
using BusinessObjects.Models;
using BusinessObjects.Models.DTOs.Order;
using DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly OrderDAO _orderDAO;

        public OrderRepository(OrderDAO orderDAO)
        {
            _orderDAO = orderDAO ?? throw new ArgumentNullException(nameof(orderDAO));
        }

        public async Task<bool> CancelOrderAsync(int orderId, int? userId)
        {
            return await _orderDAO.CancelOrderAsync(orderId, userId);
        }

        public async Task<Order> CreateOrderFromCart(int userId, CreateOrderDto orderDto)
        {
            return await _orderDAO.CreateOrderFromCart(userId, orderDto);
        }

        public async Task<List<Order>> GetAllOrders()
        {
            return await _orderDAO.GetAllOrders();
        }

        public async Task<Order?> GetOrderDetail(int orderId)
        {
            return await _orderDAO.GetOrderDetail(orderId);
        }

        public async Task<List<Order>> GetOrdersByUserId(int userId)
        {
            return await _orderDAO.GetOrdersByUserId(userId);
        }

        public async Task UpdateOrderStatus(int orderId, OrderStatus newStatus)
        {
            await _orderDAO.UpdateOrderStatus(orderId, newStatus);
        }

        public async Task<int> GetTotalOrderCountAsync()
        {
            return await _orderDAO.GetTotalOrderCountAsync();
        }

        public async Task<int> GetPendingOrderCountAsync()
        {
            return await _orderDAO.GetPendingOrderCountAsync();
        }

        public async Task<decimal> GetTotalRevenueAsync()
        {
            return await _orderDAO.GetTotalRevenueAsync();
        }

        public async Task<bool> UpdateOrderStatusWithTransaction(int orderId, OrderStatus newStatus)
        {
            return await _orderDAO.UpdateOrderStatusWithTransaction(orderId, newStatus);
        }

    }
}
