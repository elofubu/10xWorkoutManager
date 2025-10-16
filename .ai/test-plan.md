## 1. Wprowadzenie i cele testowania

- **Cel główny**: Zapewnić, że aplikacja 10xWorkoutManager działa zgodnie z wymaganiami funkcjonalnymi i niefunkcjonalnymi, z naciskiem na bezpieczeństwo (JWT, Supabase Auth), poprawność logiki domenowej (plany, dni treningowe, sesje, ćwiczenia) oraz stabilność interfejsu Blazor.
- **Cele szczegółowe**:
  - Zweryfikować przepływy uwierzytelniania i autoryzacji (WASM + API z JWT od Supabase).
  - Potwierdzić integralność operacji CRUD dla planów, sesji i ćwiczeń na warstwie API i UI.
  - Zapobiec regresjom przez automatyzację testów i włączenie ich do CI.
  - Zmierzyć kluczowe parametry wydajności i niezawodności.

## 2. Zakres testów

- **Frontend (Blazor WASM)**: Strony w `WorkoutManager.Web/Pages/**`, komponenty w `WorkoutManager.Web/Components/**`, routing i ochrona tras, `AuthorizationMessageHandler`, `SupabaseAuthenticationStateProvider`, usługi w `WorkoutManager.Web/Services/**`.
- **Backend (API .NET)**: Kontrolery w `WorkoutManager.Api/Controllers/**`, konfiguracja auth w `WorkoutManager.Api/Program.cs`, serwisy domenowe w `WorkoutManager.BusinessLogic/Services/**`, walidatory w `WorkoutManager.BusinessLogic/Validators/**`.
- **Dane (Supabase/PostgREST)**: Modele w `WorkoutManager.Data/Models/**`, migracje w `WorkoutManager.Data/supabase/migrations/**`, inicjalizacja (seeding) i konfiguracja CORS.
- **Konfiguracja i integracje**: Ustawienia `appsettings.json` (Web + API), zmienne środowiskowe `SUPABASE_URL`/`SUPABASE_KEY`, przechowywanie sesji w `Blazored.LocalStorage`.

Poza zakresem: testy przeglądarek legacy, dostępność dla czytników ekranowych na poziomie WCAG AA (opcjonalnie smoke/heurystyki).

## 3. Typy testów

- **Testy jednostkowe (BL + walidatory)**:
  - Serwisy domenowe: reguły biznesowe (np. pojedyncza aktywna sesja, właścicielstwo planów/zasobów).
  - Walidatory: `CreateExercise`, `CreateWorkoutPlan`, `Update*`, `StartSession`, `UpdateSessionExercise`.
- **Testy integracyjne (API + Supabase)**:
  - End-to-end na poziomie HTTP dla kontrolerów, z realnym JWT (lub stubowanym) i realnym dostępem do testowej bazy Supabase.
- **Testy E2E (UI + API + Auth)**:
  - Scenariusze użytkownika w przeglądarce: logowanie, tworzenie planu, rozpoczęcie/ukończenie sesji, aktualizacja serii/ciężaru, historia.
- **Testy kontraktowe (API)**:
  - Stabilność odpowiedzi (schematy DTO), kody statusu, komunikaty błędów.
- **Testy wydajnościowe**:
  - p95/p99 dla listowania (ćwiczenia, plany, historia sesji), tworzenia sesji, aktualizacji ćwiczeń w sesji.
- **Testy bezpieczeństwa**:
  - JWT (ważność, issuer, audience), brak dostępu bez tokena, odcięcie użytkownika od cudzych danych.
- **Testy użyteczności i dostępności (lekko)**:
  - Krytyczne ścieżki UI pod kątem komunikatów o błędach i nawigacji.
- **Testy regresyjne i smoke**:
  - Szybkie zestawy pokrywające najważniejsze ścieżki po każdym wdrożeniu.

