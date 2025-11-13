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
    public class ServiceCategoryDAO
    {
        private readonly MyDbContext _context;

        public ServiceCategoryDAO(MyDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }
        public async Task<IEnumerable<ServiceCategory>> GetAll()
        {
            return await _context.ServiceCategory
                .Include(sc => sc.ServiceItems)
                .Where(v => v.IsActive)
                .ToListAsync();
        }

        public async Task<(IEnumerable<ServiceCategory> items, int total)> GetAll(
            int page,
            int pageSize,
            string? sortBy = null,
            bool isAsc = true)
        {
            var query = _context.ServiceCategory.Include(sc => sc.ServiceItems).Where(v => v.IsActive);

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
                query = query.OrderByDescending(v => v.CreatedAt);
            }

            // Apply pagination
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }

        private IQueryable<ServiceCategory> ApplySorting(IQueryable<ServiceCategory> query, string sortBy, bool isAsc)
        {
            // Normalize sortBy to lowercase for case-insensitive comparison
            var sortByLower = sortBy.ToLower();

            return sortByLower switch
            {
                "id" => isAsc ? query.OrderBy(v => v.Id) : query.OrderByDescending(v => v.Id),
                "createdat" => isAsc ? query.OrderBy(v => v.CreatedAt) : query.OrderByDescending(v => v.CreatedAt),
                "updatedat" => isAsc ? query.OrderBy(v => v.UpdatedAt) : query.OrderByDescending(v => v.UpdatedAt),
                _ => query.OrderByDescending(v => v.CreatedAt) // Default sorting
            };
        }

        public async Task<ServiceCategory?> GetById(int id)
        {
            return await _context.ServiceCategory
                .Include(sc => sc.ServiceItems)
                .Where(v => v.IsActive)
                .FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task Add(ServiceCategory serviceCategory)
        {
            await _context.ServiceCategory.AddAsync(serviceCategory);
            await _context.SaveChangesAsync();
        }

        public async Task Update(ServiceCategory serviceCategory)
        {
            _context.ServiceCategory.Update(serviceCategory);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(ServiceCategory serviceCategory)
        {
            _context.ServiceCategory.Update(serviceCategory);
            await _context.SaveChangesAsync();
        }

    }
}
