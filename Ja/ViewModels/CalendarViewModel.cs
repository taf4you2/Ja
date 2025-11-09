using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Ja.Models;
using Ja.Repositories;
using Ja.Services;

namespace Ja.ViewModels
{
    public partial class CalendarViewModel : ObservableObject
    {
        private readonly TrainingRepository _trainingRepository;
        private readonly CultureInfo _polishCulture = new CultureInfo("pl-PL");

        private DateTime _currentMonth = DateTime.Today;
        public DateTime CurrentMonth
        {
            get => _currentMonth;
            set
            {
                if (SetProperty(ref _currentMonth, value))
                {
                    UpdateMonthDisplay();
                    _ = LoadCalendarDataAsync();
                }
            }
        }

        private string _monthYearText = string.Empty;
        public string MonthYearText
        {
            get => _monthYearText;
            set => SetProperty(ref _monthYearText, value);
        }

        private string _viewMode = "Month"; // Month, Week, List
        public string ViewMode
        {
            get => _viewMode;
            set
            {
                if (SetProperty(ref _viewMode, value))
                {
                    OnPropertyChanged(nameof(IsMonthView));
                    OnPropertyChanged(nameof(IsWeekView));
                    OnPropertyChanged(nameof(IsListView));
                }
            }
        }

        public bool IsMonthView => ViewMode == "Month";
        public bool IsWeekView => ViewMode == "Week";
        public bool IsListView => ViewMode == "List";

        private ObservableCollection<CalendarDay> _calendarDays = new ObservableCollection<CalendarDay>();
        public ObservableCollection<CalendarDay> CalendarDays
        {
            get => _calendarDays;
            set => SetProperty(ref _calendarDays, value);
        }

        private ObservableCollection<CalendarTraining> _listViewTrainings = new ObservableCollection<CalendarTraining>();
        public ObservableCollection<CalendarTraining> ListViewTrainings
        {
            get => _listViewTrainings;
            set => SetProperty(ref _listViewTrainings, value);
        }

        // Commands
        public ICommand PreviousMonthCommand { get; }
        public ICommand NextMonthCommand { get; }
        public ICommand GoToTodayCommand { get; }
        public ICommand SetViewModeCommand { get; }
        public ICommand DayClickedCommand { get; }

        public CalendarViewModel()
        {
            _trainingRepository = ServiceInitializer.TrainingRepository;

            // Initialize commands
            PreviousMonthCommand = new RelayCommand(PreviousMonth);
            NextMonthCommand = new RelayCommand(NextMonth);
            GoToTodayCommand = new RelayCommand(GoToToday);
            SetViewModeCommand = new RelayCommand<string>(SetViewMode);
            DayClickedCommand = new RelayCommand<CalendarDay>(DayClicked);

            UpdateMonthDisplay();
            _ = LoadCalendarDataAsync();
        }

        private void UpdateMonthDisplay()
        {
            MonthYearText = _currentMonth.ToString("MMMM yyyy", _polishCulture);
            // Capitalize first letter
            if (!string.IsNullOrEmpty(MonthYearText))
            {
                MonthYearText = char.ToUpper(MonthYearText[0]) + MonthYearText.Substring(1);
            }
        }

