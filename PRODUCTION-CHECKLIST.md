# Production Readiness Checklist — Rozhn Institute API

## Must do before going live

1. **Rotate the JWT key.** The current `Jwt:Key` in `appsettings.json` is committed to source
   control and must be considered compromised. Generate a new random 64+ character secret and set
   it only in production config (host config UI or the `Jwt__Key` environment variable) — never
   commit the real value. Rotating it logs everyone out (expected).

2. **Set production connection strings** for both `RozhnWebConnectionString` and
   `RozhnWebAuthConnectionString` in the host's config (or `ConnectionStrings__RozhnWebConnectionString`
   env var). Do not leave the localdb values.

3. **Set `Jwt:Issuer` / `Jwt:Audience`** to your real API domain, and **`Cors:AllowedOrigins`**
   to your real UI domain (https). Remove localhost origins.

4. **Set the environment to Production** (`ASPNETCORE_ENVIRONMENT=Production`) so Swagger is off and
   the global exception handler + HSTS are active.

5. **HTTPS only** — the host should terminate TLS; `UseHttpsRedirection` and `UseHsts` are enabled
   in Production.

## Already handled in code

- Global exception handler returns a clean JSON 500 and logs the error (Production only).
- HSTS enabled in Production.
- App fails fast at startup if `Jwt:Key` is missing or shorter than 32 chars.
- All admin controllers now require `[Authorize(Roles = "Admin")]`.
- EF migrations auto-apply on startup.
- Model-validation errors return a structured `{ message, errors[] }` response.
- Intentionally anonymous endpoints: `POST /api/TeacherDailyAttendance/checkin` (kiosk scanner)
  and `GET /api/TestTypes` (public dropdown).

## Recommended (not blocking)

- Consider a stronger Identity password policy for production (currently 6 chars, no complexity).
- Take a database backup before the first deploy and before running the Import "Reset".
- Point the React app at the live API via the `VITE_API_URL` build variable.
- Frontend: run `npm run build` and deploy the `dist/` output; the app already reads the API URL
  from `VITE_API_URL` and handles 401 by redirecting to login.
