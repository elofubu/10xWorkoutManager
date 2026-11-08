# Migration Plan: Supabase Auth → Microsoft Entra External ID

Implements Option A from `azure-auth-analysis.md`. Independent of `sql-server-migration-plan.md`
except for one shared decision point (Phase 4 below) — sequence the SQL migration first since
this plan's data-continuity question depends on it.

## The visible change, up front

Today, sign-in/sign-up/password-reset are custom Blazor forms built in-app
(`LoginPage.razor`, `RegisterPage.razor`, `ResetPasswordPage.razor`, `UpdatePasswordPage.razor`)
that call `Supabase.Gotrue` directly and never leave your UI. MSAL-based Entra auth is
**redirect-based by default**: clicking "sign in" navigates away to a Microsoft-hosted sign-in
page and back, via a `/authentication/login-callback` route. You can customize that hosted page's
branding (logo, colors, text), but keeping the sign-in form fully embedded in your own UI like
today requires the more advanced "native authentication" / custom-policy setup, which is
meaningfully more work. This plan assumes the standard redirect flow — flag now if the in-app
form experience is a hard requirement, since it changes the approach.

## Phase 1 — Azure portal setup

- Create a Microsoft Entra **External tenant** (distinct from any organizational/work tenant —
  this is the consumer-identity tenant type, created under your existing subscription).
