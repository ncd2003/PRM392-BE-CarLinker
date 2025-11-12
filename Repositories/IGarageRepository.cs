using BusinessObjects.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public interface IGarageRepository
    {
        Task<(IEnumerable<Garage> items, int total)> GetAllAsync(
            int page,
            int pageSize,
            string? sortBy = null,
            bool isAsc = true);
        Task<Garage?> GetByIdAsync(int id);
        Task<List<ServiceItem>> GetServiceItemsByGarageIdAsync(int garageId);
        Task<List<ServiceRecord>> GetBookingsByGarageIdAndDateAsync(int garageId, DateTime date);
        Task AddAsync(Garage garage);
        Task UpdateAsync(Garage garage);
        Task DeleteAsync(int id);
    }
}
