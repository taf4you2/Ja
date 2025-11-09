using System;
using System.Collections.Generic;
using System.Linq;

namespace Ja.Algorithms
{
    /// <summary>
    /// Algorytm wykrywania interwałów treningowych w danych mocy
    /// Implementacja 10-stopniowego algorytmu opisanego w dokumentacji
    /// </summary>
    public class IntervalDetectionAlgorithm
    {
        // ====================================================================================
        // KROK 1: USUWANIE OUTLIERÓW
        // ====================================================================================

        /// <summary>
        /// Usuwa outliery metodą Z-score (wartości > 3σ od średniej)
        /// </summary>
        private double[] RemoveOutliers(double[] power)
        {
            double mean = power.Average();
            double stdDev = CalculateStandardDeviation(power, mean);
            double[] cleaned = new double[power.Length];
            Array.Copy(power, cleaned, power.Length);

            for (int i = 0; i < power.Length; i++)
            {
                double zScore = Math.Abs(power[i] - mean) / stdDev;

                if (zScore > 3)
                {
                    if (i > 0 && i < power.Length - 1)
                    {
                        cleaned[i] = (power[i - 1] + power[i + 1]) / 2;
                    }
                    else
                    {
                        cleaned[i] = mean;
                    }
                }
            }

            return cleaned;
        }

        private double CalculateStandardDeviation(double[] values, double mean)
        {
            double sumOfSquares = values.Sum(val => Math.Pow(val - mean, 2));
            return Math.Sqrt(sumOfSquares / values.Length);
        }

        // ====================================================================================
        // KROK 2: WYGŁADZANIE DANYCH - EMA
        // ====================================================================================

        /// <summary>
        /// Exponential Moving Average dla redukcji szumu
        /// </summary>
        private double[] ExponentialMovingAverage(double[] data, double alpha = 0.2)
        {
            double[] ema = new double[data.Length];
            ema[0] = data[0];

            for (int i = 1; i < data.Length; i++)
            {
                ema[i] = alpha * data[i] + (1 - alpha) * ema[i - 1];
            }

            return ema;
        }

        // ====================================================================================
        // KROK 3: KONWERSJA NA PROCENTY FTP
        // ====================================================================================

        private double[] ConvertToFtpPercent(double[] power, double ftp)
        {
            return power.Select(p => (p / ftp) * 100).ToArray();
        }

        // ====================================================================================
        // KROK 4: DWUETAPOWE WYKRYWANIE PUNKTÓW ZMIAN
        // ====================================================================================

        private List<int> DetectChangePoints(double[] data, int windowSize, double threshold)
        {
            List<int> changePoints = new List<int>();

            for (int i = windowSize; i < data.Length - windowSize; i++)
            {
                double avgBefore = data.Skip(i - windowSize).Take(windowSize).Average();
                double avgAfter = data.Skip(i).Take(windowSize).Average();

                double change = Math.Abs(avgAfter - avgBefore) / avgBefore * 100;

                if (change > threshold)
                {
                    if (changePoints.Count == 0 || i - changePoints.Last() > 50)
                    {
                        changePoints.Add(i);
                    }
                }
            }

            return changePoints;
        }

        private List<int> MergeUniquePoints(List<int> pointsLong, List<int> pointsShort, int minGap = 20)
        {
            var allPoints = pointsLong.Concat(pointsShort).OrderBy(p => p).ToList();

            if (allPoints.Count == 0)
                return new List<int>();

            var merged = new List<int>();
            var currentGroup = new List<int> { allPoints[0] };

            for (int i = 1; i < allPoints.Count; i++)
            {
                if (allPoints[i] - currentGroup.Last() <= minGap)
                {
                    currentGroup.Add(allPoints[i]);
                }
                else
                {
                    merged.Add((int)currentGroup.Average());
                    currentGroup = new List<int> { allPoints[i] };
                }
            }

            merged.Add((int)currentGroup.Average());
            return merged;
        }

        private List<int> DetectChangePointsTwoPhase(double[] data)
        {
            // Faza 1: Długie interwały (okno 30s, próg 12%)
            var changePointsLong = DetectChangePoints(data, 30, 12);

            // Faza 2: Krótkie sprinty (okno 10s, próg 25%)
            var changePointsShort = DetectChangePoints(data, 10, 25);

            // Połącz unikając duplikatów
            return MergeUniquePoints(changePointsLong, changePointsShort, 20);
        }

