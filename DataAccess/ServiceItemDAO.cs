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
    public class ServiceItemDAO
    {
        private readonly MyDbContext _context;
        public ServiceItemDAO(MyDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }
        public async Task<(IEnumerable<ServiceItem> items, int total)> GetAll(
            int page,
            int pageSize,
            string? sortBy = null,
            bool isAsc = true)
        {
            var query = _context.ServiceItem.Where(s => s.IsActive).Include(si => si.ServiceCategory).AsQueryable();

            // Get total before pagination
            var total = await query.CountAsync();

            // Apply sorting
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                query = ApplySorting(query, sortBy, isAsc);
            }
            else
            {
                // Default sorting by CreatedAt descending
                query = query.OrderByDescending(s => s.CreatedAt);
            }

            // Apply pagination
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }

        private IQueryable<ServiceItem> ApplySorting(IQueryable<ServiceItem> query, string sortBy, bool isAsc)
        {
            var sortByLower = sortBy.ToLower();
            return sortByLower switch
            {
                "name" => isAsc ? query.OrderBy(s => s.Name) : query.OrderByDescending(s => s.Name),
                "createdat" => isAsc ? query.OrderBy(s => s.CreatedAt) : query.OrderByDescending(s => s.CreatedAt),
                _ => query.OrderByDescending(s => s.CreatedAt), // Default sorting
            };
        }

        public async Task<IEnumerable<ServiceItem>> GetAll()
        {
            return await _context.ServiceItem
                .Where(v => v.IsActive)
                .ToListAsync();
        }

        public async Task<ServiceItem?> GetById(int id)
        {
            return await _context.ServiceItem.FirstOrDefaultAsync(s => s.Id == id && s.IsActive);
        }

        public async Task Add(ServiceItem serviceItem)
        {
            await _context.ServiceItem.AddAsync(serviceItem);
            await _context.SaveChangesAsync();
        }

        public async Task Update(ServiceItem serviceItem)
        {
            _context.ServiceItem.Update(serviceItem);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(ServiceItem serviceItem)
        {
            _context.Update(serviceItem);
            await _context.SaveChangesAsync();
        }

        // ✅ THÊM: Get nhiều ServiceItems theo IDs
        public async Task<List<ServiceItem>> GetByIds(List<int> serviceItemIds)
        {
            if (serviceItemIds == null || !serviceItemIds.Any())
            {
                return new List<ServiceItem>();
            }

            var distinctIds = serviceItemIds.Distinct().ToList();

            return await _context.ServiceItem
                .Where(si => distinctIds.Contains(si.Id) && si.IsActive)
                .ToListAsync();
        }

        // Version đơn giản: Chỉ tính tổng các items có trong DB và IsActive
        public async Task<decimal> TotalPriceByIds(List<int> serviceItemIds)
        {
            // ✅ Validate input
            if (serviceItemIds == null || !serviceItemIds.Any())
            {
                return 0;
            }

            // ✅ Remove duplicates và tính tổng (chỉ items IsActive)
            var distinctIds = serviceItemIds.Distinct().ToList();

            return await _context.ServiceItem
                .Where(si => distinctIds.Contains(si.Id) && si.IsActive)
                .SumAsync(si => si.Price);
        }
    }
}
