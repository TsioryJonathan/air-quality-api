-- Known dataset: 2 cities, 2026-11-01 (3 hours) and 2026-11-02 (1 hour) for Antananarivo (key 1).
INSERT INTO dim_city VALUES (1, 'Antananarivo', 'MG', -18.8792, 47.5079), (2, 'Toamasina', 'MG', -18.1443, 49.3958);
INSERT INTO dim_date (date_key, full_date, hour) VALUES
  (2026110100, '2026-11-01', 0), (2026110101, '2026-11-01', 1), (2026110102, '2026-11-01', 2), (2026110200, '2026-11-02', 0);
INSERT INTO fact_aqi (city_key, date_key, aqi, pm2_5, pm10) VALUES
  (1, 2026110100, 2, 10, 20),
  (1, 2026110101, 4, 60, 80),
  (1, 2026110102, 3, 30, 40),
  (1, 2026110200, 5, 90, 120),
  (2, 2026110100, 1,  5, 10);
