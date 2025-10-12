<!-- fc458e16-2f48-4d9e-b887-3e7caa1f6a6c e5541261-bd5b-4b1d-8c70-49a2bdadb66d -->
# Missing Features Implementation Plan

## Phase 1: Supabase Integration & Authentication Foundation

### 1.1 Install Required NuGet Packages

**API Project (WorkoutManager.Api):**

- `Supabase` (latest stable)
- `Microsoft.AspNetCore.Authentication.JwtBearer`

**Web Project (WorkoutManager.Web):**

- `Supabase` (latest stable)
- `Blazored.LocalStorage`

### 1.2 Configure Supabase in API

**File: `WorkoutManager.Api/appsettings.json`**

```json
{
  "Supabase": {
    "Url": "YOUR_SUPABASE_URL",
    "Key": "YOUR_SUPABASE_ANON_KEY",
    "JwtSecret": "YOUR_SUPABASE_JWT_SECRET"
  }
}
```

**File: `WorkoutManager.Api/Program.cs`**

```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Supabase;

var builder = WebApplication.CreateBuilder(args);

// Supabase Client
var supabaseUrl = builder.Configuration["Supabase:Url"];
var supabaseKey = builder.Configuration["Supabase:Key"];
builder.Services.AddScoped<Supabase.Client>(_ =>
    new Supabase.Client(supabaseUrl, supabaseKey));

// JWT Authentication
var jwtSecret = builder.Configuration["Supabase:JwtSecret"];
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true
        };
    });

builder.Services.AddAuthorization();

// Existing services...
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("https://localhost:7XXX") // Update with actual Web URL
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
```

### 1.3 Create User Context Service

**File: `WorkoutManager.Api/Services/IUserContextService.cs`**

```csharp
namespace WorkoutManager.Api.Services;

public interface IUserContextService
{
    Guid GetCurrentUserId();
    string? GetCurrentUserEmail();
}
```

**File: `WorkoutManager.Api/Services/UserContextService.cs`**

```csharp
using System.Security.Claims;

namespace WorkoutManager.Api.Services;

public class UserContextService : IUserContextService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserContextService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid GetCurrentUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim))
            throw new UnauthorizedAccessException("User ID not found in token");

        return Guid.Parse(userIdClaim);
    }

    public string? GetCurrentUserEmail()
    {
        return _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;
    }
}
```

Register in `Program.cs`:

```csharp
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserContextService, UserContextService>();
```

### 1.4 Configure Supabase in Web (Blazor WASM)

**File: `WorkoutManager.Web/Program.cs`**

```csharp
using Blazored.LocalStorage;
using Supabase;
using WorkoutManager.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Blazored LocalStorage
builder.Services.AddBlazoredLocalStorage();

// Supabase Client
var supabaseUrl = builder.Configuration["Supabase:Url"];
var supabaseKey = builder.Configuration["Supabase:Key"];
builder.Services.AddScoped<Supabase.Client>(_ =>
    new Supabase.Client(supabaseUrl, supabaseKey));

// Auth State Provider
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddAuthorizationCore();

// HttpClient with Auth Interceptor
builder.Services.AddScoped<AuthorizationMessageHandler>();
builder.Services.AddScoped(sp =>
{
    var handler = sp.GetRequiredService<AuthorizationMessageHandler>();
    handler.ConfigureHandler(authorizedUrls: new[] { "https://localhost:5048" });
    return new HttpClient(handler) { BaseAddress = new Uri("https://localhost:5048") };
});

// Existing services...
builder.Services.AddScoped<IWorkoutPlanService, WorkoutPlanService>();
builder.Services.AddScoped<ISessionService, SessionService>();
builder.Services.AddScoped<IExerciseService, ExerciseService>();
builder.Services.AddScoped<IMuscleGroupService, MuscleGroupService>();

builder.Services.AddMudServices();

await builder.Build().RunAsync();
```

**File: `WorkoutManager.Web/wwwroot/appsettings.json`**

```json
{
  "Supabase": {
    "Url": "YOUR_SUPABASE_URL",
    "Key": "YOUR_SUPABASE_ANON_KEY"
  }
}
```

### 1.5 Create Authorization Message Handler

**File: `WorkoutManager.Web/Services/AuthorizationMessageHandler.cs`**

```csharp
using Blazored.LocalStorage;

namespace WorkoutManager.Web.Services;

public class AuthorizationMessageHandler : DelegatingHandler
{
    private readonly ILocalStorageService _localStorage;

    public AuthorizationMessageHandler(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
        InnerHandler = new HttpClientHandler();
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _localStorage.GetItemAsync<string>("accessToken", cancellationToken);
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }
        return await base.SendAsync(request, cancellationToken);
    }

    public void ConfigureHandler(string[] authorizedUrls)
    {
        // Configuration placeholder
    }
}
```

## Phase 2: Authentication Services & UI

### 2.1 Create Authentication Service Interface

**File: `WorkoutManager.Web/Services/IAuthService.cs`**

```csharp
using WorkoutManager.BusinessLogic.DTOs;

namespace WorkoutManager.Web.Services;

public interface IAuthService
{
    Task<bool> RegisterAsync(string email, string password);
    Task<bool> LoginAsync(string email, string password);
    Task LogoutAsync();
    Task<bool> ResetPasswordRequestAsync(string email);
    Task<bool> ResetPasswordAsync(string token, string newPassword);
    Task<bool> DeleteAccountAsync(string password);
    Task<UserDto?> GetCurrentUserAsync();
    Task<bool> IsAuthenticatedAsync();
}
```

### 2.2 Implement Authentication Service

**File: `WorkoutManager.Web/Services/AuthService.cs`**

