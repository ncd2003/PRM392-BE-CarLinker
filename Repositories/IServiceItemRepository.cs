using BusinessObjects.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public interface IServiceItemRepository
    {
        Task<IEnumerable<ServiceItem>> GetAllAsync();
        Task<(IEnumerable<ServiceItem> items, int total)> GetAllAsync(
            int page,
            int pageSize,
            string? sortBy = null,
            bool isAsc = true);
        Task<ServiceItem?> GetByIdAsync(int id);
        Task AddAsync(ServiceItem serviceItem);
        Task UpdateAsync(ServiceItem serviceItem);
        Task DeleteAsync(int id);
    }
}
