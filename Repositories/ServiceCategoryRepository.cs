using BusinessObjects.Models;
using DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public class ServiceCategoryRepository : IServiceCategoryRepository
    {
        private readonly ServiceCategoryDAO _serviceCategoryDAO;

        public ServiceCategoryRepository(ServiceCategoryDAO serviceCategoryDAO)
        {
            _serviceCategoryDAO = serviceCategoryDAO ?? throw new ArgumentNullException(nameof(serviceCategoryDAO));
        }
        public async Task<IEnumerable<ServiceCategory>> GetAllAsync()
        {
            return await _serviceCategoryDAO.GetAll();
        }

        public async Task<(IEnumerable<ServiceCategory> items, int total)> GetAllAsync(
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

            return await _serviceCategoryDAO.GetAll(page, pageSize, sortBy, isAsc);
        }

        public async Task<ServiceCategory?> GetByIdAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("ID must be greater than 0", nameof(id));
            }

            return await _serviceCategoryDAO.GetById(id);
        }

        public async Task AddAsync(ServiceCategory serviceCategory)
        {
            if (serviceCategory == null)
            {
                throw new ArgumentNullException(nameof(serviceCategory), "ServiceCategory cannot be null");
            }
            await _serviceCategoryDAO.Add(serviceCategory);
        }

        public async Task UpdateAsync(ServiceCategory serviceCategory)
        {
            if (serviceCategory == null)
            {
                throw new ArgumentNullException(nameof(serviceCategory), "ServiceCategory cannot be null");
            }

            if (serviceCategory.Id <= 0)
            {
                throw new ArgumentException("Invalid serviceCategory ID", nameof(serviceCategory.Id));
            }

            var serviceCategoryDB = await _serviceCategoryDAO.GetById(serviceCategory.Id);
            if (serviceCategoryDB == null)
            {
                throw new KeyNotFoundException($"ServiceCategory with ID {serviceCategory.Id} not found");
            }

            serviceCategoryDB.Name = serviceCategory.Name;

            await _serviceCategoryDAO.Update(serviceCategoryDB);
        }

        public async Task DeleteAsync(int id)
        {
            // Validate ID
            if (id <= 0)
            {
                throw new ArgumentException("ID must be greater than 0", nameof(id));
            }

            // Get existing serviceCategory
            var serviceCategoryDB = await _serviceCategoryDAO.GetById(id);
            if (serviceCategoryDB == null)
            {
                throw new KeyNotFoundException($"ServiceCategory with ID {id} not found");
            }

            // Soft delete
            serviceCategoryDB.IsActive = false;
            await _serviceCategoryDAO.Delete(serviceCategoryDB);
        }
    }
}