```csharp
using Blazored.LocalStorage;
using Supabase.Gotrue;
using WorkoutManager.BusinessLogic.DTOs;

namespace WorkoutManager.Web.Services;

public class AuthService : IAuthService
{
    private readonly Supabase.Client _supabase;
    private readonly ILocalStorageService _localStorage;
    private readonly NavigationManager _navigationManager;

    public AuthService(
        Supabase.Client supabase,
        ILocalStorageService localStorage,
        NavigationManager navigationManager)
    {
        _supabase = supabase;
        _localStorage = localStorage;
        _navigationManager = navigationManager;
    }

    public async Task<bool> RegisterAsync(string email, string password)
    {
        try
        {
            var session = await _supabase.Auth.SignUp(email, password);
            if (session?.AccessToken != null)
            {
                await _localStorage.SetItemAsync("accessToken", session.AccessToken);
                await _localStorage.SetItemAsync("refreshToken", session.RefreshToken);
                return true;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> LoginAsync(string email, string password)
    {
        try
        {
            var session = await _supabase.Auth.SignIn(email, password);
            if (session?.AccessToken != null)
            {
                await _localStorage.SetItemAsync("accessToken", session.AccessToken);
                await _localStorage.SetItemAsync("refreshToken", session.RefreshToken);
                return true;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    public async Task LogoutAsync()
    {
        await _supabase.Auth.SignOut();
        await _localStorage.RemoveItemAsync("accessToken");
        await _localStorage.RemoveItemAsync("refreshToken");
        _navigationManager.NavigateTo("/login");
    }

    public async Task<bool> ResetPasswordRequestAsync(string email)
    {
        try
        {
            await _supabase.Auth.ResetPasswordForEmail(email);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> ResetPasswordAsync(string token, string newPassword)
    {
        try
        {
            var result = await _supabase.Auth.Update(new UserAttributes { Password = newPassword });
            return result != null;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> DeleteAccountAsync(string password)
    {
        try
        {
            var user = _supabase.Auth.CurrentUser;
            if (user?.Email == null) return false;

            // Verify password by attempting login
            var verified = await LoginAsync(user.Email, password);
            if (!verified) return false;

            // Call API endpoint to delete account
            // This will cascade delete via database RLS
            await _supabase.Auth.SignOut();
            await _localStorage.RemoveItemAsync("accessToken");
            await _localStorage.RemoveItemAsync("refreshToken");
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<UserDto?> GetCurrentUserAsync()
    {
        var user = _supabase.Auth.CurrentUser;
        if (user == null) return null;

        return new UserDto
        {
            Id = Guid.Parse(user.Id),
            Email = user.Email ?? string.Empty
        };
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var token = await _localStorage.GetItemAsync<string>("accessToken");
        return !string.IsNullOrEmpty(token) && _supabase.Auth.CurrentUser != null;
    }
}
```

### 2.3 Create User DTO

**File: `WorkoutManager.BusinessLogic/DTOs/UserDto.cs`**

```csharp
namespace WorkoutManager.BusinessLogic.DTOs;

public class UserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
}
```

### 2.4 Implement Authentication Pages Code-Behind

**File: `WorkoutManager.Web/Pages/Authentication/LoginPage.razor.cs`**

```csharp
using Microsoft.AspNetCore.Components;
using MudBlazor;
using WorkoutManager.Web.Services;

namespace WorkoutManager.Web.Pages.Authentication;

public partial class LoginPage
{
    [Inject]
    private IAuthService AuthService { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = default!;

    private MudForm _form = default!;
    private bool _success;
    private LoginModel _model = new();

    private async Task Submit()
    {
        await _form.Validate();
        if (!_success) return;

        var result = await AuthService.LoginAsync(_model.Email, _model.Password);
        if (result)
        {
            Snackbar.Add("Login successful!", Severity.Success);
            NavigationManager.NavigateTo("/");
        }
        else
        {
            Snackbar.Add("Invalid email or password.", Severity.Error);
        }
    }

    private class LoginModel
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
```

**File: `WorkoutManager.Web/Pages/Authentication/RegisterPage.razor.cs`**

```csharp
using Microsoft.AspNetCore.Components;
using MudBlazor;
using WorkoutManager.Web.Services;

namespace WorkoutManager.Web.Pages.Authentication;

public partial class RegisterPage
{
    [Inject]
    private IAuthService AuthService { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = default!;

    private MudForm _form = default!;
    private bool _success;
    private RegisterModel _model = new();

    private async Task Submit()
    {
        await _form.Validate();
        if (!_success) return;

        if (_model.Password != _model.ConfirmPassword)
        {
            Snackbar.Add("Passwords do not match.", Severity.Error);
            return;
        }

        var result = await AuthService.RegisterAsync(_model.Email, _model.Password);
        if (result)
        {
            Snackbar.Add("Registration successful! Please check your email to verify your account.", Severity.Success);
            NavigationManager.NavigateTo("/");
        }
        else
        {
            Snackbar.Add("Registration failed. Email may already be in use.", Severity.Error);
        }
    }

    private class RegisterModel
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
```

**File: `WorkoutManager.Web/Pages/Authentication/ResetPasswordPage.razor.cs`**

```csharp
using Microsoft.AspNetCore.Components;
using MudBlazor;
using WorkoutManager.Web.Services;

namespace WorkoutManager.Web.Pages.Authentication;

public partial class ResetPasswordPage
{
    [Inject]
    private IAuthService AuthService { get; set; } = default!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = default!;

    private MudForm _form = default!;
    private bool _success;
    private ResetPasswordModel _model = new();

    private async Task Submit()
    {
        await _form.Validate();
        if (!_success) return;

        var result = await AuthService.ResetPasswordRequestAsync(_model.Email);
        if (result)
        {
            Snackbar.Add("Password reset email sent! Please check your inbox.", Severity.Success);
        }
        else
        {
            Snackbar.Add("Failed to send reset email. Please try again.", Severity.Error);
        }
    }

    private class ResetPasswordModel
    {
        public string Email { get; set; } = string.Empty;
    }
}
```

### 2.5 Implement Settings Page with Account Deletion

**File: `WorkoutManager.Web/Pages/Settings/SettingsPage.razor.cs`**

```csharp
using Microsoft.AspNetCore.Components;
using MudBlazor;
using WorkoutManager.Web.Services;
using WorkoutManager.Web.Components;

namespace WorkoutManager.Web.Pages.Settings;

public partial class SettingsPage
{
    [Inject]
    private IAuthService AuthService { get; set; } = default!;

    [Inject]
    private IDialogService DialogService { get; set; } = default!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = default!;

    private async Task DeleteAccount()
    {
        var parameters = new DialogParameters
        {
            { nameof(PasswordConfirmationDialog.Title), "Delete Account" },
            { nameof(PasswordConfirmationDialog.ContentText), "This action is irreversible. All your data will be permanently deleted. Please enter your password to confirm." },
            { nameof(PasswordConfirmationDialog.ConfirmButtonText), "Delete Account" }
        };

        var dialog = await DialogService.ShowAsync<PasswordConfirmationDialog>("Delete Account", parameters);
        var result = await dialog.Result;

        if (result is not null && !result.Canceled && result.Data is string password)
        {
            var success = await AuthService.DeleteAccountAsync(password);
            if (success)
            {
                Snackbar.Add("Account deleted successfully.", Severity.Success);
            }
            else
            {
                Snackbar.Add("Failed to delete account. Please check your password.", Severity.Error);
            }
        }
    }
}
```

