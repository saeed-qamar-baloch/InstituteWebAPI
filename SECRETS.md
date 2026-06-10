# Secret Management — Rozhn Institute API

## Rule: no real secrets in source control

`appsettings.json` and `appsettings.Production.json` are committed but contain
only empty/placeholder values.  Real secrets must be supplied at runtime via
one of the mechanisms below.

---

## Local development — `dotnet user-secrets`

User secrets are stored outside the repo on your machine and are merged into
`IConfiguration` automatically when `ASPNETCORE_ENVIRONMENT=Development`.

```bash
# 1. Enable user secrets on the project (one-time, already done if secrets.json exists)
dotnet user-secrets init --project InstituteWebAPI

# 2. Set the JWT signing key (minimum 32 characters)
dotnet user-secrets set "Jwt:Key" "YOUR-STRONG-RANDOM-SECRET-64-CHARS-OR-MORE" --project InstituteWebAPI

# 3. Override connection strings if you're not using LocalDB
dotnet user-secrets set "ConnectionStrings:RozhnWebConnectionString" \
  "Server=.;Database=RozhnWebAPIDb;User Id=sa;Password=...;TrustServerCertificate=True" \
  --project InstituteWebAPI

dotnet user-secrets set "ConnectionStrings:RozhnWebAuthConnectionString" \
  "Server=.;Database=RozhnWebAPIAuthDb;User Id=sa;Password=...;TrustServerCertificate=True" \
  --project InstituteWebAPI

# 4. List what you have stored
dotnet user-secrets list --project InstituteWebAPI
```

User secrets live at:
- **Windows:** `%APPDATA%\Microsoft\UserSecrets\<userSecretsId>\secrets.json`
- **Linux/macOS:** `~/.microsoft/usersecrets/<userSecretsId>/secrets.json`

---

## Alternative: `appsettings.Local.json`

Create `appsettings.Local.json` next to `appsettings.json`.  It is gitignored
and is loaded last so it overrides everything:

```json
{
  "Jwt": {
    "Key": "YOUR-STRONG-RANDOM-SECRET-64-CHARS-OR-MORE"
  },
  "ConnectionStrings": {
    "RozhnWebConnectionString": "..."
  }
}
```

Register it in `Program.cs` if not already done:

```csharp
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);
```

---

## Production — environment variables

The ASP.NET Core `IConfiguration` system maps environment variables to config
keys using `__` as the delimiter:

```
Jwt__Key=<64-char-secret>
ConnectionStrings__RozhnWebConnectionString=Server=...
ConnectionStrings__RozhnWebAuthConnectionString=Server=...
Jwt__Issuer=https://your-api-domain.com
Jwt__Audience=https://your-api-domain.com
Cors__AllowedOrigins__0=https://your-ui-domain.com
ASPNETCORE_ENVIRONMENT=Production
```

Set these in your hosting environment (IIS app settings, Windows Service env
block, Docker `--env`, Azure App Service application settings, etc.).

---

## Generating a secure JWT key

```bash
# PowerShell
[Convert]::ToBase64String([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(64))

# Bash / OpenSSL
openssl rand -base64 64
```

---

## Key rotation

Rotating the JWT key logs **all users out** (their tokens are immediately
invalid).  Steps:

1. Generate a new key using the command above.
2. Set the new key in production config **before** restarting the app.
3. Restart the app — existing tokens are rejected, users re-login.
4. Communicate the maintenance window to users if needed.

---

## Production deployment checklist

Run through this list before going live. Every item marked ❌ must be set.

### API environment variables / host config

| Key | Notes | Status |
|-----|-------|--------|
| `Jwt__Key` | 64-char random secret — see "Generating a secure JWT key" above | ❌ must set |
| `Jwt__Issuer` | Your API URL, e.g. `https://api.yourdomain.com` | ❌ must set |
| `Jwt__Audience` | Your API URL (same value as Issuer) | ❌ must set |
| `Jwt__TokenLifetimeMinutes` | Defaults to 480 (8 h). Adjust for your policy. | optional |
| `ConnectionStrings__RozhnWebConnectionString` | Production SQL Server connection string | ❌ must set |
| `ConnectionStrings__RozhnWebAuthConnectionString` | Production auth DB connection string | ❌ must set |
| `SeedAdmin__Email` | First admin account email for fresh deployments | ❌ must set |
| `SeedAdmin__Password` | First admin password — min 8 chars, at least 1 digit | ❌ must set |
| `Cors__AllowedOrigins__0` | Your production frontend URL, e.g. `https://app.yourdomain.com` | ❌ must set |
| `FeeManagement__LateFeeAmount` | Late fee amount in your currency | ❌ must set |
| `FeeManagement__AdmissionFeeAmount` | Admission fee amount | ❌ must set |
| `FeeManagement__CardFeeAmount` | ID card fee amount | ❌ must set |
| `AllowedHosts` | Lock down to your domain, e.g. `api.yourdomain.com` | recommended |
| `ASPNETCORE_ENVIRONMENT` | Set to `Production` | ❌ must set |

### UI — before running `vite build`

| File / Variable | Notes | Status |
|-----------------|-------|--------|
| `.env.production` → `VITE_API_URL` | Set to your real API URL | ❌ must set |

### First-startup checklist

- [ ] Run the app once — both DBs migrate automatically and the seed admin is created.
- [ ] Log in with your `SeedAdmin` credentials.
- [ ] **Immediately change the admin password** from Account settings.
- [ ] Remove or blank out `SeedAdmin__Email` and `SeedAdmin__Password` from host config (the seeder is idempotent — removing them just skips the seed step on future restarts).
- [ ] Verify Swagger is NOT accessible (`/swagger` should return 404 in Production mode).
