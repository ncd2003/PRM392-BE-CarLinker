using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Models;


namespace Repositories
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAllAsysnc();
        Task<(IEnumerable<User> items, int total)> GetAllAsysnc(
            int page,
            int pageSize,
            string? sortBy = null,
            bool isAsc = true);

        Task<User?> GetByIdAsync(int id);
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByRefreshTokenAsync(string refreshToken);
        Task AddAsync(User user);
        Task UpdateAsync(User user);
        Task DeleteAsync(int id);
    }
}