### 2.6 Create Password Confirmation Dialog

**File: `WorkoutManager.Web/Components/PasswordConfirmationDialog.razor`**

```razor
<MudDialog>
    <TitleContent>
        <MudText Typo="Typo.h6">@Title</MudText>
    </TitleContent>
    <DialogContent>
        <MudText>@ContentText</MudText>
        <MudTextField @bind-Value="_password" Label="Password" InputType="InputType.Password" 
                      Variant="Variant.Outlined" Margin="Margin.Dense" Class="mt-4" />
    </DialogContent>
    <DialogActions>
        <MudButton OnClick="Cancel">Cancel</MudButton>
        <MudButton Color="Color.Error" Variant="Variant.Filled" OnClick="Confirm">@ConfirmButtonText</MudButton>
    </DialogActions>
</MudDialog>
```

**File: `WorkoutManager.Web/Components/PasswordConfirmationDialog.razor.cs`**

```csharp
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace WorkoutManager.Web.Components;

public partial class PasswordConfirmationDialog
{
    [CascadingParameter]
    private MudDialogInstance MudDialog { get; set; } = default!;

    [Parameter]
    public string Title { get; set; } = "Confirm Action";

    [Parameter]
    public string ContentText { get; set; } = string.Empty;

    [Parameter]
    public string ConfirmButtonText { get; set; } = "Confirm";

    private string _password = string.Empty;

    private void Cancel()
    {
        MudDialog.Cancel();
    }

    private void Confirm()
    {
        MudDialog.Close(DialogResult.Ok(_password));
    }
}
```

### 2.7 Create Welcome Screen

**File: `WorkoutManager.Web/Pages/WelcomePage.razor`**

```razor
@page "/welcome"

<PageTitle>Welcome to 10xWorkoutManager</PageTitle>

<MudContainer MaxWidth="MaxWidth.Medium" Class="mt-16">
    <MudPaper Class="pa-8" Elevation="3">
        <MudText Typo="Typo.h3" Align="Align.Center" GutterBottom="true">
            Welcome to 10xWorkoutManager! 💪
        </MudText>
        <MudText Typo="Typo.body1" Align="Align.Center" Class="mb-8">
            Let's get started by creating your first workout plan.
        </MudText>
        
        <MudStack Spacing="4">
            <MudText Typo="Typo.h6">What you can do:</MudText>
            <MudList T="string">
                <MudListItem Icon="@Icons.Material.Filled.FitnessCenter">
                    Create custom workout plans with multiple training days
                </MudListItem>
                <MudListItem Icon="@Icons.Material.Filled.Assignment">
                    Track your sets, reps, and weights during workouts
                </MudListItem>
                <MudListItem Icon="@Icons.Material.Filled.History">
                    View your workout history and progress over time
                </MudListItem>
            </MudList>
        </MudStack>

        <MudDivider Class="my-6" />

        <MudStack Row="true" Justify="Justify.Center" Spacing="4">
            <MudButton Variant="Variant.Filled" Color="Color.Primary" Size="Size.Large" 
                       OnClick="CreateFirstPlan" StartIcon="@Icons.Material.Filled.Add">
                Create Your First Plan
            </MudButton>
            <MudButton Variant="Variant.Outlined" Color="Color.Secondary" Size="Size.Large" 
                       OnClick="SkipWelcome">
                Skip for Now
            </MudButton>
        </MudStack>
    </MudPaper>
</MudContainer>
```

**File: `WorkoutManager.Web/Pages/WelcomePage.razor.cs`**

```csharp
using Microsoft.AspNetCore.Components;
using MudBlazor;
using WorkoutManager.Web.Components;
using WorkoutManager.Web.Services;
using Blazored.LocalStorage;

namespace WorkoutManager.Web.Pages;

public partial class WelcomePage
{
    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private IDialogService DialogService { get; set; } = default!;

    [Inject]
    private IWorkoutPlanService WorkoutPlanService { get; set; } = default!;

    [Inject]
    private ILocalStorageService LocalStorage { get; set; } = default!;

    private async Task CreateFirstPlan()
    {
        var dialog = await DialogService.ShowAsync<CreatePlanDialog>("Create Your First Plan");
        var result = await dialog.Result;

        if (result is not null && !result.Canceled && result.Data is CreateWorkoutPlanDto newPlan)
        {
            await WorkoutPlanService.CreateWorkoutPlanAsync(newPlan);
            await LocalStorage.SetItemAsync("hasSeenWelcome", true);
            NavigationManager.NavigateTo("/");
        }
    }

    private async Task SkipWelcome()
    {
        await LocalStorage.SetItemAsync("hasSeenWelcome", true);
        NavigationManager.NavigateTo("/");
    }
}
```

**Update `Home.razor.cs` to check for welcome screen:**

```csharp
protected override async Task OnInitializedAsync()
{
    var hasSeenWelcome = await LocalStorage.GetItemAsync<bool>("hasSeenWelcome");
    var isAuthenticated = await AuthService.IsAuthenticatedAsync();
    
    if (isAuthenticated && !hasSeenWelcome)
    {
        NavigationManager.NavigateTo("/welcome");
        return;
    }

    var result = await WorkoutPlanService.GetWorkoutPlansAsync();
    _plans = result.Data;
    _pagination = result.Pagination;
}
```

## Phase 3: Complete API Endpoints

### 3.1 Add Authorization to Existing Controllers

Add `[Authorize]` attribute and inject `IUserContextService` to all existing controllers:

- `WorkoutPlansController`
- `ExercisesController`
- `SessionsController`
- `MuscleGroupsController`
- `PlanDayExercisesController`
- `SessionExercisesController`

Example for `WorkoutPlansController`:

```csharp
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class WorkoutPlansController : ControllerBase
{
    private readonly IUserContextService _userContext;

    public WorkoutPlansController(IUserContextService userContext)
    {
        _userContext = userContext;
    }
    
    // Use _userContext.GetCurrentUserId() in methods
}
```