## 4. Scenariusze testowe dla kluczowych funkcjonalności

### 4.1 Uwierzytelnianie i autoryzacja

- **Logowanie (Supabase Auth)**:
  - Poprawne dane -> sesja w `localStorage` (klucz `supabase_session`), ustawiony Bearer w `AuthorizationMessageHandler`, `AuthorizeView` pokazuje dane użytkownika.
  - Błędne dane -> brak sesji, komunikat o błędzie.
  - Wygasły token -> automatyczne wylogowanie, przekierowanie na `/authentication/login`.
- **Rejestracja**:
  - Ścieżka rejestracji i potwierdzeń (jeśli włączone). Uwaga: `RegisterAsync` zawiera placeholder – testy odnotowują aktualny status (oczekiwana porażka) i tworzą ticket na implementację.
- **Reset hasła i aktualizacja hasła**:
  - Trigger resetu, obsługa callbacku (`/authentication/update-password`), aktualizacja hasła po zalogowaniu.
- **Ochrona tras (Blazor)**:
  - Strony wymagające `[Authorize]` dostępne tylko dla zalogowanych.
  - Brak tokena/wygaśnięcie -> redirect do logowania.
- **API bez autoryzacji**:
  - Wywołania do endpointów wymagających JWT -> 401/403.
- **Wycieki uprawnień**:
  - Próba dostępu do zasobów innego użytkownika po stronie API (np. cudzy plan/sesja) -> 404/403.

### 4.2 Zarządzanie planami treningowymi

- **Lista planów**:
  - Paginacja, sortowanie domyślne, filtracja (jeżeli przewidziana).
- **Szczegóły planu**:
  - Struktura: dni treningowe, kolejność ćwiczeń w dniu.
- **Tworzenie planu**:
  - Poprawne dane -> 201 + ID planu.
  - Niepoprawne (walidacja) -> 400 z komunikatem walidatora.
- **Aktualizacja i usunięcie planu**:
  - Edycja nazwy/opisu; brak uprawnień -> 403; brak planu -> 404.
- **Dodanie ćwiczenia do dnia**:
  - Wyliczanie `order` kolejnego elementu, idempotencja (brak duplikatów, jeżeli wymóg).
  - Błędne `exerciseId` -> 404.

### 4.3 Sesje treningowe

- **Start sesji**:
  - Bez aktywnej sesji -> 201 z utworzonymi `SessionExercise` zgodnie z kolejnością dnia.
  - Z aktywną sesją -> 409 z komunikatem BusinessRuleViolation.
  - Dzień należy do innego użytkownika -> 404/403.
- **Pobranie aktywnej sesji**:
  - Gdy istnieje/nie istnieje -> 200/204.
- **Aktualizacja notatek sesji**:
  - Zmiana notatek -> 204; brak sesji -> 404.
- **Zakończenie sesji**:
  - Ustawia `EndTime`, zapisuje notatki; ponowne zakończenie -> 409.
- **Aktualizacja ćwiczenia w sesji**:
  - Edycja powtórzeń/ciężaru/serii zgodnie z walidacją; złe dane -> 400.
- **Historia sesji**:
  - Paginacja, kolejność po `start_time` malejąco.

### 4.4 Ćwiczenia i osiągi

- **Lista i szczegóły ćwiczeń**:
  - Filtrowanie po grupie mięśniowej, paginacja, szczegóły.
- **Tworzenie ćwiczenia**:
  - Walidacja nazwy unikalnej w obrębie użytkownika (jeśli wymóg), grupy mięśniowej.
- **Ostatni wynik (previous-session)**:
  - Gdy istnieje -> 200 + dane poprzedniej sesji; gdy brak -> 404 z informacją.

### 4.5 Walidatory (FluentValidation)

- Każdy walidator: dane poprawne/niepoprawne, komunikaty, zakresy liczb (`order`, liczby serii/powtórzeń), wymagane pola.

