using CommunityToolkit.Mvvm.ComponentModel;

namespace Ja.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        private string _selectedCategory = "Profil";
        public string SelectedCategory
        {
            get => _selectedCategory;
            set => SetProperty(ref _selectedCategory, value);
        }

        public SettingsViewModel()
        {
        }
    }
}
