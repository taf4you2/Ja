using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dynastream.Fit;
using Ja.Models;

// aliasy, żeby nie kolidowało z System.DateTime i System.IO.File
using FitDateTime = Dynastream.Fit.DateTime;

namespace Ja.Services
{
    /// <summary>
    /// Serwis do parsowania plików FIT i ekstrakcji danych treningowych
    /// </summary>
    public class FitFileParser
    {
        public FitFileData ParseFitFile(string filePath)
        {
            if (!System.IO.File.Exists(filePath))
                throw new FileNotFoundException("Plik FIT nie został znaleziony", filePath);

            var fitFileData = new FitFileData
            {
                FileName = Path.GetFileName(filePath)
            };

            var powerList = new List<double>();
            var heartRateList = new List<double>();
            var cadenceList = new List<double>();
            var speedList = new List<double>();

            System.DateTime? startTime = null;
            System.DateTime? endTime = null;
            double totalDistance = 0;
            ushort? ftp = null;

            try
            {
                using (var fitSource = new FileStream(filePath, FileMode.Open))
                {
                    var decoder = new Decode();
                    var mesgBroadcaster = new MesgBroadcaster();

                    if (!decoder.IsFIT(fitSource))
                        throw new InvalidOperationException("To nie jest prawidłowy plik FIT");

                    if (!decoder.CheckIntegrity(fitSource))
                        throw new InvalidOperationException("Integralność pliku FIT jest naruszona");

                    fitSource.Seek(0, SeekOrigin.Begin);

                    mesgBroadcaster.SessionMesgEvent += (sender, e) =>
                    {
                        var sessionMesg = (SessionMesg)e.mesg;

                        fitFileData.AvgPower = sessionMesg.GetAvgPower() ?? fitFileData.AvgPower;
                        fitFileData.AvgHeartRate = sessionMesg.GetAvgHeartRate() ?? fitFileData.AvgHeartRate;
                        fitFileData.AvgCadence = sessionMesg.GetAvgCadence() ?? fitFileData.AvgCadence;

                        if (sessionMesg.GetTotalDistance() != null)
                            totalDistance = sessionMesg.GetTotalDistance().Value;

                        if (sessionMesg.GetStartTime() != null)
                            startTime = sessionMesg.GetStartTime().GetDateTime();
                    };

                    mesgBroadcaster.RecordMesgEvent += (sender, e) =>
                    {
                        var recordMesg = (RecordMesg)e.mesg;


                        if (recordMesg.GetTimestamp() != null)
                            endTime = recordMesg.GetTimestamp().GetDateTime();

                        powerList.Add(recordMesg.GetPower() ?? 0);
                        heartRateList.Add(recordMesg.GetHeartRate() ?? 0);
                        cadenceList.Add(recordMesg.GetCadence() ?? 0);
                        speedList.Add(recordMesg.GetSpeed() != null ? recordMesg.GetSpeed().Value * 3.6 : 0);
                    };

                    /*mesgBroadcaster.UserProfileMesgEvent += (sender, e) =>
                    {
                        var userProfile = (UserProfileMesg)e.mesg;
                        if (userProfile.GetFunctionalThresholdPower() != null)
                            ftp = userProfile.GetFunctionalThresholdPower();
                    };*/

                    mesgBroadcaster.ZonesTargetMesgEvent += (sender, e) =>
                    {
                        var zonesTarget = (ZonesTargetMesg)e.mesg;
                        if (zonesTarget.GetFunctionalThresholdPower() != null)
                            ftp = zonesTarget.GetFunctionalThresholdPower();
                    };

                    decoder.MesgEvent += mesgBroadcaster.OnMesg;
                    decoder.Read(fitSource);
                }

                fitFileData.PowerData = powerList.ToArray();
                fitFileData.HeartRateData = heartRateList.ToArray();
                fitFileData.CadenceData = cadenceList.ToArray();
                fitFileData.SpeedData = speedList.ToArray();

                if (startTime.HasValue) fitFileData.StartTime = startTime.Value;
                if (endTime.HasValue) fitFileData.EndTime = endTime.Value;
                if (startTime.HasValue && endTime.HasValue) fitFileData.Duration = endTime.Value - startTime.Value;

                fitFileData.TotalDistance = totalDistance;

                if (speedList.Count > 0)
                    fitFileData.AvgSpeed = speedList.Where(s => s > 0).DefaultIfEmpty(0).Average();

                if (ftp.HasValue && ftp.Value > 0)
                {
                    fitFileData.Ftp = ftp.Value;
                }
                else
                {
                    if (fitFileData.AvgPower > 0)
                        fitFileData.Ftp = fitFileData.AvgPower * 1.05;
                    else
                        fitFileData.Ftp = 200;
                }

                // Oblicz zaawansowane metryki (TSS, NP, IF, VI, Work)
                if (fitFileData.PowerData.Length > 0)
                {
                    var metricsService = new MetricsCalculationService();
                    var metrics = metricsService.CalculateAllMetrics(
                        fitFileData.PowerData,
                        (int)fitFileData.Duration.TotalSeconds,
                        fitFileData.Ftp
                    );

                    fitFileData.NormalizedPower = metrics.NormalizedPower;
                    fitFileData.IntensityFactor = metrics.IntensityFactor;
                    fitFileData.TSS = metrics.TSS;
                    fitFileData.VariabilityIndex = metrics.VariabilityIndex;
                    fitFileData.WorkKJ = metrics.WorkKJ;
                    fitFileData.MaxPower = metrics.MaxPower;

                    // Oblicz power curve
                    var powerCurveService = new PowerCurveService(null!); // null bo nie potrzebujemy DbContext do samego obliczenia
                    fitFileData.PowerCurve = powerCurveService.CalculatePowerCurve(fitFileData.PowerData);
                }

                // Oblicz maksymalne wartości
                if (fitFileData.HeartRateData.Length > 0)
                {
                    fitFileData.MaxHeartRate = fitFileData.HeartRateData.Max();
                }

                if (fitFileData.CadenceData.Length > 0)
                {
                    fitFileData.MaxCadence = fitFileData.CadenceData.Max();
                }

                return fitFileData;
            }
            catch (FitException ex)
            {
                throw new InvalidOperationException($"Błąd FIT: {ex.Message}", ex);
            }
            catch (IOException ex)
            {
                throw new InvalidOperationException($"Błąd odczytu pliku: {ex.Message}", ex);
            }
        }