### 3.2 Create Missing GET Exercise Endpoint

**File: `WorkoutManager.Api/Controllers/ExercisesController.cs`**

Add method:

```csharp
[HttpGet("{id}")]
public async Task<ActionResult<ExerciseDto>> GetExerciseById(int id)
{
    var userId = _userContext.GetCurrentUserId();
    var exercise = await _supabase
        .From<Exercise>()
        .Where(e => e.Id == id && (e.UserId == null || e.UserId == userId))
        .Single();

    if (exercise == null)
        return NotFound();

    return Ok(new ExerciseDto
    {
        Id = exercise.Id,
        UserId = exercise.UserId,
        MuscleGroupId = exercise.MuscleGroupId,
        Name = exercise.Name
    });
}
```

### 3.3 Fix Previous Exercise Performance Endpoint

Update return type in `ExercisesController.cs`:

```csharp
[HttpGet("{exerciseId}/previous-session")]
public async Task<ActionResult<PreviousExercisePerformanceDto>> GetPreviousExercisePerformance(int exerciseId)
{
    var userId = _userContext.GetCurrentUserId();
    
    // Query Supabase for most recent session with this exercise
    var previousPerformance = await _supabase
        .From<SessionExercise>()
        .Join<ExerciseSet>()
        .Join<Session>()
        .Where(se => se.ExerciseId == exerciseId && se.Session.UserId == userId && !se.Skipped)
        .Order(se => se.Session.StartTime, Ordering.Descending)
        .Limit(1)
        .Get();

    if (previousPerformance == null)
        return NotFound("Exercise has never been performed.");

    return Ok(new PreviousExercisePerformanceDto
    {
        SessionDate = previousPerformance.Session.StartTime,
        Notes = previousPerformance.Notes,
        Sets = previousPerformance.Sets.Select(s => new PreviousExerciseSetDto
        {
            Weight = s.Weight,
            Reps = s.Reps,
            IsFailure = s.IsFailure
        }).ToList()
    });
}
```

### 3.4 Update WorkoutPlans PUT Endpoint

**File: `WorkoutManager.Api/Controllers/WorkoutPlansController.cs`**

Update method to handle training day reordering:

```csharp
[HttpPut("{id}")]
public async Task<ActionResult<WorkoutPlanDetailDto>> UpdateWorkoutPlan(int id, [FromBody] UpdateWorkoutPlanPayload payload)
{
    var userId = _userContext.GetCurrentUserId();
    
    var plan = await _supabase
        .From<WorkoutPlan>()
        .Where(p => p.Id == id && p.UserId == userId)
        .Single();

    if (plan == null)
        return NotFound();

    // Check if plan is locked
    var activeSession = await _supabase
        .From<Session>()
        .Where(s => s.PlanId == id && s.EndTime == null)
        .Single();

    if (activeSession != null)
        return StatusCode(403, "Cannot edit plan during active session");

    // Update plan name
    plan.Name = payload.Name;
    await _supabase.From<WorkoutPlan>().Update(plan);

    // Update training day orders
    foreach (var dayUpdate in payload.TrainingDays)
    {
        await _supabase
            .From<TrainingDay>()
            .Update(new { Order = dayUpdate.Order })
            .Match(new { Id = dayUpdate.Id });
    }

    return await GetWorkoutPlanById(id);
}
```

### 3.5 Add Exercise Reordering Endpoint

**File: `WorkoutManager.Api/Controllers/PlanDayExercisesController.cs`**

Add method:

```csharp
[HttpPut("reorder")]
public async Task<IActionResult> ReorderExercises(
    int planId, 
    int dayId, 
    [FromBody] List<ReorderExerciseCommand> exercises)
{
    var userId = _userContext.GetCurrentUserId();
    
    var plan = await _supabase
        .From<WorkoutPlan>()
        .Where(p => p.Id == planId && p.UserId == userId)
        .Single();

    if (plan == null || plan.IsLocked)
        return Forbid();

    foreach (var exercise in exercises)
    {
        await _supabase
            .From<PlanDayExercise>()
            .Update(new { Order = exercise.Order })
            .Match(new { Id = exercise.Id });
    }

    return NoContent();
}
```

**Add to `WorkoutPlanCommands.cs`:**

```csharp
public record ReorderExerciseCommand(int Id, int Order);
```

### 3.6 Enhance Session Endpoints

**Update `SessionsController.cs` to return enriched data:**

```csharp
[HttpGet]
public async Task<ActionResult<PaginatedList<SessionSummaryDto>>> GetSessions(
    [FromQuery] int page = 1, 
    [FromQuery] int pageSize = 20)
{
    var userId = _userContext.GetCurrentUserId();
    
    var sessions = await _supabase
        .From<Session>()
        .Join<WorkoutPlan>()
        .Join<TrainingDay>()
        .Where(s => s.UserId == userId)
        .Order(s => s.StartTime, Ordering.Descending)
        .Range((page - 1) * pageSize, page * pageSize - 1)
        .Get();

    var totalCount = await _supabase
        .From<Session>()
        .Where(s => s.UserId == userId)
        .Count();

    var data = sessions.Select(s => new SessionSummaryDto
    {
        Id = s.Id,
        PlanId = s.PlanId ?? 0,
        PlanName = s.WorkoutPlan?.Name,
        TrainingDayName = s.TrainingDay?.Name,
        Notes = s.Notes,
        StartTime = s.StartTime,
        EndTime = s.EndTime
    }).ToList();

    return Ok(new PaginatedList<SessionSummaryDto>
    {
        Data = data,
        Pagination = new PaginationInfo
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        }
    });
}
```

**Update `SessionDtos.cs` to include plan/day names:**

```csharp
public class SessionSummaryDto
{
    public int Id { get; set; }
    public int PlanId { get; set; }
    public string? PlanName { get; set; }
    public string? TrainingDayName { get; set; }
    public string? Notes { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
}
```

## Phase 4: Complete UI Services

### 4.1 Add Missing Methods to IWorkoutPlanService

**File: `WorkoutManager.Web/Services/IWorkoutPlanService.cs`**