        private async Task LoadCalendarDataAsync()
        {
            try
            {
                // Get first and last day of the calendar view (including days from prev/next month)
                var firstDayOfMonth = new DateTime(_currentMonth.Year, _currentMonth.Month, 1);
                var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

                // Get the Monday of the week containing the first day
                var startDate = firstDayOfMonth;
                while (startDate.DayOfWeek != DayOfWeek.Monday)
                {
                    startDate = startDate.AddDays(-1);
                }

                // Get the Sunday of the week containing the last day
                var endDate = lastDayOfMonth;
                while (endDate.DayOfWeek != DayOfWeek.Sunday)
                {
                    endDate = endDate.AddDays(1);
                }

                // Load trainings for this period (userId = 1 for now)
                var trainings = await _trainingRepository.GetTrainingsByDateRangeAsync(1, startDate, endDate);

                // Generate calendar days
                var days = new ObservableCollection<CalendarDay>();
                var currentDate = startDate;

                while (currentDate <= endDate)
                {
                    var dayTrainings = trainings.Where(t => t.TrainingDate.Date == currentDate.Date).ToList();

                    var calendarDay = new CalendarDay
                    {
                        Date = currentDate,
                        DayNumber = currentDate.Day,
                        IsCurrentMonth = currentDate.Month == _currentMonth.Month,
                        IsToday = currentDate.Date == DateTime.Today,
                        HasTrainings = dayTrainings.Any(),
                        TrainingCount = dayTrainings.Count
                    };

                    if (dayTrainings.Any())
                    {
                        calendarDay.TotalTSS = dayTrainings.Sum(t => t.TSS ?? 0);
                        var totalMinutes = dayTrainings.Sum(t => t.Duration ?? 0);
                        var hours = totalMinutes / 60;
                        var minutes = totalMinutes % 60;
                        calendarDay.DurationText = hours > 0 ? $"{hours}h {minutes}min" : $"{minutes}min";

                        // Determine dominant zone color (simplified - use first training's zone)
                        var firstTraining = dayTrainings.First();
                        calendarDay.DominantZoneColor = GetZoneColor(firstTraining.DominantZone ?? 2);

                        // Add trainings
                        foreach (var training in dayTrainings)
                        {
                            calendarDay.Trainings.Add(new CalendarTraining
                            {
                                Id = training.Id,
                                Name = training.Name ?? "Trening",
                                StartTime = training.TrainingDate,
                                Duration = FormatDuration(training.Duration ?? 0),
                                TSS = training.TSS ?? 0,
                                ZoneColor = GetZoneColor(training.DominantZone ?? 2)
                            });
                        }
                    }

                    days.Add(calendarDay);
                    currentDate = currentDate.AddDays(1);
                }

                CalendarDays = days;

                // Update list view
                if (ViewMode == "List")
                {
                    var listTrainings = new ObservableCollection<CalendarTraining>();
                    foreach (var day in days.Where(d => d.HasTrainings && d.IsCurrentMonth).OrderByDescending(d => d.Date))
                    {
                        foreach (var training in day.Trainings)
                        {
                            listTrainings.Add(training);
                        }
                    }
                    ListViewTrainings = listTrainings;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading calendar data: {ex.Message}");
            }
        }

        private string GetZoneColor(int zone)
        {
            return zone switch
            {
                1 => "#808080", // Recovery
                2 => "#4169E1", // Endurance
                3 => "#32CD32", // Tempo
                4 => "#FFD700", // Threshold
                5 => "#FF8C00", // VO2max
                6 => "#FF4500", // Anaerobic
                7 => "#8B0000", // Neuromuscular
                _ => "#1976D2"
            };
        }

        private string FormatDuration(int minutes)
        {
            var hours = minutes / 60;
            var mins = minutes % 60;
            return hours > 0 ? $"{hours}h {mins}min" : $"{mins}min";
        }

        private void PreviousMonth()
        {
            CurrentMonth = CurrentMonth.AddMonths(-1);
        }

        private void NextMonth()
        {
            CurrentMonth = CurrentMonth.AddMonths(1);
        }

        private void GoToToday()
        {
            CurrentMonth = DateTime.Today;
        }

        private void SetViewMode(string? mode)
        {
            if (mode != null)
            {
                ViewMode = mode;
                if (mode == "List")
                {
                    _ = LoadCalendarDataAsync();
                }
            }
        }

        private void DayClicked(CalendarDay? day)
        {
            if (day != null && day.HasTrainings)
            {
                // TODO: Navigate to training analysis or show day details
                System.Diagnostics.Debug.WriteLine($"Clicked day: {day.Date:yyyy-MM-dd}, Trainings: {day.TrainingCount}");
            }
        }
    }
}
