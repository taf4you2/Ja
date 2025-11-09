using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ja.Database.Entities
{
    /// <summary>
    /// Rekordy osobiste użytkownika
    /// </summary>
    [Table("Personal_Records")]
    public class PersonalRecord
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        /// <summary>
        /// ID treningu, w którym osiągnięty rekord
        /// </summary>
        public int? TrainingId { get; set; }

        /// <summary>
        /// Typ rekordu: power_5s, power_1min, power_20min, max_hr, longest_distance, highest_tss, etc.
        /// </summary>
        [MaxLength(100)]
        [Required]
        public string RecordType { get; set; } = string.Empty;

        /// <summary>
        /// Wartość rekordu
        /// </summary>
        [Required]
        public double Value { get; set; }

        /// <summary>
        /// Dodatkowa wartość (np. W/kg dla mocy)
        /// </summary>
        public double? SecondaryValue { get; set; }

        /// <summary>
        /// Data osiągnięcia rekordu
        /// </summary>
        [Required]
        public DateTime AchievedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = null!;

        [ForeignKey(nameof(TrainingId))]
        public virtual Training? Training { get; set; }
    }
}
