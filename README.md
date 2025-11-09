# JA Training - Aplikacja do Analizy Treningów Kolarskich

Aplikacja desktopowa WPF do zarządzania, analizy i wizualizacji treningów kolarskich z plików FIT.

## Opis

**JA Training** to kompleksowa aplikacja desktopowa dla kolarzy, umożliwiająca:
- Import i analizę treningów z plików FIT (Garmin, Wahoo, inne urządzenia)
- Automatyczne wykrywanie interwałów treningowych
- Śledzenie formy treningowej (PMC - Performance Management Chart)
- Zarządzanie rekordami osobistymi
- Analizę krzywej mocy (Power Curve)
- Monitorowanie historii FTP i wagi

## Główne Funkcje

### 📊 Dashboard
- **Performance Management Chart (PMC)** - wizualizacja formy treningowej
  - CTL (Chronic Training Load) - fitness długoterminowy
  - ATL (Acute Training Load) - zmęczenie
  - TSB (Training Stress Balance) - forma treningowa
- Podsumowania tygodniowe - szybki przegląd ostatnich 4 tygodni
- Wizualizacja TSS dla poszczególnych dni
- Stan pustego ekranu dla nowych użytkowników

### 📅 Kalendarz Treningów
- Widok miesięczny, tygodniowy i listowy
- Kolorowe oznaczenia stref treningowych
- Szybki podgląd treningów w poszczególnych dniach
- Filtrowanie i wyszukiwanie treningów

### 📈 Analiza Treningu
- Automatyczne wykrywanie interwałów treningowych (algorytm 10-stopniowy)
- Wykres mocy z oznaczonymi interwałami
- Time in Zones - rozkład czasu w strefach treningowych
- Power Curve - maksymalne moce dla różnych przedziałów czasowych
- Wykrywanie i klasyfikacja interwałów:
  - **Jump** - nagłe skoki mocy
  - **Gradual** - stopniowe narastanie (podjazdy, testy)
- Analiza okresów odpoczynku

### ⚡ Power Curve
- Krzywa mocy dla wybranych okresów (30/90/180/365 dni)
- Porównanie rekordów z różnych okresów
- Wykres progresji rekordów w czasie
- Tabela rekordów dla standardowych przedziałów (5s, 1min, 5min, 20min, 60min)

### 🏆 Rekordy Osobiste
- Śledzenie rekordów mocy dla wszystkich przedziałów czasowych
- Automatyczne wykrywanie i aktualizacja rekordów
- Historia rekordów
- Kategoryzacja: sprinty, krótkie interwały, długie wysiłki

### ⚙️ Ustawienia
#### Profil Użytkownika
- Dane osobowe (imię, nazwisko, data urodzenia, płeć)
- Parametry fizyczne (waga, wzrost)
- Parametry tętna (RHR, Max HR)

#### Strefy Treningowe
- **Power Zones** (Strefy mocy)
  - FTP (Functional Threshold Power) z historią
  - Obliczanie W/kg
  - Model Coggan (7 stref) - domyślny
  - Konfigurowalne zakresy % FTP
  - Kolory stref

- **Heart Rate Zones** (Strefy tętna)
  - Konfigurowalne modele (% Max HR, % HRR, LTHR)
  - 5-7 stref w zależności od modelu

#### Zaawansowane
- Parametry algorytmu wykrywania interwałów
- Konfiguracja TSS i PMC
- Zarządzanie danymi i backup

## Strefy Treningowe (Model Coggan)

| Strefa | Nazwa | Zakres % FTP | Kolor | Min. czas |
|--------|-------|--------------|-------|-----------|
| Z1 | Recovery | 0-55% | Szary | 0s |
| Z2 | Endurance | 55-75% | Niebieski | 0s |
| Z3 | Tempo | 75-90% | Zielony | 120s |
| Z4 | Threshold | 90-105% | Żółty | 60s |
| Z5 | VO2max | 105-120% | Pomarańczowy | 30s |
| Z6 | Anaerobic | 120-150% | Czerwony | 10s |
| Z7 | Neuromuscular | >150% | Ciemnoczerwony | 5s |

