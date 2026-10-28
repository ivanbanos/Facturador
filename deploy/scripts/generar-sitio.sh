#!/bin/bash
# Genera deploy/infra/sites/<cliente>.caddy con los subdominios del cliente:
#   https://<CLIENTE>.<DOMINIO_BASE>      -> web
#   https://<CLIENTE>.api.<DOMINIO_BASE>  -> api
# Subdominios adicionales: agregar SUBDOMINIOS_EXTRA en el .env (separados por espacio), apuntan a la web.
# Uso: generar-sitio.sh <cliente>
source "$(dirname "$0")/lib.sh"

cliente="${1:?Uso: generar-sitio.sh <cliente>}"
archivo=$(archivo_cliente "$cliente")
dominio=$(leer_env DOMINIO_BASE "$archivo")
extra=$(leer_env SUBDOMINIOS_EXTRA "$archivo")
[ -n "$dominio" ] || fallar "DOMINIO_BASE vacío en $archivo"

host_web="${cliente}.${dominio}"
for sub in $extra; do
    host_web="$host_web, $sub"
done

sitio="$INFRA_DIR/sites/${cliente}.caddy"
mkdir -p "$INFRA_DIR/sites"
cat > "$sitio" <<EOF
# Generado por deploy/scripts/generar-sitio.sh — se regenera en cada despliegue, no editar a mano.
${host_web} {
	import seguridad
	reverse_proxy ${cliente}-web:80
}

${cliente}.api.${dominio} {
	import seguridad
	reverse_proxy ${cliente}-api:8080
}
EOF
ok "Sitio generado: $sitio"
