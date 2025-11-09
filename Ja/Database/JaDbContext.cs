using Microsoft.EntityFrameworkCore;
using Ja.Database.Entities;
using System;
using System.IO;

namespace Ja.Database
{
    /// <summary>
    /// Kontekst bazy danych dla aplikacji JA Training
    /// </summary>
    public class JaDbContext : DbContext
    {
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<FTPHistory> FTPHistory { get; set; } = null!;
        public DbSet<WeightHistory> WeightHistory { get; set; } = null!;
        public DbSet<Training> Trainings { get; set; } = null!;
        public DbSet<TrainingInterval> TrainingIntervals { get; set; } = null!;
        public DbSet<TrainingRecord> TrainingRecords { get; set; } = null!;
        public DbSet<PersonalRecord> PersonalRecords { get; set; } = null!;
        public DbSet<PowerZone> PowerZones { get; set; } = null!;
        public DbSet<HeartRateZone> HeartRateZones { get; set; } = null!;
        public DbSet<Setting> Settings { get; set; } = null!;
        public DbSet<PMCData> PMCData { get; set; } = null!;

        public JaDbContext()
        {
        }

        public JaDbContext(DbContextOptions<JaDbContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Domyślna lokalizacja bazy danych
                // Próbujemy różne lokalizacje w kolejności preferencji
                string dbPath;

                // 1. Spróbuj LocalApplicationData (AppData/Local na Windows, ~/.local/share na Linux)
                var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                if (!string.IsNullOrEmpty(appDataPath) && Directory.Exists(Path.GetDirectoryName(appDataPath)))
                {
                    dbPath = Path.Combine(appDataPath, "JaTraining", "ja_training.db");
                }
                // 2. Jeśli LocalApplicationData nie istnieje, użyj katalogu domowego użytkownika
                else
                {
                    var homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                    if (string.IsNullOrEmpty(homePath))
                    {
                        // 3. W ostateczności użyj katalogu bieżącego
                        homePath = Directory.GetCurrentDirectory();
                    }
                    dbPath = Path.Combine(homePath, ".jatraining", "ja_training.db");
                }

                // Utwórz folder jeśli nie istnieje
                var directory = Path.GetDirectoryName(dbPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                optionsBuilder.UseSqlite(
                    $"Data Source={dbPath}",
                    options => options.CommandTimeout(30)
                );

                // Włącz szczegółowe logi w trybie Debug
#if DEBUG
                optionsBuilder.EnableSensitiveDataLogging();
                optionsBuilder.EnableDetailedErrors();
#endif
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Konfiguracja indeksów zgodnie ze specyfikacją
            modelBuilder.Entity<Training>()
                .HasIndex(t => new { t.UserId, t.TrainingDate })
                .HasDatabaseName("idx_trainings_user_date");

            modelBuilder.Entity<Training>()
                .HasIndex(t => t.TrainingDate)
                .HasDatabaseName("idx_trainings_date");

            modelBuilder.Entity<TrainingInterval>()
                .HasIndex(ti => ti.TrainingId)
                .HasDatabaseName("idx_intervals_training");

            modelBuilder.Entity<TrainingRecord>()
                .HasIndex(tr => tr.TrainingId)
                .HasDatabaseName("idx_records_training");

            modelBuilder.Entity<TrainingRecord>()
                .HasIndex(tr => tr.DurationSeconds)
                .HasDatabaseName("idx_records_duration");

            modelBuilder.Entity<PMCData>()
                .HasIndex(pmc => new { pmc.UserId, pmc.Date })
                .HasDatabaseName("idx_pmc_user_date");

            modelBuilder.Entity<PersonalRecord>()
                .HasIndex(pr => pr.RecordType)
                .HasDatabaseName("idx_personal_records_type");

            modelBuilder.Entity<Setting>()
                .HasIndex(s => new { s.UserId, s.SettingKey })
                .HasDatabaseName("idx_settings_user_key")
                .IsUnique();

            // Domyślne wartości dla stref mocy (model Coggan 7 stref)
            SeedPowerZonesData(modelBuilder);
        }

        /// <summary>
        /// Seed domyślnych stref mocy (zostanie użyty przy inicjalizacji dla nowego użytkownika)
        /// </summary>
        private void SeedPowerZonesData(ModelBuilder modelBuilder)
        {
            // Te dane będą dostępne jako wzorzec, ale nie będą automatycznie insertowane
            // Zostaną dodane przy tworzeniu pierwszego użytkownika
        }

        /// <summary>
        /// Inicjalizuje domyślne strefy mocy dla użytkownika (model Coggan)
        /// </summary>
        public static void InitializeDefaultPowerZones(JaDbContext context, int userId)
        {
            var zones = new[]
            {
                new PowerZone { UserId = userId, ZoneNumber = 1, ZoneName = "Recovery", MinPercent = 0, MaxPercent = 55, ColorHex = "#808080", MinDurationSeconds = 0 },
                new PowerZone { UserId = userId, ZoneNumber = 2, ZoneName = "Endurance", MinPercent = 55, MaxPercent = 75, ColorHex = "#4169E1", MinDurationSeconds = 0 },
                new PowerZone { UserId = userId, ZoneNumber = 3, ZoneName = "Tempo", MinPercent = 75, MaxPercent = 90, ColorHex = "#32CD32", MinDurationSeconds = 120 },
                new PowerZone { UserId = userId, ZoneNumber = 4, ZoneName = "Threshold", MinPercent = 90, MaxPercent = 105, ColorHex = "#FFD700", MinDurationSeconds = 60 },
                new PowerZone { UserId = userId, ZoneNumber = 5, ZoneName = "VO2max", MinPercent = 105, MaxPercent = 120, ColorHex = "#FF8C00", MinDurationSeconds = 30 },
                new PowerZone { UserId = userId, ZoneNumber = 6, ZoneName = "Anaerobic", MinPercent = 120, MaxPercent = 150, ColorHex = "#FF4500", MinDurationSeconds = 10 },
                new PowerZone { UserId = userId, ZoneNumber = 7, ZoneName = "Neuromuscular", MinPercent = 150, MaxPercent = 999, ColorHex = "#8B0000", MinDurationSeconds = 5 }
            };

            context.PowerZones.AddRange(zones);
            context.SaveChanges();
        }

        /// <summary>
        /// Inicjalizuje domyślne strefy tętna dla użytkownika
        /// </summary>
        public static void InitializeDefaultHeartRateZones(JaDbContext context, int userId)
        {
            var zones = new[]
            {
                new HeartRateZone { UserId = userId, ZoneNumber = 1, ZoneName = "Recovery", MinPercent = 0, MaxPercent = 60, ColorHex = "#808080" },
                new HeartRateZone { UserId = userId, ZoneNumber = 2, ZoneName = "Endurance", MinPercent = 60, MaxPercent = 70, ColorHex = "#4169E1" },
                new HeartRateZone { UserId = userId, ZoneNumber = 3, ZoneName = "Tempo", MinPercent = 70, MaxPercent = 80, ColorHex = "#32CD32" },
                new HeartRateZone { UserId = userId, ZoneNumber = 4, ZoneName = "Threshold", MinPercent = 80, MaxPercent = 90, ColorHex = "#FFD700" },
                new HeartRateZone { UserId = userId, ZoneNumber = 5, ZoneName = "VO2max", MinPercent = 90, MaxPercent = 100, ColorHex = "#FF4500" }
            };

            context.HeartRateZones.AddRange(zones);
            context.SaveChanges();
        }
    }
}
