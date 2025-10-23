using System;
using System.Collections.Generic;

namespace BusinessObjects.Models.DTOs.Order
{
    // DTO này đại diện cho Order trả về
    public class OrderResponseDto
    {
        public int Id { get; set; }
        public decimal TotalAmount { get; set; }
        public int Status { get; set; }
        public string PaymentMethod { get; set; }
        public string ShippingAddress { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime OrderDate { get; set; }

        // Quan trọng: Dùng List<OrderItemDto> thay vì ICollection<OrderItem>
        public List<OrderItemDto> OrderItems { get; set; }
    }
}