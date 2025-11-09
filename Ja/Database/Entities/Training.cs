using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ja.Database.Entities
{
    /// <summary>
    /// Główna tabela treningów
    /// </summary>
    [Table("Trainings")]
    public class Training
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        /// <summary>
        /// Nazwa oryginalnego pliku
        /// </summary>
        [MaxLength(500)]
        [Required]
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// Ścieżka do zapisanego pliku FIT
        /// </summary>
        [MaxLength(1000)]
        public string? FilePath { get; set; }

        /// <summary>
        /// Data treningu
        /// </summary>
        [Required]
        public DateTime TrainingDate { get; set; }

        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        /// <summary>
        /// Czas trwania w sekundach
        /// </summary>
        [Required]
        public int DurationSeconds { get; set; }

        /// <summary>
        /// Dystans w metrach
        /// </summary>
        public double? DistanceMeters { get; set; }

        /// <summary>
        /// Średnia moc
        /// </summary>
        public double? AvgPower { get; set; }

        /// <summary>
        /// Normalized Power (NP)
        /// </summary>
        public double? NormalizedPower { get; set; }

        /// <summary>
        /// Maksymalna moc
        /// </summary>
        public double? MaxPower { get; set; }

        /// <summary>
        /// Średnie tętno
        /// </summary>
        public double? AvgHeartRate { get; set; }

        /// <summary>
        /// Maksymalne tętno
        /// </summary>
        public int? MaxHeartRate { get; set; }

        /// <summary>
        /// Średnia kadencja
        /// </summary>
        public double? AvgCadence { get; set; }

        /// <summary>
        /// Maksymalna kadencja
        /// </summary>
        public int? MaxCadence { get; set; }

        /// <summary>
        /// Średnia prędkość
        /// </summary>
        public double? AvgSpeed { get; set; }

        /// <summary>
        /// Przewyższenie (m)
        /// </summary>
        public double? ElevationGain { get; set; }

        /// <summary>
        /// Zjazdy (m)
        /// </summary>
        public double? ElevationLoss { get; set; }

        /// <summary>
        /// Training Stress Score
        /// </summary>
        public double? TSS { get; set; }

        /// <summary>
        /// Intensity Factor
        /// </summary>
        public double? IntensityFactor { get; set; }

        /// <summary>
        /// Variability Index
        /// </summary>
        public double? VariabilityIndex { get; set; }

        /// <summary>
        /// Praca w kJ
        /// </summary>
        public double? WorkKJ { get; set; }

        /// <summary>
        /// FTP użyte do obliczeń
        /// </summary>
        public double? FtpUsed { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }

        /// <summary>
        /// Tagi jako JSON array lub osobna tabela
        /// </summary>
        [MaxLength(500)]
        public string? Tags { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = null!;

        public virtual ICollection<TrainingInterval> Intervals { get; set; } = new List<TrainingInterval>();
        public virtual ICollection<TrainingRecord> Records { get; set; } = new List<TrainingRecord>();

        /// <summary>
        /// Formatowane czasy trwania
        /// </summary>
        [NotMapped]
        public string DurationFormatted
        {
            get
            {
                var ts = TimeSpan.FromSeconds(DurationSeconds);
                return $"{(int)ts.TotalHours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";
            }
        }
    }
}
