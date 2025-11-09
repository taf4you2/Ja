using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ja.Database.Entities
{
    /// <summary>
    /// Encja użytkownika aplikacji
    /// </summary>
    [Table("Users")]
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Surname { get; set; } = string.Empty;

        public DateTime? DateOfBirth { get; set; }

        [MaxLength(50)]
        public string? Gender { get; set; }

        /// <summary>
        /// Aktualna waga w kg
        /// </summary>
        public double? Weight { get; set; }

        /// <summary>
        /// Wzrost w cm
        /// </summary>
        public double? Height { get; set; }

        /// <summary>
        /// Spoczynkowe tętno (RHR - Resting Heart Rate)
        /// </summary>
        public int? RestingHeartRate { get; set; }

        /// <summary>
        /// Maksymalne tętno
        /// </summary>
        public int? MaxHeartRate { get; set; }

        [MaxLength(500)]
        public string? ProfilePicturePath { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<Training> Trainings { get; set; } = new List<Training>();
        public virtual ICollection<FTPHistory> FTPHistory { get; set; } = new List<FTPHistory>();
        public virtual ICollection<WeightHistory> WeightHistory { get; set; } = new List<WeightHistory>();
        public virtual ICollection<PersonalRecord> PersonalRecords { get; set; } = new List<PersonalRecord>();
        public virtual ICollection<PowerZone> PowerZones { get; set; } = new List<PowerZone>();
        public virtual ICollection<HeartRateZone> HeartRateZones { get; set; } = new List<HeartRateZone>();
        public virtual ICollection<Setting> Settings { get; set; } = new List<Setting>();
        public virtual ICollection<PMCData> PMCData { get; set; } = new List<PMCData>();
    }
}