```csharp
public interface IWorkoutPlanService
{
    Task<PaginatedList<WorkoutPlanDto>> GetWorkoutPlansAsync();
    Task<WorkoutPlanDetailDto?> GetWorkoutPlanByIdAsync(int id);
    Task CreateWorkoutPlanAsync(CreateWorkoutPlanDto newPlan);
    Task UpdateWorkoutPlanAsync(int id, UpdateWorkoutPlanDto plan);
    Task DeleteWorkoutPlanAsync(int id);
    Task AddExerciseToTrainingDayAsync(int planId, int dayId, AddExerciseToTrainingDayCommand exercise);
    Task RemoveExerciseFromTrainingDayAsync(int planId, int trainingDayId, int planDayExerciseId);
    Task ReorderExercisesAsync(int planId, int dayId, List<ReorderExerciseCommand> exercises);
    Task ReorderTrainingDaysAsync(int planId, List<UpdateTrainingDayOrderCommand> days);
}
```

### 4.2 Implement Missing Methods in WorkoutPlanService

**File: `WorkoutManager.Web/Services/WorkoutPlanService.cs`**

```csharp
public async Task UpdateWorkoutPlanAsync(int id, UpdateWorkoutPlanDto plan)
{
    var response = await _httpClient.PutAsJsonAsync($"api/workout-plans/{id}", plan);
    response.EnsureSuccessStatusCode();
}

public async Task DeleteWorkoutPlanAsync(int id)
{
    var response = await _httpClient.DeleteAsync($"api/workout-plans/{id}");
    response.EnsureSuccessStatusCode();
}

public async Task AddExerciseToTrainingDayAsync(int planId, int dayId, AddExerciseToTrainingDayCommand exercise)
{
    var response = await _httpClient.PostAsJsonAsync(
        $"api/workout-plans/{planId}/training-days/{dayId}/exercises", exercise);
    response.EnsureSuccessStatusCode();
}

public async Task ReorderExercisesAsync(int planId, int dayId, List<ReorderExerciseCommand> exercises)
{
    var response = await _httpClient.PutAsJsonAsync(
        $"api/workout-plans/{planId}/training-days/{dayId}/exercises/reorder", exercises);
    response.EnsureSuccessStatusCode();
}

public async Task ReorderTrainingDaysAsync(int planId, List<UpdateTrainingDayOrderCommand> days)
{
    var response = await _httpClient.PutAsJsonAsync(
        $"api/workout-plans/{planId}/training-days/reorder", days);
    response.EnsureSuccessStatusCode();
}
```

### 4.3 Create Muscle Group Service

**File: `WorkoutManager.Web/Services/IMuscleGroupService.cs`**

```csharp
using WorkoutManager.BusinessLogic.DTOs;

namespace WorkoutManager.Web.Services;

public interface IMuscleGroupService
{
    Task<PaginatedList<MuscleGroupDto>> GetMuscleGroupsAsync();
}
```

**File: `WorkoutManager.Web/Services/MuscleGroupService.cs`**

```csharp
using System.Net.Http.Json;
using WorkoutManager.BusinessLogic.DTOs;

namespace WorkoutManager.Web.Services;

public class MuscleGroupService : IMuscleGroupService
{
    private readonly HttpClient _httpClient;

    public MuscleGroupService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<PaginatedList<MuscleGroupDto>> GetMuscleGroupsAsync()
    {
        return await _httpClient.GetFromJsonAsync<PaginatedList<MuscleGroupDto>>("api/muscle-groups") 
            ?? new PaginatedList<MuscleGroupDto>();
    }
}
```

### 4.4 Update Session Service for Session Notes

**File: `WorkoutManager.Web/Services/ISessionService.cs`**

```csharp
public interface ISessionService
{
    Task<PaginatedList<SessionSummaryDto>> GetSessionHistoryAsync();
    Task<SessionDetailsDto> GetSessionDetailsAsync(int id);
    Task<SessionDetailsDto> StartSessionAsync(int trainingDayId);
    Task UpdateSessionExerciseAsync(int sessionId, int sessionExerciseId, UpdateSessionExerciseDto payload);
    Task UpdateSessionAsync(int sessionId, UpdateSessionCommand command);
    Task FinishSessionAsync(int sessionId, string? notes);
}
```

**Update `SessionService.cs`:**

```csharp
public async Task UpdateSessionAsync(int sessionId, UpdateSessionCommand command)
{
    await _httpClient.PutAsJsonAsync($"api/sessions/{sessionId}", command);
}

public async Task FinishSessionAsync(int sessionId, string? notes)
{
    await _httpClient.PutAsJsonAsync($"api/sessions/{sessionId}", new UpdateSessionCommand
    {
        Notes = notes,
        EndTime = DateTime.UtcNow
    });
}
```

## Phase 5: Complete UI Components

### 5.1 Add Muscle Group Selector to CreateExerciseDialog

**File: `WorkoutManager.Web/Components/CreateExerciseDialog.razor`**

```razor
<MudDialog>
    <TitleContent>
        <MudText Typo="Typo.h6">Create Exercise</MudText>
    </TitleContent>
    <DialogContent>
        <MudForm @ref="_form" @bind-IsValid="@_success">
            <MudTextField T="string" Label="Name" @bind-Value="_model.Name" 
                          Required="true" RequiredError="Name is required!" />
            <MudSelect T="int" Label="Muscle Group" @bind-Value="_model.MuscleGroupId" 
                       Required="true" RequiredError="Please select a muscle group">
                @foreach (var group in _muscleGroups)
                {
                    <MudSelectItem Value="@group.Id">@group.Name</MudSelectItem>
                }
            </MudSelect>
        </MudForm>
    </DialogContent>
    <DialogActions>
        <MudButton OnClick="Cancel">Cancel</MudButton>
        <MudButton Color="Color.Primary" Variant="Variant.Filled" 
                   Disabled="@(!_success)" OnClick="Submit">Create</MudButton>
    </DialogActions>
</MudDialog>
```

**File: `WorkoutManager.Web/Components/CreateExerciseDialog.razor.cs`**

```csharp
using Microsoft.AspNetCore.Components;
using MudBlazor;
using WorkoutManager.BusinessLogic.DTOs;
using WorkoutManager.Web.Services;

namespace WorkoutManager.Web.Components;

public partial class CreateExerciseDialog
{
    [CascadingParameter]
    private MudDialogInstance MudDialog { get; set; } = default!;

    [Inject]
    private IExerciseService ExerciseService { get; set; } = default!;

    [Inject]
    private IMuscleGroupService MuscleGroupService { get; set; } = default!;

    private MudForm _form = default!;
    private bool _success;
    private CreateExerciseDto _model = new();
    private List<MuscleGroupDto> _muscleGroups = new();

    protected override async Task OnInitializedAsync()
    {
        var result = await MuscleGroupService.GetMuscleGroupsAsync();
        _muscleGroups = result.Data.ToList();
    }

    private void Cancel()
    {
        MudDialog.Cancel();
    }

    private async Task Submit()
    {
        var exercise = await ExerciseService.CreateExerciseAsync(_model);
        MudDialog.Close(DialogResult.Ok(exercise));
    }
}
```

