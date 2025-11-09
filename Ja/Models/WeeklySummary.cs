using System;
using System.Collections.ObjectModel;

namespace Ja.Models
{
    /// <summary>
    /// Model reprezentujący podsumowanie tygodnia treningowego
    /// </summary>
    public class WeeklySummary
    {
        public string WeekRange { get; set; } = string.Empty;
        public double TotalTSS { get; set; }
        public int TrainingCount { get; set; }
        public string TotalTime { get; set; } = string.Empty;
        public double TotalDistance { get; set; }
        public double AvgPower { get; set; }
        public double AvgHeartRate { get; set; }
        public double ElevationGain { get; set; }

        public ObservableCollection<DayBar> DayBars { get; set; } = new();
    }

    /// <summary>
    /// Model dla słupka dnia w tygodniowym podsumowaniu
    /// </summary>
    public class DayBar
    {
        public string DayLabel { get; set; } = string.Empty;
        public double BarHeight { get; set; }
        public string BarColor { get; set; } = "#E0E0E0";
        public string Tooltip { get; set; } = string.Empty;
    }
}
