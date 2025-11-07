using BusinessObjects.Models;
using BusinessObjects.Models.DTOs.ServiceRecord;
using DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public class ServiceRecordRepository : IServiceRecordRepository
    {
        private readonly ServiceRecordDAO _serviceRecordDAO;
        private readonly ServiceItemDAO _serviceItemDAO;

        public ServiceRecordRepository(ServiceRecordDAO serviceRecordDAO, ServiceItemDAO serviceItemDAO)
        {
            _serviceRecordDAO = serviceRecordDAO ?? throw new ArgumentNullException(nameof(serviceRecordDAO));
            _serviceItemDAO = serviceItemDAO;
        }
        public async Task AddAsync(ServiceRecord serviceRecord)
        {
            if (serviceRecord == null)
            {
                throw new ArgumentNullException(nameof(serviceRecord), "ServiceRecord cannot be null");
            }
            serviceRecord.TotalCost = _serviceItemDAO.TotalPriceByIds(serviceRecord.ServiceItems.Select(si => si.Id).ToList()).Result;
            await _serviceRecordDAO.Add(serviceRecord);
        }

        public async Task<(IEnumerable<ServiceRecord> items, int total)> GetAllAsync(int page, int pageSize, string? sortBy = null, bool isAsc = true)
        {
            // Validate pagination parameters
            if (page <= 0)
                throw new ArgumentException("Page must be greater than 0", nameof(page));

            if (pageSize <= 0)
                throw new ArgumentException("Page size must be greater than 0", nameof(pageSize));

            if (pageSize > 100)
                throw new ArgumentException("Page size cannot exceed 100", nameof(pageSize));

            return await _serviceRecordDAO.GetAll(page, pageSize, sortBy, isAsc);
        }

        public async Task<(IEnumerable<ServiceRecord> items, int total)> GetAllByUserIdAsync(int userId, int page, int pageSize, string? sortBy = null, bool isAsc = true)
        {
            // Validate pagination parameters
            if (page <= 0)
                throw new ArgumentException("Page must be greater than 0", nameof(page));

            if (pageSize <= 0)
                throw new ArgumentException("Page size must be greater than 0", nameof(pageSize));

            if (pageSize > 100)
                throw new ArgumentException("Page size cannot exceed 100", nameof(pageSize));
            return await _serviceRecordDAO.GetAllByUserId(userId, page, pageSize, sortBy, isAsc);
        }

        public async Task<ServiceRecord?> GetByIdAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("ID must be greater than 0", nameof(id));
            }

            return await _serviceRecordDAO.GetById(id);
        }
        public async Task UpdateAsync(ServiceRecord serviceRecord)
        {
            if (serviceRecord == null)
            {
                throw new ArgumentNullException(nameof(serviceRecord), "ServiceRecord cannot be null");
            }

            if (serviceRecord.Id <= 0)
            {
                throw new ArgumentException("Invalid ServiceRecord ID", nameof(serviceRecord.Id));
            }

            var serviceRecordDB = await _serviceRecordDAO.GetById(serviceRecord.Id);
            if (serviceRecordDB == null)
            {
                throw new KeyNotFoundException($"ServiceRecord with ID {serviceRecord.Id} not found");
            }

            serviceRecordDB.ServiceRecordStatus = serviceRecord.ServiceRecordStatus;
            if (serviceRecord.ServiceRecordStatus == BusinessObjects.Models.Type.ServiceRecordStatus.COMPLETED)
            {
                serviceRecordDB.EndTime = DateTime.Now;
            }

            await _serviceRecordDAO.Update(serviceRecordDB);
        }
    }
}
