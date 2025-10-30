using BusinessObjects;
using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public class CategoryDAO
    {
        private readonly MyDbContext _context;

        public CategoryDAO(MyDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            return await _context.Category
                .Where(c => c.IsActive)
                .ToListAsync();
        }
    }
}
