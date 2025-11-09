using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ja.Database.Entities
{
    /// <summary>
    /// Historia pomiarów wagi użytkownika
    /// </summary>
    [Table("Weight_History")]
    public class WeightHistory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        /// <summary>
        /// Waga w kg
        /// </summary>
        [Required]
        public double Weight { get; set; }

        /// <summary>
        /// Data pomiaru
        /// </summary>
        [Required]
        public DateTime MeasurementDate { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = null!;
    }
}
