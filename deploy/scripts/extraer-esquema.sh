#!/bin/bash
# Extrae el esquema (sin datos) de una base existente a deploy/db/schema.dacpac.
# Ese archivo es la fuente del esquema para clientes nuevos y para actualizar los existentes.
# Hacerlo desde una base de producción que esté al día (todas las tablas, tipos y SP que usa el API).
# Uso: extraer-esquema.sh            pide servidor, base, usuario y contraseña
#      SQL_ORIGEN="Server=...;Database=...;User Id=...;Password=..." extraer-esquema.sh
#      --contenedor   extraer desde el SQL Server en contenedor (servidor "sqlserver"), ej. un cliente ya migrado
source "$(dirname "$0")/lib.sh"
requiere_docker

imagen="facturador-dbmigrate:latest"
docker image inspect "$imagen" >/dev/null 2>&1 || fallar "No existe la imagen $imagen. Ejecute scripts/construir-imagenes.sh"

if [ -z "${SQL_ORIGEN:-}" ]; then
    read -r -p "Servidor (ej. 198.38.85.254,1433): " servidor
    read -r -p "Base de datos: " base
    read -r -p "Usuario: " usuario
    read -r -s -p "Contraseña: " clave; echo
    SQL_ORIGEN="Server=$servidor;Database=$base;User Id=$usuario;Password=$clave;TrustServerCertificate=True"
fi

red=()
[ "${1:-}" = "--contenedor" ] && red=(--network facturador_datos)

salida="$DEPLOY_DIR/db"
info "Extrayendo esquema a $salida/schema.dacpac"
# La cadena de conexión va por variable de entorno, no como argumento visible en la lista de procesos
docker run --rm "${red[@]}" -e SQL_ORIGEN="$SQL_ORIGEN" -v "$salida:/salida" --entrypoint /bin/bash "$imagen" -c '
    sqlpackage /Action:Extract \
        /SourceConnectionString:"$SQL_ORIGEN" \
        /TargetFile:/tmp/schema.dacpac \
        /p:ExtractAllTableData=False \
        /p:ExtractApplicationScopedObjectsOnly=True \
        /p:IgnorePermissions=True \
        /p:IgnoreUserLoginMappings=True \
        /p:VerifyExtraction=False \
    && cp /tmp/schema.dacpac /salida/schema.dacpac'

ok "Esquema extraído. Revise y versione deploy/db/schema.dacpac (no contiene datos ni contraseñas)."