### 4.6 Obsługa błędów i kody statusu

- Mapowanie wyjątków domenowych: `NotFoundException` -> 404, `BusinessRuleViolationException` -> 409/403, `ValidationException` -> 400.
- Spójność formatów odpowiedzi błędów (`{ error: "..." }`).

### 4.7 Scenariusze E2E (UI)

- **Pełny przebieg**: Logowanie -> utworzenie planu -> dodanie dnia -> dodanie ćwiczeń -> rozpoczęcie sesji -> wprowadzenie danych -> zakończenie -> weryfikacja w historii.
- **Wznawianie sesji**: Istniejąca aktywna sesja -> dialog (kontynuuj / zakończ i rozpocznij nową) -> poprawne przekierowania i stan.
- **Wylogowanie**: Usunięcie sesji z `localStorage`, brak dostępu do tras chronionych.

## 5. Środowisko testowe

- **Lokalne Supabase**:
  - `SUPABASE_URL=http://127.0.0.1:54321`, klucze testowe z `launchSettings.json`.
  - Uruchamianie przez `supabase start`, aplikacja migracji z `WorkoutManager.Data/supabase/migrations/**`.
- **API**:
  - `https://localhost:5048`, konfiguracja JWT w `WorkoutManager.Api/Program.cs`.
  - Uwaga: w kodzie widoczny testowy `IssuerSigningKey`; na testach używać wyłącznie odpowiednich kluczy testowych i/lub weryfikować podpisy w oparciu o ustawienia Supabase.
- **Frontend**:
  - Konfiguracja `WorkoutManager.Web/wwwroot/appsettings.json` z danymi Supabase (klucz ANON dla testów).
- **Dane testowe**:
  - Oddzielna baza testowa/konto testowe; seed minimalny (grupy mięśniowe, przykładowe ćwiczenia).
  - Konta: `user1@test.local`, `user2@test.local` z izolacją danych.
- **Automatyzacja**:
  - Możliwość uruchamiania testów bez interakcji (CI), deterministyczne dane wejściowe.

## 6. Narzędzia do testowania

### 6.1 Testy jednostkowe i integracyjne (.NET)
- **Framework testowy**: **xUnit** (standard dla nowoczesnych projektów .NET)
- **Asercje**: **FluentAssertions** (czytelne, rozbudowane asercje)
- **Testy API**: **WebApplicationFactory** (natywne rozwiązanie Microsoft dla testów integracyjnych)
- **Zarządzanie bazą danych**: **Respawn** (szybki reset bazy między testami, zamiast Testcontainers)
  - Respawn + lokalny Supabase przez Docker Compose = szybsze setupy testowe
  - Brak nadmiarowej złożoności kontenerów w każdym teście
- **Generowanie danych testowych**: **Bogus** (realistyczne dane testowe)
- **Snapshot testing**: **Verify** / **Verify.Http** (testowanie kontraktów API jako snapshotów)
- **DSL dla API**: **Alba** (opcjonalnie, dla czytelniejszych testów HTTP)
- **Walidatory**: **FluentValidation.TestHelper**

### 6.2 Testy komponentów Blazor
- **bUnit**: testowanie komponentów Blazor, logiki renderowania, stanu autoryzacji
- **AngleSharp**: parsowanie HTML (używane wewnątrz bUnit)

### 6.3 Testy E2E (UI)
- **Microsoft Playwright for .NET**: główne narzędzie E2E dla Blazor WASM
  - Lepsze wsparcie dla .NET niż Cypress
  - Headless mode dla CI
  - Wieloprzeglądarkowe testowanie (Chromium, Firefox, WebKit)

### 6.4 Testy kontraktowe API
- **Verify.Http**: snapshot testing dla kontraktów HTTP
- **Alba**: DSL dla testowania endpointów i kontraktów
- ~~Postman/Newman~~ (zbyt zewnętrzne dla CI, trudne w utrzymaniu)
- **Pact**: tylko jeśli wymagane consumer-driven contracts (opcjonalne, może być overkill dla monorepo)

