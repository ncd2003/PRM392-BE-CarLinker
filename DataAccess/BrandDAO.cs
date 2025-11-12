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
                .ToListAsync();
        }

        public async Task<Brand?> GetBrandByIdAsync(int id)
        {
            return await _context.Brand
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Brand> CreateBrandAsync(Brand brand)
        {
            brand.IsActive = true;
            await _context.Brand.AddAsync(brand);
            await _context.SaveChangesAsync();
            return brand;
        }

        public async Task<bool> UpdateBrandAsync(Brand brand)
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteBrandAsync(int id)
        {
            var brand = await _context.Brand.FindAsync(id);
            if (brand == null)
            {
                return false;
            }

            //soft
            brand.IsActive = false;
            return await _context.SaveChangesAsync() > 0;
        }
    }
}