using BusinessObjects.Models;
using BusinessObjects.Models.DTOs.Product;
using DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly ProductDAO _productDao;

        public ProductRepository(ProductDAO productDAO)
        {
            _productDao = productDAO ?? throw new ArgumentNullException(nameof(productDAO));
        }

        public async Task<Product?> GetProductDetailsAsync(int productId)
        {
            return await _productDao.GetProductDetailsAsync(productId);
        }

        public async Task<List<Product>> GetProductsAsync(ProductFilterParams filterParams)
        {
            return await _productDao.GetProductsAsync(filterParams);
        }

        public async Task<List<ProductVariant>> GetProductVariantDefault()
        {
            return await _productDao.GetProductVariantDefault();
        }

        public async Task<List<Product>> SearchProductsAsync(string searchTerm)
        {
            return await _productDao.SearchProductsAsync(searchTerm);
        }

        public async Task<Product> AddProductAsync(Product product)
        {
            return await _productDao.CreateProductAsync(product);
        }

        public async Task<Product?> UpdateProductAsync(Product product)
        {
            return await _productDao.UpdateProductAsync(product);
        }

        public async Task<bool> DeleteProductAsync(int productId)
        {
            return await _productDao.DeleteProductAsync(productId);
        }

        public async Task<ProductVariant> AddProductVariantAsync(ProductVariant newVariant)
        {
            return await _productDao.AddProductVariantAsync(newVariant);
        }
    }
}