### 6.5 Testy wydajnościowe
- **k6**: główne narzędzie (skrypty w JS, doskonałe metryki, integracja z CI)
- **NBomber**: alternatywa w C# jeśli preferowany natywny stos .NET

### 6.6 Testy bezpieczeństwa
- **OWASP ZAP**: DAST (Dynamic Application Security Testing)
- **Security Code Scan**: SAST (Static Analysis) - analyzer dla .NET w build time
- **SonarQube Community**: statyczna analiza kodu + security gates
- **Biblioteki JWT**: generowanie i walidacja/psowanie tokenów dla testów autoryzacji

### 6.7 Testy architektury i jakości
- **NetArchTest**: weryfikacja reguł architektury (zależności między warstwami, konwencje nazewnictwa)
- **Stryker.NET**: mutation testing (wykrywanie słabych testów)

### 6.8 Pokrycie kodu i raporty
- **coverlet.collector**: zbieranie metryk pokrycia
- **ReportGenerator**: generowanie czytelnych raportów HTML/lcov
- Integracja z CI dla automatycznych raportów

### 6.9 CI/CD
- **GitHub Actions**: 
  - Joby: restore, build, unit tests, integration tests, e2e headless
  - Artefakty: raporty pokrycia, wyniki testów, logi Playwright
  - Cache dla dependencies (.NET, npm)
  - Macierz dla różnych przeglądarek (Playwright)

## 7. Harmonogram testów

- **Tydzień 1**: Ustanowienie szkieletu testów, testy jednostkowe walidatorów i podstawowych reguł serwisów, konfiguracja CI i coverage.
- **Tydzień 2**: Testy integracyjne API (autoryzacja, CRUD planów/ćwiczeń, sesje), dane testowe i seed, kontrakty błędów.
- **Tydzień 3**: Testy E2E (Playwright) dla kluczowych ścieżek + smoke, wstępne testy wydajności (k6).
- **Tydzień 4**: Hardening (negatywne, bezpieczeństwo JWT, próby dostępu między użytkownikami), regresja i stabilizacja.

## 8. Kryteria akceptacji testów

- **Funkcjonalne**:
  - 100% zielone smoke i krytyczne E2E (logowanie, pełna sesja, CRUD planu).
  - Testy integracyjne kluczowych endpointów: min. 95% pass.
- **Pokrycie**:
  - BL + walidatory: ≥ 80% linii i ≥ 80% gałęzi (kluczowe klasy ≥ 90%).
- **Wydajność**:
  - p95: lista planów/ćwiczeń < 300 ms; start sesji < 500 ms (środowisko lokalne/CI, umowne metryki).
- **Bezpieczeństwo**:
  - Brak krytycznych/większych podatności (JWT, CORS, dostępy między użytkownikami).
- **Stabilność**:
  - 0 awarii E2E flake w 3 kolejnych przebiegach CI.

## 9. Role i odpowiedzialności

- **QA**: definiowanie scenariuszy, automatyzacja testów, raportowanie defektów, metryki.
- **Developerzy**: implementacja testów jednostkowych BL/walidatorów, wsparcie napraw, przegląd wyników.
- **DevOps**: konfiguracja CI/CD, środowisk testowych (Supabase), artefakty raportowe.
- **Product/PO**: akceptacja wyników testów i kryteriów.

## 10. Procedury raportowania błędów

- **Zgłaszanie**: GitHub Issues (szablon: kroki odtworzenia, oczekiwane vs. rzeczywiste, logi, zrzuty ekranu, środowisko, commit/SHA).
- **Priorytety**:
  - P0: blokuje krytyczną ścieżkę (logowanie, start/finish sesji).
  - P1: duży wpływ, brak obejścia (CRUD planów/ćwiczeń).
  - P2: średni wpływ/obejście istnieje.
  - P3: drobne UI/tekst.
