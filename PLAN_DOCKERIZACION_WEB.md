# Plan: Dockerizar la aplicación web (API + Worker + React + BD) con subdominios por componente

Complementa `ARQUITECTURA_WEB_REACT_DOTNET8.md`. Alcance: la **web central** de `Applicacionweb/`
(no los servicios locales de estación: `SigesServicio`, `EnviadorInformacionService`, etc.).

---

## 1. Estado actual (lo que hay que contenerizar)

| Componente | Ruta | Tecnología | Config hoy |
|---|---|---|---|
| API central | `Applicacionweb/FacturacionelectronicaCore/FacturacionelectronicaCore.Web/FacturacionelectronicaCore.Web` | ASP.NET Core **net8.0**, `Startup.cs`, Swagger, JWT | `appsettings.json` |
| Worker | `Applicacionweb/FacturacionelectronicaCore/FacturacionelectronicaCore.Worker` | .NET 8 `BackgroundService`, Serilog a archivo | `appsettings.json` |
| Frontend | `Applicacionweb/EstacionesWeb` | React 18 (CRA / `react-scripts`, CoreUI) | `public/config.js` (`SERVER_URL`, `FEATURE_FLAGS`) cargado en runtime |
| BD relacional | `Applicacionweb/FacturacionelectronicaCore/FacturacionelectronicaCore.Database` | SQL Server (`.sqlproj`, 63 scripts) | — |
| BD documental | usada por `Repositorio/Mongodb` | MongoDB (la base se llama según `RepositorioConfig.Cliente`) | `ConnectionStrings:MongoDB` |

