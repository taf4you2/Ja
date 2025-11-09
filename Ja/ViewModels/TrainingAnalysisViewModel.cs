using CommunityToolkit.Mvvm.ComponentModel;

namespace Ja.ViewModels
{
    public partial class TrainingAnalysisViewModel : ObservableObject
    {
        private int? _selectedTrainingId;
        public int? SelectedTrainingId
        {
            get => _selectedTrainingId;
            set => SetProperty(ref _selectedTrainingId, value);
        }

        public TrainingAnalysisViewModel()
        {
        }
    }
}