        // ====================================================================================
        // KROK 5: TWORZENIE SEGMENTÓW
        // ====================================================================================

        private List<Segment> CreateSegments(double[] data, List<int> changePoints)
        {
            var segments = new List<Segment>();
            int start = 0;
            int minDuration = 10;

            foreach (var cp in changePoints)
            {
                if (cp - start > minDuration)
                {
                    segments.Add(new Segment
                    {
                        Start = start,
                        End = cp,
                        AvgPower = data.Skip(start).Take(cp - start).Average()
                    });
                }
                start = cp;
            }

            if (data.Length - start > minDuration)
            {
                segments.Add(new Segment
                {
                    Start = start,
                    End = data.Length,
                    AvgPower = data.Skip(start).Take(data.Length - start).Average()
                });
            }

            return segments;
        }

        // ====================================================================================
        // KROK 6: WYKRYWANIE STOPNIOWEGO NARASTANIA
        // ====================================================================================

        private List<Segment> DetectGradualIntervals(double[] data, int windowSize = 90, double minSlope = 10)
        {
            var gradualIntervals = new List<Segment>();
            int i = 0;

            while (i < data.Length - windowSize)
            {
                var windowData = data.Skip(i).Take(windowSize).ToArray();
                double slope = CalculateSlope(windowData);
                double slopePerMinute = slope * 60;

                if (slopePerMinute > minSlope)
                {
                    gradualIntervals.Add(new Segment
                    {
                        Start = i,
                        End = i + windowSize,
                        AvgPower = windowData.Average(),
                        Type = "gradual",
                        Slope = slopePerMinute
                    });
                    i += windowSize;
                }
                else
                {
                    i += 10;
                }
            }

            return gradualIntervals;
        }

        private double CalculateSlope(double[] data)
        {
            int n = data.Length;
            double sumX = 0, sumY = 0, sumXY = 0, sumX2 = 0;

            for (int i = 0; i < n; i++)
            {
                sumX += i;
                sumY += data[i];
                sumXY += i * data[i];
                sumX2 += i * i;
            }

            return (n * sumXY - sumX * sumY) / (n * sumX2 - sumX * sumX);
        }

        // ====================================================================================
        // KROK 8: KLASYFIKACJA STREF
        // ====================================================================================

        private (int zone, string zoneName) ClassifyZone(double avgPowerPercent)
        {
            var zones = new[] { 55, 75, 90, 105, 120, 150 };
            var zoneNames = new[]
            {
                "Z1: Recovery",
                "Z2: Endurance",
                "Z3: Tempo",
                "Z4: Threshold",
                "Z5: VO2max",
                "Z6: Anaerobic",
                "Z7: Neuromuscular"
            };

            for (int i = 0; i < zones.Length; i++)
            {
                if (avgPowerPercent <= zones[i])
                    return (i + 1, zoneNames[i]);
            }

            return (7, zoneNames[6]);
        }

        // ====================================================================================
        // KROK 9: WERYFIKACJA I WALIDACJA
        // ====================================================================================

