using System;
using System.Linq;
using System.Threading.Tasks;
using Ja.Database;
using Ja.Database.Entities;
using Ja.Repositories;

namespace Ja.Services
{
    /// <summary>
    /// Serwis inicjalizacji danych aplikacji przy pierwszym uruchomieniu
    /// </summary>
    public class DataInitializationService
    {
        private readonly JaDbContext _context;
        private readonly UserRepository _userRepository;
        private readonly FTPHistoryRepository _ftpHistoryRepository;
        private readonly WeightHistoryRepository _weightHistoryRepository;

        public DataInitializationService(
            JaDbContext context,
            UserRepository userRepository,
            FTPHistoryRepository ftpHistoryRepository,
            WeightHistoryRepository weightHistoryRepository)
        {
            _context = context;
            _userRepository = userRepository;
            _ftpHistoryRepository = ftpHistoryRepository;
            _weightHistoryRepository = weightHistoryRepository;
        }

        /// <summary>
        /// Inicjalizuje domyślnego użytkownika z podstawowymi danymi (wersja synchroniczna)
        /// Wywoływane przy pierwszym uruchomieniu aplikacji
        /// </summary>
        public void InitializeDefaultUserIfNeeded()
        {
            try
            {
                // Sprawdź czy istnieje jakikolwiek użytkownik
                var hasUsers = _context.Users.Any();

                if (!hasUsers)
                {
                    System.Diagnostics.Debug.WriteLine("=== INICJALIZACJA DOMYŚLNEGO UŻYTKOWNIKA ===");

                    // Tworzenie domyślnego użytkownika
                    var defaultUser = new User
                    {
                        Name = "Jan",
                        Surname = "Kowalski",
                        DateOfBirth = new DateTime(1990, 1, 1),
                        Gender = "Male",
                        Weight = 75.0,
                        Height = 180.0,
                        RestingHeartRate = 55,
                        MaxHeartRate = 185,
                        ProfilePicturePath = null,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.Users.Add(defaultUser);
                    _context.SaveChanges();

                    System.Diagnostics.Debug.WriteLine($"Utworzono użytkownika: {defaultUser.Name} {defaultUser.Surname} (ID: {defaultUser.Id})");

                    // Inicjalizuj domyślne strefy mocy (Coggan 7 stref)
                    JaDbContext.InitializeDefaultPowerZones(_context, defaultUser.Id);
                    System.Diagnostics.Debug.WriteLine("Utworzono domyślne strefy mocy");

                    // Inicjalizuj domyślne strefy tętna
                    JaDbContext.InitializeDefaultHeartRateZones(_context, defaultUser.Id);
                    System.Diagnostics.Debug.WriteLine("Utworzono domyślne strefy tętna");

                    // Dodaj podstawowy wpis FTP (synchronicznie)
                    var initialFTP = new FTPHistory
                    {
                        UserId = defaultUser.Id,
                        FtpValue = 250.0,
                        WeightAtTest = 75.0,
                        TestDate = DateTime.Today.AddMonths(-1),
                        Source = "manual",
                        Notes = "Domyślne FTP",
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.FTPHistory.Add(initialFTP);
                    _context.SaveChanges();
                    System.Diagnostics.Debug.WriteLine($"Dodano domyślne FTP: {initialFTP.FtpValue} W");

                    // Dodaj podstawowy wpis wagi (synchronicznie)
                    var initialWeight = new WeightHistory
                    {
                        UserId = defaultUser.Id,
                        Weight = 75.0,
                        MeasurementDate = DateTime.Today.AddMonths(-1),
                        Notes = "Domyślna waga",
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.WeightHistory.Add(initialWeight);
                    _context.SaveChanges();
                    System.Diagnostics.Debug.WriteLine($"Dodano domyślną wagę: {initialWeight.Weight} kg");

                    // Dodaj domyślne ustawienia aplikacji
                    InitializeDefaultSettings(defaultUser.Id);

                    System.Diagnostics.Debug.WriteLine("=== INICJALIZACJA ZAKOŃCZONA ===");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Użytkownicy już istnieją w bazie, pomijam inicjalizację");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"BŁĄD PODCZAS INICJALIZACJI: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        /// <summary>
        /// Inicjalizuje domyślnego użytkownika z podstawowymi danymi (wersja async)
        /// Wywoływane przy pierwszym uruchomieniu aplikacji
        /// </summary>
        public async Task InitializeDefaultUserIfNeededAsync()
        {
            try
            {
                // Sprawdź czy istnieje jakikolwiek użytkownik
                var hasUsers = _context.Users.Any();

                if (!hasUsers)
                {
                    System.Diagnostics.Debug.WriteLine("=== INICJALIZACJA DOMYŚLNEGO UŻYTKOWNIKA ===");

                    // Tworzenie domyślnego użytkownika
                    var defaultUser = new User
                    {
                        Name = "Jan",
                        Surname = "Kowalski",
                        DateOfBirth = new DateTime(1990, 1, 1),
                        Gender = "Male",
                        Weight = 75.0,
                        Height = 180.0,
                        RestingHeartRate = 55,
                        MaxHeartRate = 185,
                        ProfilePicturePath = null,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    await _context.Users.AddAsync(defaultUser);
                    await _context.SaveChangesAsync();

                    System.Diagnostics.Debug.WriteLine($"Utworzono użytkownika: {defaultUser.Name} {defaultUser.Surname} (ID: {defaultUser.Id})");

                    // Inicjalizuj domyślne strefy mocy (Coggan 7 stref)
                    JaDbContext.InitializeDefaultPowerZones(_context, defaultUser.Id);
                    System.Diagnostics.Debug.WriteLine("Utworzono domyślne strefy mocy");

                    // Inicjalizuj domyślne strefy tętna
                    JaDbContext.InitializeDefaultHeartRateZones(_context, defaultUser.Id);
                    System.Diagnostics.Debug.WriteLine("Utworzono domyślne strefy tętna");

                    // Dodaj podstawowy wpis FTP
                    var initialFTP = new FTPHistory
                    {
                        UserId = defaultUser.Id,
                        FtpValue = 250.0,
                        WeightAtTest = 75.0,
                        TestDate = DateTime.Today.AddMonths(-1),
                        Source = "manual",
                        Notes = "Domyślne FTP",
                        CreatedAt = DateTime.UtcNow
                    };

                    await _ftpHistoryRepository.AddAsync(initialFTP);
                    System.Diagnostics.Debug.WriteLine($"Dodano domyślne FTP: {initialFTP.FtpValue} W");

                    // Dodaj podstawowy wpis wagi
                    var initialWeight = new WeightHistory
                    {
                        UserId = defaultUser.Id,
                        Weight = 75.0,
                        MeasurementDate = DateTime.Today.AddMonths(-1),
                        Notes = "Domyślna waga",
                        CreatedAt = DateTime.UtcNow
                    };

                    await _weightHistoryRepository.AddAsync(initialWeight);
                    System.Diagnostics.Debug.WriteLine($"Dodano domyślną wagę: {initialWeight.Weight} kg");

                    // Dodaj domyślne ustawienia aplikacji
                    await InitializeDefaultSettingsAsync(defaultUser.Id);

                    System.Diagnostics.Debug.WriteLine("=== INICJALIZACJA ZAKOŃCZONA ===");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Użytkownicy już istnieją w bazie, pomijam inicjalizację");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"BŁĄD PODCZAS INICJALIZACJI: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        /// <summary>
        /// Inicjalizuje domyślne ustawienia aplikacji (synchroniczna)
        /// </summary>
        private void InitializeDefaultSettings(int userId)
        {
            var defaultSettings = new[]
            {
                new Setting { UserId = userId, SettingKey = "pmc_ctl_days", SettingValue = "42" },
                new Setting { UserId = userId, SettingKey = "pmc_atl_days", SettingValue = "7" },
                new Setting { UserId = userId, SettingKey = "power_zone_model", SettingValue = "coggan_7" },
                new Setting { UserId = userId, SettingKey = "interval_sensitivity", SettingValue = "medium" },
                new Setting { UserId = userId, SettingKey = "chart_smoothing_alpha", SettingValue = "0.3" },
                new Setting { UserId = userId, SettingKey = "auto_analyze_on_import", SettingValue = "true" },
                new Setting { UserId = userId, SettingKey = "detect_duplicates", SettingValue = "true" },
                new Setting { UserId = userId, SettingKey = "remove_outliers", SettingValue = "true" },
                new Setting { UserId = userId, SettingKey = "z_score_threshold", SettingValue = "3.0" }
            };

            _context.Settings.AddRange(defaultSettings);
            _context.SaveChanges();

            System.Diagnostics.Debug.WriteLine($"Dodano {defaultSettings.Length} domyślnych ustawień");
        }

        /// <summary>
        /// Inicjalizuje domyślne ustawienia aplikacji (async)
        /// </summary>
        private async Task InitializeDefaultSettingsAsync(int userId)
        {
            var defaultSettings = new[]
            {
                new Setting { UserId = userId, SettingKey = "pmc_ctl_days", SettingValue = "42" },
                new Setting { UserId = userId, SettingKey = "pmc_atl_days", SettingValue = "7" },
                new Setting { UserId = userId, SettingKey = "power_zone_model", SettingValue = "coggan_7" },
                new Setting { UserId = userId, SettingKey = "interval_sensitivity", SettingValue = "medium" },
                new Setting { UserId = userId, SettingKey = "chart_smoothing_alpha", SettingValue = "0.3" },
                new Setting { UserId = userId, SettingKey = "auto_analyze_on_import", SettingValue = "true" },
                new Setting { UserId = userId, SettingKey = "detect_duplicates", SettingValue = "true" },
                new Setting { UserId = userId, SettingKey = "remove_outliers", SettingValue = "true" },
                new Setting { UserId = userId, SettingKey = "z_score_threshold", SettingValue = "3.0" }
            };

            await _context.Settings.AddRangeAsync(defaultSettings);
            await _context.SaveChangesAsync();

            System.Diagnostics.Debug.WriteLine($"Dodano {defaultSettings.Length} domyślnych ustawień");
        }

        /// <summary>
        /// Dodaje przykładowe dane treningowe dla demonstracji (opcjonalne)
        /// </summary>
        public async Task AddSampleTrainingsAsync(int userId)
        {
            // To można rozbudować w przyszłości o generowanie przykładowych treningów
            // Na razie zostawiamy pustą bazę, aby użytkownik mógł zaimportować własne pliki FIT
            await Task.CompletedTask;
        }
    }
}
