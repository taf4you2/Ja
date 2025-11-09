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

                // Sprawdź czy baza istnieje
                bool databaseExisted = System.IO.File.Exists(dbPath);
                System.Diagnostics.Debug.WriteLine($"Baza istniała przed migracją: {databaseExisted}");

                // OPCJA 1: Jeśli baza istnieje, usuń ją (tylko do testów!)
                // Usuń to po pierwszym uruchomieniu gdy wszystko zadziała
                if (databaseExisted)
                {
                    var result = MessageBox.Show(
                        $"Znaleziono starą bazę danych:\n{dbPath}\n\nCzy chcesz ją usunąć i utworzyć nową z migracjami?\n\n(Wybierz 'Tak' aby usunąć starą bazę i utworzyć nową)",
                        "JA Training - Stara baza danych",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question
                    );

                    if (result == MessageBoxResult.Yes)
                    {
                        db.Database.EnsureDeleted();
                        System.Diagnostics.Debug.WriteLine($"Stara baza została usunięta");
                    }
                }

                // Zastosuj migracje
                db.Database.Migrate();

                System.Diagnostics.Debug.WriteLine($"Migracja zakończona pomyślnie!");
                System.Diagnostics.Debug.WriteLine($"Baza istnieje: {System.IO.File.Exists(dbPath)}");

                // Inicjalizuj domyślnego użytkownika przy pierwszym uruchomieniu
                var initService = ServiceInitializer.DataInitializationService;
                initService.InitializeDefaultUserIfNeededAsync().Wait();

                // Pokaż MessageBox tylko przy pierwszym utworzeniu
                if (!databaseExisted)
                {
                    MessageBox.Show(
                        $"Baza danych została utworzona!\n\nLokalizacja:\n{dbPath}\n\nUtworzono domyślnego użytkownika z podstawowymi ustawieniami.",
                        "JA Training - Inicjalizacja",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );
                }
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
