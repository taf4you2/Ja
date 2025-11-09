# Ja.TestConsole - Testowa aplikacja do parsowania plików FIT

Prosta aplikacja konsolowa do testowania i weryfikacji parsera plików FIT.

## Wymagania

- .NET 8.0 SDK
- Dynastream.Fit.dll w katalogu `lib/FitSDK/`

## Instalacja

```bash
# 1. Pobierz FIT SDK i skopiuj DLL (jeśli jeszcze tego nie zrobiono)
cp FitSDK/cs/Dynastream.Fit.dll lib/FitSDK/Dynastream.Fit.dll

# 2. Zbuduj projekt
dotnet build Ja.TestConsole
```

## Użycie

### Opcja 1: Z argumentem wiersza poleceń

```bash
dotnet run --project Ja.TestConsole -- ścieżka/do/pliku.fit
```

### Opcja 2: Interaktywnie

```bash
dotnet run --project Ja.TestConsole
# Następnie podaj ścieżkę do pliku gdy zostaniesz o to poproszony
```

### Przykłady

```bash
# Windows
dotnet run --project Ja.TestConsole -- "C:\Treningi\activity.fit"

# Linux/Mac
dotnet run --project Ja.TestConsole -- "/home/user/activity.fit"

# Bieżący katalog
dotnet run --project Ja.TestConsole -- activity.fit
```

## Co aplikacja wyświetla?

### 1. Statystyki pliku
- Nazwa pliku
- Czas trwania
- Liczba punktów danych
- Średnia moc
- FTP (wykryte lub oszacowane)
- Średnie tętno
- Dystans
- Średnia prędkość

### 2. Walidacja
- Czy plik zawiera dane mocy
- Czy plik ma minimalną długość (120s)

### 3. Dane mocy (jeśli dostępne)
- Liczba punktów z mocą > 0
- Maksymalna moc
- Minimalna moc
- Pierwsze 10 punktów danych

### 4. Informacje o czasie
- Data i czas rozpoczęcia
- Data i czas zakończenia
- Sformatowany czas trwania

## Przykładowy output

```
=== Test Parsera Plików FIT ===

Wczytywanie pliku: C:\Treningi\workout_2024.fit

=== STATYSTYKI PLIKU ===
Plik: workout_2024.fit
Czas trwania: 01:15:30
Punkty danych: 4530
Średnia moc: 245.3 W
FTP: 280 W
Średnie tętno: 152 bpm
Dystans: 35.42 km
Średnia prędkość: 28.1 km/h

=== WALIDACJA ===
Czy plik ma dane mocy: TAK
Czy plik ma minimalną długość (120s): TAK

=== DANE MOCY ===
Liczba punktów z mocą > 0: 4485
Maksymalna moc: 680 W
Minimalna moc (>0): 85 W

Pierwsze 10 punktów mocy:
  [0] 120.0 W
  [1] 135.0 W
  [2] 142.0 W
  [3] 148.0 W
  [4] 155.0 W
  [5] 160.0 W
  [6] 165.0 W
  [7] 170.0 W
  [8] 175.0 W
  [9] 180.0 W

=== CZAS ===
Start: 2024-10-15 08:30:15
Koniec: 2024-10-15 09:45:45
Czas trwania: 01:15:30

✅ Plik FIT został pomyślnie wczytany i zwalidowany!

Naciśnij dowolny klawisz, aby zakończyć...
```

## Typowe problemy

### Błąd: "Plik nie istnieje"
- Sprawdź ścieżkę do pliku
- Użyj cudzysłowów dla ścieżek ze spacjami
- Upewnij się że plik ma rozszerzenie .fit

### Błąd: "Metadata file ... Dynastream.Fit.dll could not be found"
- Pobierz FIT SDK: https://developer.garmin.com/fit/download/
- Skopiuj DLL do `lib/FitSDK/Dynastream.Fit.dll`
- Przebuduj projekt: `dotnet build Ja.TestConsole`

### Błąd: "To nie jest prawidłowy plik FIT"
- Sprawdź czy plik nie jest uszkodzony
- Upewnij się że to rzeczywiście plik FIT (nie GPX, TCX, etc.)

### Ostrzeżenie: "Plik nie zawiera danych mocy"
- Trening nie był nagrywany z czujnikiem mocy
- Plik pochodzi z innego typu aktywności (bieg, pływanie)
- Analiza interwałów nie będzie możliwa

## Debugowanie

Aplikacja wyświetla szczegółowe informacje o błędach, w tym:
- Typ wyjątku
- Komunikat błędu
- Stos wywołań (dla nieoczekiwanych błędów)

## Rozwój

### Dodanie nowych funkcji testowych

Edytuj `Program.cs` i dodaj własne testy:

```csharp
// Przykład: Test wykrywania interwałów
if (parser.HasPowerData(fitData))
{
    var algorithm = new IntervalDetectionAlgorithm();
    var (intervals, recoveries) = algorithm.DetectAllIntervals(
        fitData.PowerData,
        fitData.Ftp);

    Console.WriteLine($"\nWykryto {intervals.Count} interwałów");
}
```

## Zależności

- **Ja** - Główny projekt (FitFileParser, Models)
- **Dynastream.Fit.dll** - Biblioteka do parsowania plików FIT

## Licencja

Zgodna z licencją projektu głównego (MIT)
