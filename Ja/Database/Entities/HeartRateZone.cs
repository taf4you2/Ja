using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ja.Database.Entities
{
    /// <summary>
    /// Konfiguracja stref tętna użytkownika
    /// </summary>
    [Table("Heart_Rate_Zones")]
    public class HeartRateZone
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        /// <summary>
        /// Numer strefy
        /// </summary>
        [Required]
        public int ZoneNumber { get; set; }

        /// <summary>
        /// Nazwa strefy
        /// </summary>
        [MaxLength(100)]
        [Required]
        public string ZoneName { get; set; } = string.Empty;

        /// <summary>
        /// Minimalna wartość (% Max HR lub % LTHR)
        /// </summary>
        [Required]
        public double MinPercent { get; set; }

        /// <summary>
        /// Maksymalna wartość (% Max HR lub % LTHR)
        /// </summary>
        [Required]
        public double MaxPercent { get; set; }

        /// <summary>
        /// Kolor strefy (hex)
        /// </summary>
        [MaxLength(20)]
        public string ColorHex { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = null!;
    }
}
