# Cookbook

Baseline backend for a home-hosted cookbook app built with ASP.NET Core and PostgreSQL.

## Current scope

- ASP.NET Core API in `src/Cookbook.Api`
- EF Core + PostgreSQL wiring in `src/Cookbook.Infrastructure`
- Local development configured to reach the Raspberry-hosted PostgreSQL instance at `192.168.1.11`
- Raspberry deployment compose in `docker/docker-compose.raspi.yml`

## Local development

Update the development connection string in `src/Cookbook.Api/appsettings.Development.json` or override it with an environment variable:

```bash
export ConnectionStrings__Default="Host=192.168.1.11;Port=5432;Database=cookbook;Username=cookbook_app;Password=YOUR_PASSWORD"
dotnet run --project src/Cookbook.Api
```

The API exposes:

- `GET /` basic app info
- `GET /health` simple health probe
- `GET /api/recipes` recipe list from PostgreSQL
- `GET /api/recipes/{id}` single recipe with ingredients and steps
- `POST /api/recipes/seed/japanese-curry` inserts the first sample recipe if missing
- `GET /openapi/v1.json` in Development

Quick local seed flow:

```bash
dotnet run --project src/Cookbook.Api
curl -X POST http://localhost:5073/api/recipes/seed/japanese-curry
curl http://localhost:5073/api/recipes
```

## Raspberry deployment

1. Copy `docker/.env.example` to `docker/.env` and set the real password.
2. Make sure the existing `postgres` container is on the external `homelab` network.
3. Deploy from the repo root on the Raspberry:

```bash
cd docker
cp .env.example .env
nano .env
docker compose -f docker-compose.raspi.yml --env-file .env up -d --build
```

The compose file assumes your Caddy setup reads Docker labels and will publish the API at `https://cookbook-api.home.arpa` using internal TLS.

## Validation

```bash
dotnet build Cookbook.sln
dotnet test Cookbook.sln
```

