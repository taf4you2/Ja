# Testowanie aplikacji Detektor Interwałów Treningowych

## Przygotowanie środowiska testowego

### Wymagania
- Visual Studio 2022 (17.8 lub nowsza) lub Visual Studio Code
- .NET 8.0 SDK
- Windows 10/11

### Instalacja środowiska

1. Zainstaluj .NET 8.0 SDK:
   - Pobierz z: https://dotnet.microsoft.com/download/dotnet/8.0
   - Uruchom instalator i postępuj zgodnie z instrukcjami

2. Zainstaluj Visual Studio 2022 (opcjonalnie):
   - Pobierz Community Edition (bezpłatna)
   - Zaznacz workload: ".NET desktop development"

## Budowanie projektu

### Z linii poleceń

```bash
# Przejdź do katalogu projektu
cd Ja

# Przywróć pakiety NuGet
dotnet restore

# Zbuduj projekt
dotnet build

# Uruchom aplikację
dotnet run --project Ja/Ja.csproj
```

### Z Visual Studio

1. Otwórz plik `Ja.sln`
2. Naciśnij `F5` lub kliknij "Start Debugging"

## Przygotowanie plików testowych

### Opcja 1: Użyj własnych plików FIT

Jeśli posiadasz urządzenie treningowe (Garmin, Wahoo, etc.):
1. Skopiuj pliki .fit z urządzenia do komputera
2. Lokalizacje typowe:
   - Garmin: `GARMIN/Activity/`
   - Wahoo: `wahoo/history/`

### Opcja 2: Pobierz przykładowe pliki

Przykładowe pliki FIT można pobrać z:
- Strava: Eksportuj trening jako plik FIT
- TrainingPeaks: Eksportuj trening
- Golden Cheetah: Eksportuj dane

### Opcja 3: Użyj symulowanych danych (dla deweloperów)

Poniżej kod Python do generowania testowego pliku FIT:

```python
from fitparse import FitFile
import numpy as np
from datetime import datetime, timedelta

def generate_test_fit():
    """
    Generuje testowy plik FIT z interwałami
    """
    # Symulacja treningu 4x4min @ 120% FTP z 4min odpoczynkiem
    ftp = 250  # watów
    duration = 40 * 60  # 40 minut

    power_data = []
    timestamp = datetime.now()

    # 5 min rozgrzewka (60% FTP)
    warmup = [int(ftp * 0.6 + np.random.normal(0, 10)) for _ in range(300)]
    power_data.extend(warmup)

    # 4x (4min @ 120% FTP + 4min @ 50% FTP)
    for rep in range(4):
        # Interwał wysokiej mocy
        interval = [int(ftp * 1.2 + np.random.normal(0, 15)) for _ in range(240)]
        power_data.extend(interval)

        # Odpoczynek (jeśli nie ostatni)
        if rep < 3:
            recovery = [int(ftp * 0.5 + np.random.normal(0, 8)) for _ in range(240)]
            power_data.extend(recovery)

    # 5 min wychładzanie (50% FTP)
    cooldown = [int(ftp * 0.5 + np.random.normal(0, 10)) for _ in range(300)]
    power_data.extend(cooldown)

    print(f"Wygenerowano {len(power_data)} punktów danych mocy")
    print(f"Czas trwania: {len(power_data) / 60:.1f} minut")

    return power_data

# Użycie:
# power = generate_test_fit()
# # Zapisz do pliku FIT używając biblioteki fit_tool lub fitparse
```

## Scenariusze testowe

### Test 1: Trening interwałowy strukturalny

**Oczekiwany wynik:**
- 4 wykryte interwały w strefie Z5 (VO2max)
- Każdy interwał trwa ~240 sekund
- 3 okresy odpoczynku w strefie Z1-Z2

**Weryfikacja:**
1. Wczytaj plik
2. Sprawdź liczbę wykrytych interwałów
3. Sprawdź czas trwania każdego interwału
4. Sprawdź średnią moc każdego interwału
5. Sprawdź klasyfikację strefową

### Test 2: Test rampowy

**Charakterystyka:**
- Stopniowe zwiększanie mocy od 100W do 400W
- Wzrost 25W co minutę

**Oczekiwany wynik:**
- Wykrycie interwałów typu "gradual"
- Gradacja od niskich do wysokich stref
- Obliczony gradient ~10-15% FTP/min

### Test 3: Trening sprinterski

**Charakterystyka:**
- 10x10s maksymalne sprinty
- 50s odpoczynku między sprintami

**Oczekiwany wynik:**
- 10 wykrytych interwałów w strefie Z6-Z7
- Każdy interwał trwa 5-15 sekund
- 9 okresów odpoczynku

### Test 4: Długi trening endurance

**Charakterystyka:**
- Stała moc 60-70% FTP przez 2 godziny

**Oczekiwany wynik:**
- Brak wykrytych interwałów (poniżej Z3)
- Cały trening w strefie Z2

### Test 5: Pagórkowaty teren

**Charakterystyka:**
- Naturalne wzniesienia i zjazdy
- Zmienna moc