        private List<Segment> VerifyAndValidateSegments(List<Segment> segments, double[] powerPercent)
        {
            var segmentsSorted = segments.OrderBy(s => s.Start).ToList();
            var filledSegments = new List<Segment>();

            for (int i = 0; i < segmentsSorted.Count; i++)
            {
                filledSegments.Add(segmentsSorted[i]);

                // Sprawdź czy jest luka przed następnym segmentem
                if (i < segmentsSorted.Count - 1)
                {
                    int gapStart = segmentsSorted[i].End;
                    int gapEnd = segmentsSorted[i + 1].Start;
                    int gapDuration = gapEnd - gapStart;

                    // Jeśli luka dłuższa niż 30s - sprawdź co tam było
                    if (gapDuration > 30)
                    {
                        double gapAvgPower = powerPercent.Skip(gapStart).Take(gapDuration).Average();
                        var (zoneNum, zoneName) = ClassifyZone(gapAvgPower);

                        filledSegments.Add(new Segment
                        {
                            Start = gapStart,
                            End = gapEnd,
                            AvgPower = gapAvgPower,
                            Zone = zoneNum,
                            ZoneName = zoneName,
                            Type = "gap_filled"
                        });
                    }
                }
            }

            // Posortuj ponownie po dodaniu luk
            filledSegments = filledSegments.OrderBy(s => s.Start).ToList();

            // Usuń nakładki
            var noOverlap = new List<Segment>();
            int idx = 0;
            while (idx < filledSegments.Count)
            {
                var current = filledSegments[idx];

                if (idx < filledSegments.Count - 1)
                {
                    var next = filledSegments[idx + 1];

                    if (current.End > next.Start)
                    {
                        if (current.AvgPower >= next.AvgPower)
                        {
                            current.End = next.Start;
                            noOverlap.Add(current);
                        }
                        else
                        {
                            idx++;
                            continue;
                        }
                    }
                    else
                    {
                        noOverlap.Add(current);
                    }
                }
                else
                {
                    noOverlap.Add(current);
                }

                idx++;
            }

            // Weryfikacja minimalnych czasów dla stref
            var minDurations = new Dictionary<int, int>
            {
                { 7, 5 },   // Neuromuscular - min 5s
                { 6, 10 },  // Anaerobic - min 10s
                { 5, 30 },  // VO2max - min 30s
                { 4, 60 },  // Threshold - min 60s
                { 3, 120 }, // Tempo - min 120s
                { 2, 0 },   // Endurance - bez limitu
                { 1, 0 }    // Recovery - bez limitu
            };

            var validSegments = new List<Segment>();
            foreach (var seg in noOverlap)
            {
                int duration = seg.End - seg.Start;
                if (minDurations.ContainsKey(seg.Zone))
                {
                    if (duration >= minDurations[seg.Zone])
                    {
                        validSegments.Add(seg);
                    }
                }
                else
                {
                    validSegments.Add(seg);
                }
            }

            return validSegments;
        }

        // ====================================================================================
        // KROK 10: FILTROWANIE I WYKRYWANIE ODPOCZYNKÓW
        // ====================================================================================

        private List<Segment> MergeCloseIntervals(List<Segment> intervals, int maxGap = 10)
        {
            if (intervals.Count == 0)
                return new List<Segment>();

            var merged = new List<Segment>();
            var current = new Segment
            {
                Start = intervals[0].Start,
                End = intervals[0].End,
                AvgPower = intervals[0].AvgPower,
                Type = intervals[0].Type,
                Slope = intervals[0].Slope,
                Zone = intervals[0].Zone,
                ZoneName = intervals[0].ZoneName
            };

            for (int i = 1; i < intervals.Count; i++)
            {
                int gap = intervals[i].Start - current.End;

                if (gap <= maxGap)
                {
                    current.End = intervals[i].End;
                    current.AvgPower = (current.AvgPower + intervals[i].AvgPower) / 2;
                    if (!string.IsNullOrEmpty(intervals[i].Type))
                    {
                        current.Type = intervals[i].Type;
                        current.Slope = intervals[i].Slope;
                    }
                }
                else
                {
                    merged.Add(current);
                    current = new Segment
                    {
                        Start = intervals[i].Start,
                        End = intervals[i].End,
                        AvgPower = intervals[i].AvgPower,
                        Type = intervals[i].Type,
                        Slope = intervals[i].Slope,
                        Zone = intervals[i].Zone,
                        ZoneName = intervals[i].ZoneName
                    };
                }
            }

            merged.Add(current);
            return merged;
        }

        private List<Segment> DetectRecoveryPeriods(List<Segment> intervals, double[] powerPercent, double ftp)
        {
            var recoveries = new List<Segment>();

            for (int i = 0; i < intervals.Count - 1; i++)
            {
                int recoveryStart = intervals[i].End;
                int recoveryEnd = intervals[i + 1].Start;

                // Tylko jeśli przerwa dłuższa niż 30s
                if (recoveryEnd - recoveryStart > 30)
                {
                    double avgPower = powerPercent.Skip(recoveryStart).Take(recoveryEnd - recoveryStart).Average();
                    var (zoneNum, zoneName) = ClassifyZone(avgPower);

                    recoveries.Add(new Segment
                    {
                        Start = recoveryStart,
                        End = recoveryEnd,
                        Duration = recoveryEnd - recoveryStart,
                        AvgPower = avgPower,
                        AvgPowerWatts = avgPower * ftp / 100,
                        Zone = zoneNum,
                        ZoneName = zoneName,
                        Type = "recovery"
                    });
                }
            }

            return recoveries;
        }

