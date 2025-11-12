using BusinessObjects.Models;
using DataAccess;

namespace Repositories
{
    public class GarageStaffRepository : IGarageStaffRepository
    {
        private readonly GarageStaffDAO _garageStaffDAO;

        public GarageStaffRepository(GarageStaffDAO garageStaffDAO)
        {
            _garageStaffDAO = garageStaffDAO ?? throw new ArgumentNullException(nameof(garageStaffDAO));
        }

        public async Task<(IEnumerable<GarageStaff> items, int total)> GetAllByGarageIdAsync(
            int garageId, 
            int page, 
            int pageSize, 
            string? sortBy = null, 
            bool isAsc = true)
        {
            if (page <= 0)
                throw new ArgumentException("Page must be greater than 0", nameof(page));

            if (pageSize <= 0)
                throw new ArgumentException("Page size must be greater than 0", nameof(pageSize));

            if (pageSize > 100)
                throw new ArgumentException("Page size cannot exceed 100", nameof(pageSize));

            return await _garageStaffDAO.GetAllByGarageId(garageId, page, pageSize, sortBy, isAsc);
        }

        public async Task<GarageStaff?> GetByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("ID must be greater than 0", nameof(id));

            return await _garageStaffDAO.GetById(id);
        }

        public async Task<GarageStaff?> GetByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be null or empty", nameof(email));

            return await _garageStaffDAO.GetByEmail(email);
        }

        public async Task<GarageStaff?> GetByRefreshTokenAsync(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                throw new ArgumentException("Refresh token cannot be null or empty", nameof(refreshToken));

            return await _garageStaffDAO.GetByRefreshToken(refreshToken);
        }

        public async Task<IEnumerable<GarageStaff>> GetActiveStaffByGarageIdAsync(int garageId)
        {
            if (garageId <= 0)
                throw new ArgumentException("Garage ID must be greater than 0", nameof(garageId));

            return await _garageStaffDAO.GetActiveStaffByGarageId(garageId);
        }

        public async Task AddAsync(GarageStaff staff)
        {
            if (staff == null)
                throw new ArgumentNullException(nameof(staff), "GarageStaff cannot be null");

            if (await IsEmailExistsAsync(staff.Email))
                throw new InvalidOperationException($"Email {staff.Email} already exists");

            await _garageStaffDAO.Add(staff);
        }

        public async Task UpdateAsync(GarageStaff staff)
        {
            if (staff == null)
                throw new ArgumentNullException(nameof(staff), "GarageStaff cannot be null");

            if (staff.Id <= 0)
                throw new ArgumentException("Invalid GarageStaff ID", nameof(staff.Id));

            var existingStaff = await _garageStaffDAO.GetById(staff.Id);
            if (existingStaff == null)
                throw new KeyNotFoundException($"GarageStaff with ID {staff.Id} not found");

            // Update fields
            existingStaff.FullName = staff.FullName;
            existingStaff.PhoneNumber = staff.PhoneNumber;
            existingStaff.Image = staff.Image;
            existingStaff.GarageRole = staff.GarageRole;
            existingStaff.UserStatus = staff.UserStatus;
            existingStaff.RefreshToken = staff.RefreshToken;
            existingStaff.RefreshTokenExpiryTime = staff.RefreshTokenExpiryTime;

            if (!string.IsNullOrEmpty(staff.PasswordHash))
            {
                existingStaff.PasswordHash = staff.PasswordHash;
            }

            await _garageStaffDAO.Update(existingStaff);
        }

        public async Task DeleteAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("ID must be greater than 0", nameof(id));

            var staff = await _garageStaffDAO.GetById(id);
            if (staff == null)
                throw new KeyNotFoundException($"GarageStaff with ID {id} not found");

            // Soft delete
            staff.IsActive = false;
            await _garageStaffDAO.Delete(staff);
        }

        public async Task<bool> IsEmailExistsAsync(string email, int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            return await _garageStaffDAO.IsEmailExists(email, excludeId);
        }
    }
}
