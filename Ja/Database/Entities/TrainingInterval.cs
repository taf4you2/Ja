using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ja.Database.Entities
{
    /// <summary>
    /// Wykryte interwały treningowe
    /// </summary>
    [Table("Training_Intervals")]
    public class TrainingInterval
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int TrainingId { get; set; }

        /// <summary>
        /// Początek interwału (sekunda)
        /// </summary>
        [Required]
        public int StartSecond { get; set; }

        /// <summary>
        /// Koniec interwału (sekunda)
        /// </summary>
        [Required]
        public int EndSecond { get; set; }

        /// <summary>
        /// Czas trwania w sekundach
        /// </summary>
        [Required]
        public int DurationSeconds { get; set; }

        /// <summary>
        /// Średnia moc w % FTP
        /// </summary>
        public double AvgPowerPercent { get; set; }

        /// <summary>
        /// Średnia moc w watach
        /// </summary>
        public double AvgPowerWatts { get; set; }

        /// <summary>
        /// Numer strefy (1-7)
        /// </summary>
        public int Zone { get; set; }

        /// <summary>
        /// Nazwa strefy (np. "Z5: VO2max")
        /// </summary>
        [MaxLength(100)]
        public string ZoneName { get; set; } = string.Empty;

        /// <summary>
        /// Typ interwału: jump, gradual, recovery
        /// </summary>
        [MaxLength(50)]
        public string IntervalType { get; set; } = string.Empty;

        /// <summary>
        /// Nachylenie (dla gradual intervals)
        /// </summary>
        public double? Slope { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        [ForeignKey(nameof(TrainingId))]
        public virtual Training Training { get; set; } = null!;

        [NotMapped]
        public string StartTime => FormatTime(StartSecond);

        [NotMapped]
        public string EndTime => FormatTime(EndSecond);

        [NotMapped]
        public string DurationFormatted => $"{DurationSeconds / 60:D2}:{DurationSeconds % 60:D2} ({DurationSeconds}s)";

        private string FormatTime(int seconds)
        {
            int minutes = seconds / 60;
            int secs = seconds % 60;
            return $"{minutes:D2}:{secs:D2}";
        }
    }
}
