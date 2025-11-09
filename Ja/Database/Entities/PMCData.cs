using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ja.Database.Entities
{
    /// <summary>
    /// Cache danych Performance Management Chart (CTL/ATL/TSB)
    /// </summary>
    [Table("PMC_Data")]
    public class PMCData
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        /// <summary>
        /// Data
        /// </summary>
        [Required]
        public DateTime Date { get; set; }

        /// <summary>
        /// CTL - Chronic Training Load (Fitness)
        /// </summary>
        [Required]
        public double CTL { get; set; }

        /// <summary>
        /// ATL - Acute Training Load (Fatigue)
        /// </summary>
        [Required]
        public double ATL { get; set; }

        /// <summary>
        /// TSB - Training Stress Balance (Form)
        /// </summary>
        [Required]
        public double TSB { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = null!;
    }
}
