# Despliegue con Docker — Facturador web

Web central (API + Worker + React + SQL Server + MongoDB) en un servidor Linux con Docker.
Cada cliente es un stack aislado con sus subdominios:

| Componente | URL |
|---|---|
| Web | `https://<cliente>.<dominio>` |
| API | `https://<cliente>.api.<dominio>` (Swagger en `/swagger`, salud en `/health`) |

Plan y decisiones: [`../PLAN_DOCKERIZACION_WEB.md`](../PLAN_DOCKERIZACION_WEB.md).

```
deploy/
├── infra/      proxy Caddy (HTTPS automático) + SQL Server + MongoDB, compartidos   → infra/.env
├── cliente/    plantilla del stack de un cliente (web, api, worker, db-migrate)
├── clientes/   <cliente>.env por cliente (NO versionado)
├── db/         schema.dacpac (esquema SQL) + imagen que lo aplica
├── backups/    backups SQL y Mongo (NO versionado)
└── scripts/    todo lo operativo
```

---

## 1. Preparar el servidor (una vez)

Requisitos: Linux con [Docker Engine](https://docs.docker.com/engine/install/) y el plugin `compose`,
puertos 80 y 443 abiertos, y DNS:

| Registro | Tipo | Valor |
|---|---|---|
| `*.sigesmas.com` | A | IP del servidor |
| `*.api.sigesmas.com` | A | IP del servidor |

Con DNS comodín, agregar un cliente no requiere tocar el DNS.

```bash
git clone https://github.com/ivanbanos/Facturador.git /opt/facturador
cd /opt/facturador
chmod +x deploy/scripts/*.sh
deploy/scripts/preparar-servidor.sh        # genera infra/.env con contraseñas y levanta proxy + BD
deploy/scripts/construir-imagenes.sh 1.0.0 # construye api, worker, web y dbmigrate
```

`infra/.env` guarda la contraseña `sa` de SQL Server y la del administrador de Mongo. Guárdela en un
gestor de contraseñas. SQL Server y Mongo solo escuchan en `127.0.0.1`; para administrarlos desde su
equipo use un túnel SSH: `ssh -L 1433:127.0.0.1:1433 usuario@servidor`.

## 2. Esquema SQL (una vez, y cada vez que cambie el esquema)

El proyecto `FacturacionelectronicaCore.Database` no tiene todos los procedimientos que usa el API, así que
el esquema de referencia se toma de una base de producción que esté al día:

```bash
deploy/scripts/extraer-esquema.sh      # pide servidor, base, usuario y contraseña
git add deploy/db/schema.dacpac        # versionarlo (solo esquema, sin datos)
```

En cada despliegue, `db-migrate` aplica las diferencias del `schema.dacpac` a la base del cliente:
nunca borra objetos y se detiene si un cambio pudiera perder datos.

## 3. Agregar un cliente

```bash
deploy/scripts/nuevo-cliente.sh acme     # preguntas → clientes/acme.env + infra/sites/acme.caddy
deploy/scripts/desplegar.sh up acme      # crea base y usuarios, aplica esquema, levanta todo, recarga proxy
```

Listo: `https://acme.sigesmas.com` y `https://acme.api.sigesmas.com`. El certificado HTTPS se emite solo
en la primera visita.

### Qué se configura y dónde

Todo está en **un archivo por cliente**: `deploy/clientes/<cliente>.env`, con comentarios por sección
(ver [`cliente/cliente.env.example`](cliente/cliente.env.example)). Después de editarlo:

```bash
deploy/scripts/validar-env.sh acme     # opcional: desplegar lo valida igual
deploy/scripts/desplegar.sh up acme
```

| Necesito cambiar… | Variable |
|---|---|
| Proveedor / credenciales de facturación electrónica | `FE_*` |
| Resolución y prefijo | `FE_RESOLUCION`, `FE_PREFIJO` |
| Funcionalidades de la web | `FEATURE_FLAGS` |
| Encender/apagar el worker | `WORKER_HABILITADO` |
| Otro dominio para la web | `SUBDOMINIOS_EXTRA` |
| Opciones complejas (`Estaciones`, `FormasPago`, offsets de hora, `Current`, `Syscafe`) | `CONFIG_AVANZADA` → JSON con formato `appsettings` |

Ejemplo de configuración avanzada (`deploy/clientes/acme.appsettings.json`, con
`CONFIG_AVANZADA=../clientes/acme.appsettings.json`):

```json
{
  "Alegra": {
    "ServerTimeOffsetHours": 5,
    "Estaciones": {
      "f426ea66-32ed-46fa-9b27-...": { "Corriente": "0010001000001", "Gas": "0010003000001" }
    }
  }
}
```

Precedencia (de menor a mayor): `appsettings.json` de la imagen → `CONFIG_AVANZADA` → variables del `.env`.

## 4. Migrar un cliente existente (desde el servidor Windows actual)

1. En el servidor actual: `BACKUP DATABASE [base] TO DISK = 'C:\temp\base.bak'` y
   `mongodump --db <Cliente> --gzip --archive=cliente.archive.gz`.
2. Copiar ambos archivos al servidor nuevo.
3. Crear el cliente con `nuevo-cliente.sh`. **`CLIENTE_DATOS` debe ser igual a `RepositorioConfig:Cliente`
   del `appsettings.json` anterior** (Mongo distingue mayúsculas).
4. Restaurar y desplegar:
   ```bash
   deploy/scripts/restaurar-backup.sh sql   acme /ruta/base.bak
   deploy/scripts/restaurar-backup.sh mongo acme /ruta/cliente.archive.gz
   deploy/scripts/desplegar.sh up acme
   ```
5. Probar con `curl https://acme.api.sigesmas.com/health` y entrando a la web. Luego apuntar las
   estaciones/servicios locales a la nueva URL del API (si cambió) y apagar el sitio anterior.

## 5. Operación diaria

```bash
deploy/scripts/desplegar.sh estado acme
deploy/scripts/desplegar.sh logs acme api          # api | worker | web | db-migrate
deploy/scripts/desplegar.sh reiniciar acme
deploy/scripts/desplegar.sh down acme              # detiene; los datos se conservan

# Nueva versión
deploy/scripts/construir-imagenes.sh 1.1.0
deploy/scripts/desplegar.sh update acme 1.1.0      # rollback: update acme 1.0.0
deploy/scripts/desplegar.sh todos                  # redespliega todos los clientes
```

### Backups

```bash
deploy/scripts/backup.sh            # todos los clientes; RETENCION_DIAS=14 por defecto
deploy/scripts/backup.sh acme
```

Programarlo con cron (`crontab -e`):

```
0 2 * * * /opt/facturador/deploy/scripts/backup.sh >> /var/log/facturador-backup.log 2>&1
```

Los archivos quedan en `deploy/backups/`. **Cópielos fuera del servidor** (rclone, S3, otro equipo) y
pruebe una restauración de vez en cuando.

## 6. Desarrollo local (Windows)

Las credenciales ya no están en git. El API y el Worker leen, además de `appsettings.json`:

- `appsettings.Local.json` y `appsettings.<Entorno>.Local.json`: no versionados, junto al `.csproj`.
  Copie `appsettings.json` y llene los valores del cliente con el que trabaja.
- Variables de entorno (`Seccion__Clave`), que tienen la última palabra.

Para probar el stack completo en su equipo use `DOMINIO_BASE=localhost` en el `.env` del cliente: Caddy
emite certificados locales y la web queda en `https://<cliente>.localhost`.

## 7. Solución de problemas

| Síntoma | Revisar |
|---|---|
| `db-migrate` falla | `desplegar.sh logs <c> db-migrate`. Si dice pérdida de datos, revisar el cambio de esquema. |
| API "unhealthy" | `curl` a `/health` desde el servidor: indica si falla `sqlserver` o `mongodb`. |
| La web no llama al API | `https://<c>.<dominio>/config.js` debe mostrar el `SERVER_URL` correcto. |
| Error CORS | El origen de la web debe ser `https://<CLIENTE>.<DOMINIO_BASE>` (`Cors__Origenes__0`). |
| Certificado no se emite | DNS del subdominio apunta al servidor y puertos 80/443 abiertos; ver `docker logs facturador-infra-caddy-1`. |
