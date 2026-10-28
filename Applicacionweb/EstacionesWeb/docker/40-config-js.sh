#!/bin/sh
# Genera /usr/share/nginx/html/config.js a partir de variables de entorno antes de arrancar nginx.
#   SERVER_URL     URL base del API, ej. https://cliente.api.sigesmas.com/api   (obligatoria)
#   FEATURE_FLAGS  JSON con funcionalidades opcionales, ej. {"usuarios-clientes": true}
set -eu

if [ -z "${SERVER_URL:-}" ]; then
    echo "ERROR: falta la variable SERVER_URL (URL del API)" >&2
    exit 1
fi

# Escapa comillas y barras invertidas para que la URL quede como string JS válido
server_url_js=$(printf '%s' "$SERVER_URL" | sed 's/\\/\\\\/g; s/"/\\"/g')

feature_flags="${FEATURE_FLAGS:-}"
if [ -z "$feature_flags" ]; then
    feature_flags='{}'
fi
case "$feature_flags" in
    \{*\}) ;;
    *) echo "ERROR: FEATURE_FLAGS debe ser un objeto JSON, ej. {\"usuarios-clientes\": true}" >&2; exit 1 ;;
esac

cat > /usr/share/nginx/html/config.js <<EOF
// Generado al iniciar el contenedor. No editar: cambie las variables del cliente (.env).
var SERVER_URL = "${server_url_js}";
var FEATURE_FLAGS = ${feature_flags};
EOF

echo "config.js generado (SERVER_URL=${SERVER_URL})"
