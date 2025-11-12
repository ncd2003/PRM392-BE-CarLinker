using BusinessObjects.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public interface IBrandRepository
    {
        Task<List<Brand>> GetAllBrandsAsync();

        Task<Brand?> GetBrandByIdAsync(int id);

        Task<Brand> CreateBrandAsync(Brand brand);

        Task<bool> UpdateBrandAsync(Brand brand);

        Task<bool> DeleteBrandAsync(int id);
    }
}
