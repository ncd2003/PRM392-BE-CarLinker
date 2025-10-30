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
            _brandDAO = brandDAO ?? throw new ArgumentNullException(nameof(brandDAO));
        }
        public async Task<List<Brand>> GetAllBrandsAsync()
        { 
            return await _brandDAO.GetAllBrandsAsync();
        }
    }
}
