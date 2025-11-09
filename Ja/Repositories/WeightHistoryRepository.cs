using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ja.Database;
using Ja.Database.Entities;

namespace Ja.Repositories
{
    /// <summary>
    /// Repozytorium dostępu do danych historii wagi
    /// </summary>
    public class WeightHistoryRepository : IWeightHistoryRepository
    {
        private readonly JaDbContext _context;

        public WeightHistoryRepository(JaDbContext context)
        {
            _context = context;
        }

        public async Task<List<WeightHistory>> GetAllForUserAsync(int userId)
        {
            return await _context.WeightHistory
                .Where(w => w.UserId == userId)
                .OrderByDescending(w => w.MeasurementDate)
                .ToListAsync();
        }

        public async Task<WeightHistory?> GetLatestForUserAsync(int userId)
        {
            return await _context.WeightHistory
                .Where(w => w.UserId == userId)
                .OrderByDescending(w => w.MeasurementDate)
                .FirstOrDefaultAsync();
        }

        public async Task<WeightHistory> AddAsync(WeightHistory weightHistory)
        {
            weightHistory.CreatedAt = DateTime.UtcNow;
            await _context.WeightHistory.AddAsync(weightHistory);
            await _context.SaveChangesAsync();
            return weightHistory;
        }

        public async Task UpdateAsync(WeightHistory weightHistory)
        {
            _context.WeightHistory.Update(weightHistory);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var weightHistory = await _context.WeightHistory.FindAsync(id);
            if (weightHistory != null)
            {
                _context.WeightHistory.Remove(weightHistory);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<WeightHistory?> GetByIdAsync(int id)
        {
            return await _context.WeightHistory.FindAsync(id);
        }

        public async Task<List<WeightHistory>> GetForDateRangeAsync(int userId, DateTime startDate, DateTime endDate)
        {
            return await _context.WeightHistory
                .Where(w => w.UserId == userId &&
                           w.MeasurementDate >= startDate &&
                           w.MeasurementDate <= endDate)
                .OrderBy(w => w.MeasurementDate)
                .ToListAsync();
        }
    }
}
