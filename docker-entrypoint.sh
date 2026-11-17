#!/bin/sh
set -eu

# Render/Railway provide PORT; local containers default to 8080.
exec dotnet /app/AirQuality.Api.dll --urls "http://0.0.0.0:${PORT:-8080}" "$@"
