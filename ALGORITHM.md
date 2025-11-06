# Szczegółowy opis algorytmu wykrywania interwałów treningowych

## Wprowadzenie

Algorytm został zaprojektowany do automatycznego wykrywania i klasyfikacji interwałów treningowych w danych z plików FIT, szczególnie dla treningów kolarskich i biegowych. Wykorzystuje zaawansowane techniki przetwarzania sygnałów i analizy statystycznej do identyfikacji zmian intensywności treningu.

## Architektura algorytmu

Algorytm składa się z 10 sekwencyjnych kroków przetwarzania, gdzie każdy krok przygotowuje dane dla kolejnego etapu. Podejście to zapewnia wysoką dokładność wykrywania różnych typów interwałów.

### Dane wejściowe
- `power` - tablica wartości mocy w watach (z częstotliwością próbkowania 1 Hz)
- `ftp` - Functional Threshold Power użytkownika (maksymalna moc utrzymywana przez godzinę)

### Dane wyjściowe
- Lista wykrytych interwałów treningowych
- Lista okresów odpoczynku między interwałami

## KROK 1: Usuwanie outlierów metodą Z-score

### Cel
Eliminacja nietypowych skoków mocy, które mogą być spowodowane błędami pomiarowymi lub chwilowymi zakłóceniami.

### Implementacja
```csharp
private double[] RemoveOutliers(double[] power)
{
    double mean = power.Average();
    double stdDev = CalculateStandardDeviation(power, mean);

    for (int i = 0; i < power.Length; i++)
    {
        double zScore = Math.Abs(power[i] - mean) / stdDev;

        if (zScore > 3)
        {
            // Zastąp outlier średnią z sąsiednich punktów
            if (i > 0 && i < power.Length - 1)
                cleaned[i] = (power[i-1] + power[i+1]) / 2;
            else
                cleaned[i] = mean;
        }
    }
}
```

### Działanie
1. Obliczenie średniej i odchylenia standardowego dla całego zestawu danych
2. Dla każdej wartości obliczenie Z-score (liczba odchyleń standardowych od średniej)
3. Wartości z Z-score > 3 są zastępowane średnią z sąsiednich punktów
4. Dla punktów brzegowych stosowana jest średnia całego zestawu

### Parametry
- Próg Z-score: **3** (wartości powyżej 3σ uznawane za outliery)

## KROK 2: Wygładzanie danych metodą EMA

### Cel
Redukcja szumu pomiarowego przy zachowaniu charakterystyki zmian mocy.

### Implementacja
```csharp
private double[] ExponentialMovingAverage(double[] data, double alpha = 0.2)
{
    double[] ema = new double[data.Length];
    ema[0] = data[0];

    for (int i = 1; i < data.Length; i++)
    {
        ema[i] = alpha * data[i] + (1 - alpha) * ema[i-1];
    }

    return ema;
}
```

### Działanie
- Exponential Moving Average z współczynnikiem α = 0.2
- Nowa wartość ma wagę 20%, historia 80%
- Nowsze dane mają większą wagę niż w zwykłej średniej ruchomej

### Zalety EMA
- Szybsza reakcja na zmiany trendu
- Lepsza redukcja szumu niż prosta średnia ruchoma
- Zachowanie ostrych zmian mocy charakterystycznych dla interwałów

## KROK 3: Normalizacja do procentów FTP

### Cel
Unifikacja danych niezależnie od poziomu wytrenowania użytkownika.

### Implementacja
```csharp
power_percent = (power / ftp) * 100
```

### Działanie
- Konwersja wartości mocy z watów na procenty FTP
- Umożliwia porównywanie treningów różnych użytkowników
- Ułatwia klasyfikację do stref treningowych

## KROK 4: Dwuetapowe wykrywanie punktów zmian

### Cel
Precyzyjna identyfikacja miejsc, gdzie następuje zmiana intensywności treningu.

### Faza 1: Wykrywanie długich interwałów

**Parametry:**
- Okno czasowe: **30 sekund**
- Próg zmiany: **12% FTP**