        public bool ValidateMinimumDuration(FitFileData data, int minimumSeconds = 120)
        {
            return data.PowerData != null && data.PowerData.Length >= minimumSeconds;
        }

        public bool HasPowerData(FitFileData data)
        {
            return data.PowerData != null &&
                   data.PowerData.Length > 0 &&
                   data.PowerData.Any(p => p > 0);
        }

        public string GetFileStatistics(FitFileData data)
        {
            if (data == null)
                return "Brak danych";

            return
                $"Plik: {data.FileName}\n" +
                $"Czas trwania: {data.DurationFormatted}\n" +
                $"Punkty danych: {data.PowerData?.Length ?? 0}\n" +
                $"Średnia moc: {data.AvgPower:F1} W\n" +
                $"Normalized Power: {data.NormalizedPower:F0} W\n" +
                $"FTP: {data.Ftp:F0} W\n" +
                $"TSS: {data.TSS:F0}\n" +
                $"IF: {data.IntensityFactor:F2}\n" +
                $"VI: {data.VariabilityIndex:F2}\n" +
                $"Work: {data.WorkKJ:F0} kJ\n" +
                $"Średnie tętno: {data.AvgHeartRate:F0} bpm\n" +
                $"Dystans: {(data.TotalDistance / 1000):F2} km\n" +
                $"Średnia prędkość: {data.AvgSpeed:F1} km/h\n";
        }
    }
}
