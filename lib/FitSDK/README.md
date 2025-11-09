# FIT SDK - Dynastream.Fit.dll

Ten katalog zawiera lokalny Dynastream.Fit DLL używany przez aplikację do parsowania plików FIT.

## Jak pobrać FIT SDK

### Opcja 1: Pobierz z oficjalnej strony Garmin (Zalecane)

1. Odwiedź oficjalną stronę Garmin FIT SDK:
   ```
   https://developer.garmin.com/fit/download/
   ```

2. Pobierz **FIT SDK** dla platformy .NET:
   - Kliknij "Download" dla "FIT SDK"
   - Rejestracja nie jest wymagana dla SDK (tylko dla protokołu)

3. Rozpakuj pobrany plik ZIP

4. Skopiuj `Dynastream.Fit.dll` do tego katalogu:
   - Lokalizacja w SDK: `FitSDK/cs/Dynastream.Fit.dll`
   - Docelowy katalog: `Ja/lib/FitSDK/Dynastream.Fit.dll`

### Opcja 2: Użyj dołączonego DLL (jeśli jest dostępne)

Jeśli `Dynastream.Fit.dll` jest już obecne w tym katalogu, nie musisz nic robić.

## Wersje SDK

Aplikacja została przetestowana z następującymi wersjami FIT SDK:
- **v21.141.0** (zalecana) - grudzień 2023
- v21.133.0 - wcześniejsze wersje też powinny działać

## Struktura katalogów

```
lib/
└── FitSDK/
    ├── Dynastream.Fit.dll          ← Umieść DLL tutaj
    ├── Dynastream.Fit.xml          (opcjonalne - dokumentacja IntelliSense)
    └── README.md                    (ten plik)
```

## Weryfikacja instalacji

Po umieszczeniu DLL, zbuduj projekt:

```bash
cd Ja
dotnet restore
dotnet build
```

Jeśli otrzymasz błąd:
```
error CS0006: Metadata file '..\lib\FitSDK\Dynastream.Fit.dll' could not be found
```

To znaczy, że DLL nie został umieszczony we właściwym miejscu.

## Licencja FIT SDK

FIT SDK jest dostarczane przez Garmin/Dynastream pod własną licencją.
- **Użycie SDK**: Darmowe dla celów rozwojowych i komercyjnych
- **Protokół FIT**: Może wymagać licencji dla niektórych zastosowań komercyjnych

Szczegóły licencji znajdują się w dokumentacji SDK.

## Dlaczego lokalny DLL zamiast NuGet?

Używamy lokalnego DLL z następujących powodów:

1. **Kontrola wersji** - Pewność, że używamy konkretnej wersji SDK
2. **Offline development** - Brak potrzeby dostępu do NuGet
3. **Kompatybilność** - Niektóre środowiska wymagają lokalnych referencji
4. **Licencjonowanie** - Jawna kontrola nad używaną wersją SDK

## Alternatywa: Użyj pakietu NuGet

Jeśli wolisz używać pakietu NuGet, zmodyfikuj `Ja.csproj`:

```xml
<!-- Usuń ten blok: -->
<ItemGroup>
  <Reference Include="Dynastream.Fit">
    <HintPath>..\lib\FitSDK\Dynastream.Fit.dll</HintPath>
    <Private>True</Private>
  </Reference>
</ItemGroup>

<!-- Dodaj to do bloku PackageReference: -->
<PackageReference Include="Dynastream.Fit" Version="21.141.0" />
```

## Wsparcie

Problemy z FIT SDK?
- 📖 [Oficjalna dokumentacja FIT SDK](https://developer.garmin.com/fit/overview/)
- 📋 [FIT File Format](https://developer.garmin.com/fit/file-types/)
- 💬 [Forum deweloperów Garmin](https://forums.garmin.com/developer/)

Problemy z aplikacją?
- Zgłoś issue na GitHub
- Sprawdź sekcję "Rozwiązywanie problemów" w README.md
