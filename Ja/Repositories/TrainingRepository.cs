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
    /// Repozytorium dostępu do danych treningów
    /// </summary>
    public class TrainingRepository : ITrainingRepository
    {
        private readonly JaDbContext _context;

        public TrainingRepository(JaDbContext context)
        {
            _context = context;
        }

        public async Task<Training?> GetTrainingByIdAsync(int id)
        {
            return await _context.Trainings
                .Include(t => t.Intervals)
                .Include(t => t.Records)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<List<Training>> GetAllTrainingsAsync(int userId, int skip = 0, int take = 50)
        {
            return await _context.Trainings
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.TrainingDate)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task<List<Training>> GetTrainingsByDateRangeAsync(int userId, DateTime startDate, DateTime endDate)
        {
            return await _context.Trainings
                .Where(t => t.UserId == userId && t.TrainingDate >= startDate && t.TrainingDate <= endDate)
                .OrderBy(t => t.TrainingDate)
                .ToListAsync();
        }

        public async Task<Training> InsertTrainingAsync(Training training)
        {
            training.CreatedAt = DateTime.UtcNow;
            training.UpdatedAt = DateTime.UtcNow;

            await _context.Trainings.AddAsync(training);
            await _context.SaveChangesAsync();

            return training;
        }

        public async Task UpdateTrainingAsync(Training training)
        {
            training.UpdatedAt = DateTime.UtcNow;
            _context.Trainings.Update(training);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteTrainingAsync(int id)
        {
            var training = await _context.Trainings.FindAsync(id);
            if (training != null)
            {
                _context.Trainings.Remove(training);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> DuplicateExistsAsync(int userId, DateTime date, int durationSeconds)
        {
            // Tolerancja 5 sekund dla porównania czasu trwania
            return await _context.Trainings
                .AnyAsync(t => t.UserId == userId
                    && t.TrainingDate.Date == date.Date
                    && Math.Abs(t.DurationSeconds - durationSeconds) < 5);
        }

        public async Task<Dictionary<string, object>> GetWeeklySummaryAsync(int userId, DateTime weekStart)
        {
            var weekEnd = weekStart.AddDays(7);

            var trainings = await _context.Trainings
                .Where(t => t.UserId == userId && t.TrainingDate >= weekStart && t.TrainingDate < weekEnd)
                .ToListAsync();

            return new Dictionary<string, object>
            {
                ["TotalTSS"] = trainings.Sum(t => t.TSS ?? 0),
                ["TrainingCount"] = trainings.Count,
                ["TotalDuration"] = trainings.Sum(t => t.DurationSeconds),
                ["TotalDistance"] = trainings.Sum(t => t.DistanceMeters ?? 0),
                ["AvgPower"] = trainings.Any() ? trainings.Average(t => t.AvgPower ?? 0) : 0,
                ["AvgHeartRate"] = trainings.Any() ? trainings.Average(t => t.AvgHeartRate ?? 0) : 0
            };
        }
    }
}
