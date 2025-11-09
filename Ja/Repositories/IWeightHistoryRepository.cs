using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ja.Database.Entities;

namespace Ja.Repositories
{
    /// <summary>
    /// Interfejs repozytorium historii wagi
    /// </summary>
    public interface IWeightHistoryRepository
    {
        Task<List<WeightHistory>> GetAllForUserAsync(int userId);
        Task<WeightHistory?> GetLatestForUserAsync(int userId);
        Task<WeightHistory> AddAsync(WeightHistory weightHistory);
        Task UpdateAsync(WeightHistory weightHistory);
        Task DeleteAsync(int id);
        Task<WeightHistory?> GetByIdAsync(int id);
        Task<List<WeightHistory>> GetForDateRangeAsync(int userId, DateTime startDate, DateTime endDate);
    }
}
