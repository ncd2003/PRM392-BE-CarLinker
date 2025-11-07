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

        public async Task<IEnumerable<Vehicle>> GetAllByUserIdAsync(int userId)
        {
            return await _vehicleDAO.GetAllByUserId(userId);
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
            if (await _vehicleDAO.IsExistByLicensePlate(vehicle.LicensePlate))
            {
                throw new ArgumentException("License plate already existed", nameof(vehicle.LicensePlate));
            }
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
            var vehicleDB = await _vehicleDAO.GetById(vehicle.Id);
            if (vehicleDB == null)
            {
                throw new KeyNotFoundException($"Vehicle with ID {vehicle.Id} not found");
            }
            if (vehicleDB.Id != vehicle.Id && await _vehicleDAO.IsExistByLicensePlate(vehicle.LicensePlate))
            {
                throw new ArgumentException("License plate already existed", nameof(vehicle.LicensePlate));
            }

            // Get existing vehicle


            // Update properties
            vehicleDB.LicensePlate = vehicle.LicensePlate;
            vehicleDB.FuelType = vehicle.FuelType;
            vehicleDB.TransmissionType = vehicle.TransmissionType;
            vehicleDB.Brand = vehicle.Brand;
            vehicleDB.Model = vehicle.Model;
            vehicleDB.Year = vehicle.Year;
            vehicleDB.Image = vehicle.Image;

            await _vehicleDAO.Update(vehicleDB);
        }

        public async Task DeleteAsync(int id)
        {
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

        public async Task<bool> IsExistLicensePlate(string licensePlate)
        {
            return await _vehicleDAO.IsExistByLicensePlate(licensePlate);
        }
    }
}
