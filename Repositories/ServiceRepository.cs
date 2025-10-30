using BusinessObjects.Models;
using BusinessObjects.Models.DTOs.Vehicle;
using DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public class ServiceRepository : IServiceRepository
    {
        private readonly ServiceDAO _serviceDAO;
        public ServiceRepository(ServiceDAO serviceDAO)
        {
            _serviceDAO = serviceDAO;
        }

        public async Task AddAsync(Service service)
        {
            await _serviceDAO.Add(service);
        }

        public async Task DeleteAsync(int id)
        {
            // Validate ID
            if (id <= 0)
            {
                throw new ArgumentException("ID must be greater than 0", nameof(id));
            }
            var serviceDB = await _serviceDAO.GetById(id);
            if (serviceDB == null)
            {
                throw new KeyNotFoundException($"Service with ID {id} not found");
            }

            serviceDB.IsActive = false;
            await _serviceDAO.Delete(serviceDB);

        }

        public async Task<(IEnumerable<Service> items, int total)> GetAllAsync(int page, int pageSize, string? sortBy = null, bool isAsc = true)
        {
            return await _serviceDAO.GetAll(page, pageSize, sortBy, isAsc);
        }

        public Task<Service?> GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task UpdateAsync(int id, Service service)
        {
            // Validation
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service), "Service cannot be null");
            }
            var serviceDB = await _serviceDAO.GetById(id);
            if (serviceDB == null)
            {
                throw new KeyNotFoundException($"Service with ID {service.Id} not found");
            }
            serviceDB.Name = service.Name;
            serviceDB.PriceFrom = service.PriceFrom;
            serviceDB.PriceTo = service.PriceTo;

            await _serviceDAO.Update(serviceDB);

        }
    }
}
