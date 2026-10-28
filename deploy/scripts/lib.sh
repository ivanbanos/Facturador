#!/bin/bash
# Funciones comunes de los scripts de despliegue. Se carga con:  source "$(dirname "$0")/lib.sh"

set -euo pipefail

# En Git Bash (Windows) evita que rutas de contenedor como /opt/... se conviertan a rutas de Windows.
export MSYS_NO_PATHCONV=1

# "pwd -W" solo existe en Git Bash: da rutas C:/... que docker.exe entiende (con MSYS_NO_PATHCONV activo)
ruta_absoluta() { (cd "$1" && { pwd -W 2>/dev/null || pwd; }); }
DEPLOY_DIR="$(ruta_absoluta "$(dirname "${BASH_SOURCE[0]}")/..")"
REPO_DIR="$(ruta_absoluta "$DEPLOY_DIR/..")"
CLIENTES_DIR="$DEPLOY_DIR/clientes"
INFRA_DIR="$DEPLOY_DIR/infra"
CLIENTE_COMPOSE="$DEPLOY_DIR/cliente/docker-compose.yml"
INFRA_PROJECT="facturador-infra"

PROVEEDORES_FE="DATAICO DATATICO SIIGO ALEGRA SILOG SILOG2 TITAN FACTURA1 CELESTE"

info()  { printf '\033[1;34m==>\033[0m %s\n' "$*"; }
ok()    { printf '\033[1;32m OK\033[0m %s\n' "$*"; }
aviso() { printf '\033[1;33m AVISO\033[0m %s\n' "$*" >&2; }
error() { printf '\033[1;31m ERROR\033[0m %s\n' "$*" >&2; }
fallar() { error "$*"; exit 1; }

# Lee KEY de un archivo .env sin ejecutarlo (no usa "source": los valores pueden tener $, espacios, etc.).
# Quita comillas simples o dobles que envuelvan el valor completo.
leer_env() {
    local clave="$1" archivo="$2" valor
    valor=$(grep -E "^${clave}=" "$archivo" | tail -n1 | cut -d= -f2- | tr -d '\r') || true
    if [[ "$valor" =~ ^\'(.*)\'$ ]] || [[ "$valor" =~ ^\"(.*)\"$ ]]; then
        valor="${BASH_REMATCH[1]}"
    fi
    printf '%s' "$valor"
}

# Escribe KEY=valor en un .env conservando comentarios y orden. Si la clave no existe la agrega al final.
escribir_env() {
    local clave="$1" valor="$2" archivo="$3" tmp
    tmp="$(mktemp)"
    if grep -qE "^${clave}=" "$archivo"; then
        VALOR="$valor" awk -v k="$clave" 'BEGIN { v = ENVIRON["VALOR"] }
            index($0, k "=") == 1 { print k "=" v; next } { print }' "$archivo" > "$tmp"
    else
        cp "$archivo" "$tmp"
        printf '%s=%s\n' "$clave" "$valor" >> "$tmp"
    fi
    mv "$tmp" "$archivo"
}

# Contraseña aleatoria alfanumérica (mayúsculas, minúsculas y números: cumple la política de SQL Server
# y no necesita escaparse en .env, cadenas de conexión ni T-SQL).
generar_clave() {
    local largo="${1:-32}" clave=""
    while ! [[ "$clave" =~ [A-Z] && "$clave" =~ [a-z] && "$clave" =~ [0-9] ]]; do
        # "|| true": head cierra la tubería y tr termina con SIGPIPE, que con pipefail abortaría el script
        clave=$(LC_ALL=C tr -dc 'A-Za-z0-9' < /dev/urandom 2>/dev/null | head -c "$largo" || true)
    done
    printf '%s' "$clave"
}

archivo_cliente() {
    local cliente="${1:?Indique el cliente}"
    local archivo="$CLIENTES_DIR/${cliente}.env"
    [ -f "$archivo" ] || fallar "No existe $archivo. Créelo con scripts/nuevo-cliente.sh"
    printf '%s' "$archivo"
}

requiere_docker() {
    command -v docker >/dev/null || fallar "Docker no está instalado"
    docker info >/dev/null 2>&1 || fallar "Docker no está corriendo o el usuario no tiene permisos (grupo docker)"
}

contenedor_infra() {
    # Nombre real del contenedor de un servicio de infraestructura (sqlserver, mongo, caddy)
    local servicio="$1" id
    id=$(docker compose -p "$INFRA_PROJECT" -f "$INFRA_DIR/docker-compose.yml" ps -q "$servicio" 2>/dev/null || true)
    [ -n "$id" ] || fallar "El servicio '$servicio' de infraestructura no está corriendo. Ejecute: cd deploy/infra && docker compose up -d"
    printf '%s' "$id"
}

recargar_caddy() {
    local caddy
    caddy=$(contenedor_infra caddy)
    docker exec "$caddy" caddy reload --config /etc/caddy/Caddyfile >/dev/null && ok "Proxy recargado"
}
