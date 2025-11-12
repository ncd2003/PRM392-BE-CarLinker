using BusinessObjects.Models;
using BusinessObjects.Models.DTOs.Product;
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

        Task<List<Product>> SearchProductsAsync(string searchTerm);
        Task<List<ProductVariant>> GetProductVariantDefault();
        Task<Product> AddProductAsync(Product product);

        Task<Product> UpdateProductAsync(Product product);

        Task<bool> DeleteProductAsync(int productId);

        Task<ProductVariant?> AddProductVariantAsync(ProductVariant newVariant, List<int> selectedOptionValueIds);

        Task<ProductOption?> AddProductOptionAsync(ProductOption productOption);

        Task<OptionValue?> AddOptionValueAsync(OptionValue optionValue);

        Task<ProductOption?> GetProductOptionAsync(int optionId);

        Task<ProductOption?> UpdateProductOptionAsync(ProductOption optionToUpdate);

        Task<bool> DeleteProductOptionAsync(int optionId);

        Task<OptionValue?> GetOptionValueAsync(int valueId);

        Task<OptionValue?> UpdateOptionValueAsync(OptionValue valueToUpdate);

        Task<bool> DeleteOptionValueAsync(int valueId);

        Task<ProductVariant?> UpdateProductVariantAsync(ProductVariant variantToUpdate);

        Task<bool> DeleteProductVariantAsync(int productVariantId);

        Task<List<ProductVariant>> GetVariantsByProductIdAsync(int productId);
    }
}
