# Blazor WASM to Interactive Server Migration Plan

## Overview

This document outlines the migration steps to convert the WorkoutManager.Web project from Blazor WebAssembly to Blazor Interactive Server.

**Current State:** Blazor WebAssembly (.NET 9.0)
**Target State:** Blazor Interactive Server (.NET 9.0)

---

## Phase 1: Project Configuration

### 1.1 Update Project SDK and Package References

**File:** `WorkoutManager.Web/WorkoutManager.Web.csproj`

**Changes:**

```xml
<!-- FROM -->
<Project Sdk="Microsoft.NET.Sdk.BlazorWebAssembly">

<!-- TO -->
<Project Sdk="Microsoft.NET.Sdk.Web">
```

**Remove packages:**
- `Microsoft.AspNetCore.Components.WebAssembly`
- `Microsoft.AspNetCore.Components.WebAssembly.DevServer`

**Add packages:**
- `Microsoft.AspNetCore.Components.Web` (if not implicitly included)

**Keep packages:**
- `Blazor.Bootstrap`
- `Blazored.LocalStorage` (temporary - will be replaced in Phase 2)
- `Microsoft.AspNetCore.Components.Authorization`
- `Microsoft.Extensions.Http`
- `MudBlazor`
- `Supabase`

### 1.2 Update Project File Structure

**Final csproj should look like:**

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <UserSecretsId>49f311c4-a305-4ee0-af5e-cb668c86e3f2</UserSecretsId>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Blazor.Bootstrap" Version="3.4.0" />
    <PackageReference Include="Microsoft.AspNetCore.Components.Authorization" Version="9.0.9" />
    <PackageReference Include="Microsoft.Extensions.Http" Version="9.0.9" />
    <PackageReference Include="MudBlazor" Version="8.13.0" />
    <PackageReference Include="Supabase" Version="1.1.1" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\WorkoutManager.BusinessLogic\WorkoutManager.BusinessLogic.csproj" />
  </ItemGroup>

</Project>
```

---

## Phase 2: Program.cs Rewrite

**File:** `WorkoutManager.Web/Program.cs`

**Replace entire content with:**

```csharp
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor.Services;
using WorkoutManager.Web.Components;
using WorkoutManager.Web.Services;
using Supabase;

var builder = WebApplication.CreateBuilder(args);

// Add Blazor Server services
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// HTTP Client configuration
builder.Services.AddScoped<AuthorizationMessageHandler>();
builder.Services.AddHttpClient("ApiHttpClient", client =>
    client.BaseAddress = new Uri(builder.Configuration["ApiUrl"]!))
    .AddHttpMessageHandler<AuthorizationMessageHandler>();

builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IHttpClientFactory>().CreateClient("ApiHttpClient"));

// Application services
builder.Services.AddScoped<IWorkoutPlanService, WorkoutPlanService>();
builder.Services.AddScoped<ISessionService, SessionService>();
builder.Services.AddScoped<IExerciseService, ExerciseService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IMuscleGroupService, MuscleGroupService>();
builder.Services.AddScoped<IWorkoutSessionStateService, WorkoutSessionStateService>();

// Authentication
builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, SupabaseAuthenticationStateProvider>();

// Supabase client
builder.Services.AddSingleton(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    var supabaseUrl = configuration["Supabase:Url"];
    var supabaseKey = configuration["Supabase:AnonKey"];
    return new Client(supabaseUrl, supabaseKey);
});

// MudBlazor
builder.Services.AddMudServices();

var app = builder.Build();

// Configure middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
```

---

## Phase 3: App.razor and Routing Updates

### 3.1 Update App.razor

**File:** `WorkoutManager.Web/App.razor`

**Replace with:**

```razor
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <base href="/" />
    <link rel="stylesheet" href="css/app.css" />
    <link rel="stylesheet" href="WorkoutManager.Web.styles.css" />
    <link href="_content/MudBlazor/MudBlazor.min.css" rel="stylesheet" />
    <HeadOutlet @rendermode="InteractiveServer" />
</head>
<body>
    <Routes @rendermode="InteractiveServer" />
    <script src="_framework/blazor.web.js"></script>
    <script src="_content/MudBlazor/MudBlazor.min.js"></script>
</body>
</html>
```

### 3.2 Create Routes.razor

**File:** `WorkoutManager.Web/Routes.razor` (NEW FILE)

```razor
@using Microsoft.AspNetCore.Components.Authorization
@using WorkoutManager.Web.Layout

<CascadingAuthenticationState>
    <Router AppAssembly="@typeof(Routes).Assembly">
        <Found Context="routeData">
            <AuthorizeRouteView RouteData="@routeData" DefaultLayout="@typeof(MainLayout)">
            </AuthorizeRouteView>
            <FocusOnNavigate RouteData="@routeData" Selector="h1" />
        </Found>
        <NotFound>
            <PageTitle>Not found</PageTitle>
            <LayoutView Layout="@typeof(MainLayout)">
                <p role="alert">Sorry, there's nothing at this address.</p>
            </LayoutView>
        </NotFound>
    </Router>
