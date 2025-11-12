using BusinessObjects.Models;
using DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly CategoryDAO _categoryDAO;

        // Giả sử CategoryDAO đã được đăng ký trong DI
        public CategoryRepository(CategoryDAO categoryDAO)
        {
            _categoryDAO = categoryDAO;
        }

        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            return await _categoryDAO.GetAllCategoriesAsync();
        }

        public async Task<Category?> GetCategoryByIdAsync(int id)
        {
            return await _categoryDAO.GetCategoryByIdAsync(id);
        }

        public async Task<Category> CreateCategoryAsync(Category category)
        {
            return await _categoryDAO.CreateCategoryAsync(category);
        }

        public async Task<bool> UpdateCategoryAsync(Category category)
        {
            return await _categoryDAO.UpdateCategoryAsync(category);
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            return await _categoryDAO.DeleteCategoryAsync(id);
        }
    }
}
