using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ja.Database.Entities
{
    /// <summary>
    /// Historia pomiarów FTP użytkownika
    /// </summary>
    [Table("FTP_History")]
    public class FTPHistory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        /// <summary>
        /// Wartość FTP w watach
        /// </summary>
        [Required]
        public double FtpValue { get; set; }

        /// <summary>
        /// Waga w momencie testu (kg)
        /// </summary>
        public double? WeightAtTest { get; set; }

        /// <summary>
        /// Data testu FTP
        /// </summary>
        [Required]
        public DateTime TestDate { get; set; }

        /// <summary>
        /// Źródło pomiaru: test_20min, ramp_test, estimated, manual
        /// </summary>
        [MaxLength(50)]
        public string Source { get; set; } = "manual";

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = null!;

        /// <summary>
        /// Oblicza W/kg na podstawie FTP i wagi
        /// </summary>
        [NotMapped]
        public double? WattsPerKg => WeightAtTest.HasValue && WeightAtTest.Value > 0
            ? FtpValue / WeightAtTest.Value
            : null;
    }
}
