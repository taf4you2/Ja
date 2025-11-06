# Szybki start - Detektor Interwałów Treningowych

## 6 kroków do pierwszej analizy

### 1. Zainstaluj środowisko

**Windows:**
```powershell
# Pobierz i zainstaluj .NET 8.0 SDK
winget install Microsoft.DotNet.SDK.8
```

**Weryfikacja:**
```bash
dotnet --version
# Powinno wyświetlić: 8.0.x
```

### 2. Pobierz projekt

```bash
git clone <repository-url>
cd Ja
```

### 3. Pobierz FIT SDK DLL

**WAŻNE:** Musisz pobrać Dynastream.Fit.dll przed budowaniem!

```bash
# 1. Pobierz FIT SDK ze strony Garmin:
#    https://developer.garmin.com/fit/download/

# 2. Rozpakuj i skopiuj DLL:
cp FitSDK/cs/Dynastream.Fit.dll lib/FitSDK/Dynastream.Fit.dll

# 3. Sprawdź:
ls lib/FitSDK/Dynastream.Fit.dll
```

**Szczegóły:** Zobacz `lib/FitSDK/README.md`

### 4. Zbuduj projekt

```bash
# Przywróć pakiety
dotnet restore

# Zbuduj aplikację
dotnet build

# Uruchom
dotnet run --project Ja/Ja.csproj
```

### 5. Wczytaj plik FIT

1. Kliknij **"Wczytaj plik FIT"**
2. Wybierz plik .fit z dysku
3. Poczekaj na automatyczną analizę

### 6. Analizuj wyniki

- **Wykres** - wizualizacja mocy przez cały trening
- **Interwały** - lista wykrytych interwałów (Z3+)
- **Odpoczynki** - okresy regeneracji

## Pierwsze kroki

### Gdzie znaleźć pliki FIT?

**Garmin Connect:**
1. Otwórz trening w Garmin Connect
2. Ikona koła zębatego → "Export to FIT"
3. Zapisz plik na dysku

**Strava:**
1. Otwórz aktywność
2. Menu "..." → "Export GPX"
3. Lub użyj narzędzia do konwersji GPX→FIT

**Z urządzenia:**
- Garmin: Podłącz urządzenie USB → `GARMIN/Activity/`
- Wahoo: Podłącz urządzenie USB → `wahoo/history/`

### Co oznaczają strefy?

| Strefa | % FTP | Zastosowanie |
|--------|-------|--------------|
| Z1 | <55% | Regeneracja |
| Z2 | 55-75% | Trening podstawowy |
| Z3 | 75-90% | Tempo |
| Z4 | 90-105% | Próg (Sweet Spot) |
| Z5 | 105-120% | VO2max |
| Z6 | 120-150% | Beztlenowe |
| Z7 | >150% | Sprinty |

### Ustawienie FTP

**Co to jest FTP?**
- Functional Threshold Power
- Maksymalna moc utrzymywana przez 1 godzinę
- Kluczowe dla prawidłowej klasyfikacji

**Jak ustawić?**
1. Znajdź swoje FTP (test FTP lub oszacowanie)
2. Wpisz wartość w prawym górnym rogu
3. Kliknij "Przelicz"

**Nie znasz swojego FTP?**
- Aplikacja oszacuje: `FTP ≈ średnia moc × 1.05`
- Lub użyj kalkulatora: `FTP ≈ 95% mocy z 20min testu`

## Przykładowe scenariusze

### Scenariusz 1: Analiza treningu interwałowego

**Masz plik:** Trening 4x4min @ wysokiej mocy

**Oczekiwany wynik:**
```
Interwały treningowe (4):
1. 05:00 - 09:00 | 04:00 | 300W | 120% FTP | Z5: VO2max
2. 13:00 - 17:00 | 04:00 | 305W | 122% FTP | Z5: VO2max
3. 21:00 - 25:00 | 04:00 | 298W | 119% FTP | Z5: VO2max
4. 29:00 - 33:00 | 04:00 | 295W | 118% FTP | Z5: VO2max

Okresy odpoczynku (3):
1. 09:00 - 13:00 | 04:00 | 120W | 48% FTP | Z1: Recovery
2. 17:00 - 21:00 | 04:00 | 125W | 50% FTP | Z1: Recovery
3. 25:00 - 29:00 | 04:00 | 118W | 47% FTP | Z1: Recovery
```

### Scenariusz 2: Analiza testu rampowego

**Masz plik:** Test rampowy +25W/min