        // ====================================================================================
        // GŁÓWNA FUNKCJA - KOMPLETNY ALGORYTM
        // ====================================================================================

        public (List<Segment> intervals, List<Segment> recoveries) DetectAllIntervals(
            double[] power,
            double ftp,
            Action<string>? progressCallback = null)
        {
            // KROK 1: Usuwamy outliery
            progressCallback?.Invoke("Krok 1/10: Usuwanie outlierów...");
            var powerClean = RemoveOutliers(power);

            // KROK 2: Wygładzamy dane
            progressCallback?.Invoke("Krok 2/10: Wygładzanie danych (EMA)...");
            var powerSmooth = ExponentialMovingAverage(powerClean, 0.2);

            // KROK 3: Konwersja na procenty FTP
            progressCallback?.Invoke("Krok 3/10: Konwersja na procenty FTP...");
            var powerPercent = ConvertToFtpPercent(powerSmooth, ftp);

            // KROK 4: Dwuetapowe wykrywanie punktów zmian
            progressCallback?.Invoke("Krok 4/10: Wykrywanie punktów zmian (dwuetapowo)...");
            var changePoints = DetectChangePointsTwoPhase(powerPercent);
            progressCallback?.Invoke($"  Znaleziono {changePoints.Count} punktów zmian");

            // KROK 5: Tworzymy segmenty
            progressCallback?.Invoke("Krok 5/10: Tworzenie segmentów...");
            var segments = CreateSegments(powerPercent, changePoints);
            progressCallback?.Invoke($"  Utworzono {segments.Count} segmentów");

            // KROK 6: Wykrywamy stopniowe narastanie
            progressCallback?.Invoke("Krok 6/10: Wykrywanie stopniowych zmian...");
            var gradual = DetectGradualIntervals(powerPercent, 90, 10);
            progressCallback?.Invoke($"  Znaleziono {gradual.Count} stopniowych interwałów");

            // KROK 7: Łączymy wszystkie segmenty
            progressCallback?.Invoke("Krok 7/10: Łączenie segmentów...");
            var allSegments = segments.Concat(gradual).OrderBy(s => s.Start).ToList();

            // KROK 8: Klasyfikujemy
            progressCallback?.Invoke("Krok 8/10: Klasyfikacja stref...");
            foreach (var segment in allSegments)
            {
                var (zoneNum, zoneName) = ClassifyZone(segment.AvgPower);
                segment.Zone = zoneNum;
                segment.ZoneName = zoneName;
                if (string.IsNullOrEmpty(segment.Type))
                    segment.Type = "jump";
            }

            // KROK 9: Weryfikacja i walidacja
            progressCallback?.Invoke("Krok 9/10: Weryfikacja i walidacja segmentów...");
            var validatedSegments = VerifyAndValidateSegments(allSegments, powerPercent);
            progressCallback?.Invoke($"  Po walidacji: {validatedSegments.Count} segmentów");

            // KROK 10: Filtrujemy tylko interwały (strefa 3+)
            progressCallback?.Invoke("Krok 10/10: Filtrowanie interwałów i wykrywanie odpoczynków...");
            var intervals = validatedSegments.Where(s => s.Zone >= 3).ToList();
            intervals = MergeCloseIntervals(intervals, 10);

            // Wykryj odpoczynki
            var recoveries = DetectRecoveryPeriods(intervals, powerPercent, ftp);

            // Dodajemy dodatkowe info
            foreach (var interval in intervals)
            {
                interval.Duration = interval.End - interval.Start;
                interval.AvgPowerWatts = interval.AvgPower * ftp / 100;
            }

            progressCallback?.Invoke($"\nGotowe!");
            progressCallback?.Invoke($"  Wykryto {intervals.Count} interwałów treningowych");
            progressCallback?.Invoke($"  Wykryto {recoveries.Count} okresów odpoczynku\n");

            return (intervals, recoveries);
        }

        // ====================================================================================
        // KLASA POMOCNICZA - SEGMENT
        // ====================================================================================

        public class Segment
        {
            public int Start { get; set; }
            public int End { get; set; }
            public double AvgPower { get; set; }
            public int Zone { get; set; }
            public string ZoneName { get; set; } = string.Empty;
            public string Type { get; set; } = string.Empty;
            public double Slope { get; set; }
            public int Duration { get; set; }
            public double AvgPowerWatts { get; set; }
        }
    }
}
