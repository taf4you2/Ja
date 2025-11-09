using System.Configuration;
using System.Data;
using System.Windows;
using Ja.Services;

namespace Ja
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Inicjalizacja bazy danych przy starcie aplikacji
            // Baza zostanie utworzona automatycznie jeśli nie istnieje
            var db = ServiceInitializer.DbContext;
            db.Database.EnsureCreated();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // Sprzątanie zasobów przy zamknięciu aplikacji
            ServiceInitializer.Cleanup();
            base.OnExit(e);
        }
    }

}
