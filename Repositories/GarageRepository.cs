using BusinessObjects.Models;
using BusinessObjects.Models.DTOs.ServiceRecord;
using BusinessObjects.Models.DTOs.Vehicle;
using DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public class GarageRepository : IGarageRepository
    {
        private readonly GarageDAO _garageDAO;
        public GarageRepository(GarageDAO garageDAO)
        {
            _garageDAO = garageDAO;
        }
        public async Task AddAsync(Garage garage)
        {
            await _garageDAO.Add(garage);
        }

        public async Task<(IEnumerable<Garage> items, int total)> GetAllAsync(int page, int pageSize, string? sortBy = null, bool isAsc = true)
        {
            // Validate pagination parameters
            if (page <= 0)
                throw new ArgumentException("Page must be greater than 0", nameof(page));

            if (pageSize <= 0)
                throw new ArgumentException("Page size must be greater than 0", nameof(pageSize));

            if (pageSize > 100)
                throw new ArgumentException("Page size cannot exceed 100", nameof(pageSize));

            return await _garageDAO.GetAll(page, pageSize, sortBy, isAsc);
        }


        public async Task UpdateAsync(Garage garage)
        {
            if (garage == null)
            {
                throw new ArgumentNullException(nameof(garage), "Garage cannot be null");
            }

            if (garage.Id <= 0)
            {
                throw new ArgumentException("Invalid Garage ID", nameof(garage.Id));
            }

            var garageDB = await _garageDAO.GetById(garage.Id);
            if (garageDB == null)
            {
                throw new KeyNotFoundException($"Garage with ID {garage.Id} not found");
            }
            garageDB.Name = garage.Name;
            garageDB.Email = garage.Email;
            garageDB.Description = garage.Description;
            garageDB.OperatingTime = garage.OperatingTime;
            garageDB.PhoneNumber = garage.PhoneNumber;
            garageDB.Latitude = garage.Latitude;
            garageDB.Longitude = garage.Longitude;

            await _garageDAO.Update(garageDB);
        }

        public async Task<Garage?> GetByIdAsync(int id)
        {
            return await _garageDAO.GetById(id);
        }


        public async Task DeleteAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("ID must be greater than 0", nameof(id));
            }

            // Get existing vehicle
            var garageDB = await _garageDAO.GetById(id);
            if (garageDB == null)
            {
                throw new KeyNotFoundException($"Garage with ID {id} not found");
            }

            // Soft delete
            garageDB.IsActive = false;
            await _garageDAO.Delete(garageDB);
        }

        public async Task<Garage?> GetByUserIdAsync(int userId)
        {
            return await _garageDAO.GetByUserId(userId);
        }

        public async Task UpdateGarageServiceItemAsync(Garage garage)
        {
            if (garage == null)
            {
                throw new ArgumentNullException(nameof(garage), "Garage cannot be null");
            }

            if (garage.Id <= 0)
            {
                throw new ArgumentException("Invalid Garage ID", nameof(garage.Id));
            }

            var garageDB = await _garageDAO.GetById(garage.Id);
            if (garageDB == null)
            {
                throw new KeyNotFoundException($"Garage with ID {garage.Id} not found");
            }
            garageDB.GarageServiceItems = garage.GarageServiceItems;
            await _garageDAO.Update(garageDB);
        }
    }
}