</CascadingAuthenticationState>
```

---

## Phase 4: Static Files Migration

### 4.1 Move wwwroot Content

The `wwwroot` folder structure remains mostly the same, but:

1. **Delete:** `wwwroot/index.html` (no longer needed)
2. **Move configuration:** `wwwroot/appsettings.json` to project root

### 4.2 Update appsettings.json Location

Move `WorkoutManager.Web/wwwroot/appsettings.json` to `WorkoutManager.Web/appsettings.json`

---

## Phase 5: State Management Migration

### 5.1 Replace LocalStorage with ProtectedSessionStorage

**File:** `WorkoutManager.Web/Services/WorkoutSessionStateService.cs`

**Replace with:**

```csharp
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Text.Json;
using WorkoutManager.Web.Helpers;

namespace WorkoutManager.Web.Services
{
    public class WorkoutSessionStateService : IWorkoutSessionStateService
    {
        private const string STORAGE_KEY = "workoutSessionState";
        private readonly ProtectedSessionStorage _sessionStorage;

        public WorkoutSessionStateService(ProtectedSessionStorage sessionStorage)
        {
            _sessionStorage = sessionStorage;
        }

        public async Task SaveStateAsync(WorkoutSessionState state)
        {
            state.Timestamp = DateTime.UtcNow;
            await _sessionStorage.SetAsync(STORAGE_KEY, state);
        }

        public async Task<WorkoutSessionState?> LoadStateAsync()
        {
            try
            {
                var result = await _sessionStorage.GetAsync<WorkoutSessionState>(STORAGE_KEY);
                return result.Success ? result.Value : null;
            }
            catch
            {
                await ClearStateAsync();
                return null;
            }
        }

        public async Task ClearStateAsync()
        {
            await _sessionStorage.DeleteAsync(STORAGE_KEY);
        }
    }
}
```

### 5.2 Handle Pre-render State Loading

Components that load state on initialization need to handle prerendering:

```csharp
protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender)
    {
        // Load state here instead of OnInitializedAsync
        var state = await WorkoutSessionStateService.LoadStateAsync();
        // ... handle state
        StateHasChanged();
    }
}
```

---

## Phase 6: Authentication Adjustments

### 6.1 Review SupabaseAuthenticationStateProvider

**File:** `WorkoutManager.Web/Services/SupabaseAuthenticationStateProvider.cs`

Verify the authentication state provider works correctly with Server-side rendering. Key considerations:

1. JWT tokens need secure server-side storage
2. Consider using `ProtectedSessionStorage` for token persistence
3. Handle circuit disconnection/reconnection gracefully

### 6.2 Update AuthorizationMessageHandler

Ensure the handler correctly retrieves tokens from server-side storage.

---

## Phase 7: _Imports.razor Updates

**File:** `WorkoutManager.Web/_Imports.razor`

Add the render mode directive if needed:

```razor
@using static Microsoft.AspNetCore.Components.Web.RenderMode
```

---

## Phase 8: Testing Checklist

### 8.1 Functional Tests

- [ ] Application starts without errors
- [ ] Home page loads correctly
- [ ] User registration works
- [ ] User login works
- [ ] User logout works
- [ ] Workout plans display correctly
- [ ] Session creation works
- [ ] Session state persists across navigation
- [ ] Session state recovers after page refresh
- [ ] History page loads correctly
- [ ] Exercise details display correctly
- [ ] Settings page functions correctly

### 8.2 Performance Tests

- [ ] Initial page load time is acceptable
- [ ] SignalR connection establishes quickly
- [ ] No memory leaks during extended use
- [ ] Multiple concurrent users work correctly

### 8.3 Edge Cases

- [ ] Handle SignalR disconnection gracefully
- [ ] Handle browser back/forward navigation
- [ ] Handle multiple browser tabs
- [ ] Handle session timeout

---

## Phase 9: Cleanup

### 9.1 Remove Unused Packages

After successful migration, remove from csproj:
- `Blazored.LocalStorage` (if fully replaced)

### 9.2 Remove Unused Files

- `wwwroot/index.html`
- Any WASM-specific configuration files

---

## Rollback Plan

If issues arise during migration:

1. Revert `WorkoutManager.Web.csproj` to original SDK
2. Restore original `Program.cs`
3. Restore original `App.razor`
4. Restore `WorkoutSessionStateService.cs` with localStorage
5. Restore `wwwroot/index.html`
6. Delete `Routes.razor`

---

## Post-Migration Considerations

### Optional Enhancements

1. **Consider consolidating API calls**: With Server rendering, you can directly inject repositories/services instead of HTTP calls
2. **Add SignalR reconnection handling**: Improve user experience during connection issues
3. **Implement server-side caching**: Reduce database load for frequently accessed data
4. **Add health checks**: Monitor SignalR hub health

### Known Limitations of Blazor Server

- Requires persistent server connection (SignalR)
- Higher server resource usage per user
- Not suitable for offline scenarios
- Latency-sensitive operations may feel slower

---

## Summary

| Phase | Description | Estimated Effort |
|-------|-------------|------------------|
| 1 | Project Configuration | 30 min |
| 2 | Program.cs Rewrite | 30 min |
| 3 | App.razor & Routing | 30 min |
| 4 | Static Files Migration | 15 min |
| 5 | State Management | 1-2 hours |
| 6 | Authentication | 1-2 hours |
| 7 | _Imports Updates | 15 min |
| 8 | Testing | 2-3 hours |
| 9 | Cleanup | 15 min |

**Total Estimated Effort: 6-9 hours**
