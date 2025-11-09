using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Ja.Services;

namespace Ja.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        private string _selectedCategory = "Profil";
        public string SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (SetProperty(ref _selectedCategory, value))
                {
                    UpdateCategoryVisibility();
                }
            }
        }

        // Category visibility
        private bool _isProfileVisible = true;
        public bool IsProfileVisible
        {
            get => _isProfileVisible;
            set => SetProperty(ref _isProfileVisible, value);
        }

        private bool _isZonesVisible = false;
        public bool IsZonesVisible
        {
            get => _isZonesVisible;
            set => SetProperty(ref _isZonesVisible, value);
        }

        private bool _isAdvancedVisible = false;
        public bool IsAdvancedVisible
        {
            get => _isAdvancedVisible;
            set => SetProperty(ref _isAdvancedVisible, value);
        }

        private bool _isDataVisible = false;
        public bool IsDataVisible
        {
            get => _isDataVisible;
            set => SetProperty(ref _isDataVisible, value);
        }

        // Profile settings
        private string _firstName = "Jan";
        public string FirstName
        {
            get => _firstName;
            set => SetProperty(ref _firstName, value);
        }

        private string _lastName = "Kowalski";
        public string LastName
        {
            get => _lastName;
            set => SetProperty(ref _lastName, value);
        }

        private double _weight = 75.0;
        public double Weight
        {
            get => _weight;
            set => SetProperty(ref _weight, value);
        }

        private double _height = 180.0;
        public double Height
        {
            get => _height;
            set => SetProperty(ref _height, value);
        }

        private int _restingHeartRate = 55;
        public int RestingHeartRate
        {
            get => _restingHeartRate;
            set => SetProperty(ref _restingHeartRate, value);
        }

        private int _maxHeartRate = 185;
        public int MaxHeartRate
        {
            get => _maxHeartRate;
            set => SetProperty(ref _maxHeartRate, value);
        }

        // Power zones settings
        private double _ftp = 250.0;
        public double FTP
        {
            get => _ftp;
            set
            {
                if (SetProperty(ref _ftp, value))
                {
                    OnPropertyChanged(nameof(WattsPerKg));
                }
            }
        }

        public string WattsPerKg => Weight > 0 ? $"{(FTP / Weight):F2} W/kg" : "0.00 W/kg";

        // Commands
        public ICommand SelectCategoryCommand { get; }
        public ICommand SaveSettingsCommand { get; }

        public SettingsViewModel()
        {
            SelectCategoryCommand = new RelayCommand<string>(SelectCategory);
            SaveSettingsCommand = new RelayCommand(SaveSettings);
        }

        private void SelectCategory(string? category)
        {
            if (category != null)
            {
                SelectedCategory = category;
            }
        }

        private void UpdateCategoryVisibility()
        {
            IsProfileVisible = SelectedCategory == "Profil";
            IsZonesVisible = SelectedCategory == "Strefy";
            IsAdvancedVisible = SelectedCategory == "Zaawansowane";
            IsDataVisible = SelectedCategory == "Dane";
        }

        private void SaveSettings()
        {
            // TODO: Save settings to database
            System.Diagnostics.Debug.WriteLine("Settings saved");
        }
    }
}
