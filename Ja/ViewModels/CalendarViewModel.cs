using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Ja.ViewModels
{
    public partial class CalendarViewModel : ObservableObject
    {
        private DateTime _currentMonth = DateTime.Today;
        public DateTime CurrentMonth
        {
            get => _currentMonth;
            set => SetProperty(ref _currentMonth, value);
        }

        private string _viewMode = "Month"; // Month, Week, List
        public string ViewMode
        {
            get => _viewMode;
            set => SetProperty(ref _viewMode, value);
        }

        public CalendarViewModel()
        {
        }
    }
}
