# Wkład w rozwój projektu

Dziękujemy za zainteresowanie projektem Detektor Interwałów Treningowych! Wszelkie wkłady są mile widziane.

## Jak możesz pomóc?

### 1. Zgłaszanie błędów

Jeśli znajdziesz błąd, proszę zgłoś go poprzez GitHub Issues:

1. Sprawdź czy błąd nie został już zgłoszony
2. Utwórz nowy issue z tytułem opisującym problem
3. Dołącz:
   - Kroki do reprodukcji błędu
   - Oczekiwane zachowanie
   - Aktualne zachowanie
   - Zrzuty ekranu (jeśli dotyczy)
   - Wersja aplikacji i system operacyjny

### 2. Proponowanie nowych funkcji

Masz pomysł na nową funkcję?

1. Sprawdź istniejące issues czy nie ma podobnego pomysłu
2. Utwórz issue z etykietą "enhancement"
3. Opisz:
   - Problem, który funkcja ma rozwiązać
   - Proponowane rozwiązanie
   - Alternatywne podejścia

### 3. Wkład w kod

#### Proces

1. **Fork** repozytorium
2. **Sklonuj** swój fork lokalnie
3. **Utwórz branch** dla swojej funkcji (`git checkout -b feature/amazing-feature`)
4. **Commituj** zmiany (`git commit -m 'Add amazing feature'`)
5. **Push** do brancha (`git push origin feature/amazing-feature`)
6. **Otwórz Pull Request**

#### Wytyczne kodowania

- Używaj C# naming conventions
- Dodaj komentarze XML do publicznych metod
- Zachowaj spójny styl formatowania
- Pisz testy jednostkowe dla nowych funkcji
- Aktualizuj dokumentację

#### Przykład komentarza XML

```csharp
/// <summary>
/// Wykrywa interwały treningowe w danych mocy
/// </summary>
/// <param name="power">Tablica wartości mocy w watach</param>
/// <param name="ftp">Functional Threshold Power użytkownika</param>
/// <returns>Krotka z listą interwałów i okresów odpoczynku</returns>
public (List<Segment> intervals, List<Segment> recoveries) DetectAllIntervals(
    double[] power,
    double ftp)
{
    // Implementacja...
}
```

### 4. Dokumentacja

Pomoc w dokumentacji jest równie ważna:

- Poprawki literówek i błędów gramatycznych
- Tłumaczenia na inne języki
- Dodawanie przykładów użycia
- Tworzenie tutoriali

### 5. Testowanie

- Testuj aplikację z różnymi plikami FIT
- Raportuj problemy z wydajnością
- Sugeruj ulepszenia UI/UX

## Style kodu

### C# Conventions

```csharp
// Klasy - PascalCase
public class IntervalDetectionAlgorithm { }

// Metody - PascalCase
public void DetectIntervals() { }

// Zmienne prywatne - _camelCase
private int _threshold;

// Parametry i zmienne lokalne - camelCase
public void Method(int parameterName)
{
    int localVariable = 0;
}

// Stałe - PascalCase
private const int MaxIterations = 100;
```

### XAML Conventions

```xml
<!-- Kontrolki - PascalCase -->
<Button x:Name="LoadFileButton"
        Content="Wczytaj plik"
        Command="{Binding LoadCommand}" />

<!-- Properties - PascalCase -->
<TextBlock Text="{Binding FileName}"
           FontSize="14" />
```

## Struktura commit message

```
typ(zakres): krótki opis

Dłuższy opis zmiany (opcjonalnie)

Fixes #123
```

**Typy:**
- `feat`: Nowa funkcja
- `fix`: Naprawa błędu
- `docs`: Zmiany w dokumentacji
- `style`: Formatowanie, brakujące średniki, etc.
- `refactor`: Refaktoryzacja kodu
- `test`: Dodawanie testów
- `chore`: Aktualizacja zadań buildowych, etc.

**Przykłady:**

```
feat(algorithm): add support for heart rate zones

docs(readme): update installation instructions

fix(parser): handle corrupted FIT files gracefully
Fixes #45
```

## Pull Request Process

1. Upewnij się, że wszystkie testy przechodzą
2. Zaktualizuj README.md jeśli to konieczne
3. Zaktualizuj CHANGELOG.md
4. PR będzie zmergowany po review przez maintainerów

### Checklist PR

- [ ] Kod kompiluje się bez błędów
- [ ] Testy jednostkowe przechodzą
- [ ] Dodano nowe testy dla nowych funkcji
- [ ] Dokumentacja została zaktualizowana
- [ ] Kod jest sformatowany zgodnie z wytycznymi
- [ ] Commit messages są opisowe

## Code Review Process

Maintainerzy będą sprawdzać:

1. **Funkcjonalność** - Czy kod działa zgodnie z oczekiwaniami?
2. **Jakość** - Czy kod jest czytelny i dobrze zorganizowany?
3. **Testy** - Czy są odpowiednie testy?
4. **Dokumentacja** - Czy dokumentacja jest aktualna?
5. **Wydajność** - Czy nie ma oczywistych problemów z wydajnością?

## Priorytetowe obszary pomocy

Obecnie szukamy pomocy w:

- [ ] Implementacja eksportu do CSV/Excel
- [ ] Dodanie wykresów dla tętna i kadencji
- [ ] Wsparcie dla plików TCX i GPX
- [ ] Tłumaczenie na język angielski
- [ ] Optymalizacja algorytmu (AVX2)
- [ ] Testy jednostkowe
- [ ] Dokumentacja API

## Pytania?

Jeśli masz jakiekolwiek pytania dotyczące wkładu w projekt:

- Otwórz issue z pytaniem
- Skontaktuj się z maintainerami

## Kod postępowania

### Nasze zobowiązanie

Zobowiązujemy się do stworzenia otwartego i przyjaznego środowiska dla wszystkich.

### Nasze standardy

Przykłady zachowań, które przyczyniają się do pozytywnego środowiska:

- Używanie przyjaznego i inkluzywnego języka
- Szanowanie różnych punktów widzenia
- Przyjmowanie konstruktywnej krytyki z wdzięcznością
- Koncentrowanie się na tym, co najlepsze dla społeczności

Przykłady nieakceptowalnych zachowań:

- Używanie języka lub obrazów o charakterze seksualnym
- Trolling, obraźliwe komentarze
- Ataki osobiste lub polityczne
- Publiczne lub prywatne nękanie
- Publikowanie prywatnych informacji innych bez pozwolenia

### Egzekwowanie

Przypadki nieakceptowalnego zachowania mogą być zgłaszane do maintainerów projektu. Wszystkie skargi będą rozpatrywane i badane.

## Podziękowania

Dziękujemy wszystkim kontrybutorkom i kontrybutom za ich wkład w rozwój projektu!

### Obecni kontrybutorzy

- Twoje imię może być tutaj! 🎉

## Licencja

Wnosząc wkład w ten projekt, zgadzasz się, że Twój wkład będzie licencjonowany na warunkach licencji MIT.
