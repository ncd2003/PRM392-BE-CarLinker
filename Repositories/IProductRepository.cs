using BusinessObjects.Models;
using BusinessObjects.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public interface IProductRepository
    {
        Task<List<Product>> GetProductsAsync(ProductFilterParams filterParams);
        Task<Product?> GetProductDetailsAsync(int productId);
    }
}
