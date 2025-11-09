using System;
using System.Linq;

namespace Ja.Services
{
    /// <summary>
    /// Serwis do obliczania metryk treningowych (TSS, NP, IF, VI, Work)
    /// Implementacja zgodna ze specyfikacją JA Training
    /// </summary>
    public class MetricsCalculationService
    {
        /// <summary>
        /// Oblicza Normalized Power (NP)
        /// Algorytm:
        /// 1. 30-sekundowa średnia krocząca
        /// 2. Podniesienie każdej wartości do 4 potęgi
        /// 3. Obliczenie średniej z tych wartości
        /// 4. Wynik podniesiony do potęgi 1/4
        /// </summary>
        public double CalculateNormalizedPower(double[] powerData)
        {
            if (powerData == null || powerData.Length < 30)
                return 0;

            // Krok 1: Oblicz 30-sekundową średnią kroczącą
            double[] rollingAvg = CalculateRollingAverage(powerData, 30);

            // Krok 2: Podnieś każdą wartość do 4 potęgi
            double[] raised = rollingAvg.Select(p => Math.Pow(p, 4)).ToArray();

            // Krok 3: Oblicz średnią
            double mean = raised.Average();

            // Krok 4: Wynik do potęgi 1/4
            return Math.Pow(mean, 0.25);
        }

        /// <summary>
        /// Oblicza 30-sekundową średnią kroczącą
        /// </summary>
        private double[] CalculateRollingAverage(double[] data, int windowSize)
        {
            double[] result = new double[data.Length];

            for (int i = 0; i < data.Length; i++)
            {
                int start = Math.Max(0, i - windowSize + 1);
                int count = i - start + 1;
                double sum = 0;

                for (int j = start; j <= i; j++)
                {
                    sum += data[j];
                }

                result[i] = sum / count;
            }

            return result;
        }

        /// <summary>
        /// Oblicza Intensity Factor (IF)
        /// IF = NP / FTP
        /// </summary>
        public double CalculateIntensityFactor(double normalizedPower, double ftp)
        {
            if (ftp <= 0)
                return 0;

            return normalizedPower / ftp;
        }

        /// <summary>
        /// Oblicza Training Stress Score (TSS)
        /// Formuła: (seconds × NP × IF) / (FTP × 3600) × 100
        /// </summary>
        public double CalculateTSS(int durationSeconds, double normalizedPower, double intensityFactor, double ftp)
        {
            if (ftp <= 0)
                return 0;

            return (durationSeconds * normalizedPower * intensityFactor) / (ftp * 3600) * 100;
        }

        /// <summary>
        /// Oblicza Variability Index (VI)
        /// VI = NP / Average Power
        /// Pokazuje jak "gładki" był trening (1.0 = idealne tempo, >1.05 = zmienny)
        /// </summary>
        public double CalculateVariabilityIndex(double normalizedPower, double avgPower)
        {
            if (avgPower <= 0)
                return 0;

            return normalizedPower / avgPower;
        }

        /// <summary>
        /// Oblicza Work (pracę) w kJ
        /// Work = suma wszystkich wartości mocy × 1 sekunda / 1000
        /// </summary>
        public double CalculateWork(double[] powerData)
        {
            if (powerData == null || powerData.Length == 0)
                return 0;

            // Suma mocy w watach × sekundy, podzielone przez 1000 dla kJ
            return powerData.Sum() / 1000.0;
        }

        /// <summary>
        /// Oblicza średnią moc
        /// </summary>
        public double CalculateAveragePower(double[] powerData)
        {
            if (powerData == null || powerData.Length == 0)
                return 0;

            return powerData.Average();
        }

        /// <summary>
        /// Oblicza maksymalną moc
        /// </summary>
        public double CalculateMaxPower(double[] powerData)
        {
            if (powerData == null || powerData.Length == 0)
                return 0;

            return powerData.Max();
        }

        /// <summary>
        /// Oblicza wszystkie metryki dla treningu
        /// </summary>
        public TrainingMetrics CalculateAllMetrics(double[] powerData, int durationSeconds, double ftp)
        {
            var avgPower = CalculateAveragePower(powerData);
            var maxPower = CalculateMaxPower(powerData);
            var normalizedPower = CalculateNormalizedPower(powerData);
            var intensityFactor = CalculateIntensityFactor(normalizedPower, ftp);
            var tss = CalculateTSS(durationSeconds, normalizedPower, intensityFactor, ftp);
            var variabilityIndex = CalculateVariabilityIndex(normalizedPower, avgPower);
            var work = CalculateWork(powerData);

            return new TrainingMetrics
            {
                AvgPower = avgPower,
                MaxPower = maxPower,
                NormalizedPower = normalizedPower,
                IntensityFactor = intensityFactor,
                TSS = tss,
                VariabilityIndex = variabilityIndex,
                WorkKJ = work
            };
        }
    }

    /// <summary>
    /// Klasa przechowująca obliczone metryki
    /// </summary>
    public class TrainingMetrics
    {
        public double AvgPower { get; set; }
        public double MaxPower { get; set; }
        public double NormalizedPower { get; set; }
        public double IntensityFactor { get; set; }
        public double TSS { get; set; }
        public double VariabilityIndex { get; set; }
        public double WorkKJ { get; set; }
    }
}
