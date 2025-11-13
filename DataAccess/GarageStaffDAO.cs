using BusinessObjects;
using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess
{
    public class GarageStaffDAO
    {
        private readonly MyDbContext _context;

        public GarageStaffDAO(MyDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<(IEnumerable<GarageStaff> items, int total)> GetAllByGarageId(
            int garageId,
            int page,
            int pageSize,
            string? sortBy = null,
            bool isAsc = true)
        {
            var query = _context.GarageStaff
                .Where(gs => gs.GarageId == garageId && gs.IsActive)
                .Include(gs => gs.Garage)
                .AsQueryable();

            var total = await query.CountAsync();

            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                query = ApplySorting(query, sortBy, isAsc);
            }
            else
            {
                query = query.OrderByDescending(gs => gs.CreatedAt);
            }

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }

        private IQueryable<GarageStaff> ApplySorting(IQueryable<GarageStaff> query, string sortBy, bool isAsc)
        {
            var sortByLower = sortBy.ToLower();
            return sortByLower switch
            {
                "fullname" => isAsc ? query.OrderBy(gs => gs.FullName) : query.OrderByDescending(gs => gs.FullName),
                "email" => isAsc ? query.OrderBy(gs => gs.Email) : query.OrderByDescending(gs => gs.Email),
                "createdat" => isAsc ? query.OrderBy(gs => gs.CreatedAt) : query.OrderByDescending(gs => gs.CreatedAt),
                _ => query.OrderByDescending(gs => gs.CreatedAt),
            };
        }

        public async Task<GarageStaff?> GetById(int id)
        {
            return await _context.GarageStaff
                .Include(gs => gs.Garage)
                .FirstOrDefaultAsync(gs => gs.Id == id && gs.IsActive);
        }

        public async Task<GarageStaff?> GetByEmail(string email)
        {
            return await _context.GarageStaff
                .Include(gs => gs.Garage)
                .FirstOrDefaultAsync(gs => gs.Email == email && gs.IsActive);
        }

        public async Task<GarageStaff?> GetByRefreshToken(string refreshToken)
        {
            return await _context.GarageStaff
                .Include(gs => gs.Garage)
                .FirstOrDefaultAsync(gs => gs.RefreshToken == refreshToken && gs.IsActive);
        }

        public async Task<IEnumerable<GarageStaff>> GetActiveStaffByGarageId(int garageId)
        {
            return await _context.GarageStaff
                .Where(gs => gs.GarageId == garageId && gs.IsActive)
                .OrderBy(gs => gs.FullName)
                .ToListAsync();
        }

        public async Task Add(GarageStaff staff)
        {
            await _context.GarageStaff.AddAsync(staff);
            await _context.SaveChangesAsync();
        }

        public async Task Update(GarageStaff staff)
        {
            _context.GarageStaff.Update(staff);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(GarageStaff staff)
        {
            _context.GarageStaff.Update(staff);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsEmailExists(string email, int? excludeId = null)
        {
            var query = _context.GarageStaff.Where(gs => gs.Email == email);
            
            if (excludeId.HasValue)
            {
                query = query.Where(gs => gs.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }
    }
}
