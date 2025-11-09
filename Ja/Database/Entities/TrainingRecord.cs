using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ja.Database.Entities
{
    /// <summary>
    /// Rekordy z pojedynczego treningu (power curve data)
    /// </summary>
    [Table("Training_Records")]
    public class TrainingRecord
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int TrainingId { get; set; }

        /// <summary>
        /// Czas trwania wysiłku (5s, 10s, 30s, 60s, 300s, 1200s, 3600s...)
        /// </summary>
        [Required]
        public int DurationSeconds { get; set; }

        /// <summary>
        /// Maksymalna średnia moc dla danego czasu
        /// </summary>
        [Required]
        public double PowerWatts { get; set; }

        /// <summary>
        /// Tętno (opcjonalnie)
        /// </summary>
        public int? HeartRate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        [ForeignKey(nameof(TrainingId))]
        public virtual Training Training { get; set; } = null!;
    }
}
