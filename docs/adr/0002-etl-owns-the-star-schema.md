# 0002. ETL tables are not managed by migrations

**Status:** accepted

## Context
`dim_city`, `dim_date` and `fact_aqi` are created and filled by the Airflow ETL, which rebuilds `dim_date` on every run. An EF migration touching them would conflict with it.

## Decision
The three tables are mapped read-only with `ExcludeFromMigrations()`. Only `alert_rules`, which belongs to the API, is created by a migration. `db/star-schema.sql` is a copy of the ETL schema, used by the tests and by an empty local database.

## Consequences
- If the ETL changes its schema, the API does not notice at compile time: the integration tests against the copied schema are the safety net and must be kept up to date.
- `date_key` (YYYYMMDDHH) gives the day with `date_key / 100`: the daily aggregation runs in PostgreSQL without joining `dim_date`.
