# Air Quality API

[![CI](https://github.com/TsioryJonathan/air-quality-api/actions/workflows/ci.yml/badge.svg)](https://github.com/TsioryJonathan/air-quality-api/actions/workflows/ci.yml)

REST API built with ASP.NET Core (.NET 10) that serves air quality measurements for six cities in Madagascar, with threshold-based alert rules.

It reads the PostgreSQL database filled by [air-quality-etl](https://github.com/0nlyDevs/air-quality-etl) (Airflow, star schema). A Grafana dashboard is connected to the same database: <https://grafana-rxj7.onrender.com/>.

## Endpoints

| Route | Role | Description |
|---|---|---|
| `GET /api/cities` | reader | Cities |
| `GET /api/cities/{id}/aqi/latest` | reader | Latest measurement |
| `GET /api/cities/{id}/aqi?from=&to=` | reader | Hourly measurements (31 days max) |
| `GET /api/cities/{id}/aqi?from=&to=&granularity=Daily` | reader | Daily summary: average and max AQI, max PM2.5 (366 days max) |
| `GET /api/alerts` | reader | Rules exceeded by each city's latest measurement |
| `GET /api/alert-rules` | reader | Alert rules |
| `POST /api/alert-rules` | admin | Create a rule `{ cityId, pollutant, threshold }` |
| `DELETE /api/alert-rules/{id}` | admin | Delete a rule |
| `GET /health` | public | Health check |
| `GET /openapi/v1.json`, `/openapi/v1.yaml`, `/scalar` | public | API documentation |

## API documentation

- Interactive reference (try the routes, enter your key): `/scalar` when the API runs
- OpenAPI document: `/openapi/v1.json` and `/openapi/v1.yaml`
- Committed copy: [`docs/openapi.yaml`](docs/openapi.yaml). A test fails if it no longer matches the code; to regenerate it after changing an endpoint, run `UPDATE_OPENAPI=1 dotnet test --filter Committed` and commit the result.

Authentication: `X-Api-Key` header. Keys and their roles come from configuration (`ApiKeys:<key>=reader|admin`), never from code.

## Architecture

Clean architecture with four projects (dependencies point toward the domain):

```
Api  →  Application  →  Domain
 └────→  Infrastructure ─→ Application
```

- `Domain`: measurements, alert rule, AQI levels. No dependencies.
- `Application`: use cases and validation. Defines the read and storage interfaces.
- `Infrastructure`: EF Core 10 and PostgreSQL. Reads the ETL tables, creates `alert_rules` through a migration.
- `Api`: endpoints, API key authentication, errors as `ProblemDetails`.

Decisions are recorded in [`docs/adr`](docs/adr).

## Run

Requirements: .NET 10 SDK, a PostgreSQL 17 database with the ETL schema (`db/star-schema.sql` for an empty database).

```bash
export ConnectionStrings__AirQuality="Host=...;Database=air_quality;Username=...;Password=..."
export ApiKeys__readerkey=reader ApiKeys__adminkey=admin   # key = name after "ApiKeys__", role = value
dotnet run --project src/AirQuality.Api --urls http://localhost:5000
curl -H "X-Api-Key: readerkey" http://localhost:5000/api/cities
```

The API never runs database migrations on startup. `Database__MigrateOnStartup` is no longer used. Schema changes must be reviewed and applied separately; do not apply migrations to an existing shared database without reviewing them first. Integration tests migrate only their disposable PostgreSQL container.

## Tests

```bash
dotnet test
```

- `AirQuality.UnitTests`: business rules and validation, no database.
- `AirQuality.IntegrationTests`: the whole API against a real PostgreSQL 17 started by Testcontainers (Docker required), with a known dataset. Set `TEST_POSTGRES_IMAGE` to use another image (default `postgres:17`).

The GitHub Actions CI runs both on every push and pull request.

## Docker

The multi-stage Dockerfile builds with the .NET 10 SDK and runs with the ASP.NET
10 runtime as the image's non-root user. Secrets and development configuration
are excluded from the build context. No PostgreSQL container is provisioned:
the API uses the existing external database configured in `ConnectionStrings__AirQuality`.
Starting the API never runs database migrations.

Copy `.env.example` to `.env` if you do not already have a `.env`, then set the real
connection string and API key variable names. In Production, the development keys
are not loaded. Keep `.env` out of Git. Docker Compose reads it automatically;
`source .env` is only needed when running `dotnet run` directly from your shell.

```bash
docker compose up --build -d
docker compose logs -f api
curl --fail-with-body http://localhost:5251/health
docker compose down
```

`API_PORT` changes the local port (default 5251). The published local port is bound
to loopback. A first Docker build downloads the Microsoft images and NuGet packages
unless they are already cached.

### Render / Railway

Connect the GitHub repository and select Docker deployment with the repository
root as the build context and `Dockerfile` as the Dockerfile. Use its default
entrypoint (no custom build/start command). Set the health check path to `/health`.
The entrypoint listens on `0.0.0.0:$PORT`, falling back to 8080.

Set these environment variables in the hosting dashboard (values without shell quotes):

- `ConnectionStrings__AirQuality`: the existing database's Npgsql connection string.
- `ApiKeys__<your_reader_secret>`: `reader`.
- `ApiKeys__<your_admin_secret>`: `admin` (only if administration is needed).

The image defaults to Production. Do not upload `.env` or set the environment to
Development on the host. Keep API keys on the server; do not embed an admin key in
public frontend code. Confirm `/health` responds before testing authenticated read routes.