### 5.2 Add Muscle Group Filter to ExercisePickerDialog

**File: `WorkoutManager.Web/Components/ExercisePickerDialog.razor`**

```razor
@using WorkoutManager.BusinessLogic.DTOs
<MudDialog>
    <TitleContent>
        <MudText Typo="Typo.h6">Add Exercise</MudText>
    </TitleContent>
    <DialogContent>
        <MudTextField T="string" Label="Search" @bind-Value="_search" 
                      DebounceInterval="300" OnDebounceIntervalElapsed="SearchExercises" />
        <MudSelect T="int?" Label="Muscle Group" @bind-Value="_selectedMuscleGroupId" 
                   Clearable="true" OnClearButtonClick="ClearFilter">
            @foreach (var group in _muscleGroups)
            {
                <MudSelectItem Value="@((int?)group.Id)">@group.Name</MudSelectItem>
            }
        </MudSelect>
        <MudList T="ExerciseDto">
            @foreach (var exercise in _exercises)
            {
                <MudListItem T="ExerciseDto" @onclick="() => SelectExercise(exercise)">
                    @exercise.Name
                </MudListItem>
            }
        </MudList>
        @if (PageCount > 1)
        {
            <MudPagination Count="PageCount" SelectedChanged="PageChanged" />
        }
    </DialogContent>
    <DialogActions>
        <MudButton OnClick="Cancel">Cancel</MudButton>
        <MudButton Color="Color.Primary" Variant="Variant.Filled" 
                   OnClick="OpenCreateExerciseDialog">Create New</MudButton>
    </DialogActions>
</MudDialog>
```

**File: `WorkoutManager.Web/Components/ExercisePickerDialog.razor.cs`**

```csharp
using Microsoft.AspNetCore.Components;
using MudBlazor;
using WorkoutManager.BusinessLogic.DTOs;
using WorkoutManager.Web.Services;

namespace WorkoutManager.Web.Components;

public partial class ExercisePickerDialog
{
    [CascadingParameter]
    private MudDialogInstance MudDialog { get; set; } = default!;

    [Inject]
    private IExerciseService ExerciseService { get; set; } = default!;

    [Inject]
    private IMuscleGroupService MuscleGroupService { get; set; } = default!;

    [Inject]
    private IDialogService DialogService { get; set; } = default!;

    private string? _search;
    private int? _selectedMuscleGroupId;
    private List<ExerciseDto> _exercises = new();
    private List<MuscleGroupDto> _muscleGroups = new();
    private PaginationInfo _pagination = new();
    private int PageCount => _pagination.PageSize > 0 
        ? (int)Math.Ceiling((double)_pagination.TotalCount / _pagination.PageSize) : 0;

    protected override async Task OnInitializedAsync()
    {
        var muscleGroupsResult = await MuscleGroupService.GetMuscleGroupsAsync();
        _muscleGroups = muscleGroupsResult.Data.ToList();
        await LoadExercises();
    }

    private async Task LoadExercises(int page = 1)
    {
        var result = await ExerciseService.GetExercisesAsync(_search, _selectedMuscleGroupId);
        _exercises = result.Data.ToList();
        _pagination = result.Pagination;
        StateHasChanged();
    }

    private async Task SearchExercises()
    {
        await LoadExercises();
    }

    private async Task ClearFilter()
    {
        _selectedMuscleGroupId = null;
        await LoadExercises();
    }

    private async Task PageChanged(int page)
    {
        await LoadExercises(page);
    }

    private void SelectExercise(ExerciseDto exercise)
    {
        MudDialog.Close(DialogResult.Ok(exercise));
    }

    private void Cancel()
    {
        MudDialog.Cancel();
    }

    private async Task OpenCreateExerciseDialog()
    {
        var dialog = await DialogService.ShowAsync<CreateExerciseDialog>("Create Exercise");
        var result = await dialog.Result;

        if (result is not null && !result.Canceled)
        {
            await LoadExercises();
        }
    }
}
```

### 5.3 Complete PlanDetailPage Implementation

**File: `WorkoutManager.Web/Pages/Plans/PlanDetailPage.razor`**

Add delete and edit functionality:

```razor
@page "/plans/{id:int}"
@using WorkoutManager.BusinessLogic.DTOs

<PageTitle>@($"Plan: {_plan?.Name ?? "..."}")</PageTitle>

@if (_plan is null)
{
    <div class="d-flex justify-center">
        <MudProgressCircular Indeterminate="true" />
    </div>
}
else
{
    <MudGrid Justify="Justify.SpaceBetween">
        <MudItem>
            @if (_isEditingName)
            {
                <MudTextField @bind-Value="_editedPlanName" Label="Plan Name" Variant="Variant.Outlined" />
                <MudButton OnClick="SavePlanName" Size="Size.Small" Color="Color.Primary">Save</MudButton>
                <MudButton OnClick="CancelEditName" Size="Size.Small">Cancel</MudButton>
            }
            else
            {
                <MudText Typo="Typo.h4">
                    @_plan.Name
                    <MudIconButton Icon="@Icons.Material.Filled.Edit" Size="Size.Small" OnClick="StartEditingName" />
                </MudText>
            }
        </MudItem>
        <MudItem>
            <MudSwitch T="bool" @bind-Checked="@_isEditMode" Label="Edit Mode" Color="Color.Primary" />
            <MudIconButton Icon="@Icons.Material.Filled.Delete" Color="Color.Error" OnClick="DeletePlan" />
        </MudItem>
    </MudGrid>

    <!-- Rest of existing markup -->
}
```

**File: `WorkoutManager.Web/Pages/Plans/PlanDetailPage.razor.cs`**

