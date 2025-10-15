# Specyfikacja Techniczna: Implementacja Uwierzytelniania i Autoryzacji

## 1. Wprowadzenie

Niniejszy dokument opisuje architekturę i szczegóły techniczne implementacji modułu uwierzytelniania i autoryzacji dla aplikacji 10xWorkoutManager. Rozwiązanie bazuje na wymaganiach zdefiniowanych w PRD (US-025, US-026) oraz na stosie technologicznym opartym o Blazor WASM, .NET REST API i Supabase.

Celem jest zabezpieczenie aplikacji w taki sposób, aby wszystkie jej funkcjonalności (poza stronami logowania, rejestracji i resetowania hasła) były dostępne wyłącznie dla zalogowanych użytkowników.

## 2. Architektura Systemu Uwierzytelniania

System będzie oparty na tokenach JWT (JSON Web Token) dostarczanych przez Supabase Auth.

**Przepływ uwierzytelniania:**

1.  **Klient (Blazor WASM)**: Użytkownik wprowadza dane logowania/rejestracji w interfejsie graficznym.
2.  **Klient -> Supabase**: Aplikacja kliencka komunikuje się bezpośrednio z Supabase Auth w celu weryfikacji poświadczeń lub rejestracji nowego użytkownika.
3.  **Supabase -> Klient**: W przypadku pomyślnego uwierzytelnienia, Supabase zwraca token JWT.
4.  **Przechowywanie tokena**: Aplikacja Blazor zapisuje token JWT w bezpiecznym miejscu po stronie klienta (przeglądarki) - `Blazored.LocalStorage`.
5.  **Klient -> API (.NET)**: Przy każdym zapytaniu do naszego backendu, aplikacja Blazor dołącza token JWT do nagłówka `Authorization` jako `Bearer token`.
6.  **API (.NET)**: Backend weryfikuje token JWT (jego sygnaturę, wystawcę i ważność) przy użyciu klucza publicznego dostarczonego przez Supabase. Jeśli token jest prawidłowy, API przetwarza zapytanie, identyfikując użytkownika na podstawie `claim` `sub` (Subject/User ID) zawartego w tokenie.

## 3. Architektura Interfejsu Użytkownika (Frontend - Blazor WASM)

### 3.1. Usługi Klienckie

#### `IAuthService` i `AuthService`
Istniejąca usługa zostanie rozbudowana o logikę komunikacji z Supabase.

-   **Kontrakt (`IAuthService`):**
    ```csharp
    public interface IAuthService
    {
        Task<bool> LoginAsync(string email, string password);
        Task RegisterAsync(string email, string password);
        Task LogoutAsync();
        Task ResetPasswordAsync(string email);
    }
    ```
-   **Implementacja (`AuthService`):**
    -   Będzie wykorzystywać oficjalny klient C# Supabase (`Supabase.Client`).
    -   `LoginAsync`: Wywołuje `_supabaseClient.Auth.SignIn()`. Po sukcesie, zapisuje sesję (w tym JWT) do `ILocalStorageService` i powiadamia `AuthenticationStateProvider` o zmianie stanu.
    -   `RegisterAsync`: Wywołuje `_supabaseClient.Auth.SignUp()`.
    -   `LogoutAsync`: Wywołuje `_supabaseClient.Auth.SignOut()`, czyści token z `ILocalStorageService` i powiadamia `AuthenticationStateProvider`.
    -   `ResetPasswordAsync`: Wywołuje `_supabaseClient.Auth.ResetPasswordForEmail()`.

#### `SupabaseAuthenticationStateProvider`
Należy stworzyć nową klasę dziedziczącą po `AuthenticationStateProvider`. Będzie to centralny punkt zarządzania stanem uwierzytelnienia w aplikacji.

-   **Odpowiedzialności:**
    -   W metodzie `GetAuthenticationStateAsync()` sprawdza, czy w `ILocalStorageService` znajduje się ważna sesja Supabase.
    -   Jeśli tak, parsuje token JWT, tworzy obiekt `ClaimsPrincipal` z informacjami o użytkowniku (ID, email) i zwraca `AuthenticationState`.
    -   Jeśli nie, zwraca pusty `AuthenticationState` dla niezalogowanego użytkownika.
    -   Udostępnia metodę `NotifyUserAuthenticationStateChanged()`, która będzie wywoływana przez `AuthService` po zalogowaniu i wylogowaniu, aby wymusić odświeżenie stanu w całej aplikacji.

