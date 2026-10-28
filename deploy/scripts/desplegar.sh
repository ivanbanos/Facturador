#!/bin/bash
# Operación de un cliente.
# Uso:
#   desplegar.sh up <cliente>                 valida, crea base/usuarios, aplica esquema y levanta web/api/worker
#   desplegar.sh update <cliente> <version>   cambia VERSION en el .env y redespliega (rollback = versión anterior)
#   desplegar.sh down <cliente>               detiene el cliente (los datos se conservan)
#   desplegar.sh reiniciar <cliente>
#   desplegar.sh estado <cliente>
#   desplegar.sh logs <cliente> [servicio]    servicio: api | worker | web | db-migrate
#   desplegar.sh todos                        "up" de todos los clientes en deploy/clientes/
source "$(dirname "$0")/lib.sh"

compose_cliente() {
    local archivo="$1"; shift
    local cliente perfiles=()
    cliente=$(leer_env CLIENTE "$archivo")
    if [ "$(leer_env WORKER_HABILITADO "$archivo")" = "true" ]; then
        perfiles=(--profile worker)
    fi
    docker compose -p "$cliente" --env-file "$archivo" -f "$CLIENTE_COMPOSE" "${perfiles[@]}" "$@"
}

# Crea (o actualiza) base, login y usuario SQL del cliente en el contenedor compartido. Idempotente.
provisionar_sql() {
    local archivo="$1" base usuario clave sa_clave contenedor
    base=$(leer_env SQL_DATABASE "$archivo")
    usuario=$(leer_env SQL_USER "$archivo")
    clave=$(leer_env SQL_PASSWORD "$archivo")
    sa_clave=$(leer_env SA_PASSWORD "$INFRA_DIR/.env")
    [ -n "$sa_clave" ] || fallar "Falta SA_PASSWORD en $INFRA_DIR/.env"
    contenedor=$(contenedor_infra sqlserver)

    # Los identificadores ya fueron validados (solo letras, números y _); la clave se escapa para T-SQL.
    local clave_sql="${clave//\'/\'\'}"
    info "SQL Server: base [$base], login [$usuario]"
    docker exec -i -e SQLCMDPASSWORD="$sa_clave" "$contenedor" \
        /opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -b -i /dev/stdin <<SQL
SET NOCOUNT ON;
IF DB_ID(N'$base') IS NULL CREATE DATABASE [$base];
GO
IF SUSER_ID(N'$usuario') IS NULL
    CREATE LOGIN [$usuario] WITH PASSWORD = N'$clave_sql', CHECK_POLICY = OFF, DEFAULT_DATABASE = [$base];
ELSE
    ALTER LOGIN [$usuario] WITH PASSWORD = N'$clave_sql';
GO
USE [$base];
-- Una base restaurada desde backup puede traer el usuario huérfano: se vuelve a enlazar al login.
IF USER_ID(N'$usuario') IS NULL
    CREATE USER [$usuario] FOR LOGIN [$usuario];
ELSE
    ALTER USER [$usuario] WITH LOGIN = [$usuario];
ALTER ROLE db_owner ADD MEMBER [$usuario];
GO
SQL
    ok "SQL Server listo"
}

# Crea (o actualiza) el usuario Mongo del cliente con acceso solo a su base. Idempotente.
provisionar_mongo() {
    local archivo="$1" contenedor
    contenedor=$(contenedor_infra mongo)
    info "MongoDB: base [$(leer_env CLIENTE_DATOS "$archivo")], usuario [$(leer_env MONGO_USER "$archivo")]"
    docker exec \
        -e ROOT_USER="$(leer_env MONGO_ROOT_USER "$INFRA_DIR/.env")" \
        -e ROOT_PASS="$(leer_env MONGO_ROOT_PASSWORD "$INFRA_DIR/.env")" \
        -e BASE="$(leer_env CLIENTE_DATOS "$archivo")" \
        -e USUARIO="$(leer_env MONGO_USER "$archivo")" \
        -e CLAVE="$(leer_env MONGO_PASSWORD "$archivo")" \
        "$contenedor" sh -c 'mongosh --quiet -u "${ROOT_USER:-admin}" -p "$ROOT_PASS" --authenticationDatabase admin --eval "
            const base = db.getSiblingDB(process.env.BASE);
            const roles = [{ role: \"readWrite\", db: process.env.BASE }];
            if (base.getUser(process.env.USUARIO)) {
                base.updateUser(process.env.USUARIO, { pwd: process.env.CLAVE, roles });
            } else {
                base.createUser({ user: process.env.USUARIO, pwd: process.env.CLAVE, roles });
            }"' >/dev/null
    ok "MongoDB listo"
}

subir() {
    local cliente="$1" archivo
    archivo=$(archivo_cliente "$cliente")
    "$DEPLOY_DIR/scripts/validar-env.sh" "$archivo"
    requiere_docker
    [ -f "$INFRA_DIR/.env" ] || fallar "Falta $INFRA_DIR/.env (copiar de .env.example)"

    if [ "$(leer_env SQL_HOST "$archivo")" = "sqlserver" ] || [ -z "$(leer_env SQL_HOST "$archivo")" ]; then
        provisionar_sql "$archivo"
    else
        aviso "SQL_HOST externo: no se crea base ni login (deben existir)"
    fi
    provisionar_mongo "$archivo"

    info "Levantando $cliente (versión $(leer_env VERSION "$archivo"))"
    compose_cliente "$archivo" up -d --remove-orphans --wait --wait-timeout 180
    "$DEPLOY_DIR/scripts/generar-sitio.sh" "$cliente"
    recargar_caddy
    ok "$cliente desplegado"
    compose_cliente "$archivo" ps
}

accion="${1:-}"
case "$accion" in
    up)
        subir "${2:?Uso: desplegar.sh up <cliente>}"
        ;;
    update)
        cliente="${2:?Uso: desplegar.sh update <cliente> <version>}"
        version="${3:?Uso: desplegar.sh update <cliente> <version>}"
        archivo=$(archivo_cliente "$cliente")
        anterior=$(leer_env VERSION "$archivo")
        escribir_env VERSION "$version" "$archivo"
        info "VERSION $anterior -> $version (rollback: desplegar.sh update $cliente $anterior)"
        [ -z "$(leer_env REGISTRO "$archivo")" ] || compose_cliente "$archivo" pull
        subir "$cliente"
        ;;
    down)
        archivo=$(archivo_cliente "${2:?Uso: desplegar.sh down <cliente>}")
        compose_cliente "$archivo" down
        ;;
    reiniciar)
        archivo=$(archivo_cliente "${2:?Uso: desplegar.sh reiniciar <cliente>}")
        compose_cliente "$archivo" restart
        ;;
    estado)
        archivo=$(archivo_cliente "${2:?Uso: desplegar.sh estado <cliente>}")
        compose_cliente "$archivo" ps
        ;;
    logs)
        archivo=$(archivo_cliente "${2:?Uso: desplegar.sh logs <cliente> [servicio]}")
        compose_cliente "$archivo" logs -f --tail 200 ${3:-}
        ;;
    todos)
        shopt -s nullglob
        for archivo in "$CLIENTES_DIR"/*.env; do
            # Subshell: un "fallar" (exit) de un cliente no detiene el despliegue de los demás
            ( subir "$(basename "$archivo" .env)" ) || error "Falló $(basename "$archivo" .env); se continúa con el siguiente"
        done
        ;;
    *)
        sed -n '2,11p' "$0" | sed 's/^# \{0,1\}//'
        exit 1
        ;;
esac