- Register **two** app registrations in that tenant:
  - one for `WorkoutManager.Api` (the protected resource — exposes a scope, e.g. `api://workout-manager-api/access_as_user`)
  - one for `WorkoutManager.Web` (the SPA/WASM client — public client, redirect URI pointing at your Static Web App's `/authentication/login-callback`)
- Create a **user flow** (sign-up + sign-in) — email/password to start, matching what Supabase
  Auth gave you today. Configure branding here if you want the hosted page to look like your app.
- Note down: tenant's authority URL, Api's Application (client) ID + exposed scope, Web's
  Application (client) ID.

## Phase 2 — Api changes

`WorkoutManager.Api/Program.cs`'s JWT bearer setup already reads config-driven
`Authority`/`Audience`/`ValidIssuer` — this is a config swap, not a rewrite:

```csharp
options.Authority = "https://<tenant>.ciamlogin.com/<tenant-id>/v2.0";
options.Audience = "<Api app registration's client ID>";
// ValidIssuer becomes the same authority URL (v2.0 endpoint)
```

`UserContextService.GetCurrentUserId()` currently reads `ClaimTypes.NameIdentifier` (mapped from
Supabase's `sub` claim). Entra tokens carry the stable user identifier as the `oid` claim instead.
Change:

```csharp
var userId = _httpContextAccessor.HttpContext?.User?.FindFirst("oid")?.Value;
```

Everything downstream — `[Authorize]` attributes, controller logic, repository `user_id`
filtering — is unchanged. This is the entire Api-side blast radius.

## Phase 3 — Web changes

- Remove: `Supabase` NuGet package (Auth usage — note `WorkoutManager.Web` currently also uses
  the Supabase client purely for `Auth`, nothing else, since data access already goes through
  `WorkoutManager.Api`), `AuthService.cs`, `SupabaseAuthenticationStateProvider.cs`.
- Add: `Microsoft.Authentication.WebAssembly.Msal` package.
- `Program.cs`: replace the Supabase client registration with:
  ```csharp
  builder.Services.AddMsalAuthentication(options =>
  {
      builder.Configuration.Bind("EntraExternalId", options.ProviderOptions.Authentication);
      options.ProviderOptions.DefaultAccessTokenScopes.Add("api://workout-manager-api/access_as_user");
  });
  ```
- **Replace, not port**: `LoginPage.razor`, `RegisterPage.razor`, `ResetPasswordPage.razor`,
  `UpdatePasswordPage.razor`, and `Callback.razor` (Supabase's own OAuth-fragment-parsing
  callback page) all go away. MSAL's `RemoteAuthenticatorView` component handles
  `/authentication/{action}` routes (login, logout, login-callback, logout-callback) generically —
  this is a `Microsoft.AspNetCore.Components.WebAssembly.Authentication`-provided component, one
  route registration instead of five custom pages.
- `RedirectToLogin.razor` (currently a manual redirect to `/authentication/login`): can mostly
  stay as-is, just confirm it still points at the right route once MSAL owns that path.
- `MainLayout.razor`'s `Logout()` method currently calls `AuthService.LogoutAsync()` then casts
  `AuthenticationStateProvider` to `SupabaseAuthenticationStateProvider` to notify a state change.
  With MSAL, logout becomes a navigation to `/authentication/logout` (handled by
  `RemoteAuthenticatorView`) instead — the manual cast/notify pattern goes away entirely, MSAL's
  own `AuthenticationStateProvider` implementation manages this.
- `AuthorizationMessageHandler.cs` (written in the WASM migration's Phase 4, currently reads the
  Supabase session from local storage to attach a Bearer token): replace with
  `Microsoft.AspNetCore.Components.WebAssembly.Authentication`'s built-in
  `AuthorizationMessageHandler` base class, which integrates with MSAL's token cache/refresh
  automatically instead of the hand-rolled local-storage read. Less code, and gets silent token
  refresh for free (something the current Supabase-session approach doesn't handle robustly).
- `SettingsPage.razor.cs`'s `DeleteAccount` flow currently calls a `TODO`-stub
  `AuthService.DeleteAccountAsync` — this needs a real implementation against Microsoft Graph
  (deleting a user from an external tenant requires Graph API calls with appropriate permissions,
  not something MSAL's client-side SDK does directly). Flagging since it's currently unfinished
  even under Supabase and easy to lose track of during this swap.

## Phase 4 — User data continuity (decision needed)

Entra's `oid` claim values are unrelated to Supabase's existing `auth.uid()` values — every row
in your database keyed by the old `user_id` won't match any user who signs in under the new
system. Same fork as the SQL migration plan:

1. **Fresh start**: acceptable if there's no real user data worth preserving yet.
2. **Manual reconciliation**: if you want to keep data, you'd need a one-off admin process —
   have each existing user sign up fresh under Entra, then run a script that matches old Supabase
   users to new Entra users (by email) and bulk-updates every `user_id` column to the new `oid`.
   Only worth it with real users/data to preserve.

Since this is the same fork as the SQL Server migration's Phase 6, make this call once, not
twice — whatever you decide there applies here too.

## Phase 5 — Testing checklist

- [ ] Sign-up flow creates a new Entra user and redirects back successfully
- [ ] Sign-in flow round-trips and lands back in the app authenticated
- [ ] Token attached correctly on Api calls (new `AuthorizationMessageHandler`)
- [ ] `[Authorize]`-protected Api endpoints reject requests without a valid token
- [ ] `UserContextService.GetCurrentUserId()` correctly reads `oid` and existing per-user
      queries (plans, sessions, exercises) scope correctly to the signed-in user
- [ ] Logout actually clears the session (MSAL cache + redirect)
- [ ] Silent token refresh works without forcing a re-login mid-session
- [ ] Password reset flow (via the Entra-hosted page) works end-to-end
- [ ] `DeleteAccount` either has a real Graph-based implementation or is explicitly deferred with
      a visible TODO, not silently broken

## Effort estimate

| Phase | Description | Est. |
|---|---|---|
| 1 | Azure portal setup (tenant, app registrations, user flow) | 1.5–2 hr (portal work, first time in this product) |
| 2 | Api JWT bearer config + UserContextService | 30 min |
| 3 | Web: remove Supabase auth code, add MSAL, replace 5 pages with RemoteAuthenticatorView, rewrite AuthorizationMessageHandler | 3–4 hr |
| 4 | Data continuity (if reconciling existing users) | variable, skip if fresh start |
| 5 | Testing | 1.5 hr |

**Total: ~6.5–8 hours**, plus real time for Phase 1's portal work since Entra External ID is a
newer product with thinner documentation than the older, more widely-written-about Azure AD B2C.
