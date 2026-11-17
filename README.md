# Air Quality API

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
| `GET /openapi/v1.json` | public | OpenAPI document |

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
export Database__MigrateOnStartup=true                     # creates the alert_rules table
dotnet run --project src/AirQuality.Api --urls http://localhost:5000
curl -H "X-Api-Key: readerkey" http://localhost:5000/api/cities
```

`Database__MigrateOnStartup=true` only creates `alert_rules`: the ETL tables are never modified.

## Tests

```bash
dotnet test
```

- `AirQuality.UnitTests`: business rules and validation, no database.
- `AirQuality.IntegrationTests`: the whole API against a real PostgreSQL 17 started by Testcontainers (Docker required), with a known dataset. Set `TEST_POSTGRES_IMAGE` to use another image (default `postgres:17`).

The GitHub Actions CI runs both on every push and pull request.
