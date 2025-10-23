using BusinessObjects;
using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess
{
    public class VehicleDAO
    {
        private readonly MyDbContext _context;

        public VehicleDAO(MyDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<Vehicle>> GetAll()
        {
            return await _context.Vehicle
                .Where(v => v.IsActive)
                .ToListAsync();
        }

        public async Task<(IEnumerable<Vehicle> items, int total)> GetAll(
            int page,
            int pageSize,
            string? sortBy = null,
            bool isAsc = true)
        {
            var query = _context.Vehicle.Where(v => v.IsActive);

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

        private IQueryable<Vehicle> ApplySorting(IQueryable<Vehicle> query, string sortBy, bool isAsc)
        {
            // Normalize sortBy to lowercase for case-insensitive comparison
            var sortByLower = sortBy.ToLower();

            return sortByLower switch
            {
                "id" => isAsc ? query.OrderBy(v => v.Id) : query.OrderByDescending(v => v.Id),
                "licenseplate" => isAsc ? query.OrderBy(v => v.LicensePlate) : query.OrderByDescending(v => v.LicensePlate),
                "fueltype" => isAsc ? query.OrderBy(v => v.FuelType) : query.OrderByDescending(v => v.FuelType),
                "transmissiontype" => isAsc ? query.OrderBy(v => v.TransmissionType) : query.OrderByDescending(v => v.TransmissionType),
                "brand" => isAsc ? query.OrderBy(v => v.Brand) : query.OrderByDescending(v => v.Brand),
                "model" => isAsc ? query.OrderBy(v => v.Model) : query.OrderByDescending(v => v.Model),
                "year" => isAsc ? query.OrderBy(v => v.Year) : query.OrderByDescending(v => v.Year),
                "createdat" => isAsc ? query.OrderBy(v => v.CreatedAt) : query.OrderByDescending(v => v.CreatedAt),
                "updatedat" => isAsc ? query.OrderBy(v => v.UpdatedAt) : query.OrderByDescending(v => v.UpdatedAt),
                _ => query.OrderByDescending(v => v.CreatedAt) // Default sorting
            };
        }

        public async Task<Vehicle?> GetById(int id)
        {
            return await _context.Vehicle
                .Where(v => v.IsActive)
                .FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task Add(Vehicle vehicle)
        {
            await _context.Vehicle.AddAsync(vehicle);
            await _context.SaveChangesAsync();
        }

        public async Task Update(Vehicle vehicle)
        {
            _context.Vehicle.Update(vehicle);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(Vehicle vehicle)
        {
            _context.Vehicle.Update(vehicle);
            await _context.SaveChangesAsync();
        }
    }
}
