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

                    // Sprawdzenie czy plik jest poprawnym plikiem FIT
                    bool isFitFile = decoder.IsFIT(fitSource);
                    if (!isFitFile)
                    {
                        throw new InvalidOperationException("To nie jest prawidłowy plik FIT");
                    }

                    // Sprawdzenie integralności pliku
                    bool integrityOk = decoder.CheckIntegrity(fitSource);
                    if (!integrityOk)
                    {
                        throw new InvalidOperationException("Integralność pliku FIT jest naruszona");
                    }

                    // Reset strumienia po sprawdzeniu
                    fitSource.Seek(0, SeekOrigin.Begin);

                    // Listener dla wiadomości sesji
                    mesgBroadcaster.SessionMesgEvent += (sender, e) =>
                    {
                        SessionMesg sessionMesg = (SessionMesg)e.mesg;

                        if (sessionMesg.GetAvgPower() != null)
                            fitFileData.AvgPower = sessionMesg.GetAvgPower().Value;

                        if (sessionMesg.GetAvgHeartRate() != null)
                            fitFileData.AvgHeartRate = sessionMesg.GetAvgHeartRate().Value;

                        if (sessionMesg.GetAvgCadence() != null)
                            fitFileData.AvgCadence = sessionMesg.GetAvgCadence().Value;

                        if (sessionMesg.GetTotalDistance() != null)
                            totalDistance = sessionMesg.GetTotalDistance().Value;

                        if (sessionMesg.GetStartTime() != null)
                            startTime = sessionMesg.GetStartTime().GetDateTime();
                    };

                    // Listener dla wiadomości rekordów (dane 1Hz)
                    mesgBroadcaster.RecordMesgEvent += (sender, e) =>
                    {
                        RecordMesg recordMesg = (RecordMesg)e.mesg;

                        if (startTime == null && recordMesg.GetTimestamp() != null)
                            startTime = recordMesg.GetTimestamp().GetDateTime();

                        if (recordMesg.GetTimestamp() != null)
                            endTime = recordMesg.GetTimestamp().GetDateTime();

                        // Moc - ważne: dodajemy każdy rekord, nawet jeśli brak mocy
                        if (recordMesg.GetPower() != null)
                            powerList.Add(recordMesg.GetPower().Value);
                        else
                            powerList.Add(0);

                        // Tętno
                        if (recordMesg.GetHeartRate() != null)
                            heartRateList.Add(recordMesg.GetHeartRate().Value);
                        else
                            heartRateList.Add(0);

                        // Kadencja
                        if (recordMesg.GetCadence() != null)
                            cadenceList.Add(recordMesg.GetCadence().Value);
                        else
                            cadenceList.Add(0);

                        // Prędkość (konwersja m/s -> km/h)
                        if (recordMesg.GetSpeed() != null)
                            speedList.Add(recordMesg.GetSpeed().Value * 3.6);
                        else
                            speedList.Add(0);
                    };

                    // Listener dla ustawień użytkownika (FTP)
                    mesgBroadcaster.UserProfileMesgEvent += (sender, e) =>
                    {
                        UserProfileMesg userProfile = (UserProfileMesg)e.mesg;
                        if (userProfile.GetFtp() != null)
                            ftp = userProfile.GetFtp();
                    };

                    // Listener dla definicji strefy mocy
                    mesgBroadcaster.ZonesTargetMesgEvent += (sender, e) =>
                    {
                        ZonesTargetMesg zonesTarget = (ZonesTargetMesg)e.mesg;
                        if (zonesTarget.GetFunctionalThresholdPower() != null)
                            ftp = zonesTarget.GetFunctionalThresholdPower();
                    };

                    // WAŻNE: Podpięcie MesgBroadcaster do dekodera
                    decoder.MesgEvent += mesgBroadcaster.OnMesg;

                    // Dekodowanie pliku
                    decoder.Read(fitSource);
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
                if (ftp.HasValue && ftp.Value > 0)
                {
                    fitFileData.Ftp = ftp.Value;
                }
                else
                {
                    // Jeśli FTP nie zostało znalezione, oszacuj na podstawie średniej mocy
                    if (fitFileData.AvgPower > 0)
                    {
                        // Oszacowanie: FTP ≈ 105% średniej mocy z treningu
                        fitFileData.Ftp = fitFileData.AvgPower * 1.05;
                    }
                    else
                    {
                        // Wartość domyślna dla początkujących
                        fitFileData.Ftp = 200;
                    }
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

        /// <summary>
        /// Waliduje czy plik ma minimalną długość wymaganą do analizy
        /// </summary>
        public bool ValidateMinimumDuration(FitFileData data, int minimumSeconds = 120)
        {
            return data.PowerData != null && data.PowerData.Length >= minimumSeconds;
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

        /// <summary>
        /// Wyświetla podstawowe statystyki z pliku FIT (do debugowania)
        /// </summary>
        public string GetFileStatistics(FitFileData data)
        {
            if (data == null)
                return "Brak danych";

            var stats = $"Plik: {data.FileName}\n";
            stats += $"Czas trwania: {data.DurationFormatted}\n";
            stats += $"Punkty danych: {data.PowerData?.Length ?? 0}\n";
            stats += $"Średnia moc: {data.AvgPower:F1} W\n";
            stats += $"FTP: {data.Ftp:F0} W\n";
            stats += $"Średnie tętno: {data.AvgHeartRate:F0} bpm\n";
            stats += $"Dystans: {(data.TotalDistance / 1000):F2} km\n";
            stats += $"Średnia prędkość: {data.AvgSpeed:F1} km/h\n";

            return stats;
        }
    }
}
