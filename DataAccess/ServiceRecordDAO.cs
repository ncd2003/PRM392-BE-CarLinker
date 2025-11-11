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
    public class ServiceRecordDAO
    {
        private readonly MyDbContext _context;
        public ServiceRecordDAO(MyDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<(IEnumerable<ServiceRecord> items, int total)> GetAll(
            int page,
            int pageSize,
            string? sortBy = null,
            bool isAsc = true)
        {
            var query = _context.ServiceRecord
                .AsSplitQuery()
                .Include(sr => sr.User)
                .Include(sr => sr.Vehicle)
                .Include(sr => sr.ServiceItems)
                .AsQueryable();

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


        public async Task<(IEnumerable<ServiceRecord> items, int total)> GetAllByUserId(
            int userId,
            int page,
            int pageSize,
            string? sortBy = null,
            bool isAsc = true)
        {
            var query = _context.ServiceRecord
                .AsSplitQuery()
                .Include(sr => sr.User)
                .Include(sr => sr.Vehicle)
                .Include(sr => sr.ServiceItems)
                .Where(sr => sr.UserId == userId)
                .AsQueryable();

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
        private IQueryable<ServiceRecord> ApplySorting(IQueryable<ServiceRecord> query, string sortBy, bool isAsc)
        {
            var sortByLower = sortBy.ToLower();
            return sortByLower switch
            {
                "createdat" => isAsc ? query.OrderBy(s => s.CreatedAt) : query.OrderByDescending(s => s.CreatedAt),
                _ => query.OrderByDescending(s => s.CreatedAt), // Default sorting
            };
        }

        public async Task<IEnumerable<ServiceRecord>> GetAll()
        {
            return await _context.ServiceRecord
                .ToListAsync();
        }

        public async Task<ServiceRecord?> GetById(int id)
        {
            return await _context.ServiceRecord
                .Include(sr => sr.User)
                .Include(sr => sr.Vehicle)
                .Include(sr => sr.ServiceItems) 
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task Add(ServiceRecord serviceRecord)
        {
            await _context.ServiceRecord.AddAsync(serviceRecord);
            await _context.SaveChangesAsync();
        }

        public async Task Update(ServiceRecord serviceRecord)
        {
            _context.ServiceRecord.Update(serviceRecord);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(ServiceRecord serviceRecord)
        {
            _context.Update(serviceRecord);
            await _context.SaveChangesAsync();
        }
    }
}
