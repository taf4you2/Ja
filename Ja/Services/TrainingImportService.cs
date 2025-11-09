using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Ja.Algorithms;
using Ja.Database;
using Ja.Database.Entities;
using Ja.Models;
using Ja.Repositories;

namespace Ja.Services
{
    /// <summary>
    /// Serwis orkiestrujący cały proces importu treningu do bazy danych
    /// Implementacja zgodna ze specyfikacją - workflow importu treningu (str. 1418-1432)
    /// </summary>
    public class TrainingImportService
    {
        private readonly JaDbContext _context;
        private readonly TrainingRepository _trainingRepository;
        private readonly UserRepository _userRepository;
        private readonly MetricsCalculationService _metricsService;
        private readonly PowerCurveService _powerCurveService;
        private readonly PMCService _pmcService;
        private readonly IntervalDetectionAlgorithm _intervalAlgorithm;

        public TrainingImportService(
            JaDbContext context,
            TrainingRepository trainingRepository,
            UserRepository userRepository,
            MetricsCalculationService metricsService,
            PowerCurveService powerCurveService,
            PMCService pmcService)
        {
            _context = context;
            _trainingRepository = trainingRepository;
            _userRepository = userRepository;
            _metricsService = metricsService;
            _powerCurveService = powerCurveService;
            _pmcService = pmcService;
            _intervalAlgorithm = new IntervalDetectionAlgorithm();
        }

