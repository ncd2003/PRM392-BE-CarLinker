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
        Task<IEnumerable<Vehicle>> GetAllByUserIdAsync(int userId);
        Task<Vehicle?> GetByIdAsync(int id);
        Task AddAsync(Vehicle vehicle);
        Task UpdateAsync(Vehicle vehicle);
        Task DeleteAsync(int id);
        Task<bool> IsExistLicensePlate(string licensePlate);
    }
}
