# Ricette Cuociriso

App per raccogliere ricette da preparare con il cuociriso Bear (4 tazze di riso crudo, vaporiera), con foto e video.

- **Backend**: ASP.NET Core Web API (`src/`, Api / Application / Domain / Infrastructure), EF Core.
  SQLite in sviluppo (`App_Data/cookbook.db`), PostgreSQL per il deploy su Raspberry (`Database:Provider = Postgres`).
- **Frontend**: Angular (`frontend/`), interfaccia in italiano.
- **Media**: foto e video caricati salvati in `App_Data/media` (`Storage:MediaPath`) e serviti su `/media/...`; i video possono anche essere link esterni (YouTube/Vimeo).

## Requisiti
.NET SDK 10+ (target net10.0) e Node.js LTS.

## Avvio in locale
```bash
# 1) API (http://localhost:5000, https://localhost:5001 col profilo "https"; Development, SQLite, schema creato al primo avvio)
dotnet run --project src/Cookbook.Api --launch-profile http

# 2) Frontend (http://localhost:4200, proxy verso l'API per /api e /media)
cd frontend && npm install && npm start

# 3) App completa su http://localhost:5000: la build Angular finisce in src/Cookbook.Api/wwwroot
#    ed è servita dall'API (fallback SPA; /api, /media, /health e la documentazione restano raggiungibili)
cd frontend && npx ng build
```

## API
- Documentazione (solo Development): Swagger UI su `/swagger`, Redoc su `/redoc`, documento OpenAPI su `/openapi/v1.json` (le pagine caricano gli asset da CDN).
- `GET /api/recipes?search=&cookerFunction=&tag=&servings=&maxMinutes=`, `POST /api/recipes`, `GET/PUT/DELETE /api/recipes/{id}`
- `POST /api/recipes/{id}/media` (multipart `file`, `caption`): foto JPEG/PNG/WebP/GIF (max 15 MB), video MP4/WebM/MOV (max 300 MB)
- `POST /api/recipes/{id}/media/video-url` (`{ url, caption }`), `DELETE /api/recipes/{id}/media/{mediaId}`
- `GET /api/tags`, `GET /api/cooker-functions`, `GET /health`

Una ricetta ha max 4 persone / 4 tazze di riso crudo. Funzioni: riso bianco, integrale, porridge, zuppa, vapore, dolce, mantenimento caldo, altro.

## Test
```bash
dotnet test
```
I test di integrazione usano un database SQLite temporaneo. Con un runtime più recente di .NET 9 usa `DOTNET_ROLL_FORWARD=Major dotnet test`.

## Deploy (Raspberry)
`docker/docker-compose.raspi.yml` imposta `ConnectionStrings__Default`; `Database:Provider=Postgres` è il default in `appsettings.json`.
Lo schema è creato con `EnsureCreated` (nessuna migrazione EF per ora). Il Dockerfile compila anche il frontend e l'API serve tutto sulla porta 8080 (mappata su `5000` in `compose.yaml`). Montare un volume su `App_Data/media` per conservare i file caricati.