**Działanie:**
- Dla każdego punktu porównanie średniej mocy przed i po punkcie
- Jeśli zmiana > 12%, punkt oznaczany jako zmiana intensywności
- Minimalna odległość między punktami: **50 sekund**

### Faza 2: Wykrywanie krótkich sprintów

**Parametry:**
- Okno czasowe: **10 sekund**
- Próg zmiany: **25% FTP**

**Działanie:**
- Krótsze okno dla wykrywania szybkich zmian
- Wyższy próg kompensuje większą wrażliwość na szum

### Łączenie wyników

```csharp
private List<int> MergeUniquePoints(List<int> pointsLong, List<int> pointsShort, int minGap = 20)
{
    // Sortowanie wszystkich wykrytych punktów
    var allPoints = pointsLong.Concat(pointsShort).OrderBy(p => p).ToList();

    // Grupowanie punktów odległych o mniej niż 20 sekund
    // Zastępowanie grupy punktów ich średnią pozycją
}
```

## KROK 5: Tworzenie segmentów

### Cel
Podział treningu na fragmenty o stałej intensywności.

### Implementacja
```csharp
private List<Segment> CreateSegments(double[] data, List<int> changePoints)
{
    int minDuration = 10;  // sekund

    foreach (var cp in changePoints)
    {
        if (cp - start > minDuration)
        {
            segments.Add(new Segment
            {
                Start = start,
                End = cp,
                AvgPower = data.Skip(start).Take(cp - start).Average()
            });
        }
        start = cp;
    }
}
```

### Działanie
- Tworzenie segmentów między kolejnymi punktami zmian
- Minimalna długość segmentu: **10 sekund**
- Obliczanie średniej mocy dla każdego segmentu

## KROK 6: Wykrywanie stopniowego narastania mocy

### Cel
Identyfikacja interwałów ze stopniowym wzrostem mocy (podjazdy, testy rampowe).

### Implementacja
```csharp
private List<Segment> DetectGradualIntervals(double[] data, int windowSize = 90, double minSlope = 10)
{
    while (i < data.Length - windowSize)
    {
        var windowData = data.Skip(i).Take(windowSize).ToArray();
        double slope = CalculateSlope(windowData);
        double slopePerMinute = slope * 60;

        if (slopePerMinute > minSlope)
        {
            // Wykryto stopniowe narastanie
            gradualIntervals.Add(...);
            i += windowSize;  // Przeskocz wykryte okno
        }
        else
        {
            i += 10;  // Przesuń okno o 10 sekund
        }
    }
}
```

### Parametry
- Okno analizy: **90 sekund**
- Minimalny gradient: **10% FTP/minutę**

### Działanie
- Przesuwne okno 90-sekundowe
- Regresja liniowa dla wyznaczenia nachylenia
- Jeśli nachylenie > 10% FTP/min, segment oznaczany jako "gradual"
- Po wykryciu, okno przesuwa się o 90 sekund; inaczej o 10 sekund

## KROK 7: Łączenie i sortowanie segmentów

### Cel
Utworzenie kompletnej, chronologiczej listy wszystkich segmentów.

### Działanie
- Połączenie segmentów ze skokami mocy i segmentów stopniowych
- Sortowanie według czasu rozpoczęcia
- Przygotowanie do klasyfikacji strefowej

## KROK 8: Klasyfikacja do stref treningowych

### Cel
Przypisanie każdego segmentu do odpowiedniej strefy intensywności.

### Model stref Coggan

| Strefa | Nazwa | Zakres % FTP | Charakterystyka |
|--------|-------|--------------|-----------------|
| Z1 | Recovery | < 55% | Regeneracja |
| Z2 | Endurance | 55-75% | Wytrzymałość tlenowa |
| Z3 | Tempo | 75-90% | Tempo maratońskie |
| Z4 | Threshold | 90-105% | Próg mleczanowy |
| Z5 | VO2max | 105-120% | Moc tlenowa |
| Z6 | Anaerobic | 120-150% | Moc beztlenowa |
| Z7 | Neuromuscular | > 150% | Sprinty maksymalne |

