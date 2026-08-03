# Blazor Server → WebAssembly + API Migration Plan

## Why

`WorkoutManager.Web` runs as Blazor Server today (`AddInteractiveServerComponents` / `AddInteractiveServerRenderMode`), deployed as a single free-tier Azure App Service. Blazor Server depends on a persistent SignalR circuit between browser and server. On Android, locking the screen suspends/throttles that connection; combined with Free-tier App Service having no "Always On" (the whole process unloads after ~20 min idle), the circuit is lost and users hit "Could not reconnect to the server."

Moving the UI to Blazor WebAssembly removes the dependency on a live server connection entirely — the UI keeps running client-side regardless of screen state, and only ordinary (stateless, retryable) HTTP calls need the network. This also matches the architecture already documented in `.ai/tech-stack.md` (WASM frontend + separate Web API), which was abandoned by the `migrate to blazor server` commit.

## Target Architecture

- **WorkoutManager.Web** — Blazor WASM, static output, hosted on **Azure Static Web Apps (Free tier)**. No server process, so nothing to idle-unload; no SignalR circuit to lose.
- **WorkoutManager.Api** — already built (JWT-bearer auth against Supabase, per-user scoped controllers), deployed to the existing **free Linux App Service plan**. Stateless REST — an idle-unload cold start costs a couple of seconds, never loses user state.
- Auth (sign in/up/refresh) happens **client-side in WASM** using the Supabase **anon key** (safe to ship — this is what the anon key is for). Data CRUD goes through `WorkoutManager.Api`, which uses the secret/service-role key server-side and validates the user's Supabase JWT.

## Phase 1 — Project file & entry point

**`WorkoutManager.Web/WorkoutManager.Web.csproj`**
- SDK: `Microsoft.NET.Sdk.Web` → `Microsoft.NET.Sdk.BlazorWebAssembly`
- Add: `Microsoft.AspNetCore.Components.WebAssembly`, `Microsoft.AspNetCore.Components.WebAssembly.DevServer`, `Blazored.LocalStorage`
- Remove: `Microsoft.Extensions.Http` is fine to keep (still need HttpClient)
- **Drop the `ProjectReference` to `WorkoutManager.BusinessLogic`** — that project talks to Supabase with the secret key and pulls in server-only deps. WASM must never reference it (this is the leak vector called out below).

**`WorkoutManager.Web/Program.cs`**
- Replace `WebApplication.CreateBuilder` with `WebAssemblyHostBuilder.CreateDefault(args)`
- Register `HttpClient` pointed at the Api's base address (from config, see Phase 2), wrapped with the new `AuthorizationMessageHandler` (Phase 4)
- Remove: Data Protection key persistence block, `AddRazorComponents`/`AddInteractiveServerComponents`/hub options — none of this exists in WASM
- Keep: MudBlazor services, app service registrations (`IWorkoutPlanService` etc.), auth core, cascading auth state

**`WorkoutManager.Web/App.razor`**
- Already has a working WASM host page at `wwwroot/index.html` (never got deleted in the Server migration — confirmed correct: `blazor.webassembly.js`, `div#app`). Point the WASM builder at `#app` and this should need minimal changes. Delete the Server-only `App.razor` render-mode wiring.

## Phase 2 — Config split (security-critical)

Current `WorkoutManager.Web/appsettings.json` holds `Supabase:Secret` — a service-role key. **This must never ship in a WASM bundle.**

- Delete `Supabase:Secret` from the Web project entirely.
- Create `WorkoutManager.Web/wwwroot/appsettings.json` (WASM reads config from `wwwroot`, not project root) containing only public-safe values:
  ```json
  {
    "Supabase": { "Url": "...", "AnonKey": "..." },
    "ApiUrl": "https://<api-app-name>.azurewebsites.net"
  }
  ```
- `WorkoutManager.Api` keeps the secret key server-side (already does, via `SupabaseSettings`/user secrets) — no change needed there.
- Update `master.yml`: the `Supabase.Secret` variable substitution currently targets `WorkoutManager.Web/appsettings.json` — redirect Web's substitution to the anon key / wwwroot config, and keep the secret substitution only for the Api job (new, see Phase 5).

## Phase 3 — Storage layer (5 files)

`ProtectedLocalStorage`/`ProtectedSessionStorage` are Blazor Server-only (they round-trip through the SignalR circuit + server-side data protection). Replace with `Blazored.LocalStorage` (`ILocalStorageService`) or plain `IJSRuntime` interop against `window.localStorage`/`sessionStorage`.

Files to update:
- `Services/AuthService.cs`
- `Services/SupabaseAuthenticationStateProvider.cs`
- `Services/WorkoutSessionStateService.cs` (session storage → `ILocalStorageService` or `IJSRuntime` session storage interop)
- `Pages/Home.razor.cs`
- `Pages/WelcomePage.razor.cs`

