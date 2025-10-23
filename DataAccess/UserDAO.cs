using BusinessObjects;
using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess
{
    public class UserDAO
    {
        private readonly MyDbContext _context;
        public UserDAO(MyDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<User>> GetAll()
        {
            return await _context.User
                .Where(u => u.IsActive)
                .ToListAsync();
        }

        public async Task<(IEnumerable<User> items, int total)> GetAll(
            int page,
            int pageSize,
            string? sortBy = null,
            bool isAsc = true)
        {
            var query = _context.User.Where(u => u.IsActive);
            // Get total before pagination
            var total = await query.CountAsync();
            // Apply sorting
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                query = ApplySorting(query, sortBy, isAsc);
            }
            else
            {
                // Default sorting by CreatedAt descending
                query = query.OrderByDescending(u => u.CreatedAt);
            }
            // Apply pagination
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            return (items, total);
        }

        private IQueryable<User> ApplySorting(IQueryable<User> query, string sortBy, bool isAsc)
        {
            // Normalize sortBy to lowercase for case-insensitive comparison
            var sortByLower = sortBy.ToLower();
            return sortByLower switch
            {
                "username" => isAsc ? query.OrderBy(u => u.FullName) : query.OrderByDescending(u => u.FullName),
                "email" => isAsc ? query.OrderBy(u => u.Email) : query.OrderByDescending(u => u.Email),
                "role" => isAsc ? query.OrderBy(u => u.UserRole) : query.OrderByDescending(u => u.UserRole),
                "status" => isAsc ? query.OrderBy(u => u.UserStatus) : query.OrderByDescending(u => u.UserStatus),
                "createdat" => isAsc ? query.OrderBy(u => u.CreatedAt) : query.OrderByDescending(u => u.CreatedAt),
                _ => query.OrderByDescending(u => u.CreatedAt), // Default sorting
            };
        }

        public async Task<User?> GetById(int id)
        {
            return await _context.User.FirstOrDefaultAsync(u => u.Id == id && u.IsActive);
        }
        public async Task<User?> GetByEmail(string email)
        {
            return await _context.User.FirstOrDefaultAsync(u => email.ToLower().Equals(u.Email.ToLower()));
        }

        public async Task<User?> GetByRefreshTokenAsync(string refreshToken)
        {
            return await _context.User.FirstOrDefaultAsync(u => u.RefreshToken != null && u.RefreshToken.Equals(refreshToken));
        }

        public async Task Add(User user)
        {
            await _context.User.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task Update(User user)
        {
            _context.User.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(User user)
        {
            user.IsActive = false;
            _context.User.Update(user);
            await _context.SaveChangesAsync();
        }
    }
}