### Implementacja
```csharp
private (int zone, string zoneName) ClassifyZone(double avgPowerPercent)
{
    var zones = new[] { 55, 75, 90, 105, 120, 150 };
    var zoneNames = new[]
    {
        "Z1: Recovery", "Z2: Endurance", "Z3: Tempo",
        "Z4: Threshold", "Z5: VO2max", "Z6: Anaerobic",
        "Z7: Neuromuscular"
    };

    for (int i = 0; i < zones.Length; i++)
    {
        if (avgPowerPercent <= zones[i])
            return (i + 1, zoneNames[i]);
    }

    return (7, zoneNames[6]);
}
```

## KROK 9: Weryfikacja i walidacja segmentów

### Cel
Poprawa jakości wykrytych segmentów.

### 9.1: Wypełnianie luk
- Identyfikacja nieprzypisanych fragmentów > 30 sekund
- Analiza średniej mocy w lukach
- Klasyfikacja jako interwał (moc > 40% FTP) lub odpoczynek

### 9.2: Usuwanie nakładek
- Wykrywanie segmentów pokrywających się czasowo
- Wybór segmentu z wyższą średnią mocą
- Opcjonalne łączenie segmentów

### 9.3: Walidacja czasu trwania

**Minimalne czasy dla stref:**
- Z1-Z2: **30 sekund**
- Z3-Z4: **60 sekund**
- Z5: **30 sekund**
- Z6-Z7: **5 sekund**

```csharp
var minDurations = new Dictionary<int, int>
{
    { 7, 5 },   // Neuromuscular - min 5s
    { 6, 10 },  // Anaerobic - min 10s
    { 5, 30 },  // VO2max - min 30s
    { 4, 60 },  // Threshold - min 60s
    { 3, 120 }, // Tempo - min 120s
    { 2, 0 },   // Endurance - bez limitu
    { 1, 0 }    // Recovery - bez limitu
};
```

### 9.4: Wygładzanie granic
- Analiza ±5 sekund wokół punktu zmiany
- Przesunięcie granicy do miejsca największej zmiany mocy
- Poprawa precyzji wykrywania początku/końca interwału

## KROK 10: Filtrowanie i analiza odpoczynków

### Cel
Finalna selekcja interwałów i identyfikacja okresów regeneracji.

### Filtrowanie interwałów
- Zachowanie tylko segmentów w strefie **Z3 lub wyższej**
- Łączenie interwałów oddzielonych przerwą < **10 sekund**
- Obliczenie czasu trwania i mocy w watach

### Wykrywanie okresów odpoczynku

```csharp
private List<Segment> DetectRecoveryPeriods(List<Segment> intervals, double[] powerPercent)
{
    for (int i = 0; i < intervals.Count - 1; i++)
    {
        int recoveryStart = intervals[i].End;
        int recoveryEnd = intervals[i+1].Start;

        // Tylko jeśli przerwa dłuższa niż 30s
        if (recoveryEnd - recoveryStart > 30)
        {
            // Klasyfikuj jako okres odpoczynku
        }
    }
}
```

**Parametry:**
- Minimalna długość odpoczynku: **30 sekund**
- Klasyfikacja strefowa odpoczynku
- Obliczenie średniej mocy w okresie regeneracji

## Struktura danych wyjściowych

### Format interwału
```csharp
{
    Start: int,              // Sekunda rozpoczęcia
    End: int,                // Sekunda zakończenia
    Duration: int,           // Czas trwania w sekundach
    AvgPower: double,        // Średnia moc w % FTP
    AvgPowerWatts: double,   // Średnia moc w watach
    Zone: int,               // Numer strefy (1-7)
    ZoneName: string,        // Nazwa strefy
    Type: string,            // 'jump' lub 'gradual'
    Slope: double            // Nachylenie (tylko dla 'gradual')
}
```

### Format okresu odpoczynku
```csharp
{
    Start: int,
    End: int,
    Duration: int,
    AvgPower: double,
    AvgPowerWatts: double,
    Zone: int,
    ZoneName: string,
    Type: "recovery"
}
```

