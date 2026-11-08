# Deployment TODO — Azure SQL + Entra External ID (branch `feature/azure-sql-entra`)

Code side is done: EF Core / Azure SQL replaces Supabase Postgres, and Entra External ID (MSAL)
replaces Supabase Auth. What's left needs the Azure portal and GitHub settings. This branch
builds on `feature/wasm-migration`, so that branch's own checklist items (Static Web App, Api App
Service) still apply — the Supabase-specific secrets from it no longer do.

## 1. Azure resources

- [ ] **Static Web App** (Free) for `WorkoutManager.Web` → note its deployment token.
- [ ] **App Service** (Linux, Free F1) for `WorkoutManager.Api` → note the app name and URL.
- [ ] **Azure SQL Database** (Free offer) — you have one. Create a **second** free DB for CI/tests
      if you want tests to run in GitHub Actions (up to 10 free DBs per subscription).
      Allow Azure services through the SQL server firewall so the App Service can connect.

## 2. Entra External ID (portal work, ~1.5–2 hr first time)

Free for the first 50,000 monthly active users. Created under your normal Azure subscription —
no enterprise/work tenant needed.

- [ ] Create an **external tenant** (Microsoft Entra admin center → Entra ID → Overview → Manage
      tenants → Create → External).
- [ ] **App registration: `WorkoutManager.Api`**. Under *Expose an API*, set the Application ID URI
      (default `api://<client-id>`) and add a scope `access_as_user`. Note its **client ID**.
- [ ] **App registration: `WorkoutManager.Web`**. Platform: *Single-page application*. Redirect URIs:
      `https://<static-web-app-host>/authentication/login-callback` and
      `https://localhost:<dev-port>/authentication/login-callback`. Add API permission for the
      Api's `access_as_user` scope and grant admin consent. Note its **client ID**.
- [ ] **User flow** (sign up and sign in, email + password), and add both app registrations to it.
- [ ] Note the tenant subdomain and tenant ID.

## 3. Fill in config

Placeholders are in `WorkoutManager.Api/appsettings.json` and `WorkoutManager.Web/wwwroot/appsettings.json`.

- [ ] `.github/workflows/master.yml` → `deploy_api` job: set (done: `workout-manager-api`; the service principal behind `AZURE_CREDENTIALS` needs Contributor on this App Service).
- [ ] Local dev, Api: `dotnet user-secrets set "ConnectionStrings:WorkoutManagerDb" "<azure sql connection string>"`
      plus `EntraExternalId:Authority` and `EntraExternalId:Audience` (in `WorkoutManager.Api`).
- [ ] Local dev, Web: edit `wwwroot/appsettings.json` (or a local override) with the values below.

## 4. GitHub secrets

Authority formats differ: Api uses the tenant **ID** (`https://<sub>.ciamlogin.com/<tenant-id>/v2.0`),
Web uses the tenant **domain** (`https://<sub>.ciamlogin.com/<sub>.onmicrosoft.com/v2.0`).

- [ ] `ENTRA_AUTHORITY` — Api issuer (tenant ID form)
- [ ] `ENTRA_API_CLIENT_ID` — Api app registration client ID (token audience)
- [ ] `ENTRA_WEB_AUTHORITY` — Web authority (domain form)
- [ ] `ENTRA_WEB_CLIENT_ID` — Web app registration client ID
- [ ] `ENTRA_API_SCOPE` — `api://<api-client-id>/access_as_user`
- [ ] `CONNECTIONSTRINGS_WORKOUTMANAGERDB_PROD` — production Azure SQL connection string
- [ ] `CONNECTIONSTRINGS_WORKOUTMANAGERDB` — test DB connection string (CI integration tests)
- [ ] `API_URL`, `AZURE_STATIC_WEB_APPS_API_TOKEN`, `AZURE_CREDENTIALS` — as in the WASM checklist
- [ ] Obsolete, can be deleted: `SUPABASESETTINGS_*`, `JWTBEARERSETTINGS_*`, `CONNECTIONSTRINGS_DEFAULTCONNECTION`
- [ ] **Rotate the old Supabase secret/service key** — it was committed in `appsettings.json` /
      `appsettings.Testing.json` history before this branch removed it.

## 5. After URLs exist

- [ ] Tighten the `AllowAnyOrigin()` CORS policy in `WorkoutManager.Api/Program.cs` to the Static Web App origin.
- [ ] First Api start applies EF migrations and seeds muscle groups/exercises automatically
      (`DatabaseSeeder`). Check the log for "migrations" errors on first deploy.

## 6. Testing checklist

- [ ] Sign up → lands back in the app authenticated; sign in; sign out clears the session
- [ ] Token attached on Api calls; Api rejects calls without one
- [ ] New user sees only their own data (`oid` claim → `user_id`); plans/sessions/exercises CRUD
- [ ] Silent token refresh works mid-session (no forced re-login)
- [ ] Password reset via the Entra-hosted page
- [ ] In-progress workout survives navigation/refresh; Android lock-screen resume (original WASM bug)
- [ ] No secrets in the published `wwwroot` output

## Known gaps

- **Delete account** is stubbed: shows "isn't available yet". Real deletion needs an Api endpoint
  that calls Microsoft Graph to delete the Entra user and removes their DB rows.
- **E2E tests** (`WorkoutManager.E2E.Tests`) still target the removed Supabase login form and
  will need rewriting for the Entra redirect flow (they were not run here).
- `ReorderExercisesAsync` / `ReorderTrainingDaysAsync` in the Web services still have no matching Api route.
- Fresh start: existing Supabase users and workout data are not migrated.
- `README.md`, `.ai/*` docs, `supabase/` and `WorkoutManager.Data/supabase/` still describe the old Supabase setup.
