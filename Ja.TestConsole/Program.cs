using System;
using System.IO;
using Ja.Services;
using Ja.Models;

namespace Ja.TestConsole
{
    /// <summary>
    /// Testowa aplikacja konsolowa do weryfikacji parsowania plików FIT
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Test Parsera Plików FIT ===\n");

            string fitFilePath;

            // Sprawdzenie argumentów
            if (args.Length > 0)
            {
                fitFilePath = args[0];
            }
            else
            {
                Console.Write("Podaj ścieżkę do pliku FIT: ");
                fitFilePath = Console.ReadLine()?.Trim() ?? "";
            }

            // Usunięcie cudzysłowów jeśli są
            fitFilePath = fitFilePath.Trim('"');

            // Sprawdzenie czy plik istnieje
            if (string.IsNullOrEmpty(fitFilePath) || !File.Exists(fitFilePath))
            {
                Console.WriteLine($"Błąd: Plik '{fitFilePath}' nie istnieje.");
                Console.WriteLine("\nUżycie:");
                Console.WriteLine("  dotnet run --project Ja.TestConsole <ścieżka_do_pliku.fit>");
                Console.WriteLine("\nLub uruchom bez argumentów i podaj ścieżkę interaktywnie.");
                Console.WriteLine("\nNaciśnij dowolny klawisz, aby zakończyć...");
                Console.ReadKey();
                return;
            }

            try
            {
                // Parsowanie pliku FIT
                Console.WriteLine($"Wczytywanie pliku: {fitFilePath}\n");
                var parser = new FitFileParser();
                var fitData = parser.ParseFitFile(fitFilePath);

                // Wyświetlenie statystyk
                Console.WriteLine("=== STATYSTYKI PLIKU ===");
                Console.WriteLine(parser.GetFileStatistics(fitData));

                // Walidacja danych
                Console.WriteLine("\n=== WALIDACJA ===");
                Console.WriteLine($"Czy plik ma dane mocy: {(parser.HasPowerData(fitData) ? "TAK" : "NIE")}");
                Console.WriteLine($"Czy plik ma minimalną długość (120s): {(parser.ValidateMinimumDuration(fitData) ? "TAK" : "NIE")}");

                // Szczegóły danych mocy
                if (parser.HasPowerData(fitData))
                {
                    Console.WriteLine("\n=== DANE MOCY ===");
                    Console.WriteLine($"Liczba punktów z mocą > 0: {fitData.PowerData.Count(p => p > 0)}");
                    Console.WriteLine($"Maksymalna moc: {fitData.PowerData.Max():F0} W");
                    Console.WriteLine($"Minimalna moc (>0): {fitData.PowerData.Where(p => p > 0).DefaultIfEmpty(0).Min():F0} W");

                    // Pierwsze 10 punktów danych
                    Console.WriteLine("\nPierwsze 10 punktów mocy:");
                    for (int i = 0; i < Math.Min(10, fitData.PowerData.Length); i++)
                    {
                        Console.WriteLine($"  [{i}] {fitData.PowerData[i]:F1} W");
                    }
                }
                else
                {
                    Console.WriteLine("\n⚠️  UWAGA: Plik nie zawiera danych mocy!");
                    Console.WriteLine("Analiza interwałów nie będzie możliwa.");
                }

                // Informacje o czasie
                Console.WriteLine("\n=== CZAS ===");
                Console.WriteLine($"Start: {fitData.StartTime}");
                Console.WriteLine($"Koniec: {fitData.EndTime}");
                Console.WriteLine($"Czas trwania: {fitData.DurationFormatted}");

                Console.WriteLine("\n✅ Plik FIT został pomyślnie wczytany i zwalidowany!");
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine($"❌ Błąd: Nie znaleziono pliku - {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"❌ Błąd parsowania: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Nieoczekiwany błąd: {ex.Message}");
                Console.WriteLine($"\nStos wywołań:\n{ex.StackTrace}");
            }

            Console.WriteLine("\nNaciśnij dowolny klawisz, aby zakończyć...");
            Console.ReadKey();
        }
    }
}