**Oczekiwany wynik:**
- Podjazdy wykryte jako interwały (jeśli > 75% FTP)
- Zjazdy jako okresy odpoczynku
- Możliwe interwały typu "gradual" na długich podjazadch

## Testy jednostkowe

### Test usuwania outlierów

```csharp
[Test]
public void RemoveOutliers_ShouldRemoveHighValues()
{
    // Arrange
    var power = new double[] { 100, 102, 98, 500, 101, 99 };
    var algorithm = new IntervalDetectionAlgorithm();

    // Act
    var cleaned = algorithm.RemoveOutliers(power);

    // Assert
    Assert.IsTrue(cleaned[3] < 200); // Outlier powinien być zastąpiony
}
```

### Test EMA

```csharp
[Test]
public void EMA_ShouldSmoothData()
{
    // Arrange
    var power = new double[] { 100, 110, 105, 115, 108 };
    var algorithm = new IntervalDetectionAlgorithm();

    // Act
    var smoothed = algorithm.ExponentialMovingAverage(power);

    // Assert
    Assert.IsTrue(Math.Abs(smoothed[1] - power[1]) > 0); // Dane powinny być wygładzone
}
```

### Test wykrywania punktów zmian

```csharp
[Test]
public void DetectChangePoints_ShouldFindIntervalStarts()
{
    // Arrange
    var power = CreateTestData(); // 5min @ 100W, 4min @ 200W, 5min @ 100W
    var algorithm = new IntervalDetectionAlgorithm();

    // Act
    var changePoints = algorithm.DetectChangePointsTwoPhase(power);

    // Assert
    Assert.AreEqual(2, changePoints.Count); // Powinny być 2 punkty zmian
}
```

## Walidacja wyników

### Metryki jakości

1. **Precyzja detekcji**
   - Czy wszystkie interwały zostały wykryte?
   - Czy są fałszywe wykrycia?

2. **Dokładność czasu**
   - Czy start/koniec interwału są precyzyjne?
   - Tolerancja: ±5 sekund

3. **Klasyfikacja strefowa**
   - Czy strefa jest prawidłowa dla danej mocy?
   - Czy FTP jest poprawnie interpretowane?

4. **Wydajność**
   - Czas przetwarzania dla 1h treningu: < 2 sekundy
   - Zużycie pamięci: < 100 MB

### Checklist weryfikacji

- [ ] Aplikacja się uruchamia bez błędów
- [ ] Można wczytać plik FIT
- [ ] Wykres mocy się wyświetla poprawnie
- [ ] Interwały są poprawnie wykrywane
- [ ] Klasyfikacja strefowa jest prawidłowa
- [ ] Można zmienić FTP i przeliczyć
- [ ] Okresy odpoczynku są wykrywane
- [ ] Informacje o pliku są wyświetlane
- [ ] UI jest responsywne (bez zawieszania)
- [ ] Wszystkie kontrolki działają

## Znane problemy i rozwiązania

### Problem 1: "Plik nie zawiera danych mocy"
**Przyczyna:** Plik FIT nie ma zapisanych danych z czujnika mocy
**Rozwiązanie:** Użyj pliku z treningu z czujnikiem mocy

### Problem 2: Zbyt wiele fałszywych wykryć
**Przyczyna:** Szum w danych lub niestabilna moc
**Rozwiązanie:** Zwiększ parametr `threshold` w `DetectChangePoints`

### Problem 3: Nie wykrywa krótkich sprintów
**Przyczyna:** Sprinty są za krótkie lub próg jest za wysoki
**Rozwiązanie:** Zmniejsz `Short interval threshold` z 25% do 20%

### Problem 4: Aplikacja jest wolna dla długich treningów
**Przyczyna:** Brak optymalizacji dla dużych zbiorów danych
**Rozwiązanie:** Implementacja AVX2 lub próbkowanie danych

## Raporty testowe

### Format raportu

```markdown
# Raport testowy - [Data]

## Informacje o teście
- Tester: [Imię]
- Wersja: [Wersja aplikacji]
- Środowisko: Windows [wersja], .NET [wersja]

## Plik testowy
- Nazwa: [nazwa pliku]
- Długość: [czas trwania]
- Źródło: [Garmin/Wahoo/Inne]

## Wyniki
- Liczba wykrytych interwałów: [X]
- Liczba okresów odpoczynku: [X]
- Czas przetwarzania: [X sekund]

## Walidacja
- Czy wyniki są poprawne: TAK/NIE
- Uwagi: [dodatkowe uwagi]

## Problemy
- [Lista napotkanych problemów]

## Załączniki
- [Zrzuty ekranu]
- [Pliki testowe]
```

## Automatyzacja testów

### Continuous Integration

Przykładowy plik `.github/workflows/test.yml`:

```yaml
name: Test Application

on: [push, pull_request]

jobs:
  test:
    runs-on: windows-latest

    steps:
    - uses: actions/checkout@v3

    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '8.0.x'

    - name: Restore dependencies
      run: dotnet restore

    - name: Build
      run: dotnet build --no-restore

    - name: Run tests
      run: dotnet test --no-build --verbosity normal
```

## Kontakt

W razie problemów z testowaniem:
- Zgłoś issue na GitHub
- Dołącz raport testowy
- Załącz logi błędów
