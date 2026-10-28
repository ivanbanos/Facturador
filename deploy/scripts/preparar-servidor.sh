#!/bin/bash
# Preparación inicial de un servidor Linux (una sola vez). Requiere Docker Engine + plugin compose.
# Instalar Docker: https://docs.docker.com/engine/install/  (luego: sudo usermod -aG docker $USER)
# Uso: preparar-servidor.sh
source "$(dirname "$0")/lib.sh"
requiere_docker
docker compose version >/dev/null 2>&1 || fallar "Falta el plugin docker compose"

# Carpetas de backups montadas en los contenedores. SQL Server corre con el usuario mssql (uid 10001).
mkdir -p "$DEPLOY_DIR/backups/sql" "$DEPLOY_DIR/backups/mongo" "$CLIENTES_DIR" "$INFRA_DIR/sites"
if [ "$(id -u)" -eq 0 ]; then
    chown 10001:0 "$DEPLOY_DIR/backups/sql"
else
    sudo chown 10001:0 "$DEPLOY_DIR/backups/sql" || aviso "No se pudo asignar permisos a backups/sql (BACKUP DATABASE fallará)"
fi
chmod 700 "$CLIENTES_DIR"

if [ ! -f "$INFRA_DIR/.env" ]; then
    cp "$INFRA_DIR/.env.example" "$INFRA_DIR/.env"
    chmod 600 "$INFRA_DIR/.env"
    escribir_env SA_PASSWORD "$(generar_clave 24)" "$INFRA_DIR/.env"
    escribir_env MONGO_ROOT_PASSWORD "$(generar_clave 24)" "$INFRA_DIR/.env"
    read -r -p "Correo para certificados HTTPS (Let's Encrypt): " correo
    escribir_env ACME_EMAIL "$correo" "$INFRA_DIR/.env"
    ok "Creado $INFRA_DIR/.env con contraseñas generadas"
else
    ok "$INFRA_DIR/.env ya existe; no se modifica"
fi

info "Levantando infraestructura (proxy, SQL Server, MongoDB)"
docker compose -p "$INFRA_PROJECT" -f "$INFRA_DIR/docker-compose.yml" up -d --wait --wait-timeout 180
ok "Infraestructura lista. Siguiente paso: scripts/construir-imagenes.sh y scripts/nuevo-cliente.sh"
