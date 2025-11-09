using CommunityToolkit.Mvvm.ComponentModel;

namespace Ja.ViewModels
{
    public partial class PowerCurveViewModel : ObservableObject
    {
        private string _timeRange = "All"; // All, 90Days, 30Days, CurrentYear
        public string TimeRange
        {
            get => _timeRange;
            set => SetProperty(ref _timeRange, value);
        }

        public PowerCurveViewModel()
        {
        }
    }
}
