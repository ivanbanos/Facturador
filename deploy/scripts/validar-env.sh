#!/bin/bash
# Valida deploy/clientes/<cliente>.env antes de desplegar.
# Uso: validar-env.sh <cliente>      (o la ruta a un archivo .env)
# Sale con código 1 si hay errores; los avisos no bloquean.
source "$(dirname "$0")/lib.sh"

objetivo="${1:?Uso: validar-env.sh <cliente | archivo.env>}"
if [ -f "$objetivo" ]; then archivo="$objetivo"; else archivo=$(archivo_cliente "$objetivo"); fi

errores=0
mal() { error "$*"; errores=$((errores + 1)); }
v() { leer_env "$1" "$archivo"; }

obligatorio() {
    local clave
    for clave in "$@"; do
        [ -n "$(v "$clave")" ] || mal "$clave es obligatorio"
    done
}

booleano() {
    local clave valor
    for clave in "$@"; do
        valor=$(v "$clave")
        [ -z "$valor" ] || [[ "$valor" =~ ^(true|false)$ ]] || mal "$clave debe ser true o false (es '$valor')"
    done
}

identificador_sql() {
    local clave="$1" valor
    valor=$(v "$clave")
    [ -z "$valor" ] || [[ "$valor" =~ ^[A-Za-z][A-Za-z0-9_]{0,63}$ ]] \
        || mal "$clave solo puede tener letras, números y _ (y empezar con letra): '$valor'"
}

info "Validando $archivo"

obligatorio CLIENTE CLIENTE_DATOS DOMINIO_BASE SQL_DATABASE SQL_USER SQL_PASSWORD MONGO_USER MONGO_PASSWORD JWT_SECRET FE_PROVEEDOR

cliente=$(v CLIENTE)
[ -z "$cliente" ] || [[ "$cliente" =~ ^[a-z0-9]([a-z0-9-]{0,40}[a-z0-9])?$ ]] \
    || mal "CLIENTE debe tener solo minúsculas, números y guiones (es '$cliente')"

if [[ "$archivo" == "$CLIENTES_DIR"/*.env ]]; then
    nombre_archivo=$(basename "$archivo" .env)
    [ "$nombre_archivo" = "$cliente" ] \
        || mal "El archivo se llama '$nombre_archivo.env' pero CLIENTE=$cliente; deben coincidir"
fi

dominio=$(v DOMINIO_BASE)
# "localhost" sirve para probar en un equipo local (Caddy emite certificados propios para *.localhost)
[ -z "$dominio" ] || [[ "$dominio" =~ ^([a-z0-9.-]+\.[a-z]{2,}|localhost)$ ]] || mal "DOMINIO_BASE no parece un dominio válido: '$dominio'"

[[ "$(v CLIENTE_DATOS)" =~ ^[A-Za-z0-9_-]*$ ]] || mal "CLIENTE_DATOS solo puede tener letras, números, _ y -"
identificador_sql SQL_DATABASE
identificador_sql SQL_USER
identificador_sql MONGO_USER

for clave in SQL_PASSWORD MONGO_PASSWORD; do
    valor=$(v "$clave")
    [ -z "$valor" ] || [ "${#valor}" -ge 12 ] || mal "$clave debe tener al menos 12 caracteres"
    [[ "$valor" != *[\;\'\"@/:?#]* ]] || mal "$clave no puede tener ; ' \" @ / : ? # (rompen la cadena de conexión)"
done

jwt=$(v JWT_SECRET)
[ -z "$jwt" ] || [ "${#jwt}" -ge 32 ] || mal "JWT_SECRET debe tener al menos 32 caracteres"

proveedor=$(v FE_PROVEEDOR)
[ -z "$proveedor" ] || [[ " $PROVEEDORES_FE " == *" $proveedor "* ]] \
    || mal "FE_PROVEEDOR '$proveedor' no es válido. Opciones: $PROVEEDORES_FE"

booleano FE_ENVIO_DIRECTO FE_ENVIA_CREDITOS FE_ENVIA_MES FE_VALIDA_TERCERO FE_MULTIPLICAR_POR_DIEZ WORKER_HABILITADO

flags=$(v FEATURE_FLAGS)
[ -z "$flags" ] || [[ "$flags" =~ ^\{.*\}$ ]] || mal "FEATURE_FLAGS debe ser un objeto JSON entre comillas simples, ej. '{\"usuarios-clientes\": true}'"

api_url=$(v API_URL)
[ -z "$api_url" ] || [[ "$api_url" =~ ^https?:// ]] || mal "API_URL debe empezar con http:// o https://"

avanzada=$(v CONFIG_AVANZADA)
if [ -n "$avanzada" ]; then
    ruta="$DEPLOY_DIR/cliente/$avanzada"
    [ -f "$ruta" ] || mal "CONFIG_AVANZADA apunta a un archivo que no existe: $ruta"
fi

# Avisos (no bloquean)
if [ -z "$(v FE_TOKEN)$(v FE_USUARIO)" ]; then
    aviso "No hay credenciales del proveedor de FE (FE_TOKEN / FE_USUARIO). Ignore si se configuran en la resolución."
fi
[ -n "$(v FE_RESOLUCION)" ] || aviso "FE_RESOLUCION está vacío"
[ -n "$(v FE_PREFIJO)" ] || aviso "FE_PREFIJO está vacío"

if [ "$errores" -gt 0 ]; then
    error "$errores error(es) en $archivo"
    exit 1
fi
ok "Configuración válida: web https://$cliente.$dominio  api ${api_url:-https://$cliente.api.$dominio/api}"
