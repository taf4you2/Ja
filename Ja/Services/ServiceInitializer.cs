using Ja.Database;
using Ja.Repositories;
using Ja.Services;
using System;

namespace Ja.Services
{
    /// <summary>
    /// Inicjalizacja i zarządzanie serwisami aplikacji
    /// Singleton pattern dla łatwego dostępu do serwisów w całej aplikacji
    /// </summary>
    public static class ServiceInitializer
    {
        private static JaDbContext? _dbContext;
        private static UserRepository? _userRepository;
        private static TrainingRepository? _trainingRepository;
        private static MetricsCalculationService? _metricsService;
        private static PowerCurveService? _powerCurveService;
        private static PMCService? _pmcService;
        private static TrainingImportService? _importService;

        /// <summary>
        /// Kontekst bazy danych (DbContext)
        /// </summary>
        public static JaDbContext DbContext
        {
            get
            {
                if (_dbContext == null)
                {
                    _dbContext = new JaDbContext();
                    // Upewniamy się, że baza danych jest utworzona
                    _dbContext.Database.EnsureCreated();
                }
                return _dbContext;
            }
        }

        /// <summary>
        /// Repozytorium użytkowników
        /// </summary>
        public static UserRepository UserRepository
        {
            get
            {
                _userRepository ??= new UserRepository(DbContext);
                return _userRepository;
            }
        }

        /// <summary>
        /// Repozytorium treningów
        /// </summary>
        public static TrainingRepository TrainingRepository
        {
            get
            {
                _trainingRepository ??= new TrainingRepository(DbContext);
                return _trainingRepository;
            }
        }

        /// <summary>
        /// Serwis obliczania metryk (TSS, NP, IF, VI, Work)
        /// </summary>
        public static MetricsCalculationService MetricsService
        {
            get
            {
                _metricsService ??= new MetricsCalculationService();
                return _metricsService;
            }
        }

        /// <summary>
        /// Serwis power curve i rekordów osobistych
        /// </summary>
        public static PowerCurveService PowerCurveService
        {
            get
            {
                _powerCurveService ??= new PowerCurveService(DbContext);
                return _powerCurveService;
            }
        }

        /// <summary>
        /// Serwis Performance Management Chart (CTL/ATL/TSB)
        /// </summary>
        public static PMCService PMCService
        {
            get
            {
                _pmcService ??= new PMCService(DbContext);
                return _pmcService;
            }
        }

        /// <summary>
        /// Serwis importu treningów (orkiestruje cały proces)
        /// </summary>
        public static TrainingImportService ImportService
        {
            get
            {
                _importService ??= new TrainingImportService(
                    DbContext,
                    TrainingRepository,
                    UserRepository,
                    MetricsService,
                    PowerCurveService,
                    PMCService
                );
                return _importService;
            }
        }

        /// <summary>
        /// Czyszczenie zasobów przy zamykaniu aplikacji
        /// Powinno być wywołane w App.OnExit()
        /// </summary>
        public static void Cleanup()
        {
            _dbContext?.Dispose();
            _dbContext = null;
            _userRepository = null;
            _trainingRepository = null;
            _metricsService = null;
            _powerCurveService = null;
            _pmcService = null;
            _importService = null;
        }
    }
}
