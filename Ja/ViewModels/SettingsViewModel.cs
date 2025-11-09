using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Ja.Database.Entities;
using Ja.Repositories;
using Ja.Services;

namespace Ja.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        private readonly UserRepository _userRepository;
        private readonly FTPHistoryRepository _ftpHistoryRepository;
        private readonly WeightHistoryRepository _weightHistoryRepository;
        private User? _currentUser;

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
        private string _firstName = "";
        public string FirstName
        {
            get => _firstName;
            set => SetProperty(ref _firstName, value);
        }

        private string _lastName = "";
        public string LastName
        {
            get => _lastName;
            set => SetProperty(ref _lastName, value);
        }

        private DateTime? _dateOfBirth;
        public DateTime? DateOfBirth
        {
            get => _dateOfBirth;
            set => SetProperty(ref _dateOfBirth, value);
        }

        private string _gender = "Male";
        public string Gender
        {
            get => _gender;
            set => SetProperty(ref _gender, value);
        }

        private double _weight = 75.0;
        public double Weight
        {
            get => _weight;
            set
            {
                if (SetProperty(ref _weight, value))
                {
                    OnPropertyChanged(nameof(WattsPerKg));
                }
            }
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
                    UpdatePowerZoneRanges();
                }
            }
        }

        public string WattsPerKg => Weight > 0 ? $"{(FTP / Weight):F2} W/kg" : "0.00 W/kg";

        private DateTime _ftpTestDate = DateTime.Today;
        public DateTime FTPTestDate
        {
            get => _ftpTestDate;
            set => SetProperty(ref _ftpTestDate, value);
        }

        private ObservableCollection<PowerZone> _powerZones = new();
        public ObservableCollection<PowerZone> PowerZones
        {
            get => _powerZones;
            set => SetProperty(ref _powerZones, value);
        }

        private ObservableCollection<FTPHistory> _ftpHistory = new();
        public ObservableCollection<FTPHistory> FTPHistory
        {
            get => _ftpHistory;
            set => SetProperty(ref _ftpHistory, value);
        }

        private ObservableCollection<WeightHistory> _weightHistory = new();
        public ObservableCollection<WeightHistory> WeightHistory
        {
            get => _weightHistory;
            set => SetProperty(ref _weightHistory, value);
        }

        // Commands
        public ICommand SelectCategoryCommand { get; }
        public ICommand SaveSettingsCommand { get; }
        public ICommand AddFTPCommand { get; }
        public ICommand AddWeightCommand { get; }

        public SettingsViewModel()
        {
            _userRepository = ServiceInitializer.UserRepository;
            _ftpHistoryRepository = ServiceInitializer.FTPHistoryRepository;
            _weightHistoryRepository = ServiceInitializer.WeightHistoryRepository;

            SelectCategoryCommand = new RelayCommand<string>(SelectCategory);
            SaveSettingsCommand = new RelayCommand(async () => await SaveSettingsAsync());
            AddFTPCommand = new RelayCommand(async () => await AddFTPAsync());
            AddWeightCommand = new RelayCommand(async () => await AddWeightAsync());

            _ = LoadUserDataAsync();
        }

        private async Task LoadUserDataAsync()
        {
            try
            {
                _currentUser = await _userRepository.GetDefaultUserAsync();

                if (_currentUser != null)
                {
                    FirstName = _currentUser.Name ?? "";
                    LastName = _currentUser.Surname ?? "";
                    DateOfBirth = _currentUser.DateOfBirth;
                    Gender = _currentUser.Gender ?? "Male";
                    Weight = _currentUser.Weight ?? 75.0;
                    Height = _currentUser.Height ?? 180.0;
                    RestingHeartRate = _currentUser.RestingHeartRate ?? 55;
                    MaxHeartRate = _currentUser.MaxHeartRate ?? 185;

                    // Load latest FTP
                    var latestFTP = await _ftpHistoryRepository.GetLatestForUserAsync(_currentUser.Id);
                    if (latestFTP != null)
                    {
                        FTP = latestFTP.FtpValue;
                        FTPTestDate = latestFTP.TestDate;
                    }

                    // Load FTP history
                    var ftpList = await _ftpHistoryRepository.GetAllForUserAsync(_currentUser.Id);
                    FTPHistory = new ObservableCollection<FTPHistory>(ftpList);

                    // Load weight history
                    var weightList = await _weightHistoryRepository.GetAllForUserAsync(_currentUser.Id);
                    WeightHistory = new ObservableCollection<WeightHistory>(weightList);

                    // Load power zones
                    var zones = _currentUser.PowerZones?.OrderBy(z => z.ZoneNumber).ToList();
                    if (zones != null && zones.Any())
                    {
                        PowerZones = new ObservableCollection<PowerZone>(zones);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading user data: {ex.Message}");
            }
        }

        private async Task SaveSettingsAsync()
        {
            try
            {
                if (_currentUser == null)
                    return;

                _currentUser.Name = FirstName;
                _currentUser.Surname = LastName;
                _currentUser.DateOfBirth = DateOfBirth;
                _currentUser.Gender = Gender;
                _currentUser.Weight = Weight;
                _currentUser.Height = Height;
                _currentUser.RestingHeartRate = RestingHeartRate;
                _currentUser.MaxHeartRate = MaxHeartRate;

                await _userRepository.UpdateUserAsync(_currentUser);

                MessageBox.Show("Ustawienia zostały zapisane!", "Sukces",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd podczas zapisywania ustawień: {ex.Message}", "Błąd",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task AddFTPAsync()
        {
            try
            {
                if (_currentUser == null)
                    return;

                var ftpEntry = new FTPHistory
                {
                    UserId = _currentUser.Id,
                    FtpValue = FTP,
                    WeightAtTest = Weight,
                    TestDate = FTPTestDate,
                    Source = "manual",
                    Notes = ""
                };

                await _ftpHistoryRepository.AddAsync(ftpEntry);

                // Reload FTP history
                var ftpList = await _ftpHistoryRepository.GetAllForUserAsync(_currentUser.Id);
                FTPHistory = new ObservableCollection<FTPHistory>(ftpList);

                MessageBox.Show("FTP zostało dodane do historii!", "Sukces",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd podczas dodawania FTP: {ex.Message}", "Błąd",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task AddWeightAsync()
        {
            try
            {
                if (_currentUser == null)
                    return;

                var weightEntry = new WeightHistory
                {
                    UserId = _currentUser.Id,
                    Weight = Weight,
                    MeasurementDate = DateTime.Today,
                    Notes = ""
                };

                await _weightHistoryRepository.AddAsync(weightEntry);

                // Update user's current weight
                _currentUser.Weight = Weight;
                await _userRepository.UpdateUserAsync(_currentUser);

                // Reload weight history
                var weightList = await _weightHistoryRepository.GetAllForUserAsync(_currentUser.Id);
                WeightHistory = new ObservableCollection<WeightHistory>(weightList);

                MessageBox.Show("Waga została dodana do historii!", "Sukces",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd podczas dodawania wagi: {ex.Message}", "Błąd",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdatePowerZoneRanges()
        {
            foreach (var zone in PowerZones)
            {
                // Calculate watt ranges based on FTP and percentages
                zone.MinWatts = (int)(FTP * zone.MinPercent / 100);
                zone.MaxWatts = (int)(FTP * zone.MaxPercent / 100);
            }
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
    }
}
