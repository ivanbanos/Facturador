#!/bin/bash
# Construye las 4 imágenes (api, worker, web, dbmigrate) con el tag indicado y "latest".
# Uso: construir-imagenes.sh [version] [registro/]
#   construir-imagenes.sh 1.2.0                       imágenes locales facturador-*:1.2.0
#   construir-imagenes.sh 1.2.0 ghcr.io/ivanbanos/    además las sube al registro (requiere docker login)
source "$(dirname "$0")/lib.sh"
requiere_docker

version="${1:-latest}"
registro="${2:-}"
backend="$REPO_DIR/Applicacionweb/FacturacionelectronicaCore"

construir() {
    local nombre="$1"; shift
    info "Construyendo $nombre:$version"
    docker build -t "${registro}${nombre}:${version}" -t "${registro}${nombre}:latest" "$@"
}

construir facturador-api -f "$backend/Dockerfile.api" "$backend"
construir facturador-worker -f "$backend/Dockerfile.worker" "$backend"
construir facturador-web "$REPO_DIR/Applicacionweb/EstacionesWeb"
construir facturador-dbmigrate "$DEPLOY_DIR/db"

if [ -n "$registro" ]; then
    for nombre in facturador-api facturador-worker facturador-web facturador-dbmigrate; do
        docker push "${registro}${nombre}:${version}"
        docker push "${registro}${nombre}:latest"
    done
fi
ok "Imágenes listas (versión $version)"
