using System.Configuration;
using System.Data;
using System.Windows;
using Microsoft.EntityFrameworkCore;
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

            try
            {
                // Inicjalizacja bazy danych przy starcie aplikacji
                var db = ServiceInitializer.DbContext;

                // Pobierz ścieżkę do bazy danych
                var connection = db.Database.GetDbConnection();
                var dbPath = connection.DataSource;

                System.Diagnostics.Debug.WriteLine($"=== INICJALIZACJA BAZY DANYCH ===");
                System.Diagnostics.Debug.WriteLine($"Ścieżka do bazy: {dbPath}");

                // Zastosuj migracje
                db.Database.Migrate();

                System.Diagnostics.Debug.WriteLine($"Migracja zakończona pomyślnie!");
                System.Diagnostics.Debug.WriteLine($"Baza istnieje: {System.IO.File.Exists(dbPath)}");

                // Opcjonalnie: pokaż MessageBox dla użytkownika
                MessageBox.Show(
                    $"Baza danych została zainicjalizowana!\n\nLokalizacja:\n{dbPath}\n\nPlik istnieje: {System.IO.File.Exists(dbPath)}",
                    "JA Training - Inicjalizacja",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"BŁĄD INICJALIZACJI BAZY: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");

                MessageBox.Show(
                    $"Błąd podczas inicjalizacji bazy danych:\n\n{ex.Message}\n\n{ex.InnerException?.Message}",
                    "Błąd",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );

                throw;
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // Sprzątanie zasobów przy zamknięciu aplikacji
            ServiceInitializer.Cleanup();
            base.OnExit(e);
        }
    }

}