        /// <summary>
        /// Kompletny import treningu do bazy danych
        /// Workflow zgodnie ze specyfikacją (krok po kroku):
        /// 1. Sprawdzenie duplikatów
        /// 2. Kopiowanie pliku FIT
        /// 3. Kalkulacja metryk
        /// 4. Wykrywanie interwałów
        /// 5. Wykrywanie rekordów
        /// 6. Zapis do bazy
        /// 7. Aktualizacja PMC
        /// </summary>
        public async Task<Training> ImportTrainingAsync(FitFileData fitData, int userId, string originalFilePath)
        {
            // KROK 1: Sprawdź czy trening już istnieje (duplikat)
            var isDuplicate = await _trainingRepository.DuplicateExistsAsync(
                userId,
                fitData.StartTime,
                (int)fitData.Duration.TotalSeconds
            );

            if (isDuplicate)
            {
                throw new InvalidOperationException("Ten trening już istnieje w bazie danych!");
            }

            // KROK 2: Kopiowanie pliku FIT do folderu z treningami (opcjonalnie)
            string? savedFilePath = null;
            if (!string.IsNullOrEmpty(originalFilePath) && File.Exists(originalFilePath))
            {
                var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var trainingsFolder = Path.Combine(documentsPath, "JaTraining", "Trainings");

                if (!Directory.Exists(trainingsFolder))
                {
                    Directory.CreateDirectory(trainingsFolder);
                }

                var fileName = Path.GetFileName(originalFilePath);
                savedFilePath = Path.Combine(trainingsFolder, $"{fitData.StartTime:yyyyMMdd_HHmmss}_{fileName}");

                if (!File.Exists(savedFilePath))
                {
                    File.Copy(originalFilePath, savedFilePath);
                }
            }

            // KROK 3: Kalkulacja wszystkich metryk (TSS, NP, IF, VI, Work)
            if (fitData.NormalizedPower == 0 && fitData.PowerData.Length > 0)
            {
                var metrics = _metricsService.CalculateAllMetrics(
                    fitData.PowerData,
                    (int)fitData.Duration.TotalSeconds,
                    fitData.Ftp
                );

                fitData.NormalizedPower = metrics.NormalizedPower;
                fitData.IntensityFactor = metrics.IntensityFactor;
                fitData.TSS = metrics.TSS;
                fitData.VariabilityIndex = metrics.VariabilityIndex;
                fitData.WorkKJ = metrics.WorkKJ;
                fitData.MaxPower = metrics.MaxPower;
            }

            // KROK 4: Wykrywanie interwałów algorytmem
            if (fitData.Intervals.Count == 0 && fitData.PowerData.Length > 0)
            {
                var (intervals, recoveries) = _intervalAlgorithm.DetectAllIntervals(
                    fitData.PowerData,
                    fitData.Ftp
                );

                fitData.Intervals = intervals.Select(s => new Models.TrainingInterval
                {
                    Start = s.Start,
                    End = s.End,
                    Duration = s.Duration,
                    AvgPower = s.AvgPower,
                    AvgPowerWatts = s.AvgPowerWatts,
                    Zone = s.Zone,
                    ZoneName = s.ZoneName,
                    Type = s.Type,
                    Slope = s.Slope
                }).ToList();

                fitData.RecoveryPeriods = recoveries.Select(s => new RecoveryPeriod
                {
                    Start = s.Start,
                    End = s.End,
                    Duration = s.Duration,
                    AvgPower = s.AvgPower,
                    AvgPowerWatts = s.AvgPowerWatts,
                    Zone = s.Zone,
                    ZoneName = s.ZoneName
                }).ToList();
            }

            // KROK 5: Wykrywanie rekordów (power curve)
            if (fitData.PowerCurve.Count == 0 && fitData.PowerData.Length > 0)
            {
                fitData.PowerCurve = _powerCurveService.CalculatePowerCurve(fitData.PowerData);
            }

            // KROK 6: Zapis do bazy danych - INSERT do Trainings
            var training = new Training
            {
                UserId = userId,
                FileName = fitData.FileName,
                FilePath = savedFilePath ?? originalFilePath,
                TrainingDate = fitData.StartTime.Date,
                StartTime = fitData.StartTime,
                EndTime = fitData.EndTime,
                DurationSeconds = (int)fitData.Duration.TotalSeconds,
                DistanceMeters = fitData.TotalDistance,
                AvgPower = fitData.AvgPower,
                MaxPower = fitData.MaxPower,
                NormalizedPower = fitData.NormalizedPower,
                AvgHeartRate = fitData.AvgHeartRate,
                MaxHeartRate = (int)fitData.MaxHeartRate,
                AvgCadence = fitData.AvgCadence,
                MaxCadence = (int)fitData.MaxCadence,
                AvgSpeed = fitData.AvgSpeed,
                ElevationGain = fitData.ElevationGain,
                ElevationLoss = fitData.ElevationLoss,
                TSS = fitData.TSS,
                IntensityFactor = fitData.IntensityFactor,
                VariabilityIndex = fitData.VariabilityIndex,
                WorkKJ = fitData.WorkKJ,
                FtpUsed = fitData.Ftp
            };

            var savedTraining = await _trainingRepository.InsertTrainingAsync(training);

            // KROK 7: INSERT wszystkich interwałów do Training_Intervals
            foreach (var interval in fitData.Intervals)
            {
                var dbInterval = new Database.Entities.TrainingInterval
                {
                    TrainingId = savedTraining.Id,
                    StartSecond = interval.Start,
                    EndSecond = interval.End,
                    DurationSeconds = interval.Duration,
                    AvgPowerPercent = interval.AvgPower,
                    AvgPowerWatts = interval.AvgPowerWatts,
                    Zone = interval.Zone,
                    ZoneName = interval.ZoneName,
                    IntervalType = interval.Type,
                    Slope = interval.Slope
                };
                await _context.TrainingIntervals.AddAsync(dbInterval);
            }

            // KROK 8: INSERT rekordów do Training_Records (power curve)
            await _powerCurveService.SavePowerCurveForTrainingAsync(
                savedTraining.Id,
                fitData.PowerCurve
            );

            // KROK 9: UPDATE Personal_Records jeśli pobity rekord
            var user = await _userRepository.GetUserByIdAsync(userId);
            await _powerCurveService.DetectAndUpdatePersonalRecordsAsync(
                userId,
                savedTraining.Id,
                fitData.PowerCurve,
                user?.Weight ?? fitData.Weight
            );

            await _context.SaveChangesAsync();

            // KROK 10: Aktualizacja PMC (przeliczenie CTL/ATL/TSB)
            await _pmcService.UpdatePMCForUserAsync(userId);

            return savedTraining;
        }

        /// <summary>
        /// Import treningu - wersja synchroniczna dla starszego kodu
        /// </summary>
        public Training ImportTraining(FitFileData fitData, int userId, string originalFilePath)
        {
            return ImportTrainingAsync(fitData, userId, originalFilePath).GetAwaiter().GetResult();
        }
    }
}
