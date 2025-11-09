using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ja.Database.Entities
{
    /// <summary>
    /// Konfiguracja stref mocy użytkownika
    /// </summary>
    [Table("Power_Zones")]
    public class PowerZone
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        /// <summary>
        /// Numer strefy (1-7)
        /// </summary>
        [Required]
        public int ZoneNumber { get; set; }

        /// <summary>
        /// Nazwa strefy (np. "Recovery", "Endurance")
        /// </summary>
        [MaxLength(100)]
        [Required]
        public string ZoneName { get; set; } = string.Empty;

        /// <summary>
        /// Minimalna wartość % FTP
        /// </summary>
        [Required]
        public double MinPercent { get; set; }

        /// <summary>
        /// Maksymalna wartość % FTP
        /// </summary>
        [Required]
        public double MaxPercent { get; set; }

        /// <summary>
        /// Kolor strefy (hex, np. "#FF5733")
        /// </summary>
        [MaxLength(20)]
        public string ColorHex { get; set; } = string.Empty;

        /// <summary>
        /// Minimalny czas trwania w sekundach dla wykrywania interwałów
        /// </summary>
        public int MinDurationSeconds { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = null!;
    }
}
