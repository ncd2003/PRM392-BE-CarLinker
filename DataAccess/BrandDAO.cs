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
    public class BrandDAO
    {
        private readonly MyDbContext _context;

        public BrandDAO(MyDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<List<Brand>> GetAllBrandsAsync()
        {
            return await _context.Brand
                .Where(c => c.IsActive)
                .ToListAsync();
        }
    }
}
