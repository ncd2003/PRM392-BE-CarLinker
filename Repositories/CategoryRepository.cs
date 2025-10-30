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

        public CategoryRepository(CategoryDAO categoryDAO)
        {
            _categoryDAO = categoryDAO ?? throw new ArgumentNullException(nameof(categoryDAO));
        }
        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            return await _categoryDAO.GetAllCategoriesAsync();
        }
    }
}
