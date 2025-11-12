using BusinessObjects.Models;
using DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public class BrandRepository : IBrandRepository
    {
        private readonly BrandDAO _brandDAO;

        public BrandRepository(BrandDAO brandDAO)
        {
            _brandDAO = brandDAO;
        }

        public async Task<List<Brand>> GetAllBrandsAsync()
        {
            return await _brandDAO.GetAllBrandsAsync();
        }

        public async Task<Brand?> GetBrandByIdAsync(int id)
        {
            return await _brandDAO.GetBrandByIdAsync(id);
        }

        public async Task<Brand> CreateBrandAsync(Brand brand)
        {
            return await _brandDAO.CreateBrandAsync(brand);
        }

        public async Task<bool> UpdateBrandAsync(Brand brand)
        {
            return await _brandDAO.UpdateBrandAsync(brand);
        }

        public async Task<bool> DeleteBrandAsync(int id)
        {
            return await _brandDAO.DeleteBrandAsync(id);
        }
    }
}