- **Triage**: codziennie, przypisanie, ETA, link do testu automatycznego (jeśli istnieje).
- **Weryfikacja napraw**: test automatyczny + manualna walidacja scenariusza użytkownika.
- **Metryki**: trend pass/fail, MTTR, defekty per moduł, pokrycie.

### Załączniki operacyjne (do realizacji w repo)

- **Struktura testów**:
  - `tests/WorkoutManager.BusinessLogic.Tests` – unit/walidatory (xUnit, FluentAssertions, Bogus).
  - `tests/WorkoutManager.Api.IntegrationTests` – integracja (WebApplicationFactory, Respawn, Alba).
  - `tests/WorkoutManager.Web.ComponentTests` – testy komponentów (bUnit).
  - `tests/WorkoutManager.Web.E2E` – Playwright (headless), scenariusze użytkownika.
  - `tests/WorkoutManager.ArchitectureTests` – reguły architektury (NetArchTest).
- **Dane testowe i setup**:
  - Skrypty migracji/seed dla środowiska testowego, użytkownicy testowi.
  - Respawn checkpoint dla czyszczenia bazy między testami.
  - Bogus Faker dla generowania danych testowych.
- **Uruchamianie**:
  - `dotnet test --collect:"XPlat Code Coverage"` – wszystkie projekty testowe.
  - `playwright test` – testy E2E headless.
  - `k6 run performance/api-load-test.js` – testy wydajnościowe.
  - `dotnet stryker` – mutation testing (opcjonalnie).
- **Przykładowe konfiguracje**:
  ```csharp
  // Respawn setup w IntegrationTests
  private static readonly Checkpoint _checkpoint = new() 
  {
      TablesToIgnore = new[] { "__EFMigrationsHistory", "muscle_groups" },
      SchemasToInclude = new[] { "public" }
  };
  
  // Bogus faker dla danych testowych
  private readonly Faker<CreateExerciseCommand> _exerciseFaker = new Faker<CreateExerciseCommand>()
      .RuleFor(x => x.Name, f => f.Commerce.ProductName())
      .RuleFor(x => x.Description, f => f.Lorem.Sentence())
      .RuleFor(x => x.MuscleGroupId, f => f.PickRandom(muscleGroupIds));
  
  // Alba DSL dla testów API
  await using var host = await AlbaHost.For<Program>();
  var result = await host.Scenario(_ =>
  {
      _.Post.Json(createPlanCommand).ToUrl("/api/plans");
      _.StatusCodeShouldBe(201);
      _.Header("Location").ShouldHaveValues();
  });
  ```

## 11. Rekomendacje dodatkowe i best practices

### 11.1 Instalacja i konfiguracja pakietów

**Projekty testowe - podstawowe pakiety NuGet:**
```xml
<!-- WorkoutManager.BusinessLogic.Tests -->
<PackageReference Include="xunit" Version="2.6.0" />
<PackageReference Include="xunit.runner.visualstudio" Version="2.5.0" />
<PackageReference Include="FluentAssertions" Version="6.12.0" />
<PackageReference Include="Bogus" Version="35.0.0" />
<PackageReference Include="FluentValidation.TestHelper" Version="11.9.0" />
<PackageReference Include="coverlet.collector" Version="6.0.0" />

<!-- WorkoutManager.Api.IntegrationTests -->
<PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="9.0.0" />
<PackageReference Include="Respawn" Version="6.1.0" />
<PackageReference Include="Alba" Version="7.5.0" />
<PackageReference Include="Verify.Xunit" Version="22.0.0" />
<PackageReference Include="Verify.Http" Version="4.0.0" />

<!-- WorkoutManager.Web.ComponentTests -->
<PackageReference Include="bUnit" Version="1.26.0" />
<PackageReference Include="bUnit.web" Version="1.26.0" />

<!-- WorkoutManager.ArchitectureTests -->
<PackageReference Include="NetArchTest.Rules" Version="1.3.2" />
```

