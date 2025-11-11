using BusinessObjects.Models;
using BusinessObjects.Models.DTOs.ServiceCategory;
using BusinessObjects.Models.DTOs.ServiceItem;
using DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public class ServiceItemRepository : IServiceItemRepository
    {
        private readonly ServiceItemDAO _serviceItemDAO;
        public ServiceItemRepository(ServiceItemDAO serviceItemDAO)
        {
            _serviceItemDAO = serviceItemDAO;
        }

        public async Task AddAsync(ServiceItem serviceItem)
        {
            if (serviceItem == null)
            {
                throw new ArgumentNullException(nameof(serviceItem), "ServiceItem cannot be null");
            }
            await _serviceItemDAO.Add(serviceItem);
        }

        public async Task DeleteAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("ID must be greater than 0", nameof(id));
            }

            // Get existing serviceItem
            var serviceItemDB = await _serviceItemDAO.GetById(id);
            if (serviceItemDB == null)
            {
                throw new KeyNotFoundException($"ServiceItem with ID {id} not found");
            }
            serviceItemDB.IsActive = false;
            await _serviceItemDAO.Delete(serviceItemDB);
        }

        public async Task<IEnumerable<ServiceItem>> GetAllAsync()
        {
            return await _serviceItemDAO.GetAll();
        }

        public async Task<(IEnumerable<ServiceItem> items, int total)> GetAllAsync(int page, int pageSize, string? sortBy = null, bool isAsc = true)
        {
            if (page <= 0)
                throw new ArgumentException("Page must be greater than 0", nameof(page));

            if (pageSize <= 0)
                throw new ArgumentException("Page size must be greater than 0", nameof(pageSize));

            if (pageSize > 100)
                throw new ArgumentException("Page size cannot exceed 100", nameof(pageSize));

            return await _serviceItemDAO.GetAll(page, pageSize, sortBy, isAsc);
        }

        public async Task<ServiceItem?> GetByIdAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("ID must be greater than 0", nameof(id));
            }

            return await _serviceItemDAO.GetById(id);
        }

        public async Task UpdateAsync(ServiceItem serviceItem)
        {
            if (serviceItem == null)
            {
                throw new ArgumentNullException(nameof(serviceItem), "ServiceItem cannot be null");
            }

            if (serviceItem.Id <= 0)
            {
                throw new ArgumentException("Invalid serviceItem ID", nameof(serviceItem.Id));
            }

            var serviceItemDB = await _serviceItemDAO.GetById(serviceItem.Id);
            if (serviceItemDB == null)
            {
                throw new KeyNotFoundException($"ServiceItem with ID {serviceItem.Id} not found");
            }

            serviceItemDB.Name = serviceItem.Name;

            await _serviceItemDAO.Update(serviceItemDB);
        }

        // ✅ THÊM: Get nhiều ServiceItems theo IDs
        public async Task<List<ServiceItem>> GetByIdsAsync(List<int> serviceItemIds)
        {
            if (serviceItemIds == null || !serviceItemIds.Any())
            {
                return new List<ServiceItem>();
            }

            return await _serviceItemDAO.GetByIds(serviceItemIds);
        }

        // ✅ THÊM: Tính tổng giá theo IDs
        public async Task<decimal> TotalPriceByIdsAsync(List<int> serviceItemIds)
        {
            if (serviceItemIds == null || !serviceItemIds.Any())
            {
                return 0;
            }

            return await _serviceItemDAO.TotalPriceByIds(serviceItemIds);
        }
    }
}
