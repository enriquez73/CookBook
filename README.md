# Ricette Cuociriso

App per raccogliere ricette da preparare con il cuociriso Bear (4 tazze di riso crudo, vaporiera), con foto e video.

- **Backend**: ASP.NET Core Web API (`src/`, Api / Application / Domain / Infrastructure), EF Core.
  PostgreSQL in sviluppo e per il deploy (`Database:Provider = Postgres`). In locale, `appsettings.Development.json` punta al PostgreSQL del Raspberry (`192.168.1.11:5432`); credenziali e database si configurano nel file o via variabili d'ambiente.
- **Frontend**: Angular (`frontend/`), interfaccia in italiano.
- **Media**: foto e video caricati salvati in `App_Data/media` (`Storage:MediaPath`) e serviti su `/media/...`; le immagini aggiunte tramite URL vengono scaricate in `App_Data/media/images`. I video possono anche essere link esterni (YouTube/Vimeo).

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
- `GET /api/recipes?search=&cookingAppliance=&cookerFunction=&tag=&servings=&maxMinutes=`, `POST /api/recipes`, `GET/PUT/DELETE /api/recipes/{id}`
- `POST /api/recipes/{id}/media` (multipart `file`, `caption`): foto JPEG/PNG/WebP/GIF (max 15 MB), video MP4/WebM/MOV (max 300 MB)
- `POST /api/recipes/{id}/media/image-url` (`{ url, caption }`): scarica immagini JPEG/PNG/WebP/GIF (max 15 MB) da URL HTTP(S) pubblico e le salva in `App_Data/media/images`
- `POST /api/recipes/{id}/steps/{stepId}/image` (multipart `file`), `POST /api/recipes/{id}/steps/{stepId}/image-url` (`{ url }`) e `DELETE /api/recipes/{id}/steps/{stepId}/image`: carica, scarica o rimuove l'immagine associata al singolo passaggio; le risposte dettaglio espongono `stepDetails` con ID e `imageUrl`.
- `POST /api/recipes/{id}/media/video-url` (`{ url, caption }`), `DELETE /api/recipes/{id}/media/{mediaId}`
- `GET /api/tags`, `GET /api/cooker-functions`, `GET /api/cooking-appliances`, `GET /health`

Per eseguire un comando EF Core usando le credenziali del `.env` (il CLI `dotnet ef` non carica `.env` automaticamente):
```bash
. ./.env
ConnectionStrings__Default="Host=192.168.1.11;Port=5432;Database=cookbook_dev;Username=cookbook_app;Password=${DB_PASSWORD}" \
  dotnet ef database update --project src/Cookbook.Infrastructure/Cookbook.Infrastructure.csproj \
  --startup-project src/Cookbook.Api/Cookbook.Api.csproj --context CookbookDbContext --configuration Debug
```

Attrezzature: `RiceCooker`, `AirFryer`, `Stovetop` (fornello con pentola/padella). Una ricetta può indicarne più di una nel campo `cookingAppliances`; `cookerFunction` è richiesta solo quando la lista include `RiceCooker`. Le ricette possono servire fino a 100 persone; `riceCups` indica le tazze di riso e può essere zero per ricette senza riso. Funzioni: riso bianco, integrale, porridge, zuppa, vapore, dolce, mantenimento caldo, altro.

## Test
```bash
dotnet test
```
I test di integrazione usano un database SQLite temporaneo. Con un runtime più recente di .NET 9 usa `DOTNET_ROLL_FORWARD=Major dotnet test`.

## Deploy automatico (Raspberry)
Il workflow `.github/workflows/deploy-raspi.yml` si avvia a ogni push su `master` (oppure manualmente da GitHub Actions). GitHub costruisce le immagini ARM64 dell'API e del frontend, le pubblica su GHCR con un tag legato al commit e poi chiede al runner sul Raspberry di scaricarle e riavviare Compose.

### Configurazione iniziale del Raspberry
1. In GitHub aprire **Settings → Actions → Runners → New self-hosted runner**, scegliere Linux e ARM64 e seguire i comandi mostrati per scaricare e configurare il runner. Registrarlo con l'etichetta aggiuntiva `cookbook-prod`.
2. Sul Raspberry configurare il runner come servizio (`sudo ./svc.sh install enrico` e `sudo ./svc.sh start`) e assicurarsi che l'utente `enrico` possa eseguire Docker (`docker ps` senza `sudo`). Il runner deve restare avviato.
3. Lasciare il file segreto `/home/enrico/docker/cookbook/docker/.env` sul Raspberry. Il Compose usa per default i media esistenti in `/home/enrico/docker/cookbook/src/Cookbook.Api/App_Data/media`; si può impostare `COOKBOOK_MEDIA_PATH` nel `.env` per scegliere un percorso persistente diverso.
4. Dopo la prima pubblicazione, aprire le impostazioni di ciascun pacchetto GHCR e verificare in **Manage Actions access** che il repository `CookBook` abbia accesso. Se manca, aggiungerlo con ruolo **Read**. La visibilità dei pacchetti può restare privata.
5. Dopo il primo push su `master`, verificare in **Actions** che terminino sia il job di pubblicazione immagini sia il deploy sul runner `cookbook-prod`.

Il runner effettua solo connessioni in uscita a GitHub, quindi non serve esporre SSH su Internet. Limitare i push su `master` a utenti fidati: un workflow su quel branch può eseguire comandi sul Raspberry con i permessi dell'utente `enrico`. Le immagini sono versionate con lo SHA del commit e il workflow aggiorna Compose solo dopo che entrambe sono state pubblicate.