### 11.2 Wzorce testowe dla projektu

**Test jednostkowy z Bogus:**
```csharp
public class ExerciseServiceTests
{
    private readonly Faker<Exercise> _exerciseFaker;
    
    public ExerciseServiceTests()
    {
        _exerciseFaker = new Faker<Exercise>()
            .RuleFor(x => x.Id, f => f.Random.Guid())
            .RuleFor(x => x.Name, f => f.Commerce.ProductName())
            .RuleFor(x => x.UserId, _ => TestData.UserId);
    }
    
    [Fact]
    public async Task CreateExercise_ShouldReturnExercise_WhenValid()
    {
        // Arrange
        var exercise = _exerciseFaker.Generate();
        
        // Act & Assert
        result.Should().NotBeNull();
    }
}
```

**Test integracyjny z Respawn:**
```csharp
public class ApiIntegrationTest : IAsyncLifetime
{
    private readonly Checkpoint _checkpoint = new()
    {
        TablesToIgnore = new[] { "__EFMigrationsHistory", "muscle_groups" },
        DbAdapter = DbAdapter.Postgres
    };
    
    public async Task InitializeAsync()
    {
        await _checkpoint.Reset(_connectionString);
    }
}
```

**Test architektury z NetArchTest:**
```csharp
[Fact]
public void BusinessLogic_Should_Not_Reference_Web()
{
    var result = Types.InAssembly(typeof(IExerciseService).Assembly)
        .Should()
        .NotHaveDependencyOn("WorkoutManager.Web")
        .GetResult();
        
    result.IsSuccessful.Should().BeTrue();
}
```

### 11.3 GitHub Actions - przykładowa konfiguracja

```yaml
name: Tests

on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    
    services:
      postgres:
        image: supabase/postgres
        env:
          POSTGRES_PASSWORD: postgres
    
    steps:
      - uses: actions/checkout@v4
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '9.0.x'
      
      - name: Restore
        run: dotnet restore
      
      - name: Build
        run: dotnet build --no-restore
      
      - name: Unit Tests
        run: dotnet test --no-build --filter Category=Unit --collect:"XPlat Code Coverage"
      
      - name: Integration Tests
        run: dotnet test --no-build --filter Category=Integration
      
      - name: Generate Coverage Report
        uses: danielpalme/ReportGenerator-GitHub-Action@5
        with:
          reports: '**/coverage.cobertura.xml'
          targetdir: 'coverage-report'
      
      - name: Upload Coverage
        uses: codecov/codecov-action@v3
        with:
          files: coverage-report/Cobertura.xml

  e2e:
    runs-on: ubuntu-latest
    
    steps:
      - uses: actions/checkout@v4
      
      - name: Setup Playwright
        run: dotnet tool install --global Microsoft.Playwright.CLI
      
      - name: Run E2E Tests
        run: |
          cd tests/WorkoutManager.Web.E2E
          playwright install
          dotnet test
```

### 11.4 Priorytetyzacja implementacji

**Faza 1 (Tydzień 1)** - Fundament:
1. xUnit + FluentAssertions
2. Bogus (generowanie danych)
3. Testy jednostkowe walidatorów
4. Podstawowe pokrycie serwisów BL

**Faza 2 (Tydzień 2)** - Integracja:
1. WebApplicationFactory
2. Respawn (reset DB)
3. Testy integracyjne kluczowych endpointów
4. Verify.Http (snapshoty kontraktów)

**Faza 3 (Tydzień 3)** - E2E i wydajność:
1. Playwright (krytyczne ścieżki)
2. bUnit (komponenty Blazor)
3. k6 (podstawowe scenariusze wydajnościowe)

