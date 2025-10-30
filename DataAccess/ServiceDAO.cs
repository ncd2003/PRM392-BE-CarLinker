using BusinessObjects;
using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public class ServiceDAO
    {
        private readonly MyDbContext _context;

        public ServiceDAO(MyDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<(IEnumerable<Service> items, int total)> GetAll(
            int page,
            int pageSize,
            string? sortBy = null,
            bool isAsc = true)
        {
            var query = _context.Service.Where(s => s.IsActive).AsQueryable();

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

        private IQueryable<Service> ApplySorting(IQueryable<Service> query, string sortBy, bool isAsc)
        {
            var sortByLower = sortBy.ToLower();
            return sortByLower switch
            {
                "name" => isAsc ? query.OrderBy(s => s.Name) : query.OrderByDescending(s => s.Name),
                "createdat" => isAsc ? query.OrderBy(s => s.CreatedAt) : query.OrderByDescending(s => s.CreatedAt),
                _ => query.OrderByDescending(s => s.CreatedAt), // Default sorting
            };
        }
        public async Task<Service?> GetById(int id)
        {
            return await _context.Service.FirstOrDefaultAsync(s => s.Id == id && s.IsActive);
        }

        public async Task Add(Service service)
        {
            await _context.Service.AddAsync(service);
            await _context.SaveChangesAsync();
        }

        public async Task Update(Service service)
        {
            _context.Service.Update(service);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(Service service)
        {
            _context.Update(service);
            await _context.SaveChangesAsync();
        }

    }
}
