using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Ja.Database.Entities;
using Ja.Models;
using Ja.Repositories;
using Ja.Services;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using Microsoft.Win32;

namespace Ja.ViewModels
{
    public partial class DashboardViewModel : ObservableObject
    {
        private readonly PMCService _pmcService;
        private readonly TrainingImportService _importService;
        private readonly TrainingRepository _trainingRepository;

        [ObservableProperty]
        private string _currentDateText;

        [ObservableProperty]
        private string _profilePicture = "";

        // PMC Metrics
        [ObservableProperty]
        private double _ctl;

        [ObservableProperty]
        private double _atl;

        [ObservableProperty]
        private double _tsb;

        [ObservableProperty]
        private string _ctlChange = "";

        [ObservableProperty]
        private string _atlChange = "";

        [ObservableProperty]
        private string _tsbStatus = "";

        [ObservableProperty]
        private string _ctlChangeColor = "#4CAF50";

        [ObservableProperty]
        private string _atlChangeColor = "#4CAF50";

        [ObservableProperty]
        private string _tsbBackground = "#E8F5E9";

        [ObservableProperty]
        private string _tsbForeground = "#4CAF50";

        [ObservableProperty]
        private string _pmcLastUpdate = "";

        [ObservableProperty]
        private ObservableCollection<ISeries> _pmcSeries = new();

        [ObservableProperty]
        private Axis[] _pmcXAxes = Array.Empty<Axis>();

        [ObservableProperty]
        private Axis[] _pmcYAxes = Array.Empty<Axis>();

        [ObservableProperty]
        private ObservableCollection<WeeklySummary> _weeklySummaries = new();

        [ObservableProperty]
        private bool _showEmptyState = false;

        public DashboardViewModel()
        {
            _pmcService = ServiceInitializer.PMCService;
            _importService = ServiceInitializer.ImportService;
            _trainingRepository = ServiceInitializer.TrainingRepository;

            // Set current date
            var culture = new CultureInfo("pl-PL");
            var now = DateTime.Now;
            CurrentDateText = $"{culture.DateTimeFormat.GetDayName(now.DayOfWeek)}, {now:d MMMM yyyy}";

            // Load dashboard data
            _ = LoadDashboardDataAsync();
        }