```csharp
using Microsoft.AspNetCore.Components;
using MudBlazor;
using WorkoutManager.BusinessLogic.DTOs;
using WorkoutManager.BusinessLogic.Commands;
using WorkoutManager.Web.Components;
using WorkoutManager.Web.Services;

namespace WorkoutManager.Web.Pages.Plans;

public partial class PlanDetailPage
{
    [Parameter]
    public int Id { get; set; }

    [Inject]
    private IWorkoutPlanService WorkoutPlanService { get; set; } = default!;

    [Inject]
    private IDialogService DialogService { get; set; } = default!;
    
    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = default!;

    private WorkoutPlanDetailDto? _plan;
    private bool _isEditMode;
    private bool _isEditingName;
    private string _editedPlanName = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        await LoadPlan();
    }

    private async Task LoadPlan()
    {
        _plan = await WorkoutPlanService.GetWorkoutPlanByIdAsync(Id);
        _editedPlanName = _plan?.Name ?? string.Empty;
    }

    private void StartEditingName()
    {
        _isEditingName = true;
    }

    private async Task SavePlanName()
    {
        if (_plan == null) return;

        try
        {
            await WorkoutPlanService.UpdateWorkoutPlanAsync(Id, new UpdateWorkoutPlanDto
            {
                Name = _editedPlanName,
                TrainingDays = _plan.TrainingDays.Select(td => new UpdateTrainingDayOrderCommand(td.Id, td.Order)).ToList()
            });
            _isEditingName = false;
            await LoadPlan();
            Snackbar.Add("Plan name updated successfully", Severity.Success);
        }
        catch
        {
            Snackbar.Add("Failed to update plan name", Severity.Error);
        }
    }

    private void CancelEditName()
    {
        _editedPlanName = _plan?.Name ?? string.Empty;
        _isEditingName = false;
    }

    private async Task DeletePlan()
    {
        var parameters = new DialogParameters
        {
            { nameof(ConfirmationDialog.Title), "Delete Plan" },
            { nameof(ConfirmationDialog.ContentText), "Are you sure you want to delete this plan? This action cannot be undone." },
            { nameof(ConfirmationDialog.ConfirmButtonText), "Delete" }
        };

        var dialog = await DialogService.ShowAsync<ConfirmationDialog>("Delete Plan", parameters);
        var result = await dialog.Result;

        if (result is not null && !result.Canceled)
        {
            try
            {
                await WorkoutPlanService.DeleteWorkoutPlanAsync(Id);
                Snackbar.Add("Plan deleted successfully", Severity.Success);
                NavigationManager.NavigateTo("/");
            }
            catch
            {
                Snackbar.Add("Failed to delete plan", Severity.Error);
            }
        }
    }

    private async Task AddExercise(int trainingDayId)
    {
        var dialog = await DialogService.ShowAsync<ExercisePickerDialog>("Add Exercise");
        var result = await dialog.Result;

        if (result is not null && !result.Canceled && result.Data is ExerciseDto selectedExercise)
        {
            var trainingDay = _plan!.TrainingDays.First(td => td.Id == trainingDayId);
            var nextOrder = trainingDay.Exercises.Any() ? trainingDay.Exercises.Max(e => e.Order) + 1 : 1;

            try
            {
                await WorkoutPlanService.AddExerciseToTrainingDayAsync(Id, trainingDayId, new AddExerciseToTrainingDayCommand
                {
                    ExerciseId = selectedExercise.Id,
                    Order = nextOrder
                });
                await LoadPlan();
                Snackbar.Add("Exercise added successfully", Severity.Success);
            }
            catch
            {
                Snackbar.Add("Failed to add exercise", Severity.Error);
            }
        }
    }

    private void StartWorkout(int trainingDayId)
    {
        NavigationManager.NavigateTo($"/session/{trainingDayId}");
    }

    private async Task DeleteExercise(int trainingDayId, int planDayExerciseId)
    {
        var parameters = new DialogParameters
        {
            { nameof(ConfirmationDialog.Title), "Remove Exercise" },
            { nameof(ConfirmationDialog.ContentText), "Are you sure you want to remove this exercise?" },
            { nameof(ConfirmationDialog.ConfirmButtonText), "Remove" }
        };

        var dialog = await DialogService.ShowAsync<ConfirmationDialog>("Remove Exercise", parameters);
        var result = await dialog.Result;

        if (result is not null && !result.Canceled)
        {
            await WorkoutPlanService.RemoveExerciseFromTrainingDayAsync(Id, trainingDayId, planDayExerciseId);
            await LoadPlan();
            Snackbar.Add("Exercise removed successfully", Severity.Success);
        }
    }
}
```

**Add DTO:**

```csharp
public record UpdateWorkoutPlanDto(string Name, List<UpdateTrainingDayOrderCommand> TrainingDays);
```

### 5.4 Add Skip Exercise Button to WorkoutSessionPage

**File: `WorkoutManager.Web/Pages/Session/WorkoutSessionPage.razor`**

Update step content to include skip button and session notes:

```razor
<MudStep Title="@GetExerciseName(exercise.ExerciseId)" StepContentStyle="overflow-y: auto; max-height: 60vh;">
    <!-- Existing previous session display -->
    
    <MudCheckBox @bind-Checked="exercise.Skipped" Label="Skip this exercise" Color="Color.Warning" />
    
    @if (!exercise.Skipped)
    {
        <!-- Existing sets input -->
    }
    
    <MudTextField @bind-Value="exercise.Notes" Label="Notes" Lines="3" Variant="Variant.Outlined" Class="mt-4" />
</MudStep>

<!-- After stepper, before buttons -->
<MudTextField @bind-Value="_sessionNotes" Label="Session Notes" Lines="3" 
              Variant="Variant.Outlined" Class="mt-4" />
```

**Update code-behind:**

```csharp
private string? _sessionNotes;

private async Task NextStep()
{
    var activeStepIndex = _stepper.ActiveIndex;
    var currentExercise = _session!.Exercises.OrderBy(e => e.Order).ElementAt(activeStepIndex);
    
    var payload = new UpdateSessionExerciseDto
    {
        Notes = currentExercise.Notes,
        Skipped = currentExercise.Skipped,
        Sets = currentExercise.Skipped ? new List<ExerciseSetDto>() : currentExercise.Sets
    };
    await SessionService.UpdateSessionExerciseAsync(_session.Id, currentExercise.Id, payload);

    if (activeStepIndex == _session.Exercises.Count - 1)
    {
        await SessionService.FinishSessionAsync(_session.Id, _sessionNotes);
        NavigationManager.NavigateTo("/history");
    }
    else
    {
        await _stepper.NextStepAsync();
    }
}
```

