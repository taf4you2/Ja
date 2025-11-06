using System;
using System.Collections.Generic;

namespace Ja.Models
{
    /// <summary>
    /// Reprezentuje kompletne dane wczytane z pliku FIT
    /// </summary>
    public class FitFileData
    {
        public string FileName { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan Duration { get; set; }
        public double[] PowerData { get; set; } = Array.Empty<double>();
        public double[] HeartRateData { get; set; } = Array.Empty<double>();
        public double[] CadenceData { get; set; } = Array.Empty<double>();
        public double[] SpeedData { get; set; } = Array.Empty<double>();
        public double Ftp { get; set; }
        public double TotalDistance { get; set; }
        public double AvgPower { get; set; }
        public double AvgHeartRate { get; set; }
        public double AvgCadence { get; set; }
        public double AvgSpeed { get; set; }

        public List<TrainingInterval> Intervals { get; set; } = new List<TrainingInterval>();
        public List<RecoveryPeriod> RecoveryPeriods { get; set; } = new List<RecoveryPeriod>();

        public string DurationFormatted
        {
            get
            {
                int hours = (int)Duration.TotalHours;
                int minutes = Duration.Minutes;
                int seconds = Duration.Seconds;
                return $"{hours:D2}:{minutes:D2}:{seconds:D2}";
            }
        }
    }
}
