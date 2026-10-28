#!/bin/bash
# Restaura datos de un cliente en los contenedores (migración desde el servidor actual o recuperación).
# Uso:
#   restaurar-backup.sh sql   <cliente> <archivo.bak>          [--reemplazar]
#   restaurar-backup.sh mongo <cliente> <archivo.archive[.gz]> [--reemplazar]
# Sin --reemplazar se niega a sobrescribir una base que ya existe.
# Después de restaurar ejecute: desplegar.sh up <cliente>   (re-enlaza el login SQL y aplica esquema)
source "$(dirname "$0")/lib.sh"

tipo="${1:-}"; cliente="${2:-}"; origen="${3:-}"; opcion="${4:-}"
[ -n "$tipo" ] && [ -n "$cliente" ] && [ -n "$origen" ] || { sed -n '2,7p' "$0" | sed 's/^# \{0,1\}//'; exit 1; }
[ -f "$origen" ] || fallar "No existe el archivo $origen"
archivo=$(archivo_cliente "$cliente")
requiere_docker
reemplazar=false
[ "$opcion" = "--reemplazar" ] && reemplazar=true

restaurar_sql() {
    local base contenedor sa_clave nombre destino lista mover="" logico tipo_archivo n=0
    base=$(leer_env SQL_DATABASE "$archivo")
    contenedor=$(contenedor_infra sqlserver)
    sa_clave=$(leer_env SA_PASSWORD "$INFRA_DIR/.env")
    nombre="restaurar_${base}.bak"
    destino="/var/opt/mssql/backups/$nombre"

    sqlcmd() {
        docker exec -i -e SQLCMDPASSWORD="$sa_clave" "$contenedor" \
            /opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -b "$@"
    }

    existe=$(sqlcmd -h -1 -W -Q "SET NOCOUNT ON; SELECT CASE WHEN DB_ID(N'$base') IS NULL THEN 0 ELSE 1 END")
    if [ "$(echo "$existe" | tr -d '[:space:]')" = "1" ] && [ "$reemplazar" != true ]; then
        fallar "La base [$base] ya existe. Use --reemplazar para sobrescribirla (se pierden sus datos actuales)."
    fi

    info "Copiando $origen al contenedor"
    docker cp "$origen" "$contenedor:$destino"

    # Nombres lógicos de los archivos del backup para moverlos a la ruta de datos del contenedor
    lista=$(sqlcmd -h -1 -W -s '|' -Q "SET NOCOUNT ON; RESTORE FILELISTONLY FROM DISK = N'$destino'")
    while IFS='|' read -r logico _ tipo_archivo _; do
        [ -n "$logico" ] || continue
        case "$tipo_archivo" in
            D) ext="mdf" ;;
            L) ext="ldf" ;;
            *) ext="ndf" ;;
        esac
        n=$((n + 1))
        mover="$mover, MOVE N'${logico//\'/\'\'}' TO N'/var/opt/mssql/data/${base}_${n}.${ext}'"
    done <<< "$lista"
    [ -n "$mover" ] || fallar "No se pudieron leer los archivos del backup"

    if [ "$reemplazar" = true ]; then
        # RESTORE ... REPLACE falla si el API/worker tienen conexiones abiertas: se cierran primero
        sqlcmd -Q "IF DB_ID(N'$base') IS NOT NULL ALTER DATABASE [$base] SET SINGLE_USER WITH ROLLBACK IMMEDIATE"
    fi

    info "Restaurando [$base]"
    sqlcmd -Q "RESTORE DATABASE [$base] FROM DISK = N'$destino' WITH RECOVERY, STATS = 10 ${mover}$( [ "$reemplazar" = true ] && echo ', REPLACE')"
    docker exec "$contenedor" rm -f "$destino"
    ok "Base [$base] restaurada"
}

restaurar_mongo() {
    local contenedor datos borrar=()
    contenedor=$(contenedor_infra mongo)
    datos=$(leer_env CLIENTE_DATOS "$archivo")
    [ "$reemplazar" = true ] && borrar=(--drop)
    info "Copiando $origen al contenedor"
    docker cp "$origen" "$contenedor:/tmp/restaurar.archive"
    gzip_flag=""
    [[ "$origen" == *.gz ]] && gzip_flag="--gzip"
    info "Restaurando base Mongo [$datos]"
    docker exec \
        -e ROOT_USER="$(leer_env MONGO_ROOT_USER "$INFRA_DIR/.env")" \
        -e ROOT_PASS="$(leer_env MONGO_ROOT_PASSWORD "$INFRA_DIR/.env")" \
        "$contenedor" sh -c "mongorestore -u \"\${ROOT_USER:-admin}\" -p \"\$ROOT_PASS\" --authenticationDatabase admin \
            --archive=/tmp/restaurar.archive $gzip_flag --nsInclude='$datos.*' ${borrar[*]:-} && rm -f /tmp/restaurar.archive"
    ok "Mongo [$datos] restaurado"
}

case "$tipo" in
    sql) restaurar_sql ;;
    mongo) restaurar_mongo ;;
    *) fallar "Tipo no válido: $tipo (sql | mongo)" ;;
esac
