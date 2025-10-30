using BusinessObjects.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public interface IServiceRepository
    {
        Task<(IEnumerable<Service> items, int total)> GetAllAsync(
            int page,
            int pageSize,
            string? sortBy = null,
            bool isAsc = true);

        Task AddAsync(Service service);
        Task UpdateAsync(int id, Service service);
        Task DeleteAsync(int id);

        Task<Service?> GetByIdAsync(int id);
    }
}