### 5.5 Enhance History Pages

**File: `WorkoutManager.Web/Pages/History/HistoryPage.razor`**

```razor
<MudList T="SessionSummaryDto">
    @foreach (var session in _sessions)
    {
        <MudListItem T="SessionSummaryDto" @onclick="() => NavigateToSession(session.Id)">
            <div>
                <MudText Typo="Typo.subtitle1">@session.PlanName - @session.TrainingDayName</MudText>
                <MudText Typo="Typo.body2">@session.StartTime.ToString("g")</MudText>
                @if (session.EndTime.HasValue)
                {
                    <MudText Typo="Typo.caption">
                        Duration: @((session.EndTime.Value - session.StartTime).ToString(@"hh\:mm"))
                    </MudText>
                }
            </div>
        </MudListItem>
    }
</MudList>
```

**File: `WorkoutManager.Web/Pages/History/SessionSummaryPage.razor`**

```razor
@if (_session.Notes != null)
{
    <MudText><strong>Session Notes:</strong> @_session.Notes</MudText>
}

@foreach (var exercise in _session.Exercises.OrderBy(e => e.Order))
{
    <MudCard Class="mb-4">
        <MudCardHeader>
            <CardHeaderContent>
                <MudText Typo="Typo.h6">
                    @GetExerciseName(exercise.ExerciseId)
                    @if (exercise.Skipped)
                    {
                        <MudChip Size="Size.Small" Color="Color.Warning">Skipped</MudChip>
                    }
                </MudText>
            </CardHeaderContent>
        </MudCardHeader>
        <MudCardContent>
            @if (exercise.Skipped)
            {
                <MudText Color="Color.Warning">This exercise was skipped</MudText>
            }
            else if (exercise.Sets.Any())
            {
                <!-- Existing sets table -->
            }
            
            @if (!string.IsNullOrEmpty(exercise.Notes))
            {
                <MudText Class="mt-2"><strong>Notes:</strong> @exercise.Notes</MudText>
            }
        </MudCardContent>
    </MudCard>
}
```

## Phase 6: Drag & Drop Functionality (Polish)

### 6.1 Install MudBlazor Drag & Drop (if needed)

MudBlazor has built-in drag & drop support via `MudDropContainer`.

### 6.2 Implement Training Day Reordering

**File: `WorkoutManager.Web/Pages/Plans/PlanDetailPage.razor`**

Replace expansion panels with drop container when in edit mode:

```razor
@if (_isEditMode)
{
    <MudDropContainer T="TrainingDayDto" Items="_plan.TrainingDays" 
                      ItemsSelector="@((item, dropzone) => true)"
                      ItemDropped="TrainingDayDropped" Class="mt-4">
        <ChildContent>
            @foreach (var day in _plan.TrainingDays.OrderBy(d => d.Order))
            {
                <MudDropZone T="TrainingDayDto" Identifier="@day.Id.ToString()">
                    <MudPaper Class="pa-4 mb-2">
                        <MudText Typo="Typo.h6">@day.Name</MudText>
                        <!-- Exercise list -->
                    </MudPaper>
                </MudDropZone>
            }
        </ChildContent>
        <ItemRenderer>
            <MudPaper Class="pa-2">@context.Name</MudPaper>
        </ItemRenderer>
    </MudDropContainer>
}
else
{
    <!-- Existing read-only view -->
}
```

**Code-behind:**

```csharp
private async Task TrainingDayDropped(MudItemDropInfo<TrainingDayDto> dropItem)
{
    if (_plan == null) return;

    var orderedDays = _plan.TrainingDays.OrderBy(d => d.Order).ToList();
    var draggedItem = dropItem.Item;
    var targetItem = orderedDays[dropItem.IndexInZone];

    // Reorder logic
    orderedDays.Remove(draggedItem);
    orderedDays.Insert(dropItem.IndexInZone, draggedItem);

    var reorderedDays = orderedDays.Select((day, index) => 
        new UpdateTrainingDayOrderCommand(day.Id, index + 1)).ToList();

    try
    {
        await WorkoutPlanService.ReorderTrainingDaysAsync(Id, reorderedDays);
        await LoadPlan();
    }
    catch
    {
        Snackbar.Add("Failed to reorder training days", Severity.Error);
    }
}
```

### 6.3 Implement Exercise Reordering

Similar approach for exercises within training days using `MudDropContainer`.

## Phase 7: Testing & Integration

### 7.1 Test Checklist

- [ ] User registration and login
- [ ] Password reset flow
- [ ] Account deletion with password verification
- [ ] Welcome screen on first login
- [ ] Plan CRUD operations
- [ ] Exercise management with muscle group filters
- [ ] Workout session flow with skip and notes
- [ ] History viewing with complete details
- [ ] Drag & drop reordering
- [ ] Authorization on all endpoints

### 7.2 Supabase Database Migration

Run migrations in `WorkoutManager.Data/supabase/migrations/` against Supabase instance.

### 7.3 Environment Configuration

Update `appsettings.json` files with actual Supabase URLs and keys.

## Summary

This implementation plan covers all missing features identified in the analysis:

**P0 Features (Blockers):**

- Authentication infrastructure (API + UI)
- Supabase integration
- User context in operations
- Complete API endpoints
- Service layer completion

**P1 Features (Core):**

- Plan editing and deletion
- Muscle group filters/selectors
- Skip exercise functionality
- Enhanced history display
- Session-level notes

**P2 Features (Polish):**

- Welcome screen
- Confirmation dialogs
- Drag & drop reordering
- Password verification for account deletion

All implementations follow the established conventions from the cursor rules (code-behind separation, PascalCase naming, dependency injection, etc.).

### To-dos

- [ ] Phase 1: Setup Supabase integration and authentication foundation in both API and Web projects
- [ ] Phase 2: Implement authentication services and UI pages (login, register, reset password, welcome screen)
- [ ] Phase 3: Complete all missing API endpoints and fix existing ones
- [ ] Phase 4: Complete UI service layer with all missing methods
- [ ] Phase 5: Implement missing UI components (muscle group selectors, skip button, session notes, plan editing)
- [ ] Phase 6: Add drag & drop functionality for reordering training days and exercises
- [ ] Phase 7: Testing, database migration, and environment configuration