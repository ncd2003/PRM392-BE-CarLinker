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
    public class UserRepository : IUserRepository
    {
        private readonly UserDAO _userDAO;
        public UserRepository(UserDAO userDAO)
        {
            _userDAO = userDAO;
        }
        public async Task AddAsync(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user), "User cannot be null");
            }
            if (string.IsNullOrWhiteSpace(user.Email))
            {
                throw new ArgumentException("Email is required", nameof(user.Email));
            }
            await _userDAO.Add(user);
        }

        public async Task DeleteAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("ID must be greater than 0", nameof(id));
            }

            var userDB = await _userDAO.GetById(id);
            if (userDB == null)
            {
                throw new KeyNotFoundException($"User with ID {id} not found");
            }

            userDB.IsActive = false;
            await _userDAO.Delete(userDB);
        }

        public async Task<IEnumerable<User>> GetAllAsysnc()
        {
            return await _userDAO.GetAll();
        }

        public async Task<(IEnumerable<User> items, int total)> GetAllAsysnc(int page, int pageSize, string? sortBy = null, bool isAsc = true)
        {
            if (page <= 0)
                throw new ArgumentException("Page must be greater than 0", nameof(page));
            if (pageSize <= 0)
                throw new ArgumentException("Page size must be greater than 0", nameof(pageSize));
            if (pageSize > 100)
                throw new ArgumentException("Page size cannot exceed 100", nameof(pageSize));

            return await _userDAO.GetAll(page, pageSize, sortBy, isAsc);
        }

        public Task<User?> GetByEmailAsync(string email)
        {
            return _userDAO.GetByEmail(email);
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("ID must be greater than 0", nameof(id));
            }
            return await _userDAO.GetById(id);
        }

        public Task<User?> GetByRefreshTokenAsync(string refreshToken)
        {
            return _userDAO.GetByRefreshTokenAsync(refreshToken);
        }

        public async Task UpdateAsync(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user), "User cannot be null");
            }
            if (user.Id <= 0)
            {
                throw new ArgumentException("User ID must be greater than 0", nameof(user.Id));
            }

            var userDB = await _userDAO.GetById(user.Id);
            if (userDB == null)
            {
                throw new KeyNotFoundException($"User with ID {user.Id} not found");
            }

            await _userDAO.Update(user);
        }
    }
}
