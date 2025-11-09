using System;

namespace Ja.Models
{
    /// <summary>
    /// Reprezentuje wykryty interwał treningowy
    /// </summary>
    public class TrainingInterval
    {
        public int Start { get; set; }
        public int End { get; set; }
        public int Duration { get; set; }
        public double AvgPower { get; set; }
        public double AvgPowerWatts { get; set; }
        public int Zone { get; set; }
        public string ZoneName { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public double Slope { get; set; }

        public string StartTime => FormatTime(Start);
        public string EndTime => FormatTime(End);
        public string DurationFormatted => FormatDuration(Duration);

        private string FormatTime(int seconds)
        {
            int minutes = seconds / 60;
            int secs = seconds % 60;
            return $"{minutes:D2}:{secs:D2}";
        }

        private string FormatDuration(int seconds)
        {
            int minutes = seconds / 60;
            int secs = seconds % 60;
            return $"{minutes:D2}:{secs:D2} ({seconds}s)";
        }
    }
}
