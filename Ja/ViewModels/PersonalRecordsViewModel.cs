using CommunityToolkit.Mvvm.ComponentModel;

namespace Ja.ViewModels
{
    public partial class PersonalRecordsViewModel : ObservableObject
    {
        private string _recordType = "Power"; // Power, HeartRate, Distance, Speed
        public string RecordType
        {
            get => _recordType;
            set => SetProperty(ref _recordType, value);
        }

        public PersonalRecordsViewModel()
        {
        }
    }
}