**Faza 4 (Tydzień 4)** - Zaawansowane:
1. NetArchTest (reguły architektury)
2. Security Code Scan + SonarQube
3. Stryker.NET (opcjonalnie)
4. OWASP ZAP (DAST)

### 11.5 Alternatywy i trade-offs

| Narzędzie | Alternatywa | Kiedy wybrać alternatywę |
|-----------|-------------|--------------------------|
| Alba | Czysty WebApplicationFactory | Mały projekt, proste testy |
| k6 | NBomber | Wolisz C# zamiast JS |
| Verify.Http | Ręczne asercje | Nie potrzebujesz snapshot testing |
| NetArchTest | Brak | Mała aplikacja, jednolita architektura |
| Stryker.NET | Brak | Brak zasobów na mutation testing |

### 11.6 Metryki jakości do śledzenia

- **Pokrycie kodu**: ≥80% dla BL, ≥60% dla Controllers
- **Mutation score**: ≥75% (jeśli używasz Stryker)
- **Pass rate**: ≥95% testów green w CI
- **Flakiness**: <2% testów niestabilnych
- **Czas wykonania**: Unit <5s, Integration <30s, E2E <5min
- **Technical debt**: SonarQube Quality Gate passed

### Ryzyka i działania łagodzące

- **Auth/konfiguracja JWT**: rozbieżności między kluczami/issuer w DEV a Supabase – testy weryfikują poprawność walidacji i odświeżania sesji.
- **Brak RLS po stronie DB (jeśli wyłączone)**: wzmocnić testy autoryzacji na API i odseparować dane użytkowników w testach.
- **Placeholders (rejestracja)**: testy oznaczają znanym defektem; zdefiniować kryterium ukończenia po implementacji.
- **Zależność od czasu (wygaśnięcie tokenu, `EndTime`)**: testy z kontrolą zegara/mockingiem daty lub krótkimi TTL na tokenach w środowisku testowym.

---

## 12. Podsumowanie aktualizacji planu

### 12.1 Zmiany w stosunku do wersji oryginalnej

**Usunięte/Zastąpione technologie:**
- ❌ **NUnit** → Usunięty (xUnit jest standardem)
- ❌ **Testcontainers** → Zastąpiony przez **Respawn** (prostsze, szybsze)
- ❌ **WireMock.Net** → Usunięty (niepotrzebny z WebApplicationFactory)
- ❌ **Postman/Newman** → Zastąpiony przez **Verify.Http + Alba** (lepiej integruje się z .NET)
- ⚠️ **Cypress** → Usunięty jako alternatywa (słabe wsparcie dla .NET/Blazor)
- ⚠️ **Artillery** → Usunięty jako alternatywa (k6 jest lepszy)

**Dodane technologie:**
- ✅ **Bogus** → Generowanie realistycznych danych testowych (krytyczne!)
- ✅ **Verify / Verify.Http** → Snapshot testing dla kontraktów API
- ✅ **Alba** → DSL dla czytelniejszych testów HTTP
- ✅ **NetArchTest** → Weryfikacja reguł architektury
- ✅ **Stryker.NET** → Mutation testing (opcjonalnie)
- ✅ **Security Code Scan** → SAST w build time
- ✅ **SonarQube Community** → Statyczna analiza + quality gates
- ✅ **ReportGenerator** → Czytelne raporty pokrycia
- ✅ **NBomber** → Alternatywa dla k6 w C#
- ✅ **AngleSharp** → Parsowanie HTML (używane przez bUnit)

**Zachowane technologie (potwierdzenie):**
- ✅ xUnit, FluentAssertions, WebApplicationFactory
- ✅ FluentValidation.TestHelper
- ✅ Playwright for .NET
- ✅ bUnit
- ✅ k6
- ✅ OWASP ZAP
- ✅ coverlet.collector
- ✅ GitHub Actions

### 12.2 Uzasadnienie kluczowych zmian

