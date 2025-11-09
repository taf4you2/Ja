using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ja.Database.Entities;

namespace Ja.Repositories
{
    /// <summary>
    /// Interfejs repozytorium treningów
    /// </summary>
    public interface ITrainingRepository
    {
        Task<Training?> GetTrainingByIdAsync(int id);
        Task<List<Training>> GetAllTrainingsAsync(int userId, int skip = 0, int take = 50);
        Task<List<Training>> GetTrainingsByDateRangeAsync(int userId, DateTime startDate, DateTime endDate);
        Task<Training> InsertTrainingAsync(Training training);
        Task UpdateTrainingAsync(Training training);
        Task DeleteTrainingAsync(int id);
        Task<bool> DuplicateExistsAsync(int userId, DateTime date, int durationSeconds);
        Task<Dictionary<string, object>> GetWeeklySummaryAsync(int userId, DateTime weekStart);
    }
}