Puntos que facilitan la dockerización:
- Todo el backend ya está en .NET 8 y no hay dependencias exclusivas de Windows (sin rutas `C:\`, `System.Drawing`, `EventLog`, `UseWindowsService`).
- `Host.CreateDefaultBuilder` ya lee variables de entorno, así que cualquier clave de `appsettings` se sobreescribe con `Seccion__Clave` (ej. `ConnectionStrings__ConnStr`, `Alegra__Token`) **sin tocar código**.
- El frontend ya lee su configuración en runtime desde `config.js`, así que **una sola imagen** sirve a todos los clientes.
- El despliegue ya es **un stack por cliente** (`brisasdelrio`, `cootranshuila`, `Castilla`, …) con subdominio tipo `cootranshuila.api.sigesmas.com`.

Riesgos encontrados (se atienden en la Fase 0):
- **Secretos versionados en git**: `appsettings.json` de Web y Worker (y otros proyectos) tienen contraseñas de SQL, credenciales/tokens de Dataico y el secreto JWT. Hay que sacarlos del repo y **rotarlos**.
- CORS abierto: `AllowAnyOrigin()` en `Startup.cs:135`.
- Hay claves duplicadas en la sección `Alegra` (`EnvioDirecto`, `EnviaCreditos`).
- El Worker escribe logs en `Logs/` dentro del contenedor, así que necesita un volumen.

---

## 2. Arquitectura objetivo

```
                    Internet (80/443)
                          │
                ┌─────────▼─────────┐   TLS automático (Let's Encrypt)
                │   Caddy (proxy)   │   enruta por subdominio
                └──┬──────┬──────┬──┘
   {cliente}.sigesmas.com │      │ {cliente}.api.sigesmas.com
          ┌────────▼──┐ ┌─▼──────────┐  ┌────────────┐
          │ web (nginx│ │ api (.NET8)│  │ worker     │   ← un stack por cliente
          │ + build)  │ └─────┬──────┘  └─────┬──────┘
          └───────────┘       │               │
                    red interna "datos" ──────┤
                    ┌─────────▼───┐   ┌───────▼─────┐
                    │ SQL Server  │   │  MongoDB    │   ← compartidos en el host,
                    │ 1 BD/cliente│   │ 1 BD/cliente│     1 base por cliente
                    └─────────────┘   └─────────────┘
```

Decisiones (recomendadas):
1. **Proxy: Caddy.** Su configuración es la más corta y obtiene y renueva los certificados HTTPS solo. Agregar un subdominio es agregar 3 líneas (o nada, ver Fase 4).
2. **Infra compartida + stacks por cliente.** Un `docker compose` de infraestructura (Caddy, SQL Server, Mongo) y un `docker compose` por cliente (web, api, worker) que se conecta a las redes externas `proxy` y `datos`. Así un cliente se levanta o baja sin tocar a los demás.
3. **DNS comodín.** Registros `*.sigesmas.com` y `*.api.sigesmas.com` apuntando al servidor, para que un cliente nuevo no requiera cambios de DNS.
4. **Una sola fuente de configuración por cliente:** `clientes/<cliente>.env`. No se editan `appsettings.json` ni `config.js` en el servidor.
5. **Imágenes únicas** (`facturador-api`, `facturador-worker`, `facturador-web`) versionadas por tag y reutilizadas por todos los clientes.

---

## 3. Estructura de archivos a crear

```
deploy/
├── infra/
│   ├── docker-compose.yml        # caddy, sqlserver, mongo
│   ├── Caddyfile                 # importa sites/*.caddy
│   ├── sites/                    # un archivo por cliente (generado)
│   └── .env.example              # SA_PASSWORD, MONGO_ROOT_*, ACME_EMAIL
├── cliente/
│   ├── docker-compose.yml        # web, api, worker, db-migrate (plantilla única)
│   └── cliente.env.example       # plantilla comentada para llenar
├── clientes/                     # <cliente>.env reales  ← en .gitignore
├── scripts/
│   ├── nuevo-cliente.ps1 / .sh   # asistente: pregunta datos y genera .env + sitio Caddy
│   ├── validar-env.ps1 / .sh     # verifica que no falte ninguna clave obligatoria
│   ├── desplegar.ps1 / .sh       # up/down/logs/update de un cliente
│   └── backup.sh                 # backups SQL + Mongo
└── README.md                     # guía de despliegue paso a paso
Applicacionweb/FacturacionelectronicaCore/Dockerfile.api
Applicacionweb/FacturacionelectronicaCore/Dockerfile.worker
Applicacionweb/EstacionesWeb/Dockerfile
Applicacionweb/EstacionesWeb/docker/entrypoint.sh   # genera config.js desde variables
```

---

## 4. Configuración fácil de llenar

### 4.1 `cliente.env.example` (lo único que se llena por cliente)

> Versión final implementada: [`deploy/cliente/cliente.env.example`](deploy/cliente/cliente.env.example). El bloque de abajo es el boceto inicial.

```dotenv
# ===================== IDENTIDAD =====================
CLIENTE=cootranshuila                 # minúsculas, sin espacios. Nombra BD, Mongo y contenedores
DOMINIO_BASE=sigesmas.com             # web: https://<CLIENTE>.<DOMINIO_BASE>
                                      # api: https://<CLIENTE>.api.<DOMINIO_BASE>
VERSION=1.0.0                         # tag de imágenes a usar

# ===================== BASE DE DATOS =================
SQL_DATABASE=cootranshuila
SQL_USER=app_cootranshuila
SQL_PASSWORD=                         # obligatorio
# Si la BD NO está en el contenedor compartido, poner el host externo:
SQL_HOST=sqlserver                    # ej. 198.38.85.254,1433

# ===================== SEGURIDAD =====================
JWT_SECRET=                           # obligatorio, mínimo 32 caracteres (el script lo genera)

# ===================== FACTURACIÓN ELECTRÓNICA =======
FE_PROVEEDOR=DATATICO                 # DATATICO | SIIGO | ALEGRA
FE_URL=https://api.dataico.com/direct/dataico_api/v2/
FE_TOKEN=
FE_ACCOUNT_ID=
FE_RESOLUCION=
FE_PREFIJO=
FE_ENVIO_DIRECTO=true
FE_ENVIA_CREDITOS=false
FE_MULTIPLICAR_POR_DIEZ=false
# Códigos de producto
FE_COD_CORRIENTE=
FE_COD_ACPM=
FE_COD_EXTRA=
FE_COD_GAS=

# ===================== FRONTEND ======================
FEATURE_USUARIOS_CLIENTES=true

# ===================== WORKER ========================
WORKER_HABILITADO=true
```

`cliente/docker-compose.yml` traduce estas variables a las claves de .NET para no exponer la
estructura interna de `appsettings`:

```yaml
api:
  environment:
    ConnectionStrings__ConnStr: "Server=${SQL_HOST};Database=${SQL_DATABASE};User Id=${SQL_USER};Password=${SQL_PASSWORD};TrustServerCertificate=True"
    ConnectionStrings__MongoDB: "mongodb://${MONGO_USER}:${MONGO_PASSWORD}@mongo:27017"
    AppSettings__Secret: ${JWT_SECRET}
    RepositorioConfig__Cliente: ${CLIENTE}
    Alegra__Proveedor: ${FE_PROVEEDOR}
    Alegra__Token: ${FE_TOKEN}
    # ...
    Cors__Origenes: "https://${CLIENTE}.${DOMINIO_BASE}"
```

### 4.2 Asistente `nuevo-cliente`
1. Pregunta, con valores por defecto: nombre del cliente, proveedor de FE (y muestra solo los campos de ese proveedor), resolución, prefijo y códigos.
2. Genera solo `JWT_SECRET` y `SQL_PASSWORD`.
3. Escribe `clientes/<cliente>.env` y `infra/sites/<cliente>.caddy`.
4. Corre `validar-env` y muestra el resumen con las URLs que tendrá el cliente.

### 4.3 Validación al arranque
- **Script** `validar-env`: falla si falta una clave obligatoria o tiene formato inválido.
- **API/Worker**: agregar validación de opciones (`ValidateOnStart`) para que el contenedor no arranque con un mensaje claro ("Falta Alegra:Token") en lugar de fallar en la primera factura.

---

## 5. Fases

### Fase 0: Higiene de secretos y config (requisito previo, ~1 día)
- [ ] Reemplazar valores sensibles de los `appsettings.json` versionados por placeholders vacíos; crear `appsettings.example.json`.
- [ ] Agregar `deploy/clientes/`, `*.env` (excepto `*.example`) y `appsettings.*.local.json` a `.gitignore`.
- [ ] **Rotar** contraseñas SQL, tokens/contraseñas de Dataico y secreto JWT expuestos en el historial de git.
- [ ] Unificar claves duplicadas de la sección `Alegra`.
- [ ] Inventariar todas las claves que usan API y Worker (`Alegra`, `Syscafe`, `AppSettings`, `RepositorioConfig`, `Serilog`) → tabla en `deploy/README.md`.

### Fase 1: Imágenes Docker (~2 días)
- [ ] `Dockerfile.api`: multi-stage `mcr.microsoft.com/dotnet/sdk:8.0` → `aspnet:8.0`, usuario no root, `ASPNETCORE_HTTP_PORTS=8080`, `HEALTHCHECK`.
- [ ] `Dockerfile.worker`: multi-stage a `runtime:8.0`; logs a consola (Serilog ya la tiene) y volumen opcional `/app/Logs`.
- [ ] `EstacionesWeb/Dockerfile`: `node:20` hace `npm ci && npm run build` → `nginx:alpine` sirve `build/` con fallback SPA (`try_files $uri /index.html`) y caché larga para `static/`.
- [ ] `entrypoint.sh` del web: genera `config.js` con `SERVER_URL=https://${CLIENTE}.api.${DOMINIO_BASE}` y `FEATURE_FLAGS` antes de arrancar nginx, y `config.js` sin caché.
- [ ] Agregar endpoint `/health` en la API (SQL + Mongo) para healthchecks del proxy y de compose.
- [ ] `.dockerignore` (excluir `bin/`, `obj/`, `node_modules/`, `build/`, `*.zip`, `Logs/`, PDFs).
- [ ] Revisar si `@ironsoftware/ironpdf` es necesario en el frontend; es pesado y pensado para Node, no para el navegador.

### Fase 2: Bases de datos en contenedores (~2–3 días)
- [ ] `infra/docker-compose.yml`:
  - `sqlserver`: `mcr.microsoft.com/mssql/server:2022-latest`, `MSSQL_PID=Express` (gratis, límite de 10 GB por base; usar `Standard` si se requiere más), volumen `sqldata`, sin puerto público.
  - `mongo`: `mongo:7`, usuario root por variable, volumen `mongodata`, sin puerto público.
  - Red `datos` (interna) y red `proxy`.
- [ ] Esquema reproducible: `.dacpac` extraído de una base de producción al día (el `.sqlproj` está incompleto, ver sección 7).
- [ ] Servicio `db-migrate` (one-shot) en el stack del cliente: `sqlpackage /Action:Publish` del `.dacpac` contra `SQL_DATABASE`. Corre antes de `api`/`worker` (`depends_on: condition: service_completed_successfully`).
- [ ] Script para crear el login/usuario SQL del cliente con permisos solo sobre su base (no usar `sa` en la app).
- [ ] Migración de datos existentes: `BACKUP DATABASE` en el servidor actual → copiar `.bak` al volumen → `RESTORE` en el contenedor. Documentar por cliente.
- [ ] `backup.sh`: backup diario SQL (`BACKUP DATABASE`) + `mongodump`, retención configurable, copia fuera del servidor.
- [ ] Modo "BD externa": si `SQL_HOST` apunta fuera, no se usa el contenedor SQL (para clientes que mantengan su servidor actual).

### Fase 3: Stack por cliente (~1–2 días)
- [ ] `cliente/docker-compose.yml` con `web`, `api`, `worker` (`profiles: [worker]` para activarlo con `WORKER_HABILITADO`), `db-migrate`.
- [ ] Nombres por proyecto: `docker compose -p <cliente> --env-file clientes/<cliente>.env -f cliente/docker-compose.yml up -d`, para que cada cliente quede aislado sin duplicar el YAML.
- [ ] `restart: unless-stopped`, límites de memoria y healthchecks.
- [ ] CORS: reemplazar `AllowAnyOrigin()` por la lista en `Cors:Origenes`.
- [ ] `ForwardedHeaders` en la API (está detrás de proxy TLS).

### Fase 4: Proxy y subdominios (~1 día)
- [ ] `infra/Caddyfile`:
  ```caddy
  { email {$ACME_EMAIL} }
  import sites/*.caddy
  ```
- [ ] Sitio por cliente generado por el asistente (`sites/cootranshuila.caddy`):
  ```caddy
  cootranshuila.sigesmas.com      { reverse_proxy cootranshuila-web-1:80 }
  cootranshuila.api.sigesmas.com  { reverse_proxy cootranshuila-api-1:8080 }
  ```
- [ ] Agregar un subdominio = generar el archivo + `docker exec caddy caddy reload` (sin downtime).
- [ ] DNS comodín `*.sigesmas.com` y `*.api.sigesmas.com` → IP del servidor.
- [ ] Opcional: `admin.sigesmas.com` para RabbitMQ/monitoreo, protegido con `basic_auth`.
- [ ] Mantener compatibilidad con URLs actuales (`cootranshuila.api.sigesmas.com`) para no reconfigurar los servicios de estación que ya apuntan ahí.

### Fase 5: Operación y despliegue continuo (~2 días)
- [ ] GitHub Actions: build y push de las 3 imágenes a GHCR con tag `vX.Y.Z` al crear un tag de git.
- [ ] `desplegar update <cliente> <version>`: cambia `VERSION`, `pull`, `up -d`; rollback = volver al tag anterior.
- [ ] Logs centralizados: `docker compose logs`, con rotación del driver `json-file` (`max-size`, `max-file`).
- [ ] Monitoreo básico: Uptime Kuma (en su propio subdominio) contra `/health` de cada cliente.

### Fase 6: Piloto y migración de clientes (~1 semana)
- [ ] Piloto con un cliente de bajo volumen en servidor de pruebas (`*.staging.sigesmas.com`).
- [ ] Validar flujo completo: estación → API → factura Dataico → visualización en web.
- [ ] Migrar clientes uno por uno: congelar, backup, restore, cambiar el DNS si no es comodín, verificar, desmontar el anterior.
- [ ] Actualizar `GUIA_DESPLIEGUE_WEB_LOCAL.md` y `ARQUITECTURA_WEB_REACT_DOTNET8.md` (DoD punto 5).

---

## 6. Flujo final para agregar un cliente

```bash
./deploy/scripts/nuevo-cliente.sh            # preguntas → clientes/acme.env + sites/acme.caddy
./deploy/scripts/desplegar.sh up acme        # migra BD, levanta web/api/worker
docker exec caddy caddy reload               # https://acme.sigesmas.com listo
```

---

## 7. Decisiones tomadas (2026-10-20)

| Tema | Decisión |
|---|---|
| Servidor | **Linux con Docker Engine** |
| SQL Server | **En contenedor** compartido, 1 base y 1 login por cliente (Express por defecto). `SQL_HOST` externo queda como opción. |
| Worker | **Uno por cliente** (perfil `worker`, `WORKER_HABILITADO=true`) |
| Proxy | Caddy |
| RabbitMQ / tiempo real | Fuera de este plan; agregar como servicio de infra en la Fase 3 de la arquitectura |

### Hallazgos que cambiaron el diseño
- **El proyecto `.sqlproj` está incompleto**: el código llama ~70 procedimientos y el proyecto define 37;
  ~23 no existen en ningún script del repo (`ListarIslas`, `SaveOrUpdateCliente`, `ObtenerOrdenDespachoPorIdVentaLocal`…).
  Por eso el esquema de referencia es un `deploy/db/schema.dacpac` **extraído de una base de producción**
  (`scripts/extraer-esquema.sh`) en lugar de compilar el `.sqlproj`.
- **Mongo usa `RepositorioConfig:Cliente` como nombre de base** y distingue mayúsculas (`Castilla` ≠ `castilla`),
  así que el `.env` separa `CLIENTE` (subdominio) de `CLIENTE_DATOS` (nombre en los datos).
- **Las rutas del API son `api/[controller]`** pero el frontend arma `${SERVER_URL}/Recurso`, por lo que en
  Docker `SERVER_URL` termina en `/api` (configurable con `API_URL`).

## 7.1 Estado de implementación

- [x] Fase 0 (parcial): secretos fuera de los `appsettings*.json` versionados de Web y Worker; valores movidos
      a `appsettings.Local.json` / `appsettings.Development.Local.json` (ignorados por git) que el API y el Worker
      cargan antes de las variables de entorno. **Pendiente (usuario): rotar las credenciales expuestas** y limpiar
      los `appsettings.json` de los demás proyectos (`FacturadorAPI`, `SigesServicio`, `ControladorEstacion`, `_publish`…).
- [x] Fase 1: `Dockerfile.api`, `Dockerfile.worker`, `EstacionesWeb/Dockerfile`, `/health`, CORS configurable,
      `ForwardedHeaders`, `config.js` generado al arrancar.
- [x] Fase 2: SQL Server y Mongo en `deploy/infra`, `db-migrate` con `sqlpackage`, provisión de base/usuarios,
      backup y restauración. **Pendiente: extraer `schema.dacpac` de producción.**
- [x] Fase 3–4: plantilla de stack por cliente, Caddy con un archivo por cliente, asistente `nuevo-cliente.sh`.
- [ ] Fase 5: CI (GitHub Actions → GHCR) y monitoreo.
- [ ] Fase 6: piloto en servidor real y migración de clientes.

## 8. Criterios de listo
1. `docker compose up` levanta un cliente completo desde cero (BD vacía + esquema) en un servidor limpio.
2. Ningún secreto en git; todo sale de `clientes/<cliente>.env`.
3. Agregar un cliente nuevo toma menos de 15 minutos con el asistente.
4. HTTPS válido en `<cliente>.sigesmas.com` y `<cliente>.api.sigesmas.com`.
5. Backups automáticos probados con una restauración real.
