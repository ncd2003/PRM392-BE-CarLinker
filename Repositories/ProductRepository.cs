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

        public async Task<ProductVariant?> AddProductVariantAsync(ProductVariant newVariant, List<int> selectedOptionValueIds)
        {
            return await _productDao.AddProductVariantAsync(newVariant, selectedOptionValueIds);
        }

        public async Task<ProductOption?> AddProductOptionAsync(ProductOption productOption)
        {
            return await _productDao.AddProductOptionAsync(productOption);
        }

        public async Task<OptionValue?> AddOptionValueAsync(OptionValue optionValue)
        {
            return await _productDao.AddOptionValueAsync(optionValue);
        }

        public async Task<ProductOption?> GetProductOptionAsync(int optionId)
        {
            return await _productDao.GetProductOptionAsync(optionId);
        }

        public async Task<ProductOption?> UpdateProductOptionAsync(ProductOption optionToUpdate)
        {
            return await _productDao.UpdateProductOptionAsync(optionToUpdate);
        }

        public async Task<bool> DeleteProductOptionAsync(int optionId)
        {
            return await _productDao.DeleteProductOptionAsync(optionId);
        }

        public async Task<OptionValue?> GetOptionValueAsync(int valueId)
        {
            return await _productDao.GetOptionValueAsync(valueId);
        }

        public async Task<OptionValue?> UpdateOptionValueAsync(OptionValue valueToUpdate)
        {
            return await _productDao.UpdateOptionValueAsync(valueToUpdate);
        }

        public async Task<bool> DeleteOptionValueAsync(int valueId)
        {
            return await _productDao.DeleteOptionValueAsync(valueId);
        }

        public async Task<ProductVariant?> UpdateProductVariantAsync(ProductVariant variantToUpdate)
        {
            return await _productDao.UpdateProductVariantAsync(variantToUpdate);
        }

        public async Task<bool> DeleteProductVariantAsync(int productVariantId)
        {
            return await _productDao.DeleteProductVariantAsync(productVariantId);
        }

        public async Task<List<ProductVariant>> GetVariantsByProductIdAsync(int productId)
        {
            return await _productDao.GetVariantsByProductIdAsync(productId);
        }
    }
}