### 3.2. Konfiguracja Aplikacji (`WorkoutManager.Web/Program.cs`)

Należy dodać i skonfigurować następujące elementy:

1.  **Rejestracja usług autoryzacji:**
    ```csharp
    builder.Services.AddAuthorizationCore();
    ```
2.  **Rejestracja niestandardowego `AuthenticationStateProvider`:**
    ```csharp
    builder.Services.AddScoped<AuthenticationStateProvider, SupabaseAuthenticationStateProvider>();
    ```
3.  **Konfiguracja `HttpClient`:**
    Należy stworzyć `DelegatingHandler` (np. `AuthorizationMessageHandler`), który będzie dołączał token JWT do każdego wychodzącego zapytania do API. Handler ten będzie pobierał token z `ILocalStorageService`.
    ```csharp
    // W Program.cs
    builder.Services.AddScoped<AuthorizationMessageHandler>();
    builder.Services.AddHttpClient("ApiHttpClient", client => 
        client.BaseAddress = new Uri("https://localhost:5048"))
        .AddHttpMessageHandler<AuthorizationMessageHandler>();
    
    // Zmiana sposobu wstrzykiwania HttpClient w serwisach
    builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("ApiHttpClient"));
    ```

### 3.3. Komponenty UI

#### `App.razor`
Należy opakować `Router` w komponent `CascadingAuthenticationState`, aby stan uwierzytelnienia był dostępny w całej aplikacji. Komponent `RouteView` należy zastąpić `AuthorizeRouteView`, aby włączyć ochronę routingu.

```html
<CascadingAuthenticationState>
    <Router AppAssembly="@typeof(App).Assembly">
        <Found Context="routeData">
            <AuthorizeRouteView RouteData="@routeData" DefaultLayout="@typeof(MainLayout)">
                <NotAuthorized>
                    <RedirectToLogin />
                </NotAuthorized>
            </AuthorizeRouteView>
            <FocusOnNavigate RouteData="@routeData" Selector="h1" />
        </Found>
        ...
    </Router>
</CascadingAuthenticationState>
```

#### `RedirectToLogin.razor` (nowy komponent)
Prosty komponent, który będzie nawigował niezalogowanego użytkownika do strony logowania.

```csharp
// W @code
[Inject] private NavigationManager Navigation { get; set; }

protected override void OnInitialized()
{
    Navigation.NavigateTo("authentication/login");
}
```

#### `MainLayout.razor`
Layout główny musi zostać zaktualizowany, aby dynamicznie wyświetlać informacje o użytkowniku.

-   Należy użyć komponentu `<AuthorizeView>`.
-   W sekcji `<Authorized>`: wyświetlić email zalogowanego użytkownika (`context.User.Identity.Name`) oraz przycisk "Wyloguj", który wywołuje `AuthService.LogoutAsync()`.
-   W sekcji `<NotAuthorized>`: można pozostawić pustą lub wyświetlić przyciski "Zaloguj" / "Zarejestruj", chociaż dzięki `AuthorizeRouteView` użytkownik i tak zostanie przekierowany.

### 3.4. Ochrona Stron

Wszystkie strony (pliki `.razor`), które mają być dostępne tylko dla zalogowanych użytkowników, muszą zostać opatrzone atrybutem `[Authorize]`.

```csharp
@page "/workout-plans"
@attribute [Authorize]

<h3>Workout Plans</h3>
...
```

Strony w katalogu `/Pages/Authentication` **nie powinny** mieć tego atrybutu.

### 3.5. Walidacja i Obsługa Błędów

Formularze w `LoginPage.razor` i `RegisterPage.razor` powinny używać wbudowanych walidatorów Blazor (`DataAnnotationsValidator`). Logika w `*.razor.cs` powinna obsługiwać wyjątki zwracane przez `AuthService` (np. nieprawidłowe hasło, użytkownik już istnieje) i wyświetlać stosowne komunikaty użytkownikowi.

## 4. Logika Backendowa (API - .NET)

### 4.1. Konfiguracja Uwierzytelniania (`WorkoutManager.Api/Program.cs`)

Należy dodać i skonfigurować middleware do obsługi uwierzytelniania JWT.

