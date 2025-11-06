# Changelog

Wszystkie znaczące zmiany w projekcie będą dokumentowane w tym pliku.

Format bazuje na [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
a projekt stosuje [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Changed
- Zmiana z pakietu NuGet Dynastream.Fit na lokalny DLL
- Dodano folder `lib/FitSDK/` dla bibliotek zewnętrznych
- Zaktualizowano dokumentację instalacji (README, QUICKSTART)
- Dodano szczegółowe instrukcje pobierania FIT SDK

### Added
- Dokumentacja FIT SDK w `lib/FitSDK/README.md`
- Sekcja rozwiązywania problemów dla błędów DLL
- Możliwość eksportu wyników do CSV (planowane)
- Wsparcie dla plików TCX (planowane)
- Analiza tętna i kadencji (planowane)
- Porównywanie wielu treningów (planowane)

## [1.0.0] - 2025-11-06

### Added
- Implementacja 10-stopniowego algorytmu wykrywania interwałów
- Wczytywanie plików FIT
- Wykrywanie interwałów treningowych (jump i gradual)
- Klasyfikacja do 7 stref treningowych (Coggan)
- Automatyczne wykrywanie okresów odpoczynku
- Wizualizacja danych mocy na wykresie (LiveCharts)
- Konfigurowalne FTP
- Interfejs użytkownika WPF z tabelami interwałów
- Usuwanie outlierów metodą Z-score
- Wygładzanie danych metodą EMA
- Dwuetapowe wykrywanie punktów zmian
- Detekcja stopniowego narastania mocy
- Weryfikacja i walidacja segmentów
- Dokumentacja algorytmu
- README z instrukcjami użycia
- Testy jednostkowe (podstawowe)
- Licencja MIT

### Technical Details
- .NET 8.0
- WPF (Windows Presentation Foundation)
- CommunityToolkit.Mvvm dla wzorca MVVM
- Dynastream.Fit dla parsowania plików FIT
- LiveChartsCore dla wizualizacji

### Algorithm Parameters
- Z-score threshold: 3
- EMA alpha: 0.2
- Long interval window: 30s, threshold: 12%
- Short interval window: 10s, threshold: 25%
- Gradual window: 90s, min slope: 10%/min
- Minimum segment duration: 10s
- Recovery minimum duration: 30s

## [0.9.0] - 2025-10-20 (Beta)

### Added
- Prototyp algorytmu w Python
- Podstawowa struktura projektu C#
- Parser plików FIT
- Podstawowy UI

### Changed
- Refaktoryzacja algorytmu z Python na C#

### Fixed
- Problem z wykrywaniem krótkich sprintów
- Błąd w klasyfikacji stref

## [0.5.0] - 2025-09-15 (Alpha)

### Added
- Pierwszy działający prototyp
- Podstawowe wykrywanie interwałów
- Klasyfikacja do 5 stref

## Typy zmian

- `Added` - Nowe funkcje
- `Changed` - Zmiany w istniejących funkcjach
- `Deprecated` - Funkcje, które wkrótce zostaną usunięte
- `Removed` - Usunięte funkcje
- `Fixed` - Naprawione błędy
- `Security` - Poprawki bezpieczeństwa

[Unreleased]: https://github.com/your-repo/compare/v1.0.0...HEAD
[1.0.0]: https://github.com/your-repo/compare/v0.9.0...v1.0.0
[0.9.0]: https://github.com/your-repo/compare/v0.5.0...v0.9.0
[0.5.0]: https://github.com/your-repo/releases/tag/v0.5.0
