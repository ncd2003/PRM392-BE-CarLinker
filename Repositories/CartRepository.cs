using BusinessObjects.Models;
using BusinessObjects.Models.DTOs.Cart;
using DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly CartDAO _cartDAO;

        public CartRepository(CartDAO cartDAO)
        {
            _cartDAO = cartDAO ?? throw new ArgumentNullException(nameof(cartDAO));
        }

        public async Task<CartItem> AddProductToCart(AddProductVariantDto productVariantDto, int userId)
        {
            return await _cartDAO.AddProductToCart(productVariantDto, userId);
        }

        public async Task<int> GetCartIdByUserId(int userId)
        {
            return await _cartDAO.GetCartIdByUserId(userId);
        }

        public async Task<IEnumerable<CartItem>> GetListCartItemByCartId(int cartId)
        {
            return await _cartDAO.GetListCartItemByCartId(cartId);
        }

        public async Task RemoveItemFromCart(int userId, int productVariantId)
        {
           await _cartDAO.RemoveItemFromCart(userId, productVariantId);
        }

        public async Task<CartItem> UpdateCartItemQuantity(int userId, int productVariantId, int newQuantity)
        {
            return await _cartDAO.UpdateCartItemQuantity(userId, productVariantId, newQuantity);
        }
    }
}
