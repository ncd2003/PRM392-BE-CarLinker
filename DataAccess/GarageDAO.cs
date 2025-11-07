using BusinessObjects;
using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public class GarageDAO
    {
        private readonly MyDbContext _context;

        public GarageDAO(MyDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Garage> Get()
        {
            return await _context.Garage.FirstAsync();
        }

        public async Task Add(Garage garage)
        {
            await _context.Garage.AddAsync(garage);
            await _context.SaveChangesAsync();
        }

        public async Task Update(Garage garage)
        {
            _context.Garage.Update(garage);
            await _context.SaveChangesAsync();
        }
    }
}
