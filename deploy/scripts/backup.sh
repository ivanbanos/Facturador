#!/bin/bash
# Backup de SQL Server + MongoDB de uno o todos los clientes. Pensado para cron (diario):
#   0 2 * * * /opt/facturador/deploy/scripts/backup.sh >> /var/log/facturador-backup.log 2>&1
# Uso: backup.sh [cliente]       sin cliente = todos
# Variables opcionales: RETENCION_DIAS (default 14)
# Los archivos quedan en deploy/backups/{sql,mongo}. Cópielos fuera del servidor (rclone, scp, etc.).
source "$(dirname "$0")/lib.sh"

RETENCION_DIAS="${RETENCION_DIAS:-14}"
BACKUPS_DIR="$DEPLOY_DIR/backups"
fecha=$(date +%Y%m%d_%H%M%S)
requiere_docker

sql=$(contenedor_infra sqlserver)
mongo=$(contenedor_infra mongo)
sa_clave=$(leer_env SA_PASSWORD "$INFRA_DIR/.env")

respaldar() {
    local archivo="$1" base datos
    base=$(leer_env SQL_DATABASE "$archivo")
    datos=$(leer_env CLIENTE_DATOS "$archivo")

    if [ "$(leer_env SQL_HOST "$archivo")" = "sqlserver" ]; then
        info "SQL: $base"
        # Express no soporta COMPRESSION; CHECKSUM valida páginas al respaldar
        docker exec -e SQLCMDPASSWORD="$sa_clave" "$sql" /opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -b \
            -Q "BACKUP DATABASE [$base] TO DISK = N'/var/opt/mssql/backups/${base}_${fecha}.bak' WITH CHECKSUM, INIT" >/dev/null
    else
        aviso "$base usa SQL externo: no se respalda aquí"
    fi

    info "Mongo: $datos"
    docker exec \
        -e ROOT_USER="$(leer_env MONGO_ROOT_USER "$INFRA_DIR/.env")" \
        -e ROOT_PASS="$(leer_env MONGO_ROOT_PASSWORD "$INFRA_DIR/.env")" \
        "$mongo" sh -c "mongodump --quiet -u \"\${ROOT_USER:-admin}\" -p \"\$ROOT_PASS\" --authenticationDatabase admin \
            --db '$datos' --gzip --archive=/backups/${datos}_${fecha}.archive.gz"
}

shopt -s nullglob
if [ -n "${1:-}" ]; then
    archivos=("$(archivo_cliente "$1")")
else
    archivos=("$CLIENTES_DIR"/*.env)
fi

fallos=0
for archivo in "${archivos[@]}"; do
    ( respaldar "$archivo" ) || { error "Falló el backup de $(basename "$archivo" .env)"; fallos=$((fallos + 1)); }
done

info "Borrando backups de más de $RETENCION_DIAS días"
find "$BACKUPS_DIR" -type f \( -name '*.bak' -o -name '*.archive.gz' \) -mtime +"$RETENCION_DIAS" -delete

[ "$fallos" -eq 0 ] || fallar "$fallos backup(s) fallaron"
ok "Backups completos en $BACKUPS_DIR"
