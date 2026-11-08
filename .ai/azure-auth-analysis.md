# Auth Options Analysis: Replacing Supabase Auth on Azure (No Enterprise Tenant)

## What Supabase Auth is doing today

Worth being precise about what's actually being replaced, since it's more than "login":

- **Sign-up / sign-in** (`AuthService.RegisterAsync`/`LoginAsync`) via `Supabase.Gotrue`, email+password.
- **Session/token storage** client-side (`SupabaseAuthenticationStateProvider`, browser storage).
- **Token issuance**: Supabase issues a JWT on login; `WorkoutManager.Api`'s `Program.cs` validates
  it via standard JWT bearer middleware (`Authority` = Supabase project URL, checks issuer/audience/
  signing key). This part is already provider-agnostic — any OIDC-compliant issuer can slot in here
  with a config change, no controller code changes needed.
- **User identity → app data**: `UserContextService.GetCurrentUserId()` reads `ClaimTypes.NameIdentifier`
  from the validated token and that's the `user_id` written into every table. Whatever replaces
  Supabase Auth just needs to keep putting a stable user ID into that claim.

So the real shape of this decision is: *what issues the JWT, and where do credentials/passwords
live*. Two realistic paths on Azure without an enterprise/org tenant, plus one to rule out.

## Option A: Microsoft Entra External ID (recommended)

Microsoft's current Azure-native CIAM (customer identity) product. Despite the "Entra" branding,
this does **not** require an enterprise/work account — it's built specifically for exactly this
scenario: an "external tenant" for your app's end users, entirely separate from any organizational
Azure AD tenant. You create it under your existing Azure subscription like any other resource.

- **Cost**: free for the first 50,000 monthly active users, $0.03/MAU after that. At personal-app
  scale this is free indefinitely.
- **What it replaces**: hosted (or embeddable) sign-up/sign-in UI, password storage and reset
  flows, MFA if you want it later, social login (Google etc.) if you ever want it — all handled
  for you.
- **What changes in the code**:
  - `WorkoutManager.Api/Program.cs`: swap the JWT bearer `Authority`/`Audience`/`ValidIssuer` to
    Entra External ID's values. The `[Authorize]` + `UserContextService` pattern stays identical —
    it just reads a different issuer's claims. The user ID claim becomes Entra's `oid` (object ID)
    instead of Supabase's `sub`; `UserContextService` needs a one-line change to read that claim
    instead of (or in addition to) `NameIdentifier`, and existing `user_id` values in the database
    won't match new Entra object IDs — this is a real data-continuity issue if you're keeping
    existing users/data (ties into the "fresh start vs. data migration" decision in the SQL plan).
  - `WorkoutManager.Web`: replace `Supabase.Gotrue`-based `AuthService`/`SupabaseAuthenticationStateProvider`
    with `Microsoft.Authentication.WebAssembly.Msal` (the standard Blazor WASM MSAL integration) —
    this is a first-class, well-documented package specifically for "Blazor WASM + Entra" auth,
    noticeably less custom code than the current hand-rolled Supabase session-storage logic.
  - Requires setting up an app registration in the external tenant (redirect URIs, scopes) —
    portal work, roughly comparable in effort to what you already did for Supabase's project setup.
- **Caveat**: this is the *newer* product — Azure AD B2C (the older, more widely-documented Azure
  CIAM product) stopped being available to new customers in May 2025 specifically because Entra
  External ID is meant to replace it. Documentation and community examples for External ID are
  still catching up relative to B2C's much longer history, so expect to lean on official Microsoft
  Learn docs more than blog posts/Stack Overflow for anything non-trivial.

## Option B: Self-rolled ASP.NET Core Identity + JWT

Skip external identity providers entirely — `WorkoutManager.Api` issues and validates its own
JWTs, backed by ASP.NET Core Identity tables living in the same Azure SQL database from the
migration plan above.

- **Cost**: $0 forever, no MAU billing, ever.
- **What it replaces**: everything — you own password hashing (Identity handles this well, it's
  not naive), token issuance/refresh, and critically, things Supabase/Entra currently give you for
  free: email verification and password-reset *flows* (you still need an actual email-sending
  service for these — Azure Communication Services or SendGrid both have free tiers, but it's one
  more thing to wire up), account lockout policies, and general auth security hardening.
- **What changes in the code**:
  - New `AspNetUsers`/`AspNetRoles` etc. tables via Identity's own EF Core migrations, added to
    the same `WorkoutManagerDbContext` from the SQL migration.
  - `WorkoutManager.Api` gets new endpoints (or a small auth-specific controller) for
    register/login/refresh, issuing JWTs signed with your own key (`JwtBearerSettings` already
    has the shape for this — `IssuerSigningKey`, `Audience`, etc. — you'd just become the issuer
    instead of validating someone else's).
  - `WorkoutManager.Web`: `AuthService` becomes simpler in one sense (plain HTTP calls to your own
    `/api/auth/*` endpoints instead of a third-party SDK) but you're now responsible for
    everything that SDK was doing correctly.
  - User ID becomes Identity's own `Guid` user key — same continuity caveat as Option A regarding
    existing data.
- **Honest tradeoff**: this is meaningfully more security surface area for you to own correctly
  (password reset token expiry, email confirmation bypass risks, rate-limiting login attempts,
  etc.). ASP.NET Core Identity itself is solid and well-tested, but the flows *around* it
  (transactional email, brute-force protection) are on you.

## Option C: Azure AD B2C — not recommended for a new setup

Ruled out, noting why for completeness: closed to new customers since May 2025 (existing
customers can keep using it, and Microsoft supports it until at least May 2030, but you can't
create a fresh B2C tenant today), and its P2 tier is being retired entirely in March 2026.
Entra External ID is explicitly the successor product — starting fresh on B2C now would mean
building on something already being phased out.

## Comparison

| | Entra External ID | Self-rolled Identity | Azure AD B2C |
|---|---|---|---|
| Cost at your scale | Free (< 50k MAU) | Free always | N/A — don't start here |
| Setup effort | Medium (external tenant + app registration + MSAL wiring) | Medium-high (auth endpoints + email service + security hardening) | — |
| Ongoing effort | Low (Microsoft runs it) | Higher (you own password reset, lockout, etc.) | — |
| Vendor dependency | Yes (Microsoft, but it's Azure-native and you're already all-in on Azure) | None | — |
| Matches "get rid of Supabase" most fully | Mostly (still an external identity provider) | Completely | — |

## Recommendation

**Entra External ID**, given you're already committing to Azure for hosting and now the
database — it's the path of least new engineering effort, stays free at personal-project scale,
and the Web-side integration (MSAL for Blazor WASM) is a well-trodden, first-party path rather
than something you're maintaining yourself. The main real cost is learning a newer product with
thinner community documentation than B2C had.

Lean toward **self-rolled Identity** instead only if avoiding any external identity dependency
matters more to you than saved engineering time — it's the more complete answer to "get rid of
Supabase," just a more expensive one in your own time, and it puts you on the hook for auth
security properties you currently get for free.

Either way, this is a separable decision from the SQL Server migration — the JWT-issuer swap only
touches `Program.cs`'s bearer config and `UserContextService`, plus the Web-side login flow. It
doesn't block or get blocked by the database migration, though I'd sequence the database first
since Option B specifically wants Identity's tables living in that same database.
