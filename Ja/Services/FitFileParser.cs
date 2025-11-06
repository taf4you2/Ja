using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dynastream.Fit;
using Ja.Models;

namespace Ja.Services
{
    /// <summary>
    /// Serwis do parsowania plików FIT i ekstrakcji danych treningowych
    /// </summary>
    public class FitFileParser
    {
        public FitFileData ParseFitFile(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Plik FIT nie został znaleziony", filePath);

            var fitFileData = new FitFileData
            {
                FileName = Path.GetFileName(filePath)
            };

            var powerList = new List<double>();
            var heartRateList = new List<double>();
            var cadenceList = new List<double>();
            var speedList = new List<double>();

            DateTime? startTime = null;
            DateTime? endTime = null;
            double totalDistance = 0;
            ushort? ftp = null;

            try
            {
                using (var fitSource = new FileStream(filePath, FileMode.Open))
                {
                    var decoder = new Decode();
                    var mesgBroadcaster = new MesgBroadcaster();

                    // Listener dla wiadomości sesji
                    mesgBroadcaster.SessionMesgEvent += (sender, e) =>
                    {
                        if (e.mesg.GetAvgPower() != null)
                            fitFileData.AvgPower = e.mesg.GetAvgPower().Value;

                        if (e.mesg.GetAvgHeartRate() != null)
                            fitFileData.AvgHeartRate = e.mesg.GetAvgHeartRate().Value;

                        if (e.mesg.GetAvgCadence() != null)
                            fitFileData.AvgCadence = e.mesg.GetAvgCadence().Value;

                        if (e.mesg.GetTotalDistance() != null)
                            totalDistance = e.mesg.GetTotalDistance().Value;
                    };

                    // Listener dla wiadomości rekordów (dane 1Hz)
                    mesgBroadcaster.RecordMesgEvent += (sender, e) =>
                    {
                        if (startTime == null && e.mesg.GetTimestamp() != null)
                            startTime = e.mesg.GetTimestamp().GetDateTime();

                        if (e.mesg.GetTimestamp() != null)
                            endTime = e.mesg.GetTimestamp().GetDateTime();

                        // Moc
                        if (e.mesg.GetPower() != null)
                            powerList.Add(e.mesg.GetPower().Value);
                        else
                            powerList.Add(0);

                        // Tętno
                        if (e.mesg.GetHeartRate() != null)
                            heartRateList.Add(e.mesg.GetHeartRate().Value);
                        else
                            heartRateList.Add(0);

                        // Kadencja
                        if (e.mesg.GetCadence() != null)
                            cadenceList.Add(e.mesg.GetCadence().Value);
                        else
                            cadenceList.Add(0);

                        // Prędkość
                        if (e.mesg.GetSpeed() != null)
                            speedList.Add(e.mesg.GetSpeed().Value * 3.6); // m/s -> km/h
                        else
                            speedList.Add(0);
                    };

                    // Listener dla ustawień użytkownika (FTP)
                    mesgBroadcaster.UserProfileMesgEvent += (sender, e) =>
                    {
                        if (e.mesg.GetFtp() != null)
                            ftp = e.mesg.GetFtp();
                    };

                    // Listener dla definicji strefy mocy
                    mesgBroadcaster.ZonesTargetMesgEvent += (sender, e) =>
                    {
                        if (e.mesg.GetFunctionalThresholdPower() != null)
                            ftp = e.mesg.GetFunctionalThresholdPower();
                    };

                    // Dekodowanie pliku
                    if (!decoder.IsFIT(fitSource))
                        throw new InvalidOperationException("Plik nie jest poprawnym plikiem FIT");

                    if (!decoder.CheckIntegrity(fitSource))
                        throw new InvalidOperationException("Integralność pliku FIT jest naruszona");

                    decoder.Read(fitSource, mesgBroadcaster);
                }

                // Wypełnienie struktury danych
                fitFileData.PowerData = powerList.ToArray();
                fitFileData.HeartRateData = heartRateList.ToArray();
                fitFileData.CadenceData = cadenceList.ToArray();
                fitFileData.SpeedData = speedList.ToArray();

                if (startTime.HasValue)
                    fitFileData.StartTime = startTime.Value;

                if (endTime.HasValue)
                    fitFileData.EndTime = endTime.Value;

                if (startTime.HasValue && endTime.HasValue)
                    fitFileData.Duration = endTime.Value - startTime.Value;

                fitFileData.TotalDistance = totalDistance;

                // Oblicz średnią prędkość
                if (speedList.Count > 0)
                    fitFileData.AvgSpeed = speedList.Where(s => s > 0).DefaultIfEmpty(0).Average();

                // Ustaw FTP (jeśli nie znaleziono w pliku, użyj wartości domyślnej)
                if (ftp.HasValue)
                {
                    fitFileData.Ftp = ftp.Value;
                }
                else
                {
                    // Jeśli FTP nie zostało znalezione, oszacuj na podstawie średniej mocy
                    fitFileData.Ftp = fitFileData.AvgPower > 0 ? fitFileData.AvgPower * 1.05 : 200;
                }

                return fitFileData;
            }
            catch (FitException ex)
            {
                throw new InvalidOperationException($"Błąd podczas parsowania pliku FIT: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Waliduje czy plik ma minimalną długość wymaganą do analizy
        /// </summary>
        public bool ValidateMinimumDuration(FitFileData data, int minimumSeconds = 120)
        {
            return data.PowerData.Length >= minimumSeconds;
        }

        /// <summary>
        /// Sprawdza czy plik zawiera dane mocy
        /// </summary>
        public bool HasPowerData(FitFileData data)
        {
            return data.PowerData != null &&
                   data.PowerData.Length > 0 &&
                   data.PowerData.Any(p => p > 0);
        }
    }
}
