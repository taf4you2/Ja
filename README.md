# Detektor Interwałów Treningowych

Aplikacja desktopowa WPF do automatycznego wykrywania i analizy interwałów treningowych z plików FIT.

## Opis

Aplikacja implementuje zaawansowany 10-stopniowy algorytm wykrywania interwałów treningowych w danych z treningów kolarskich i biegowych. Algorytm wykorzystuje:

- Usuwanie outlierów metodą Z-score
- Wygładzanie danych metodą EMA (Exponential Moving Average)
- Dwuetapowe wykrywanie punktów zmian (długie interwały + krótkie sprinty)
- Detekcję stopniowego narastania mocy (podjazdy, testy rampowe)
- Klasyfikację do 7 stref treningowych według modelu Coggan
- Automatyczne wykrywanie okresów odpoczynku

## Funkcje

### Główne funkcje:
- ✅ Wczytywanie plików FIT
- ✅ Automatyczne wykrywanie interwałów treningowych
- ✅ Klasyfikacja do stref treningowych (Z1-Z7)
- ✅ Wykrywanie okresów odpoczynku
- ✅ Wizualizacja danych mocy na wykresie
- ✅ Konfigurowalne FTP (Functional Threshold Power)
- ✅ Szczegółowe informacje o każdym interwale

### Wykrywane typy interwałów:
- **Jump** - nagłe skoki mocy (standardowe interwały)
- **Gradual** - stopniowe narastanie mocy (podjazdy, testy rampowe)
- **Recovery** - okresy odpoczynku między interwałami

## Strefy treningowe

Aplikacja klasyfikuje interwały według 7 stref Coggan:

| Strefa | Nazwa | Zakres % FTP | Charakterystyka |
|--------|-------|--------------|-----------------|
| Z1 | Recovery | < 55% | Regeneracja |
| Z2 | Endurance | 55-75% | Wytrzymałość tlenowa |
| Z3 | Tempo | 75-90% | Tempo maratońskie |
| Z4 | Threshold | 90-105% | Próg mleczanowy |
| Z5 | VO2max | 105-120% | Moc tlenowa |
| Z6 | Anaerobic | 120-150% | Moc beztlenowa |
| Z7 | Neuromuscular | > 150% | Sprinty maksymalne |

## Wymagania systemowe

- Windows 10/11
- .NET 8.0 Runtime
- 4 GB RAM (zalecane 8 GB)
- Rozdzielczość ekranu: 1400x900 lub wyższa

## Instalacja

### Krok 1: Sklonuj repozytorium
```bash
git clone <repository-url>
cd Ja
```

### Krok 2: Pobierz FIT SDK DLL

**WAŻNE:** Aplikacja wymaga lokalnego Dynastream.Fit.dll

1. Pobierz FIT SDK z oficjalnej strony Garmin:
   - https://developer.garmin.com/fit/download/
   - Kliknij "Download" przy "FIT SDK"

2. Rozpakuj pobrany plik ZIP

3. Skopiuj `Dynastream.Fit.dll` do katalogu projektu:
   ```bash
   # W rozpakowanym FIT SDK znajdź:
   # FitSDK/cs/Dynastream.Fit.dll

   # Skopiuj do:
   cp FitSDK/cs/Dynastream.Fit.dll lib/FitSDK/Dynastream.Fit.dll
   ```

4. Sprawdź czy plik jest we właściwym miejscu:
   ```bash
   ls lib/FitSDK/Dynastream.Fit.dll
   # Powinno wyświetlić: lib/FitSDK/Dynastream.Fit.dll
   ```

**Szczegółowe instrukcje:** Zobacz `lib/FitSDK/README.md`

### Krok 3: Przywróć pakiety NuGet
```bash
dotnet restore
```

### Krok 4: Zbuduj projekt
```bash
dotnet build
```

Jeśli otrzymasz błąd o brakującym DLL, wróć do Kroku 2.

### Krok 5: Uruchom aplikację
```bash
dotnet run --project Ja/Ja.csproj
```

## Użycie

### Krok 1: Wczytaj plik FIT
1. Kliknij przycisk **"Wczytaj plik FIT"**
2. Wybierz plik .fit z dysku
3. Aplikacja automatycznie wczyta dane i wykryje FTP

### Krok 2: Dostosuj FTP (opcjonalnie)
1. Sprawdź wykryte FTP w prawym górnym rogu
2. Jeśli wartość jest nieprawidłowa, wprowadź swoją wartość FTP
3. Kliknij **"Przelicz"** aby ponownie przeanalizować interwały

### Krok 3: Analizuj wyniki
- **Wykres mocy** - wizualizacja danych mocy przez cały trening
- **Interwały treningowe** - lista wszystkich wykrytych interwałów (strefa Z3 i wyższe)
- **Okresy odpoczynku** - lista przerw między interwałami

### Informacje o interwale:
- **Start/Koniec** - czas rozpoczęcia i zakończenia (mm:ss)
- **Czas** - czas trwania interwału
- **Moc (W)** - średnia moc w watach
- **% FTP** - średnia moc jako procent FTP
- **Strefa** - klasyfikacja strefowa
- **Typ** - rodzaj interwału (jump/gradual)

## Struktura projektu

```
Ja/
├── Algorithms/
│   └── IntervalDetectionAlgorithm.cs    # Główny algorytm wykrywania
├── Models/
│   ├── FitFileData.cs                   # Model danych z pliku FIT
│   ├── TrainingInterval.cs              # Model interwału treningowego
│   ├── RecoveryPeriod.cs                # Model okresu odpoczynku
│   └── PowerDataPoint.cs                # Model punktu danych dla wykresu
├── Services/
│   └── FitFileParser.cs                 # Serwis parsowania plików FIT
├── ViewModels/
│   └── MainWindowViewModel.cs           # ViewModel dla głównego okna
├── Views/
│   └── MainWindow.xaml                  # Główne okno aplikacji
└── App.xaml                             # Konfiguracja aplikacji
```

