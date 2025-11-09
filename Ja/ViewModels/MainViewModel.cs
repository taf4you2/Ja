using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Ja.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private ObservableObject _currentViewModel;
        public ObservableObject CurrentViewModel
        {
            get => _currentViewModel;
            set => SetProperty(ref _currentViewModel, value);
        }

        private string _currentView = "Dashboard";
        public string CurrentView
        {
            get => _currentView;
            set => SetProperty(ref _currentView, value);
        }

        public DashboardViewModel DashboardViewModel { get; }
        public SettingsViewModel SettingsViewModel { get; }
        public CalendarViewModel CalendarViewModel { get; }
        public TrainingAnalysisViewModel TrainingAnalysisViewModel { get; }
        public PowerCurveViewModel PowerCurveViewModel { get; }
        public PersonalRecordsViewModel PersonalRecordsViewModel { get; }

        public MainViewModel()
        {
            // Initialize all ViewModels
            DashboardViewModel = new DashboardViewModel();
            SettingsViewModel = new SettingsViewModel();
            CalendarViewModel = new CalendarViewModel();
            TrainingAnalysisViewModel = new TrainingAnalysisViewModel();
            PowerCurveViewModel = new PowerCurveViewModel();
            PersonalRecordsViewModel = new PersonalRecordsViewModel();

            // Set Dashboard as default
            _currentViewModel = DashboardViewModel;
        }

        [RelayCommand]
        private void NavigateToDashboard()
        {
            CurrentViewModel = DashboardViewModel;
            CurrentView = "Dashboard";
        }

        [RelayCommand]
        private void NavigateToSettings()
        {
            CurrentViewModel = SettingsViewModel;
            CurrentView = "Settings";
        }

        [RelayCommand]
        private void NavigateToCalendar()
        {
            CurrentViewModel = CalendarViewModel;
            CurrentView = "Calendar";
        }

        [RelayCommand]
        private void NavigateToTrainingAnalysis()
        {
            CurrentViewModel = TrainingAnalysisViewModel;
            CurrentView = "TrainingAnalysis";
        }

        [RelayCommand]
        private void NavigateToPowerCurve()
        {
            CurrentViewModel = PowerCurveViewModel;
            CurrentView = "PowerCurve";
        }

        [RelayCommand]
        private void NavigateToPersonalRecords()
        {
            CurrentViewModel = PersonalRecordsViewModel;
            CurrentView = "PersonalRecords";
        }
    }
}
