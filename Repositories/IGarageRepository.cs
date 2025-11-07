using BusinessObjects.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public interface IGarageRepository
    {
        Task<Garage> GetAsync();
        Task AddAsync(Garage garage);
        Task UpdateAsync(Garage garage);
    }
}
