# JA Training - Przewodnik Integracji

## Przegląd zaimplementowanych funkcji

Aplikacja została rozszerzona o kompleksową obsługę bazy danych i zaawansowane obliczenia zgodnie ze specyfikacją JA Training.

### Nowe komponenty:

1. **Warstwa bazodanowa (Database)** - 11 tabel encji
2. **Repozytoria (Repositories)** - wzorzec Repository Pattern
3. **Serwisy (Services)** - logika biznesowa i obliczenia
4. **Inicjalizator serwisów (ServiceInitializer)** - singleton pattern

---

## Szybki Start

### 1. Dostęp do serwisów w całej aplikacji

```csharp
using Ja.Services;

// W dowolnym miejscu aplikacji możesz użyć:
var db = ServiceInitializer.DbContext;
var user = await ServiceInitializer.UserRepository.GetDefaultUserAsync();
var metricsService = ServiceInitializer.MetricsService;
```

### 2. Import treningu do bazy danych

```csharp
// Po sparsowaniu pliku FIT:
var fitParser = new FitFileParser();
var fitData = fitParser.ParseFitFile(filePath);

// Import do bazy danych (automatycznie oblicza metryki, wykrywa interwały i rekordy):
var user = await ServiceInitializer.UserRepository.GetDefaultUserAsync();
var training = await ServiceInitializer.ImportService.ImportTrainingAsync(
    fitData,
    user.Id,
    filePath
);

Console.WriteLine($"Zaimportowano trening: TSS={training.TSS:F0}, IF={training.IntensityFactor:F2}");
```

### 3. Obliczanie metryk ręcznie

```csharp
var metricsService = ServiceInitializer.MetricsService;
var metrics = metricsService.CalculateAllMetrics(
    powerData: fitData.PowerData,
    durationSeconds: (int)fitData.Duration.TotalSeconds,
    ftp: 285
);

Console.WriteLine($"NP: {metrics.NormalizedPower:F0} W");
Console.WriteLine($"TSS: {metrics.TSS:F0}");
Console.WriteLine($"IF: {metrics.IntensityFactor:F2}");
Console.WriteLine($"VI: {metrics.VariabilityIndex:F2}");
```

### 4. Pobieranie danych PMC (Fitness/Fatigue/Form)

```csharp
var pmcService = ServiceInitializer.PMCService;

// Pobierz najnowsze dane
var latestPmc = await pmcService.GetLatestPMCDataAsync(userId);
Console.WriteLine($"CTL (Fitness): {latestPmc.CTL:F1}");
Console.WriteLine($"ATL (Fatigue): {latestPmc.ATL:F1}");
Console.WriteLine($"TSB (Form): {latestPmc.TSB:F1}");
Console.WriteLine($"Status: {pmcService.GetFormStatus(latestPmc.TSB)}");

// Pobierz dane dla wykresu (ostatnie 12 tygodni)
var endDate = DateTime.Today;
var startDate = endDate.AddDays(-84);
var pmcData = await pmcService.GetPMCDataAsync(userId, startDate, endDate);
```

### 5. Pobieranie rekordów osobistych

```csharp
var powerCurveService = ServiceInitializer.PowerCurveService;

// Wszystkie rekordy mocy
var powerRecords = await powerCurveService.GetPowerRecordsAsync(userId);

foreach (var record in powerRecords)
{
    var duration = powerCurveService.FormatRecordName(record.RecordType);
    Console.WriteLine($"{duration}: {record.Value:F0} W ({record.SecondaryValue:F2} W/kg)");
}
```

### 6. Pobieranie treningów

```csharp
var trainingRepo = ServiceInitializer.TrainingRepository;

// Ostatnie 10 treningów
var recentTrainings = await trainingRepo.GetAllTrainingsAsync(userId, skip: 0, take: 10);

// Treningi z zakresu dat
var startDate = new DateTime(2025, 11, 1);
var endDate = new DateTime(2025, 11, 30);
var novemberTrainings = await trainingRepo.GetTrainingsByDateRangeAsync(userId, startDate, endDate);

// Podsumowanie tygodnia
var weekStart = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
var summary = await trainingRepo.GetWeeklySummaryAsync(userId, weekStart);
Console.WriteLine($"TSS tego tygodnia: {summary["TotalTSS"]}");
Console.WriteLine($"Liczba treningów: {summary["TrainingCount"]}");
```

