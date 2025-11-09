using System;
using System.Collections.Generic;

namespace Ja.Models
{
    /// <summary>
    /// Reprezentuje kompletne dane wczytane z pliku FIT
    /// Rozszerzone o dodatkowe metryki zgodnie ze specyfikacją JA Training
    /// </summary>
    public class FitFileData
    {
        // Podstawowe informacje o pliku
        public string FileName { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan Duration { get; set; }

        // Surowe dane z pliku
        public double[] PowerData { get; set; } = Array.Empty<double>();
        public double[] HeartRateData { get; set; } = Array.Empty<double>();
        public double[] CadenceData { get; set; } = Array.Empty<double>();
        public double[] SpeedData { get; set; } = Array.Empty<double>();

        // Podstawowe dane użytkownika
        public double Ftp { get; set; }
        public double? Weight { get; set; }

        // Podstawowe metryki
        public double TotalDistance { get; set; }
        public double AvgPower { get; set; }
        public double MaxPower { get; set; }
        public double AvgHeartRate { get; set; }
        public double MaxHeartRate { get; set; }
        public double AvgCadence { get; set; }
        public double MaxCadence { get; set; }
        public double AvgSpeed { get; set; }
        public double? ElevationGain { get; set; }
        public double? ElevationLoss { get; set; }

        // Zaawansowane metryki (zgodnie ze specyfikacją)
        public double NormalizedPower { get; set; }
        public double IntensityFactor { get; set; }
        public double TSS { get; set; }
        public double VariabilityIndex { get; set; }
        public double WorkKJ { get; set; }

        // Wykryte interwały i okresy odpoczynku
        public List<TrainingInterval> Intervals { get; set; } = new List<TrainingInterval>();
        public List<RecoveryPeriod> RecoveryPeriods { get; set; } = new List<RecoveryPeriod>();

        // Power curve (mean maximal power)
        public Dictionary<int, double> PowerCurve { get; set; } = new Dictionary<int, double>();

        // Formatowane wartości
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

        public string DistanceFormatted
        {
            get
            {
                if (TotalDistance >= 1000)
                    return $"{TotalDistance / 1000:F2} km";
                else
                    return $"{TotalDistance:F0} m";
            }
        }

        public string TSSFormatted => $"{TSS:F0}";

        public string IntensityFactorFormatted => $"{IntensityFactor:F2}";

        public string NormalizedPowerFormatted => $"{NormalizedPower:F0} W";

        public string VariabilityIndexFormatted => $"{VariabilityIndex:F2}";
    }
}