## Technologia

### Stack Technologiczny
- **.NET 8.0** - platforma aplikacji
- **WPF (Windows Presentation Foundation)** - interfejs użytkownika
- **Entity Framework Core** - ORM dla SQLite
- **SQLite** - baza danych
- **CommunityToolkit.Mvvm** - MVVM framework
- **LiveCharts2** - wizualizacja danych
- **Dynastream.Fit** - parsowanie plików FIT

### Architektura
- **MVVM Pattern** - separacja logiki i UI
- **Repository Pattern** - warstwa dostępu do danych
- **Service Layer** - logika biznesowa
- **Dependency Injection** - zarządzanie zależnościami

### Baza Danych
- **SQLite** z migracjami Entity Framework Core
- Optymalizowane indeksy dla wydajności
- WAL mode dla lepszej współbieżności
- Cache PMC dla szybkiego dostępu

## Wymagania Systemowe

- **OS**: Windows 10/11 (64-bit)
- **.NET Runtime**: .NET 8.0 Desktop Runtime
- **RAM**: 4 GB (zalecane 8 GB)
- **Miejsce na dysku**: 500 MB
- **Rozdzielczość**: Minimum 1280x720 (zalecane 1920x1080)

## Instalacja

### Wymagania wstępne
1. Zainstaluj [.NET 8.0 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0)
2. Pobierz Dynastream.Fit SDK z [developer.garmin.com/fit](https://developer.garmin.com/fit/download/)
3. Umieść `Dynastream.Fit.dll` w folderze `lib/FitSDK/`

### Kompilacja ze źródeł
```bash
# Sklonuj repozytorium
git clone https://github.com/taf4you2/Ja.git
cd Ja

# Zainstaluj zależności i skompiluj
dotnet restore
dotnet build

# Uruchom aplikację
dotnet run --project Ja/Ja.csproj
```

### Pierwsze uruchomienie
Przy pierwszym uruchomieniu aplikacja:
1. Utworzy bazę danych SQLite w folderze `Data/`
2. Zastosuje wszystkie migracje
3. Utworzy domyślnego użytkownika z przykładowymi danymi:
   - Imię: Jan Kowalski
   - FTP: 250 W
   - Waga: 75 kg
   - Domyślne strefy treningowe (Coggan)

## Użytkowanie

### Import treningu
1. Kliknij przycisk **"+ Dodaj trening"** w Dashboard lub prawym dolnym rogu
2. Wybierz plik FIT z dysku
3. Aplikacja automatycznie:
   - Sparsuje plik FIT
   - Obliczy wszystkie metryki (TSS, NP, IF, VI, Work)
   - Wykryje interwały treningowe
   - Wykryje rekordy osobiste
   - Zaktualizuje PMC (CTL/ATL/TSB)

### Analiza treningu
1. W Dashboard kliknij na wybrany tydzień lub przejdź do Kalendarza
2. Kliknij na wybrany trening
3. Zobacz szczegółową analizę:
   - Wykres mocy z interwałami
   - Time in Zones
   - Power Curve
   - Tabela interwałów
   - Okresy odpoczynku

### Zarządzanie FTP
1. Przejdź do **Ustawień** → **Strefy**
2. Wprowadź nowe FTP
3. Ustaw datę testu
4. Kliknij **"Dodaj FTP do historii"**
5. Wszystkie strefy zostaną automatycznie przeliczone

### Zarządzanie wagą
1. Przejdź do **Ustawień** → **Profil**
2. Wprowadź aktualną wagę
3. Kliknij **"Dodaj wagę do historii"**
4. W/kg zostanie automatycznie przeliczone dla wszystkich rekordów

## Algorytm Wykrywania Interwałów

Aplikacja wykorzystuje zaawansowany 10-stopniowy algorytm:

1. **Walidacja danych** - sprawdzenie poprawności danych wejściowych
2. **Usuwanie outlierów** - metoda Z-score (threshold: 3.0σ)
3. **Wygładzanie** - Exponential Moving Average (α=0.3)
4. **Obliczanie średnich kroczących** - okna długie (30s) i krótkie (10s)
5. **Wykrywanie punktów zmian** - progi 12% (długie) i 25% (krótkie)
6. **Filtrowanie krótkich skoków** - minimalne czasy dla stref
7. **Łączenie bliskich punktów** - maksymalny gap 15s
8. **Filtrowanie krótkich interwałów** - minimalne czasy per strefa
9. **Wykrywanie interwałów stopniowych** - analiza trendu (slope)
10. **Wykrywanie okresów odpoczynku** - między interwałami

## Performance Management Chart (PMC)

### Wzory
- **CTL** (Chronic Training Load): `CTL_today = CTL_yesterday + (TSS_today - CTL_yesterday) / 42`
- **ATL** (Acute Training Load): `ATL_today = ATL_yesterday + (TSS_today - ATL_yesterday) / 7`
- **TSB** (Training Stress Balance): `TSB = CTL - ATL`

### Interpretacja TSB
- **TSB > 25**: Świeży (dobra forma, możliwy wyścig)
- **TSB -10 do 25**: Optymalny (balans treningowy)
- **TSB < -30**: Przeciążony (ryzyko przetrenowania)

## Metryki Treningowe

### TSS (Training Stress Score)
`TSS = (seconds × NP × IF) / (FTP × 3600) × 100`

### NP (Normalized Power)
1. Oblicz 30-sekundową średnią kroczącą
2. Podnieś każdą wartość do 4. potęgi
3. Oblicz średnią
4. Wynik podnieś do potęgi 1/4

### IF (Intensity Factor)
`IF = NP / FTP`

### VI (Variability Index)
`VI = NP / Average Power`

## Struktura Projektu

```
Ja/
├── Algorithms/          # Algorytmy (wykrywanie interwałów)
├── Converters/          # WPF value converters
├── Database/
│   ├── Entities/        # Entity Framework entities
│   └── Migrations/      # EF Core migrations
├── Models/              # View models (POCO)
├── Repositories/        # Warstwa dostępu do danych
├── Services/            # Logika biznesowa
├── ViewModels/          # MVVM ViewModels
└── Views/               # XAML views
```

## Rozwój

### Planowane funkcje
- [ ] Export treningów do CSV/TCX
- [ ] Import z innych formatów (GPX, TCX)
- [ ] Plany treningowe
- [ ] Porównanie treningów
- [ ] Multi-user support
- [ ] Cloud sync
- [ ] Mobile companion app

### Znane ograniczenia
- Brak obsługi pływania i biegania (focus na kolarstwie)
- Tylko pojedynczy użytkownik
- Brak synchronizacji z chmurą

## Licencja

Ten projekt jest udostępniony na licencji MIT. Zobacz plik [LICENSE](LICENSE) dla szczegółów.

## Autorzy

- **Jacek Antoniewicz** - Główny deweloper

## Podziękowania

- Garmin/Dynastream za FIT SDK
- Społeczność TrainingPeaks za dokumentację PMC
- Andrew Coggan za model stref treningowych

## Wsparcie

Jeśli napotkasz problemy lub masz pytania:
1. Sprawdź [Issues](https://github.com/taf4you2/Ja/issues)
2. Utwórz nowy Issue z opisem problemu
3. Dołącz logi z folderu `Logs/`

## Changelog

Zobacz [CHANGELOG.md](CHANGELOG.md) dla historii zmian.

## Contributing

Zobacz [CONTRIBUTING.md](CONTRIBUTING.md) dla wytycznych dotyczących współpracy.
