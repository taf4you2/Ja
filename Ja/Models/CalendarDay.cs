using System;
using System.Collections.ObjectModel;

namespace Ja.Models
{
    public class CalendarDay
    {
        public DateTime Date { get; set; }
        public int DayNumber { get; set; }
        public bool IsCurrentMonth { get; set; }
        public bool IsToday { get; set; }
        public bool HasTrainings { get; set; }
        public int TrainingCount { get; set; }
        public double TotalTSS { get; set; }
        public string DurationText { get; set; } = string.Empty;
        public string DominantZoneColor { get; set; } = "#E0E0E0";
        public ObservableCollection<CalendarTraining> Trainings { get; set; } = new ObservableCollection<CalendarTraining>();

        // UI Properties
        public string BackgroundColor => IsToday ? "#E3F2FD" : "White";
        public string BorderColor => IsToday ? "#1976D2" : "#E0E0E0";
        public string TextColor => IsCurrentMonth ? "#212121" : "#BDBDBD";
        public string BorderThickness => IsToday ? "2" : "1";
    }

    public class CalendarTraining
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public string Duration { get; set; } = string.Empty;
        public double TSS { get; set; }
        public string ZoneColor { get; set; } = "#1976D2";
    }
}
