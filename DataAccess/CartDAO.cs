using BusinessObjects;
using BusinessObjects.Models;
using BusinessObjects.Models.DTOs.Cart;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public class CartDAO
    {
        private readonly MyDbContext _context;

        public CartDAO(MyDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<int> GetCartIdByUserId(int userId)
        {
            return await _context.Cart
                .Where(c => c.UserId == userId) 
                .Select(c => c.Id)              
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<CartItem>> GetListCartItemByCartId(int cartId)
        {
            return await _context.CartItem
                .Where(c => c.CartId == cartId)
                .ToListAsync();
        }

        public async Task<CartItem> AddProductToCart(AddProductVariantDto productVariantDto, int userId)
        {
            // 1. Kiểm tra số lượng thêm vào phải > 0
            if (productVariantDto.Quantity <= 0)
            {
                throw new ArgumentException("Số lượng thêm vào giỏ hàng phải lớn hơn 0.");
            }

            var productVariant = await _context.ProductVariant
                .FirstOrDefaultAsync(pv => pv.Id == productVariantDto.ProductVariantId);

            if (productVariant == null || !productVariant.IsActive)
            {
                throw new Exception("Sản phẩm không tồn tại hoặc đã ngừng kinh doanh.");
            }

            // 3. Tìm giỏ hàng của người dùng
            var cart = await _context.Cart
                .FirstOrDefaultAsync(c => c.UserId == userId);

            // 4. Nếu chưa có giỏ hàng, tạo mới
            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                _context.Cart.Add(cart);
                await _context.SaveChangesAsync();
            }

            // 5. Tìm CartItem đã tồn tại (nếu có)
            var existingCartItem = await _context.CartItem
                .FirstOrDefaultAsync(ci => ci.CartId == cart.Id &&
                                        ci.ProductVariantId == productVariantDto.ProductVariantId);

            int currentQuantityInCart = existingCartItem?.Quantity ?? 0;
            int newTotalQuantity = currentQuantityInCart + productVariantDto.Quantity;

            // 6. KIỂM TRA TỒN KHO
            int availableStock = productVariant.StockQuantity - productVariant.HoldQuantity;
            if (newTotalQuantity > availableStock)
            {
                throw new Exception($"Không đủ tồn kho. Chỉ còn {availableStock} sản phẩm có sẵn.");
            }

            // 7. Thêm mới hoặc cập nhật CartItem
            if (existingCartItem == null)
            {
                var newCartItem = new CartItem
                {
                    CartId = cart.Id,
                    ProductVariantId = productVariantDto.ProductVariantId,
                    Quantity = productVariantDto.Quantity, // Chỉ thêm số lượng mới, không cộng lại
                    UnitPrice = productVariant.Price,
                    AddedDate = DateTime.UtcNow
                };
                _context.CartItem.Add(newCartItem);
                await _context.SaveChangesAsync();
                return newCartItem;
            }
            else
            {
                existingCartItem.Quantity = newTotalQuantity;
                _context.CartItem.Update(existingCartItem);
                await _context.SaveChangesAsync();
                return existingCartItem;
            }
        }

        public async Task RemoveItemFromCart(int userId, int productVariantId)
        {
            var cartId = await GetCartIdByUserId(userId);
            if (cartId <= 0) return; // Không có giỏ hàng

            var cartItem = await _context.CartItem
                .FirstOrDefaultAsync(ci => ci.CartId == cartId && ci.ProductVariantId == productVariantId);

            if (cartItem != null)
            {
                _context.CartItem.Remove(cartItem);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<CartItem> UpdateCartItemQuantity(int userId, int productVariantId, int newQuantity)
        {
            if (newQuantity <= 0)
            {
                await RemoveItemFromCart(userId, productVariantId);
                throw new Exception("Số lượng phải lớn hơn 0. Sản phẩm đã được xóa khỏi giỏ hàng.");
            }

            var cartId = await GetCartIdByUserId(userId);
            if (cartId <= 0)
            {
                throw new Exception("Giỏ hàng không tồn tại.");
            }
            var cartItem = await _context.CartItem
                .Include(ci => ci.ProductVariant) 
                .FirstOrDefaultAsync(ci => ci.CartId == cartId && ci.ProductVariantId == productVariantId);

            if (cartItem == null)
            {
                throw new Exception("Sản phẩm không tìm thấy trong giỏ hàng.");
            }

            // 3. Kiểm tra tồn kho
            int availableStock = cartItem.ProductVariant.StockQuantity - cartItem.ProductVariant.HoldQuantity;
            if (newQuantity > availableStock)
            {
                throw new Exception($"Không đủ tồn kho. Chỉ còn {availableStock} sản phẩm có sẵn.");
            }

            // 4. Cập nhật số lượng
            cartItem.Quantity = newQuantity;
            // Cập nhật giá (phòng trường hợp giá thay đổi)
            cartItem.UnitPrice = cartItem.ProductVariant.Price;

            _context.CartItem.Update(cartItem);
            await _context.SaveChangesAsync();
            return await _context.CartItem
                        .AsNoTracking()
                        .FirstOrDefaultAsync(ci => ci.Id == cartItem.Id);
        }

        public async Task ClearCart(int userId)
        {
            var cartId = await GetCartIdByUserId(userId);
            if (cartId <= 0) return;

            var allItems = await _context.CartItem
                .Where(ci => ci.CartId == cartId)
                .ToListAsync();

            if (allItems.Any())
            {
                _context.CartItem.RemoveRange(allItems);
                await _context.SaveChangesAsync();
            }
        }
    }
}
