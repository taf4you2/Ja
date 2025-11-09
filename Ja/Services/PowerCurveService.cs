using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ja.Database;
using Ja.Database.Entities;

namespace Ja.Services
{
    /// <summary>
    /// Serwis do obliczania power curve (mean maximal power) i wykrywania rekordów
    /// Implementacja zgodna ze specyfikacją JA Training
    /// </summary>
    public class PowerCurveService
    {
        private readonly JaDbContext? _context;

        // Standardowe przedziały czasowe dla power curve (w sekundach)
        private readonly int[] _standardDurations = new[]
        {
            5, 10, 15, 30,           // Sprinty
            60, 120, 180, 300,       // Krótkie interwały (1-5 min)
            480, 600, 1200,          // Średnie interwały (8-20 min)
            1800, 3600, 5400, 7200   // Długie wysiłki (30-120 min)
        };

        public PowerCurveService(JaDbContext? context)
        {
            _context = context;
        }

        /// <summary>
        /// Oblicza power curve dla pojedynczego treningu
        /// Algorytm sliding window dla każdego przedziału czasowego
        /// </summary>
        public Dictionary<int, double> CalculatePowerCurve(double[] powerData)
        {
            var results = new Dictionary<int, double>();

            foreach (var duration in _standardDurations)
            {
                if (duration > powerData.Length)
                    continue;

                var maxAvgPower = CalculateMaxAveragePower(powerData, duration);
                results[duration] = maxAvgPower;
            }

            return results;
        }

        /// <summary>
        /// Oblicza maksymalną średnią moc dla danego okna czasowego (sliding window)
        /// </summary>
        private double CalculateMaxAveragePower(double[] powerData, int windowSize)
        {
            if (powerData.Length < windowSize)
                return 0;

            double maxAvg = 0;

            // Sliding window przez wszystkie dane
            for (int i = 0; i <= powerData.Length - windowSize; i++)
            {
                double sum = 0;
                for (int j = i; j < i + windowSize; j++)
                {
                    sum += powerData[j];
                }

                double avg = sum / windowSize;
                if (avg > maxAvg)
                    maxAvg = avg;
            }

            return maxAvg;
        }

        /// <summary>
        /// Zapisuje wyniki power curve dla treningu do bazy danych
        /// </summary>
        public async Task SavePowerCurveForTrainingAsync(int trainingId, Dictionary<int, double> powerCurve)
        {
            if (_context == null)
                throw new InvalidOperationException("DbContext is required for database operations");

            var records = powerCurve.Select(kvp => new TrainingRecord
            {
                TrainingId = trainingId,
                DurationSeconds = kvp.Key,
                PowerWatts = Math.Round(kvp.Value, 2),
                CreatedAt = DateTime.UtcNow
            }).ToList();

            await _context.TrainingRecords.AddRangeAsync(records);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Wykrywa i aktualizuje rekordy osobiste użytkownika
        /// </summary>
        public async Task DetectAndUpdatePersonalRecordsAsync(int userId, int trainingId, Dictionary<int, double> powerCurve, double? weight = null)
        {
            if (_context == null)
                throw new InvalidOperationException("DbContext is required for database operations");

            foreach (var kvp in powerCurve)
            {
                var duration = kvp.Key;
                var powerValue = kvp.Value;
                var recordType = $"power_{duration}s";

                // Sprawdź czy to rekord
                var existingRecord = await _context.PersonalRecords
                    .Where(pr => pr.UserId == userId && pr.RecordType == recordType)
                    .FirstOrDefaultAsync();

                if (existingRecord == null || powerValue > existingRecord.Value)
                {
                    // Nowy rekord!
                    var newRecord = new PersonalRecord
                    {
                        UserId = userId,
                        TrainingId = trainingId,
                        RecordType = recordType,
                        Value = Math.Round(powerValue, 2),
                        SecondaryValue = weight.HasValue ? Math.Round(powerValue / weight.Value, 2) : null, // W/kg
                        AchievedAt = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow
                    };

                    if (existingRecord != null)
                    {
                        _context.PersonalRecords.Remove(existingRecord);
                    }

                    await _context.PersonalRecords.AddAsync(newRecord);
                }
            }

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Pobiera power curve dla użytkownika w określonym zakresie dat
        /// </summary>
        public async Task<Dictionary<int, PowerRecord>> GetPowerCurveForUserAsync(int userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            if (_context == null)
                throw new InvalidOperationException("DbContext is required for database operations");

            var query = _context.TrainingRecords
                .Include(tr => tr.Training)
                .Where(tr => tr.Training.UserId == userId);

            if (startDate.HasValue)
                query = query.Where(tr => tr.Training.TrainingDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(tr => tr.Training.TrainingDate <= endDate.Value);

            var records = await query.ToListAsync();

            // Grupuj po duration i znajdź maksimum dla każdego
            var result = records
                .GroupBy(r => r.DurationSeconds)
                .ToDictionary(
                    g => g.Key,
                    g =>
                    {
                        var best = g.OrderByDescending(r => r.PowerWatts).First();
                        return new PowerRecord
                        {
                            DurationSeconds = best.DurationSeconds,
                            PowerWatts = best.PowerWatts,
                            TrainingId = best.TrainingId,
                            Date = best.Training.TrainingDate
                        };
                    }
                );

            return result;
        }

        /// <summary>
        /// Pobiera wszystkie rekordy osobiste użytkownika
        /// </summary>
        public async Task<List<PersonalRecord>> GetAllPersonalRecordsAsync(int userId)
        {
            if (_context == null)
                throw new InvalidOperationException("DbContext is required for database operations");

            return await _context.PersonalRecords
                .Where(pr => pr.UserId == userId)
                .Include(pr => pr.Training)
                .OrderBy(pr => pr.RecordType)
                .ToListAsync();
        }

        /// <summary>
        /// Pobiera rekordy mocy dla określonego użytkownika
        /// </summary>
        public async Task<List<PersonalRecord>> GetPowerRecordsAsync(int userId)
        {
            if (_context == null)
                throw new InvalidOperationException("DbContext is required for database operations");

            return await _context.PersonalRecords
                .Where(pr => pr.UserId == userId && pr.RecordType.StartsWith("power_"))
                .Include(pr => pr.Training)
                .OrderBy(pr => pr.RecordType)
                .ToListAsync();
        }

        /// <summary>
        /// Formatuje nazwę rekordu dla wyświetlenia
        /// </summary>
        public string FormatRecordName(string recordType)
        {
            if (recordType.StartsWith("power_") && recordType.EndsWith("s"))
            {
                var durationStr = recordType.Replace("power_", "").Replace("s", "");
                if (int.TryParse(durationStr, out int seconds))
                {
                    if (seconds < 60)
                        return $"{seconds}s";
                    else if (seconds < 3600)
                        return $"{seconds / 60}min";
                    else
                        return $"{seconds / 3600}h";
                }
            }

            return recordType;
        }
    }

    /// <summary>
    /// Klasa reprezentująca rekord z power curve
    /// </summary>
    public class PowerRecord
    {
        public int DurationSeconds { get; set; }
        public double PowerWatts { get; set; }
        public int TrainingId { get; set; }
        public DateTime Date { get; set; }

        public string FormattedDuration
        {
            get
            {
                if (DurationSeconds < 60)
                    return $"{DurationSeconds}s";
                else if (DurationSeconds < 3600)
                    return $"{DurationSeconds / 60}min";
                else
                    return $"{DurationSeconds / 3600}h {(DurationSeconds % 3600) / 60}min";
            }
        }
    }
}
