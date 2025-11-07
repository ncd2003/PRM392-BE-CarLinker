using BusinessObjects.Models;
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
        public async Task AddAsync(Garage vehicle)
        {
            await _garageDAO.Add(vehicle);
        }

        public async Task<Garage> GetAsync()
        {
            return await _garageDAO.Get();
        }

        public async Task UpdateAsync(Garage garage)
        {
            await _garageDAO.Update(garage);
        }
    }
}
