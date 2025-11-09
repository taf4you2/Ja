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
        private static FTPHistoryRepository? _ftpHistoryRepository;
        private static WeightHistoryRepository? _weightHistoryRepository;
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
                    // Migracje są stosowane w App.xaml.cs OnStartup
                    // NIE używamy EnsureCreated() bo jest niekompatybilne z migracjami!
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
        /// Repozytorium historii FTP
        /// </summary>
        public static FTPHistoryRepository FTPHistoryRepository
        {
            get
            {
                _ftpHistoryRepository ??= new FTPHistoryRepository(DbContext);
                return _ftpHistoryRepository;
            }
        }

        /// <summary>
        /// Repozytorium historii wagi
        /// </summary>
        public static WeightHistoryRepository WeightHistoryRepository
        {
            get
            {
                _weightHistoryRepository ??= new WeightHistoryRepository(DbContext);
                return _weightHistoryRepository;
            }
        }

        /// <summary>
        /// Serwis inicjalizacji danych
        /// </summary>
        public static DataInitializationService DataInitializationService
        {
            get
            {
                return new DataInitializationService(
                    DbContext,
                    UserRepository,
                    FTPHistoryRepository,
                    WeightHistoryRepository
                );
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
            _ftpHistoryRepository = null;
            _weightHistoryRepository = null;
            _metricsService = null;
            _powerCurveService = null;
            _pmcService = null;
            _importService = null;
        }
    }
}
