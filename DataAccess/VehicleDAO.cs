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

        public async Task<IEnumerable<Vehicle>> GetAllByUserId(int userId)
        {
            return await _context.Vehicle.Where(v => v.UserId == userId && v.IsActive).ToListAsync();
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

        public async Task<bool> IsExistByLicensePlate(string licensenPlate)
        {
            return await _context.Vehicle.AnyAsync(v => v.LicensePlate.ToLower().Trim().Equals(licensenPlate.ToLower().Trim()));
        }


    }
}
