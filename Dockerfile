FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source

# Cache dependency restore separately from application source changes.
COPY src/AirQuality.Domain/AirQuality.Domain.csproj src/AirQuality.Domain/
COPY src/AirQuality.Application/AirQuality.Application.csproj src/AirQuality.Application/
COPY src/AirQuality.Infrastructure/AirQuality.Infrastructure.csproj src/AirQuality.Infrastructure/
COPY src/AirQuality.Api/AirQuality.Api.csproj src/AirQuality.Api/
RUN dotnet restore src/AirQuality.Api/AirQuality.Api.csproj

COPY src/ src/
RUN dotnet publish src/AirQuality.Api/AirQuality.Api.csproj \
    --configuration Release --no-restore --output /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_ENVIRONMENT=Production
EXPOSE 8080
COPY --from=build /app/publish .
COPY --chmod=755 docker-entrypoint.sh /app/docker-entrypoint.sh
USER $APP_UID
ENTRYPOINT ["/app/docker-entrypoint.sh"]