        private async Task LoadDashboardDataAsync()
        {
            try
            {
                // Get default user (for now, assume user ID = 1)
                int userId = 1;

                // Load PMC data
                await LoadPMCDataAsync(userId);

                // Load weekly summaries
                await LoadWeeklySummariesAsync(userId);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd podczas ładowania danych: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadPMCDataAsync(int userId)
        {
            try
            {
                var today = DateTime.Today;
                var pmcData = await _pmcService.GetPMCDataAsync(userId, today.AddDays(-90), today);

                if (pmcData != null && pmcData.Any())
                {
                    var latest = pmcData.OrderByDescending(p => p.Date).FirstOrDefault();
                    if (latest != null)
                    {
                        CTL = latest.CTL;
                        ATL = latest.ATL;
                        TSB = latest.TSB;

                        // Calculate changes (from yesterday)
                        var yesterday = pmcData.OrderByDescending(p => p.Date).Skip(1).FirstOrDefault();
                        if (yesterday != null)
                        {
                            var ctlDiff = CTL - yesterday.CTL;
                            var atlDiff = ATL - yesterday.ATL;

                            CTLChange = $"{(ctlDiff >= 0 ? "▲" : "▼")} {Math.Abs(ctlDiff):F1}";
                            CTLChangeColor = ctlDiff >= 0 ? "#4CAF50" : "#F44336";

                            ATLChange = $"{(atlDiff >= 0 ? "▲" : "▼")} {Math.Abs(atlDiff):F1}";
                            ATLChangeColor = atlDiff >= 0 ? "#4CAF50" : "#F44336";
                        }

                        // Set TSB status and colors
                        if (TSB > 25)
                        {
                            TSBStatus = "Świeży";
                            TSBBackground = "#E8F5E9";
                            TSBForeground = "#4CAF50";
                        }
                        else if (TSB >= -10)
                        {
                            TSBStatus = "Optymalny";
                            TSBBackground = "#FFF9C4";
                            TSBForeground = "#F57F17";
                        }
                        else
                        {
                            TSBStatus = "Przeciążony";
                            TSBBackground = "#FFEBEE";
                            TSBForeground = "#F44336";
                        }

                        PMCLastUpdate = $"Ostatnia aktualizacja: {DateTime.Now:d MMMM yyyy, HH:mm}";
                    }

                    // Create PMC chart
                    CreatePMCChart(pmcData);
                }
                else
                {
                    // No data
                    CTL = 0;
                    ATL = 0;
                    TSB = 0;
                    PMCLastUpdate = "Brak treningów w tym okresie";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading PMC data: {ex.Message}");
            }
        }

        private void CreatePMCChart(IEnumerable<PMCData> pmcData)
        {
            var orderedData = pmcData.OrderBy(p => p.Date).ToList();

            PMCSeries = new ObservableCollection<ISeries>
            {
                new LineSeries<double>
                {
                    Name = "Fitness (CTL)",
                    Values = orderedData.Select(p => p.CTL).ToArray(),
                    Stroke = new SolidColorPaint(SKColors.Blue) { StrokeThickness = 3 },
                    Fill = null,
                    GeometrySize = 0
                },
                new LineSeries<double>
                {
                    Name = "Fatigue (ATL)",
                    Values = orderedData.Select(p => p.ATL).ToArray(),
                    Stroke = new SolidColorPaint(SKColors.Orange) { StrokeThickness = 2 },
                    Fill = null,
                    GeometrySize = 0
                },
                new LineSeries<double>
                {
                    Name = "Form (TSB)",
                    Values = orderedData.Select(p => p.TSB).ToArray(),
                    Stroke = new SolidColorPaint(SKColors.Gray) { StrokeThickness = 2 },
                    Fill = new SolidColorPaint(new SKColor(128, 128, 128, 50)),
                    GeometrySize = 0
                }
            };

            PMCXAxes = new Axis[]
            {
                new Axis
                {
                    Name = "Data",
                    Labels = orderedData.Select(p => p.Date.ToString("dd MMM")).ToArray(),
                    NamePaint = new SolidColorPaint(SKColors.Black),
                    LabelsPaint = new SolidColorPaint(SKColors.Gray),
                    TextSize = 10
                }
            };

            PMCYAxes = new Axis[]
            {
                new Axis
                {
                    Name = "TSS",
                    NamePaint = new SolidColorPaint(SKColors.Black),
                    LabelsPaint = new SolidColorPaint(SKColors.Gray),
                    TextSize = 12
                }
            };
        }

        private async Task LoadWeeklySummariesAsync(int userId)
        {
            try
            {
                WeeklySummaries.Clear();

                var today = DateTime.Today;
                var startDate = today.AddDays(-28); // Last 4 weeks

                var trainings = await _trainingRepository.GetTrainingsByDateRangeAsync(userId, startDate, today);

                if (!trainings.Any())
                {
                    ShowEmptyState = true;
                    return;
                }

                ShowEmptyState = false;

                // Group by week
                var weekGroups = trainings
                    .GroupBy(t => CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(
                        t.TrainingDate, CalendarWeekRule.FirstDay, DayOfWeek.Monday))
                    .OrderByDescending(g => g.Key)
                    .Take(4);

                foreach (var week in weekGroups)
                {
                    var weekTrainings = week.OrderBy(t => t.TrainingDate).ToList();
                    var firstDay = weekTrainings.First().TrainingDate;
                    var lastDay = weekTrainings.Last().TrainingDate;

                    var summary = new WeeklySummary
                    {
                        WeekRange = $"Tydzień {firstDay:d MMM} - {lastDay:d MMM}",
                        TotalTSS = weekTrainings.Sum(t => t.TSS ?? 0),
                        TrainingCount = weekTrainings.Count,
                        TotalTime = FormatDuration(weekTrainings.Sum(t => t.DurationSeconds)),
                        TotalDistance = weekTrainings.Sum(t => t.DistanceMeters ?? 0) / 1000,
                        AvgPower = weekTrainings.Average(t => t.AvgPower ?? 0),
                        AvgHeartRate = weekTrainings.Average(t => t.AvgHeartRate ?? 0),
                        ElevationGain = weekTrainings.Sum(t => t.ElevationGain ?? 0)
                    };

                    // Create day bars
                    var weekStart = firstDay.StartOfWeek(DayOfWeek.Monday);
                    for (int i = 0; i < 7; i++)
                    {
                        var day = weekStart.AddDays(i);
                        var dayTrainings = weekTrainings.Where(t => t.TrainingDate.Date == day.Date).ToList();

                        var dayBar = new DayBar
                        {
                            DayLabel = day.ToString("ddd", new CultureInfo("pl-PL")).Substring(0, 2),
                            BarHeight = 0,
                            BarColor = "#E0E0E0",
                            Tooltip = day.ToString("dd MMM")
                        };

                        if (dayTrainings.Any())
                        {
                            var dayTSS = dayTrainings.Sum(t => t.TSS ?? 0);
                            dayBar.BarHeight = Math.Min(dayTSS / 2, 100); // Scale to max 100px
                            dayBar.BarColor = GetZoneColor(dayTrainings.First());
                            dayBar.Tooltip = $"{day:dd MMM}: {dayTSS:F0} TSS";
                        }

                        summary.DayBars.Add(dayBar);
                    }

                    WeeklySummaries.Add(summary);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading weekly summaries: {ex.Message}");
            }
        }

        private string GetZoneColor(Training training)
        {
            if (training.AvgPower == null || training.FtpUsed == null || training.FtpUsed == 0)
                return "#9E9E9E";

            var percentFTP = (training.AvgPower.Value / training.FtpUsed.Value) * 100;

            if (percentFTP < 55) return "#9E9E9E"; // Z1
            if (percentFTP < 75) return "#4CAF50"; // Z2
            if (percentFTP < 90) return "#8BC34A"; // Z3
            if (percentFTP < 105) return "#FFEB3B"; // Z4
            if (percentFTP < 120) return "#FF9800"; // Z5
            return "#F44336"; // Z6+
        }

        private string FormatDuration(int totalSeconds)
        {
            var ts = TimeSpan.FromSeconds(totalSeconds);
            return $"{(int)ts.TotalHours}h {ts.Minutes}min";
        }

        [RelayCommand]
        private async Task AddTrainingAsync()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Pliki FIT (*.fit)|*.fit|Wszystkie pliki (*.*)|*.*",
                Title = "Wybierz plik FIT"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    // Parse FIT file first
                    var fitParser = new FitFileParser();
                    var fitData = fitParser.ParseFitFile(openFileDialog.FileName);

                    // Import training with parsed data
                    await _importService.ImportTrainingAsync(fitData, 1, openFileDialog.FileName);

                    // Reload dashboard
                    await LoadDashboardDataAsync();

                    MessageBox.Show("Trening został zaimportowany pomyślnie!", "Sukces",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Błąd podczas importu treningu: {ex.Message}", "Błąd",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        [RelayCommand]
        private void OpenSettings()
        {
            MessageBox.Show("Widok ustawień w budowie", "Informacja",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        [RelayCommand]
        private void ViewAllTrainings()
        {
            MessageBox.Show("Widok kalendarza w budowie", "Informacja",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    // Extension method for getting start of week
    public static class DateTimeExtensions
    {
        public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
            return dt.AddDays(-1 * diff).Date;
        }
    }
}