## Testowanie parsera FIT

Projekt zawiera testową aplikację konsolową do weryfikacji parsowania plików FIT.

### Uruchomienie testu:

```bash
dotnet run --project Ja.TestConsole -- ścieżka/do/pliku.fit
```

**Szczegóły:** Zobacz `Ja.TestConsole/README.md`

Aplikacja testowa wyświetla:
- Statystyki pliku (moc, tętno, dystans, czas)
- Walidację danych (czy zawiera moc, czy ma minimalną długość)
- Szczegóły danych mocy (pierwsze 10 punktów, min/max)
- Diagnostykę błędów

## Technologie

- **WPF** - Windows Presentation Foundation
- **.NET 8.0** - Framework aplikacji
- **CommunityToolkit.Mvvm** - Wzorzec MVVM
- **Dynastream.Fit** - Parsowanie plików FIT
- **LiveCharts2** - Wykresy i wizualizacja danych

## Algorytm - szczegóły techniczne

### 10 kroków algorytmu:

1. **Usuwanie outlierów** - Metoda Z-score (próg: 3σ)
2. **Wygładzanie danych** - EMA z α=0.2
3. **Normalizacja** - Konwersja do % FTP
4. **Wykrywanie zmian** - Dwuetapowe (30s/12% + 10s/25%)
5. **Tworzenie segmentów** - Podział na fragmenty
6. **Detekcja stopniowych zmian** - Analiza gradientu (okno 90s, min 10%/min)
7. **Łączenie segmentów** - Sortowanie i grupowanie
8. **Klasyfikacja stref** - Przypisanie do Z1-Z7
9. **Weryfikacja** - Wypełnianie luk, usuwanie nakładek, walidacja czasów
10. **Filtrowanie** - Finalna selekcja i wykrywanie odpoczynków

### Parametry algorytmu:

| Parametr | Wartość | Opis |
|----------|---------|------|
| Z-score threshold | 3 | Próg dla outlierów |
| EMA alpha | 0.2 | Współczynnik wygładzania |
| Long interval window | 30s | Okno dla długich interwałów |
| Long interval threshold | 12% | Próg zmiany dla długich interwałów |
| Short interval window | 10s | Okno dla krótkich sprintów |
| Short interval threshold | 25% | Próg zmiany dla sprintów |
| Gradual window | 90s | Okno analizy stopniowych zmian |
| Gradual min slope | 10%/min | Minimalny gradient |
| Min segment duration | 10s | Minimalna długość segmentu |
| Recovery min duration | 30s | Minimalna długość odpoczynku |

## Przykłady użycia

### 1. Trening interwałowy 4x4min
Aplikacja wykryje:
- 4 interwały o czasie ~4 minuty każdy
- Klasyfikację do strefy Z4-Z5 (Threshold/VO2max)
- 3 okresy odpoczynku między interwałami
- Średnią moc dla każdego interwału

### 2. Test rampowy
Aplikacja wykryje:
- Stopniowe narastanie mocy (typ: gradual)
- Podział na segmenty według stref
- Gradient wzrostu mocy (% FTP/minutę)

### 3. Trening sprinterski 10x10s
Aplikacja wykryje:
- 10 krótkich sprintów (typ: jump)
- Klasyfikację do strefy Z6-Z7 (Anaerobic/Neuromuscular)
- Okresy regeneracji między sprintami

## Ograniczenia

- Minimalna długość treningu: 2 minuty
- Wymagane dane mocy w pliku FIT
- Częstotliwość próbkowania: 1 Hz
- Aplikacja nie obsługuje danych z wieloma sportami jednocześnie

## Rozwiązywanie problemów

### Błąd kompilacji: "Metadata file ... Dynastream.Fit.dll could not be found"
**Rozwiązanie:**
1. Pobierz FIT SDK z https://developer.garmin.com/fit/download/
2. Skopiuj `Dynastream.Fit.dll` do `lib/FitSDK/`
3. Szczegóły: Zobacz `lib/FitSDK/README.md`

### Błąd runtime: "Could not load file or assembly 'Dynastream.Fit'"
**Rozwiązanie:**
1. Sprawdź czy DLL jest w `lib/FitSDK/Dynastream.Fit.dll`
2. Wyczyść i przebuduj: `dotnet clean && dotnet build`
3. Jeśli problem nadal występuje, pobierz najnowszą wersję FIT SDK

### Błąd: "Plik nie zawiera danych mocy"
- Upewnij się, że plik FIT został nagrany z czujnikiem mocy
- Sprawdź czy plik nie jest uszkodzony

### Błąd: "Plik jest zbyt krótki"
- Trening musi trwać minimum 2 minuty
- Sprawdź czy plik został poprawnie zapisany

### Niewłaściwe wykrywanie interwałów
- Sprawdź czy FTP jest ustawione prawidłowo
- Dla nietypowych treningów może być potrzebne dostosowanie parametrów algorytmu

## Rozwój

### Planowane funkcje:
- [ ] Eksport wyników do CSV/Excel
- [ ] Generowanie raportów PDF
- [ ] Porównywanie treningów
- [ ] Analiza trendu wydolności
- [ ] Optymalizacja AVX2 dla szybszego przetwarzania
- [ ] Wsparcie dla więcej typów plików (TCX, GPX)

## Licencja

MIT License - szczegóły w pliku LICENSE

## Autor

Training Interval Detection Algorithm
Implementacja: WPF Application

## Kontakt

Zgłaszanie błędów: [Issues](https://github.com/your-repo/issues)
