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
    public class GarageDAO
    {
        private readonly MyDbContext _context;

        public GarageDAO(MyDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<(IEnumerable<Garage> items, int total)> GetAll(
            int page,
            int pageSize,
            string? sortBy = null,
            bool isAsc = true)
        {
            var query = _context.Garage.Where(s => s.IsActive).AsQueryable();

            var total = await query.CountAsync();

            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                query = ApplySorting(query, sortBy, isAsc);
            }
            else
            {
                query = query.OrderByDescending(s => s.CreatedAt);
            }

            // Apply pagination
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }

        private IQueryable<Garage> ApplySorting(IQueryable<Garage> query, string sortBy, bool isAsc)
        {
            var sortByLower = sortBy.ToLower();
            return sortByLower switch
            {
                "name" => isAsc ? query.OrderBy(s => s.Name) : query.OrderByDescending(s => s.Name),
                "createdat" => isAsc ? query.OrderBy(s => s.CreatedAt) : query.OrderByDescending(s => s.CreatedAt),
                _ => query.OrderByDescending(s => s.CreatedAt), // Default sorting
            };
        }

        public async Task Add(Garage garage)
        {
            await _context.Garage.AddAsync(garage);
            await _context.SaveChangesAsync();
        }
        public async Task<Garage?> GetById(int id)
        {
            return await _context.Garage
                .Include(g => g.ServiceCategories)
                    .ThenInclude(sc => sc.ServiceItems)
                .FirstOrDefaultAsync(g => g.Id == id && g.IsActive);
        }

        public async Task<List<ServiceItem>> GetServiceItemsByGarageId(int garageId)
        {
            return await _context.ServiceItem
                .Where(si => si.ServiceCategory != null && 
                            si.ServiceCategory.GarageId == garageId && 
                            si.IsActive)
                .ToListAsync();
        }

        public async Task<List<ServiceRecord>> GetBookingsByGarageIdAndDate(int garageId, DateTime date)
        {
            var startOfDay = date.Date;
            var endOfDay = startOfDay.AddDays(1);

            return await _context.ServiceRecord
                .Where(sr => sr.GarageId == garageId &&
                            sr.StartTime >= startOfDay &&
                            sr.StartTime < endOfDay &&
                            sr.ServiceRecordStatus != BusinessObjects.Models.Type.ServiceRecordStatus.CANCELLED)
                .ToListAsync();
        }

        public async Task Update(Garage garage)
        {
            _context.Garage.Update(garage);
            await _context.SaveChangesAsync();
        }
        public async Task Delete(Garage garage)
        {
            _context.Garage.Update(garage);
            await _context.SaveChangesAsync();
        }
    }
}
