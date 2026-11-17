-- Star schema written by the Airflow ETL (air-quality-etl, sql/schema.sql, schema "public").
-- Copied here so the API tests and a local database can start without Airflow.
-- In production the ETL owns these tables: the API never migrates them.

CREATE TABLE IF NOT EXISTS dim_city (
    city_key    INTEGER PRIMARY KEY,
    city_name   VARCHAR(100) NOT NULL,
    country     VARCHAR(100),
    latitude    DOUBLE PRECISION,
    longitude   DOUBLE PRECISION
);

CREATE TABLE IF NOT EXISTS dim_date (
    date_key    INTEGER PRIMARY KEY,
    full_date   DATE NOT NULL,
    hour        INTEGER NOT NULL,
    day_of_week VARCHAR(10),
    is_weekend  BOOLEAN,
    month       INTEGER,
    year        INTEGER
);

CREATE TABLE IF NOT EXISTS fact_aqi (
    fact_id     SERIAL PRIMARY KEY,
    city_key    INTEGER NOT NULL REFERENCES dim_city (city_key),
    date_key    INTEGER NOT NULL REFERENCES dim_date (date_key),
    aqi         INTEGER,
    co          DOUBLE PRECISION,
    no          DOUBLE PRECISION,
    no2         DOUBLE PRECISION,
    o3          DOUBLE PRECISION,
    so2         DOUBLE PRECISION,
    pm2_5       DOUBLE PRECISION,
    pm10        DOUBLE PRECISION,
    nh3         DOUBLE PRECISION
);

CREATE UNIQUE INDEX IF NOT EXISTS uq_fact_aqi_city_date ON fact_aqi (city_key, date_key);
