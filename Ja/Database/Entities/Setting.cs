using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ja.Database.Entities
{
    /// <summary>
    /// Ustawienia aplikacji
    /// </summary>
    [Table("Settings")]
    public class Setting
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int? UserId { get; set; }

        /// <summary>
        /// Klucz ustawienia (np. "default_ftp", "theme", "algorithm_sensitivity")
        /// </summary>
        [MaxLength(200)]
        [Required]
        public string SettingKey { get; set; } = string.Empty;

        /// <summary>
        /// Wartość ustawienia (JSON lub string)
        /// </summary>
        [Required]
        public string SettingValue { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        [ForeignKey(nameof(UserId))]
        public virtual User? User { get; set; }
    }
}