---

## Automatyczne obliczenia podczas parsowania

`FitFileParser` automatycznie oblicza:
- ✅ Normalized Power (NP)
- ✅ Training Stress Score (TSS)
- ✅ Intensity Factor (IF)
- ✅ Variability Index (VI)
- ✅ Work (kJ)
- ✅ Power Curve (5s-120min)
- ✅ Maksymalne wartości (moc, tętno, kadencja)

---

## Integracja z istniejącym MainWindowViewModel

```csharp
// W MainWindowViewModel.cs
private async void LoadFitFile(string filePath)
{
    var parser = new FitFileParser();
    var fitData = parser.ParseFitFile(filePath);

    // Wyświetl dane w UI
    UpdateUI(fitData);

    // Zapisz do bazy danych
    try
    {
        var user = await ServiceInitializer.UserRepository.GetDefaultUserAsync();
        var training = await ServiceInitializer.ImportService.ImportTrainingAsync(
            fitData,
            user.Id,
            filePath
        );

        MessageBox.Show($"Trening zapisany!\nTSS: {training.TSS:F0}\nIF: {training.IntensityFactor:F2}");
    }
    catch (InvalidOperationException ex) when (ex.Message.Contains("już istnieje"))
    {
        MessageBox.Show("Ten trening jest już w bazie danych.");
    }
}
```

---

## Lokalizacja bazy danych

Baza danych SQLite jest automatycznie tworzona w:
```
C:\Users\[TwojaNazwa]\Documents\JaTraining\ja_training.db
```

Pliki FIT są kopiowane do:
```
C:\Users\[TwojaNazwa]\Documents\JaTraining\Trainings\
```

---

## Konfiguracja stref mocy/tętna

Domyślne strefy (model Coggan) są automatycznie tworzone dla każdego użytkownika.

Aby zmienić strefy:
```csharp
var zones = await db.PowerZones
    .Where(z => z.UserId == userId)
    .ToListAsync();

// Modyfikuj strefy według potrzeb
foreach (var zone in zones)
{
    // Zmień kolory, zakresy %, minimalne czasy, etc.
}

await db.SaveChangesAsync();
```

---

## Workflow importu treningu

Zgodnie ze specyfikacją (str. 1418-1432), `TrainingImportService` wykonuje:

1. ✅ Sprawdzenie duplikatów
2. ✅ Kopiowanie pliku FIT
3. ✅ Kalkulacja metryk (TSS, NP, IF, VI, Work)
4. ✅ Wykrywanie interwałów algorytmem
5. ✅ Wykrywanie rekordów (power curve)
6. ✅ Zapis do bazy danych (Training + Intervals + Records)
7. ✅ Update rekordów osobistych
8. ✅ Aktualizacja PMC (CTL/ATL/TSB)

---

## Następne kroki

1. Dodaj UI zgodne ze specyfikacją:
   - Dashboard z wykresem PMC
   - Lista treningów (tydzień po tygodniu)
   - Widok analizy treningu
   - Power Curve view
   - Settings view

2. Dodaj migracje przy modyfikacjach schematu:
```powershell
Add-Migration NazwaMigracji
Update-Database
```

3. Rozważ dodanie:
   - Async/await w całym UI
   - Progress bar dla długich operacji
   - Eksport danych do CSV
   - Backup/restore bazy danych

---

## Wsparcie

W razie problemów sprawdź:
- Czy pakiety NuGet są przywrócone (Restore NuGet Packages)
- Czy migracje są zastosowane (Update-Database)
- Czy baza danych istnieje w Documents/JaTraining/
- Logi błędów w Debug Output
