using BusinessObjects.Models;

namespace Repositories
{
    public interface IGarageStaffRepository
    {
        Task<(IEnumerable<GarageStaff> items, int total)> GetAllByGarageIdAsync(
            int garageId,
            int page,
            int pageSize,
            string? sortBy = null,
            bool isAsc = true);

        Task<GarageStaff?> GetByIdAsync(int id);
        
        Task<GarageStaff?> GetByEmailAsync(string email);
        
        Task<GarageStaff?> GetByRefreshTokenAsync(string refreshToken);
        
        Task<IEnumerable<GarageStaff>> GetActiveStaffByGarageIdAsync(int garageId);
        
        Task AddAsync(GarageStaff staff);
        
        Task UpdateAsync(GarageStaff staff);
        
        Task DeleteAsync(int id);
        
        Task<bool> IsEmailExistsAsync(string email, int? excludeId = null);
    }
}
