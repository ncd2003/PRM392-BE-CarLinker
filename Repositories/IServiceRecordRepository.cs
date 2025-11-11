using BusinessObjects.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public interface IServiceRecordRepository
    {
        Task<(IEnumerable<ServiceRecord> items, int total)> GetAllAsync(
            int page,
            int pageSize,
            string? sortBy = null,
            bool isAsc = true);
        Task<(IEnumerable<ServiceRecord> items, int total)> GetAllByUserIdAsync(
            int userId,
            int page,
            int pageSize,
            string? sortBy = null,
            bool isAsc = true);


        Task<ServiceRecord?>GetByIdAsync(int id);
        Task AddAsync(ServiceRecord serviceItem);
        Task UpdateAsync(ServiceRecord serviceItem);
    }
}