**Oczekiwany wynik:**
- Interwały typu "gradual"
- Gradient: ~10-15% FTP/min
- Klasyfikacja od Z2 do Z7

### Scenariusz 3: Trening endurance

**Masz plik:** 2h @ stała moc 65% FTP

**Oczekiwany wynik:**
```
Interwały treningowe (0):
Brak interwałów powyżej Z3

Cały trening w strefie Z2: Endurance
```

## Rozwiązywanie problemów

### ❌ Błąd kompilacji: "Metadata file ... Dynastream.Fit.dll could not be found"

**Przyczyna:** Brak biblioteki Dynastream.Fit.dll w projekcie

**Rozwiązanie:**
1. Pobierz FIT SDK z https://developer.garmin.com/fit/download/
2. Rozpakuj pobrany plik
3. Skopiuj `FitSDK/cs/Dynastream.Fit.dll` do `lib/FitSDK/Dynastream.Fit.dll`
4. Sprawdź: `ls lib/FitSDK/Dynastream.Fit.dll`
5. Uruchom ponownie: `dotnet build`

**Szczegóły:** Zobacz `lib/FitSDK/README.md`

### ❌ Błąd w runtime: "Could not load file or assembly 'Dynastream.Fit'"

**Przyczyna:** DLL nie został skopiowany do katalogu wyjściowego lub jest niekompatybilny

**Rozwiązanie:**
1. Sprawdź czy DLL jest we właściwym miejscu: `lib/FitSDK/Dynastream.Fit.dll`
2. Wyczyść projekt: `dotnet clean`
3. Przebuduj: `dotnet build`
4. Jeśli problem nadal występuje, pobierz najnowszą wersję FIT SDK

### ❌ "Plik nie zawiera danych mocy"

**Przyczyna:** Plik FIT nie ma zapisanych danych z czujnika mocy

**Rozwiązanie:**
- Sprawdź czy urządzenie było sparowane z czujnikiem mocy
- Użyj pliku z innego treningu
- Sprawdź czy plik nie jest uszkodzony

### ❌ "Plik jest zbyt krótki"

**Przyczyna:** Trening < 2 minuty

**Rozwiązanie:**
- Używaj plików z pełnych treningów
- Minimalna długość: 120 sekund

### ⚠️ Nieprawidłowe wykrywanie interwałów

**Przyczyna 1:** Błędne FTP

**Rozwiązanie:**
1. Sprawdź swoją rzeczywistą wartość FTP
2. Wprowadź poprawną wartość
3. Kliknij "Przelicz"

**Przyczyna 2:** Nietypowy trening (pagórkowaty teren)

**Rozwiązanie:**
- To normalne - algorytm wykrywa naturalne wzniesienia
- Dostosuj progi jeśli potrzeba

### 🐌 Aplikacja działa wolno

**Przyczyna:** Bardzo długi trening (>3h)

**Rozwiązanie:**
- To normalne dla długich treningów
- Oczekiwany czas: ~2s na godzinę treningu

## Następne kroki

### Zaawansowane użycie

1. **Eksport wyników** (planowane)
   - Eksport do CSV/Excel
   - Generowanie raportów PDF

2. **Analiza trendu** (planowane)
   - Porównywanie treningów
   - Śledzenie postępów

3. **Dostosowanie parametrów** (dla deweloperów)
   - Modyfikacja progów wykrywania
   - Dostosowanie stref do własnego modelu

### Dokumentacja

- **README.md** - Pełna dokumentacja
- **ALGORITHM.md** - Szczegóły algorytmu
- **TESTING.md** - Jak testować aplikację
- **CONTRIBUTING.md** - Jak pomóc w rozwoju

### Wsparcie

Masz pytania?
- 📖 Przeczytaj pełną dokumentację
- 🐛 Zgłoś błąd na GitHub Issues
- 💡 Zaproponuj nową funkcję
- 🤝 Dołącz do rozwoju projektu

## Podsumowanie

```bash
# Kompletny workflow
git clone <repo>
cd Ja
dotnet restore
dotnet build
dotnet run --project Ja/Ja.csproj

# W aplikacji:
# 1. Wczytaj plik FIT
# 2. Sprawdź FTP
# 3. Analizuj wyniki
```

**Gotowe!** Możesz teraz analizować swoje treningi 🚴💨

---

**Przydatne linki:**
- [Garmin FIT SDK](https://developer.garmin.com/fit/)
- [Model stref Coggan](https://www.trainingpeaks.com/blog/power-training-levels/)
- [Jak obliczyć FTP](https://www.trainingpeaks.com/blog/what-is-threshold-power/)
