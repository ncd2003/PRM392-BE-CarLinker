using BusinessObjects.Models;
using BusinessObjects.Models.DTOs.Cart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public interface ICartRepository
    {
        Task<int> GetCartIdByUserId(int userId);
        Task<IEnumerable<CartItem>> GetListCartItemByCartId(int cartId);

        Task<CartItem> AddProductToCart(AddProductVariantDto productVariantDto, int userId);

        Task <CartItem> UpdateCartItemQuantity(int userId, int productVariantId, int newQuantity);

        Task RemoveItemFromCart(int userId, int productVariantId);

    }
}
