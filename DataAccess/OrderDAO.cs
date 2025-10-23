using BusinessObjects;
using BusinessObjects.Models;
using BusinessObjects.Models.DTOs.Order;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public class OrderDAO
    {
        private readonly MyDbContext _context;
        private readonly ILogger<OrderDAO> _logger;
        private readonly CartDAO _cartDAO;

        public OrderDAO(MyDbContext context, ILogger<OrderDAO> logger, CartDAO cartDAO)
        {
            _context = context;
            _logger = logger;
            _cartDAO = cartDAO;
        }

        /// Xử lý checkout, chuyển Cart -> Order
        public async Task<Order> CreateOrderFromCart(int userId, CreateOrderDto orderDto)
        {
            // Đảm bảo rằng tất cả các bước hoặc thành công hoặc thất bại cùng nhau.
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 2. Lấy giỏ hàng của người dùng
                var cartId = await _cartDAO.GetCartIdByUserId(userId);
                if (cartId == 0)
                {
                    throw new Exception("Giỏ hàng của bạn đang trống.");
                }

                var cartItems = (await _cartDAO.GetListCartItemByCartId(cartId)).ToList();
                if (!cartItems.Any())
                {
                    throw new Exception("Giỏ hàng của bạn đang trống.");
                }

                decimal totalAmount = 0;
                var productsToUpdate = new List<ProductVariant>();

                // Tải các ProductVariantId từ giỏ hàng để kiểm tra
                var listVariantId = cartItems.Select(ci => ci.ProductVariantId).ToList();
                var listVariants = await _context.ProductVariant
                                    .Where(pv => listVariantId.Contains(pv.Id))
                                    .ToListAsync();

                // 3. Kiểm tra tồn kho (Lần cuối) và tính tổng tiền
                foreach (var item in cartItems)
                {
                    var variant = listVariants.FirstOrDefault(v => v.Id == item.ProductVariantId);
                    if (variant == null)
                    {
                        throw new Exception($"Sản phẩm với ID {item.ProductVariantId} không còn tồn tại.");
                    }

                    int availableStock = variant.StockQuantity - variant.HoldQuantity;
                    if (item.Quantity > availableStock)
                    {
                        // Nếu dùng Include() ở GetListCartItemByCartId thì có thể lấy tên
                        throw new Exception($"Sản phẩm '{variant.Name}' không đủ tồn kho (chỉ còn {availableStock}).");
                    }

                    // 4. CẬP NHẬT HOLD QUANTITY (Tạm giữ hàng)
                    variant.HoldQuantity += item.Quantity;
                    productsToUpdate.Add(variant);

                    // Tính tổng tiền dựa trên giá lúc thêm vào giỏ
                    totalAmount += (item.Quantity * item.UnitPrice);
                }

                // 5. Tạo Order mới
                var newOrder = new Order
                {
                    UserId = userId,
                    TotalAmount = totalAmount,
                    Status = OrderStatus.Pending, // Dùng enum
                    PaymentMethod = orderDto.PaymentMethod,
                    ShippingAddress = orderDto.ShippingAddress,
                    PhoneNumber = orderDto.PhoneNumber,
                    OrderDate = DateTime.UtcNow,
                    OrderItems = new List<OrderItem>() // Khởi tạo danh sách
                };

                // 6. Tạo các OrderItem từ CartItem
                foreach (var item in cartItems)
                {
                    var orderItem = new OrderItem
                    {
                        ProductVariantId = item.ProductVariantId,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice, // Lấy giá từ giỏ hàng (giá tại thời điểm mua)
                        Subtotal = item.Quantity * item.UnitPrice
                    };
                    newOrder.OrderItems.Add(orderItem); // Thêm vào danh sách của Order
                }

                // 7. Thêm Order (và OrderItems) vào context
                _context.Order.Add(newOrder);

                // 8. Cập nhật số lượng tạm giữ (HoldQuantity) của sản phẩm
                _context.ProductVariant.UpdateRange(productsToUpdate);

                // 9. Xóa các mục trong giỏ hàng
                _context.CartItem.RemoveRange(cartItems);

                // 10. Lưu tất cả thay đổi vào DB
                await _context.SaveChangesAsync();

                // 11. Commit giao dịch (chốt)
                await transaction.CommitAsync();

                _logger.LogInformation("Tạo đơn hàng {OrderId} cho UserId {UserId} thành công.", newOrder.Id, userId);
                return newOrder;
            }
            catch (Exception ex)
            {
                // 12. Nếu có lỗi, Rollback (hủy bỏ) tất cả thay đổi
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Lỗi khi tạo đơn hàng cho UserId {UserId}", userId);
                // Ném lỗi ra ngoài để Controller bắt
                throw;
            }
        }

        /// <summary>
        /// Lấy TẤT CẢ đơn hàng của một người dùng cụ thể.
        /// </summary>
        public async Task<List<Order>> GetOrdersByUserId(int userId)
        {
            return await _context.Order
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate) // Sắp xếp mới nhất lên trên
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Lấy chi tiết MỘT đơn hàng (bao gồm cả các sản phẩm bên trong).
        /// </summary>
        public async Task<Order?> GetOrderDetail(int orderId)
        {
            return await _context.Order
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.ProductVariant)
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }

        /// <summary>
        /// Lấy TẤT CẢ đơn hàng (cho Admin - có thể thêm Paging/Filter sau)
        /// </summary>
        public async Task<List<Order>> GetAllOrders()
        {
            return await _context.Order
               .OrderByDescending(o => o.OrderDate)
               .Include(o => o.User) // Tải thông tin người dùng
               .AsNoTracking()
               .ToListAsync();
        }

        /// <summary>
        /// Cập nhật trạng thái của một đơn hàng (dùng cho Admin).
        /// </summary>
        public async Task<bool> UpdateOrderStatus(int orderId, OrderStatus newStatus)
        {
            var order = await _context.Order.FindAsync(orderId);
            if (order == null)
            {
                throw new Exception("Không tìm thấy đơn hàng.");
            }

            // TODO: Bạn có thể thêm logic kiểm tra newStatus có hợp lệ không
            // (Vd: không thể chuyển từ "Delivered" về "Pending")

            order.Status = newStatus;
            _context.Order.Update(order);
            return await _context.SaveChangesAsync() > 0;
        }

        // --- THÊM VÀO ORDERDAO ---

        // --- HÀM NÀY CẦN SỬA ---
        /// <summary>
        /// Hủy một đơn hàng (cho Customer hoặc Admin).
        /// </summary>
        public async Task<bool> CancelOrderAsync(int orderId, int? userId)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var order = await _context.Order
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                {
                    throw new Exception("Không tìm thấy đơn hàng.");
                }

                if (userId.HasValue && order.UserId != userId.Value)
                {
                    throw new UnauthorizedAccessException("Bạn không có quyền hủy đơn hàng này.");
                }

                // SỬA: So sánh với enum
                if (order.Status != OrderStatus.Pending)
                {
                    throw new Exception($"Không thể hủy đơn hàng. Đơn hàng đang ở trạng thái '{order.Status}'.");
                }

                // 1. TRẢ LẠI HÀNG TẠM GIỮ (HoldQuantity)
                var variantIds = order.OrderItems.Select(oi => oi.ProductVariantId).ToList();
                var variantsToUpdate = await _context.ProductVariant
                                            .Where(pv => variantIds.Contains(pv.Id))
                                            .ToListAsync();

                foreach (var item in order.OrderItems)
                {
                    var variant = variantsToUpdate.FirstOrDefault(v => v.Id == item.ProductVariantId);
                    if (variant != null)
                    {
                        variant.HoldQuantity -= item.Quantity;
                        if (variant.HoldQuantity < 0)
                        {
                            variant.HoldQuantity = 0;
                        }
                    }
                }
                _context.ProductVariant.UpdateRange(variantsToUpdate);

                // 2. CẬP NHẬT TRẠNG THÁI ORDER
                order.Status = OrderStatus.Cancelled; // <-- SỬA: Gán bằng enum
                _context.Order.Update(order);

                // 3. LƯU VÀ COMMIT
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Lỗi khi hủy đơn hàng {OrderId}", orderId);
                throw;
            }
        }

    }
}
