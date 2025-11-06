using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Ja.Algorithms;
using Ja.Models;
using Ja.Services;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Microsoft.Win32;
using SkiaSharp;

namespace Ja.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private readonly FitFileParser _fitFileParser;
        private readonly IntervalDetectionAlgorithm _intervalDetection;

        [ObservableProperty]
        private string _fileName = "Brak wczytanego pliku";

        [ObservableProperty]
        private string _fileInfo = string.Empty;

        [ObservableProperty]
        private string _progressText = string.Empty;

        [ObservableProperty]
        private bool _isAnalyzing = false;

        [ObservableProperty]
        private bool _hasData = false;

        [ObservableProperty]
        private double _ftp = 200;

        [ObservableProperty]
        private ObservableCollection<TrainingInterval> _intervals = new();

        [ObservableProperty]
        private ObservableCollection<RecoveryPeriod> _recoveryPeriods = new();

        [ObservableProperty]
        private ObservableCollection<ISeries> _series = new();

        [ObservableProperty]
        private Axis[] _xAxes = new Axis[]
        {
            new Axis
            {
                Name = "Czas (sekundy)",
                NamePaint = new SolidColorPaint(SKColors.Black),
                LabelsPaint = new SolidColorPaint(SKColors.Gray),
                TextSize = 12
            }
        };

        [ObservableProperty]
        private Axis[] _yAxes = new Axis[]
        {
            new Axis
            {
                Name = "Moc (W)",
                NamePaint = new SolidColorPaint(SKColors.Black),
                LabelsPaint = new SolidColorPaint(SKColors.Gray),
                TextSize = 12
            }
        };

        private FitFileData? _currentFileData;

        public MainWindowViewModel()
        {
            _fitFileParser = new FitFileParser();
            _intervalDetection = new IntervalDetectionAlgorithm();
        }

        [RelayCommand]
        private async Task LoadFitFileAsync()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Pliki FIT (*.fit)|*.fit|Wszystkie pliki (*.*)|*.*",
                Title = "Wybierz plik FIT"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                await LoadAndAnalyzeFileAsync(openFileDialog.FileName);
            }
        }

        private async Task LoadAndAnalyzeFileAsync(string filePath)
        {
            try
            {
                IsAnalyzing = true;
                HasData = false;
                ProgressText = "Wczytywanie pliku FIT...";

                // Wczytaj plik w tle
                _currentFileData = await Task.Run(() => _fitFileParser.ParseFitFile(filePath));

                FileName = _currentFileData.FileName;
                Ftp = _currentFileData.Ftp;

                // Walidacja
                if (!_fitFileParser.HasPowerData(_currentFileData))
                {
                    MessageBox.Show(
                        "Plik nie zawiera danych mocy. Analiza interwałów nie jest możliwa.",
                        "Brak danych mocy",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                if (!_fitFileParser.ValidateMinimumDuration(_currentFileData))
                {
                    MessageBox.Show(
                        "Plik jest zbyt krótki (minimum 2 minuty wymagane).",
                        "Plik za krótki",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                // Wyświetl informacje o pliku
                FileInfo = $"Czas trwania: {_currentFileData.DurationFormatted}\n" +
                          $"Średnia moc: {_currentFileData.AvgPower:F0} W\n" +
                          $"FTP: {_currentFileData.Ftp:F0} W\n" +
                          $"Średnie tętno: {_currentFileData.AvgHeartRate:F0} bpm\n" +
                          $"Dystans: {(_currentFileData.TotalDistance / 1000):F2} km";

                // Wykryj interwały
                await AnalyzeIntervalsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Błąd podczas wczytywania pliku: {ex.Message}",
                    "Błąd",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                IsAnalyzing = false;
            }
        }

        [RelayCommand(CanExecute = nameof(CanAnalyze))]
        private async Task AnalyzeIntervalsAsync()
        {
            if (_currentFileData == null)
                return;

            try
            {
                IsAnalyzing = true;

                // Wykryj interwały w tle
                var (intervals, recoveries) = await Task.Run(() =>
                {
                    return _intervalDetection.DetectAllIntervals(
                        _currentFileData.PowerData,
                        Ftp,
                        progress => Application.Current.Dispatcher.Invoke(() => ProgressText = progress));
                });

                // Konwertuj na modele viewmodel
                Intervals.Clear();
                foreach (var interval in intervals)
                {
                    Intervals.Add(new TrainingInterval
                    {
                        Start = interval.Start,
                        End = interval.End,
                        Duration = interval.Duration,
                        AvgPower = interval.AvgPower,
                        AvgPowerWatts = interval.AvgPowerWatts,
                        Zone = interval.Zone,
                        ZoneName = interval.ZoneName,
                        Type = interval.Type,
                        Slope = interval.Slope
                    });
                }

                RecoveryPeriods.Clear();
                foreach (var recovery in recoveries)
                {
                    RecoveryPeriods.Add(new RecoveryPeriod
                    {
                        Start = recovery.Start,
                        End = recovery.End,
                        Duration = recovery.Duration,
                        AvgPower = recovery.AvgPower,
                        AvgPowerWatts = recovery.AvgPowerWatts,
                        Zone = recovery.Zone,
                        ZoneName = recovery.ZoneName
                    });
                }

                // Zaktualizuj wykres
                UpdateChart();

                HasData = true;
                ProgressText = $"Analiza zakończona: {Intervals.Count} interwałów, {RecoveryPeriods.Count} okresów odpoczynku";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Błąd podczas analizy interwałów: {ex.Message}",
                    "Błąd analizy",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                IsAnalyzing = false;
            }
        }

        private bool CanAnalyze() => _currentFileData != null && !IsAnalyzing;

        private void UpdateChart()
        {
            if (_currentFileData == null)
                return;

            var powerValues = new ObservableCollection<ObservablePoint>();

            for (int i = 0; i < _currentFileData.PowerData.Length; i++)
            {
                powerValues.Add(new ObservablePoint(i, _currentFileData.PowerData[i]));
            }

            var powerSeries = new LineSeries<ObservablePoint>
            {
                Values = powerValues,
                Name = "Moc",
                Stroke = new SolidColorPaint(SKColors.Blue) { StrokeThickness = 1 },
                Fill = null,
                GeometrySize = 0
            };

            Series.Clear();
            Series.Add(powerSeries);

            // Dodaj linie dla interwałów
            foreach (var interval in Intervals)
            {
                var color = GetZoneColor(interval.Zone);
                var intervalSeries = new LineSeries<ObservablePoint>
                {
                    Values = new ObservableCollection<ObservablePoint>
                    {
                        new ObservablePoint(interval.Start, 0),
                        new ObservablePoint(interval.Start, _currentFileData.PowerData.Max()),
                    },
                    Name = $"Interwał {interval.Zone}",
                    Stroke = new SolidColorPaint(color) { StrokeThickness = 2 },
                    Fill = null,
                    GeometrySize = 0
                };
                Series.Add(intervalSeries);
            }
        }

        private SKColor GetZoneColor(int zone)
        {
            return zone switch
            {
                1 => SKColors.LightGray,
                2 => SKColors.LightBlue,
                3 => SKColors.Green,
                4 => SKColors.Yellow,
                5 => SKColors.Orange,
                6 => SKColors.Red,
                7 => SKColors.DarkRed,
                _ => SKColors.Gray
            };
        }

        partial void OnFtpChanged(double value)
        {
            if (_currentFileData != null)
            {
                AnalyzeIntervalsCommand.Execute(null);
            }
        }
    }
}
