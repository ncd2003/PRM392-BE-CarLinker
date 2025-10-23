using BusinessObjects.Models;
using DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public class VehicleRepository : IVehicleRepository
    {
        private readonly VehicleDAO _vehicleDAO;

        public VehicleRepository(VehicleDAO vehicleDAO)
        {
            _vehicleDAO = vehicleDAO ?? throw new ArgumentNullException(nameof(vehicleDAO));
        }

        public async Task<IEnumerable<Vehicle>> GetAllAsync()
        {
            return await _vehicleDAO.GetAll();
        }

        public async Task<(IEnumerable<Vehicle> items, int total)> GetAllAsync(
            int page,
            int pageSize,
            string? sortBy = null,
            bool isAsc = true)
        {
            // Validate pagination parameters
            if (page <= 0)
                throw new ArgumentException("Page must be greater than 0", nameof(page));

            if (pageSize <= 0)
                throw new ArgumentException("Page size must be greater than 0", nameof(pageSize));

            if (pageSize > 100)
                throw new ArgumentException("Page size cannot exceed 100", nameof(pageSize));

            return await _vehicleDAO.GetAll(page, pageSize, sortBy, isAsc);
        }

        public async Task<Vehicle?> GetByIdAsync(int id)
        {
            // Validate ID
            if (id <= 0)
            {
                throw new ArgumentException("ID must be greater than 0", nameof(id));
            }

            return await _vehicleDAO.GetById(id);
        }

        public async Task AddAsync(Vehicle vehicle)
        {
            // Validation
            if (vehicle == null)
            {
                throw new ArgumentNullException(nameof(vehicle), "Vehicle cannot be null");
            }

            if (string.IsNullOrWhiteSpace(vehicle.LicensePlate))
            {
                throw new ArgumentException("License plate is required", nameof(vehicle.LicensePlate));
            }

            // Set default values
            //vehicle.IsActive = true;
            User user = new User();
            user.Id = 1;
            vehicle.User = user; // Set a default user or handle accordingly
            await _vehicleDAO.Add(vehicle);
        }

        public async Task UpdateAsync(Vehicle vehicle)
        {
            // Validation
            if (vehicle == null)
            {
                throw new ArgumentNullException(nameof(vehicle), "Vehicle cannot be null");
            }

            if (vehicle.Id <= 0)
            {
                throw new ArgumentException("Invalid vehicle ID", nameof(vehicle.Id));
            }

            if (string.IsNullOrWhiteSpace(vehicle.LicensePlate))
            {
                throw new ArgumentException("License plate is required", nameof(vehicle.LicensePlate));
            }

            // Get existing vehicle
            var vehicleDB = await _vehicleDAO.GetById(vehicle.Id);
            if (vehicleDB == null)
            {
                throw new KeyNotFoundException($"Vehicle with ID {vehicle.Id} not found");
            }

            // Update properties
            vehicleDB.LicensePlate = vehicle.LicensePlate;
            vehicleDB.FuelType = vehicle.FuelType;
            vehicleDB.TransmissionType = vehicle.TransmissionType;  // ✅ Fixed: Added missing field
            vehicleDB.Brand = vehicle.Brand;
            vehicleDB.Model = vehicle.Model;
            vehicleDB.Year = vehicle.Year;

            await _vehicleDAO.Update(vehicleDB);
        }

        public async Task DeleteAsync(int id)
        {
            // Validate ID
            if (id <= 0)
            {
                throw new ArgumentException("ID must be greater than 0", nameof(id));
            }

            // Get existing vehicle
            var vehicleDB = await _vehicleDAO.GetById(id);
            if (vehicleDB == null)
            {
                throw new KeyNotFoundException($"Vehicle with ID {id} not found");
            }

            // Soft delete
            vehicleDB.IsActive = false;
            await _vehicleDAO.Delete(vehicleDB);
        }
    }
}