Behavior should be equivalent (session token, "hasSeenWelcome" flag, in-progress workout state) — just swap the storage mechanism, not the logic.

## Phase 4 — Data access rewrite

Today `Services/WorkoutPlanService.cs`, `SessionService.cs`, `ExerciseService.cs`, `MuscleGroupService.cs` call the BusinessLogic layer **in-process**. These need to become HTTP calls to `WorkoutManager.Api`'s existing controllers instead.

- Write `AuthorizationMessageHandler` (`DelegatingHandler`) that reads the current Supabase access token (from the new storage layer) and attaches it as `Authorization: Bearer <token>` to outgoing requests. Referenced in the old migration doc but never actually present in the codebase — needs to be written from scratch.
- Register a named `HttpClient` ("ApiHttpClient") with base address = `ApiUrl` from config, `.AddHttpMessageHandler<AuthorizationMessageHandler>()`.
- Rewrite each of the four services to call the matching Api controller (`WorkoutPlansController`, `SessionsController`, `ExercisesController`, `MuscleGroupsController`, plus `SessionExercisesController`/`PlanDayExercisesController` for nested operations) via the HttpClient instead of the injected BizLogic interfaces.
- DTOs (`WorkoutPlanDto`, etc.) already live in `WorkoutManager.BusinessLogic.DTOs` — either move the DTOs to a shared contracts project the Web app *can* reference (no server logic, just POCOs), or duplicate minimal DTOs in the Web project. Prefer extracting DTOs to a small `WorkoutManager.Contracts` project referenced by both Web and Api, to avoid duplication and keep the Web project fully decoupled from BusinessLogic/Data.

## Phase 5 — Deploy the Api project

The Api project is fully built (controllers, JWT bearer auth against Supabase, `UserContextService` for per-user scoping) but **is not currently deployed anywhere** — `master.yml` only builds/publishes `WorkoutManager.Web`.

- Provision a new Azure App Service (Linux, Free F1, same subscription/plan family as today) for `WorkoutManager.Api`.
- Restrict CORS in `WorkoutManager.Api/Program.cs` from `AllowAnyOrigin` to the Static Web App's actual origin once known.
- Add a `deploy_api` job to `.github/workflows/master.yml`: build/publish `WorkoutManager.Api`, substitute `SupabaseSettings.Secret`/`JwtBearerSettings.Key` from GitHub secrets (already exist, currently only used for tests), deploy via `azure/webapps-deploy@v2` to the new Api app.

## Phase 6 — Deploy the Web project

- Provision an Azure Static Web App (Free tier).
- Add a `deploy_web` job (or convert the existing one) using `Azure/static-web-apps-deploy@v1` instead of `azure/webapps-deploy@v2`, pointing at the WASM publish output.
- Point `wwwroot/appsettings.json`'s `ApiUrl` at the deployed Api's URL; confirm CORS on the Api allows the SWA's domain.

## Phase 7 — Testing checklist

- [ ] App loads and runs entirely client-side (check Network tab: no persistent WebSocket to the Web host)
- [ ] Login / register / logout via Supabase Auth (anon key, client-side)
- [ ] Token attached correctly on Api calls (`AuthorizationMessageHandler`)
- [ ] All CRUD flows: plans, sessions, exercises, muscle groups
- [ ] In-progress workout session state survives navigation and refresh
- [ ] **The actual bug**: lock Android screen mid-session, wait >1 min, unlock — app resumes without a "reconnect" error
- [ ] Api cold start after idle (Free tier) doesn't lose any user data (it shouldn't — it's stateless)
- [ ] No secret/service-role key present anywhere in the published WASM `wwwroot` output (`grep` the publish folder for the key value as a sanity check before going live)

## Rollback

If this branch doesn't pan out: discard the branch, redeploy `master` (current Blazor Server version) to the existing App Service. No production changes happen until Phase 5/6 deploy steps are run.

## Effort estimate

| Phase | Description | Est. |
|---|---|---|
| 1 | csproj & Program.cs | 1 hr |
| 2 | Config split | 30 min |
| 3 | Storage rewrite (5 files) | 1.5 hr |
| 4 | Data access rewrite + AuthorizationMessageHandler + DTO extraction | 3–4 hr |
| 5 | Api deployment + CI/CD | 1.5 hr |
| 6 | Web (SWA) deployment + CI/CD | 1 hr |
| 7 | Testing | 1.5–2 hr |

**Total: ~10–12 hours**, plus Azure portal setup (new App Service + Static Web App, GitHub secrets) that needs to happen interactively in the Azure portal / GitHub settings.
