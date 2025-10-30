using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Models;

namespace Repositories
{
    public interface IVehicleRepository
    {
        Task<IEnumerable<Vehicle>> GetAllAsync();
        Task<(IEnumerable<Vehicle> items, int total)> GetAllAsync(
            int page, 
            int pageSize, 
            string? sortBy = null, 
            bool isAsc = true);
        Task<Vehicle?> GetByIdAsync(int id);
        Task AddAsync(Vehicle vehicle);
        Task UpdateAsync(int id,Vehicle vehicle);
        Task DeleteAsync(int id);

        Task<bool> IsExistLicensePlate(string licensePlate);
    }
}