## Optymalizacje i wydajność

### Złożoność obliczeniowa
- Usuwanie outlierów: **O(n)**
- EMA: **O(n)**
- Wykrywanie punktów zmian: **O(n)**
- Tworzenie segmentów: **O(m)**, gdzie m = liczba punktów zmian
- Wykrywanie stopniowych zmian: **O(n)**
- Weryfikacja: **O(m²)** w najgorszym przypadku
- **Całkowita złożoność: O(n)** dla typowych danych

### Możliwości optymalizacji z AVX2

1. **Obliczenia Z-score (Krok 1)**
   - Wektoryzacja obliczania średniej i odchylenia standardowego
   - Równoległe obliczanie Z-score dla wielu próbek

2. **EMA (Krok 2)**
   - Trudne do wektoryzacji ze względu na zależność sekwencyjną
   - Możliwa optymalizacja cache-friendly

3. **Wykrywanie punktów zmian (Krok 4)**
   - Wektoryzacja obliczania średnich w oknach
   - SIMD dla porównań progowych

4. **Regresja liniowa (Krok 6)**
   - Wektoryzacja obliczeń macierzowych
   - AVX2 dla sum i iloczynów skalarnych

## Parametry do dostrojenia

| Parametr | Wartość domyślna | Zakres | Wpływ |
|----------|------------------|--------|-------|
| Z-score threshold | 3 | 2-4 | Czułość na outliery |
| EMA alpha | 0.2 | 0.1-0.3 | Stopień wygładzania |
| Long interval window | 30s | 20-40s | Wykrywanie długich zmian |
| Long interval threshold | 12% | 10-15% | Czułość na zmiany |
| Short interval window | 10s | 5-15s | Wykrywanie sprintów |
| Short interval threshold | 25% | 20-30% | Czułość na krótkie zmiany |
| Gradual window | 90s | 60-120s | Długość analizowanych ramp |
| Gradual min slope | 10%/min | 5-15%/min | Czułość na podjazdy |
| Min segment duration | 10s | 5-20s | Filtrowanie krótkich skoków |
| Merge gap | 10s | 5-20s | Łączenie bliskich interwałów |
| Recovery min duration | 30s | 20-60s | Definicja odpoczynku |

## Przykłady zastosowań

### 1. Trening interwałowy 4x4min
- Wykryje 4 interwały w strefie Z4-Z5
- Zidentyfikuje 3 okresy odpoczynku między nimi
- Obliczy średnią moc każdego interwału

### 2. Test rampowy
- Wykryje stopniowe narastanie mocy
- Podzieli na segmenty według stref
- Obliczy gradient wzrostu mocy

### 3. Trening sprinterski 10x10s
- Wykryje krótkie sprinty dzięki fazie 2
- Zidentyfikuje okresy regeneracji
- Klasyfikuje sprinty do strefy Z6-Z7

### 4. Jazda po pagórkowatym terenie
- Wykryje podjazdy jako interwały stopniowe
- Zjazdy jako okresy odpoczynku
- Fragmenty płaskie według intensywności

## Ograniczenia i uwagi

1. **Częstotliwość próbkowania**: Algorytm zakłada dane z częstotliwością 1 Hz
2. **Minimalna długość treningu**: Wymagane minimum 2 minuty danych
3. **Dokładność FTP**: Nieprawidłowa wartość FTP znacząco wpływa na klasyfikację
4. **Typ treningu**: Zoptymalizowany dla treningów strukturalnych
5. **Szum w danych**: Zbyt duży szum może wymagać dostrojenia parametrów

## Podsumowanie

Algorytm stanowi kompleksowe rozwiązanie do automatycznej analizy treningów wytrzymałościowych. Dzięki wieloetapowemu przetwarzaniu i zaawansowanym technikom detekcji, skutecznie identyfikuje różne typy interwałów - od krótkich sprintów po długie interwały progowe i stopniowe testy rampowe. Modułowa architektura umożliwia łatwe dostosowanie parametrów do specyficznych wymagań różnych dyscyplin sportowych.
