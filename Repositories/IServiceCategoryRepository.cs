using BusinessObjects.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public interface IServiceCategoryRepository
    {
        Task<(IEnumerable<ServiceCategory> items, int total)> GetAllAsync(
            int page,
            int pageSize,
            string? sortBy = null,
            bool isAsc = true);
        Task<ServiceCategory?> GetByIdAsync(int id);
        Task AddAsync(ServiceCategory serviceCategory);
        Task UpdateAsync(ServiceCategory serviceCategory);
        Task DeleteAsync(int id);
    }
}