```csharp
// Dodanie usług uwierzytelniania
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Supabase:Url"]; // URL projektu Supabase
        options.Audience = "authenticated"; // Standardowa publiczność dla Supabase
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Supabase:Url"],
            ValidAudience = "authenticated",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Supabase:JwtSecret"]))
        };
    });

builder.Services.AddAuthorization();

// W pipeline aplikacji (przed app.MapControllers()):
app.UseAuthentication();
app.UseAuthorization();
```

Klucze `Supabase:Url` i `Supabase:JwtSecret` muszą zostać dodane do pliku `appsettings.json`. Klucz JWT Secret jest dostępny w ustawieniach projektu Supabase.

### 4.2. Ochrona Endpointów

Wszystkie kontrolery lub poszczególne akcje, które wymagają autoryzacji, muszą zostać opatrzone atrybutem `[Authorize]`.

```csharp
[ApiController]
[Route("[controller]")]
[Authorize] // Ochrona na poziomie całego kontrolera
public class WorkoutPlansController : ControllerBase
{
    // ...
}
```

### 4.3. Kontekst Użytkownika (`IUserContextService`)

Implementacja `UserContextService` powinna zostać dostosowana do odczytywania ID użytkownika z `ClaimsPrincipal` dostarczonego przez middleware.

-   **Implementacja (`UserContextService`):**
    -   Należy wstrzyknąć `IHttpContextAccessor`.
    -   Właściwość `UserId` będzie odczytywać `claim` `NameIdentifier` (który Supabase mapuje na `sub` - ID użytkownika) z `_httpContextAccessor.HttpContext.User`.
    -   Jeśli `claim` nie istnieje, należy rzucić wyjątek (np. `InvalidOperationException`), ponieważ oznacza to, że usługa jest wywoływana w kontekście bez uwierzytelnionego użytkownika, co nie powinno mieć miejsca na chronionych endpointach.

    ```csharp
    public class UserContextService : IUserContextService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserContextService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetUserId()
        {
            var userId = _httpContextAccessor.HttpContext?.User?
                .FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userId))
            {
                throw new InvalidOperationException("User ID not found in token.");
            }
            
            return userId;
        }
    }
    ```

### 4.4. Obsługa Wyjątków

API powinno być przygotowane na obsługę błędów autoryzacji:

-   **401 Unauthorized**: Zwracane automatycznie przez middleware, gdy tokenu brakuje, jest on nieprawidłowy lub wygasł.
-   **403 Forbidden**: Zwracane, jeśli użytkownik jest uwierzytelniony, ale próbuje uzyskać dostęp do zasobów, do których nie ma uprawnień (chociaż w tym modelu, gdzie użytkownik ma dostęp tylko do swoich danych, ten błąd będzie rzadszy i raczej zastąpiony przez **404 Not Found**).

### 4.5. Zarządzanie Kontem Użytkownika (US-004)

#### `DELETE /api/users`

-   **Opis**: Trwale usuwa konto zalogowanego użytkownika oraz wszystkie powiązane z nim dane (plany, sesje, historia itp.). Zgodnie z kryteriami akceptacji US-004.
-   **Zabezpieczenia**: Endpoint musi być chroniony atrybutem `[Authorize]`. Operacja jest wykonywana wyłącznie dla użytkownika, którego tożsamość jest potwierdzona przez token JWT.
-   **Logika implementacji**:
    1.  Pobranie `userId` z kontekstu (`IUserContextService`).
    2.  Wykonanie kaskadowego usunięcia danych użytkownika z bazy danych. Należy zapewnić odpowiednią kolejność operacji, aby nie naruszyć więzów integralności (kluczy obcych), lub skonfigurować w bazie danych `ON DELETE CASCADE`.
    3.  Wywołanie metody usunięcia użytkownika z systemu Supabase Auth. **Ważne**: ta operacja wymaga uprawnień administratora, więc po stronie API należy użyć klienta Supabase zainicjowanego z kluczem `ServiceRoleKey`, a nie kluczem publicznym (`anon key`).
-   **Odpowiedź (sukces)**: `204 No Content`.
-   **Kody błędów**: `401 Unauthorized`.

#### Aktualizacja `IAuthService` (Frontend)

Kontrakt serwisu po stronie klienckiej musi zostać rozszerzony o metodę do usuwania konta.

```csharp
public interface IAuthService
{
    // ... istniejące metody
    Task DeleteAccountAsync();
}
```

Implementacja w `AuthService` będzie wywoływała nowy endpoint `DELETE /api/users` i w przypadku sukcesu wylogowywała użytkownika i czyściła lokalny stan.