**1. Respawn zamiast Testcontainers:**
- Testcontainers wprowadza dużą złożoność dla prostego use case (reset bazy)
- Respawn to lightweight, szybkie czyszczenie bazy między testami
- Masz już Supabase w Docker Compose - nie potrzebujesz kontenerów w każdym teście
- Dramatycznie szybsze setupy testowe

**2. Alba + Verify.Http zamiast Postman/Newman:**
- Postman/Newman to zewnętrzne narzędzia, trudne w CI i refaktoringu
- Alba daje czytelny DSL bezpośrednio w C#
- Verify.Http automatycznie tworzy snapshoty kontraktów
- Łatwiejsze utrzymanie i wersjonowanie z kodem

**3. Bogus - krytyczna luka w oryginalnym planie:**
- Oryginalny plan nie wspominał o generatorze danych testowych
- Bez Bogus testujesz tylko happy path z hardcodowanymi wartościami
- Bogus generuje realistyczne, losowe dane - lepsze pokrycie edge cases

**4. NetArchTest + Stryker.NET - proaktywna jakość:**
- NetArchTest zapobiega naruszeniom architektury (np. Web → Data bezpośrednio)
- Stryker.NET wykrywa słabe testy (mutation testing)
- Automatyzacja tego co zwykle jest manualnym code review

**5. Security Code Scan + SonarQube:**
- OWASP ZAP to tylko DAST (dynamiczna analiza działającej aplikacji)
- Security Code Scan + SonarQube to SAST (statyczna analiza kodu)
- Łapią błędy bezpieczeństwa przed deploymentem

### 12.3 Rekomendowany stack - finalna wersja

```
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
🎯 CORE TESTING STACK
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Unit & Integration:
  • xUnit                  [Framework]
  • FluentAssertions       [Assertions]
  • Bogus                  [Test data generation] ⭐ NEW
  • WebApplicationFactory  [API testing]
  • Respawn               [DB reset] ⭐ INSTEAD OF Testcontainers

API Contracts:
  • Alba                   [DSL for HTTP] ⭐ NEW
  • Verify.Http           [Snapshot testing] ⭐ NEW

Components:
  • bUnit                  [Blazor components]

E2E:
  • Playwright for .NET    [Browser automation]

Performance:
  • k6                     [Load testing]
  • NBomber (optional)     [.NET alternative] ⭐ NEW

Security:
  • OWASP ZAP             [DAST]
  • Security Code Scan    [SAST] ⭐ NEW
  • SonarQube Community   [Code quality + security] ⭐ NEW

Architecture:
  • NetArchTest           [Architecture rules] ⭐ NEW
  • Stryker.NET (opt.)    [Mutation testing] ⭐ NEW

CI/CD:
  • GitHub Actions        [Automation]
  • coverlet.collector    [Coverage]
  • ReportGenerator       [Coverage reports] ⭐ NEW
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
```

### 12.4 Harmonogram - bez zmian

Harmonogram pozostaje ten sam (4 tygodnie), ale z zaktualizowanymi narzędziami:
- **Tydzień 1**: xUnit + FluentAssertions + **Bogus** + walidatory
- **Tydzień 2**: WebApplicationFactory + **Respawn** + **Alba/Verify.Http**
- **Tydzień 3**: Playwright + bUnit + k6
- **Tydzień 4**: **NetArchTest** + Security (ZAP/**SCS**/**SonarQube**) + stabilizacja

### 12.5 Następne kroki

1. ✅ Zatwierdzenie zaktualizowanego planu
2. 🔨 Utworzenie struktury projektów testowych
3. 📦 Instalacja pakietów NuGet (sekcja 11.1)
4. 🧪 Implementacja pierwszych testów jednostkowych (Tydzień 1)
5. 🔄 Konfiguracja CI/CD (sekcja 11.3)

**Gotowe do rozpoczęcia implementacji!** 🚀


