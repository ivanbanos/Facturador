#!/bin/bash
# Asistente para dar de alta un cliente: pregunta los datos, genera contraseñas y escribe
#   deploy/clientes/<cliente>.env      configuración del cliente (no versionada)
#   deploy/infra/sites/<cliente>.caddy subdominios web y api
# Uso: nuevo-cliente.sh [cliente]
source "$(dirname "$0")/lib.sh"

# Las variables locales llevan prefijo "__" para no ocultar la variable del llamador que se asigna con printf -v
preguntar() {
    # preguntar VARIABLE "Texto" [valor por defecto]
    local __var="$1" __texto="$2" __defecto="${3:-}" __respuesta=""
    if [ -n "$__defecto" ]; then
        read -r -p "$__texto [$__defecto]: " __respuesta || true
        __respuesta="${__respuesta:-$__defecto}"
    else
        read -r -p "$__texto: " __respuesta || true
    fi
    printf -v "$__var" '%s' "$__respuesta"
}

preguntar_si_no() {
    local __var_sn="$1" __texto_sn="$2" __defecto_sn="$3" __respuesta_sn=""
    while true; do
        preguntar __respuesta_sn "$__texto_sn (s/n)" "$__defecto_sn"
        case "$__respuesta_sn" in
            s|S|si|SI|Si) printf -v "$__var_sn" 'true'; return ;;
            n|N|no|NO|No) printf -v "$__var_sn" 'false'; return ;;
        esac
        echo "Responda s o n"
    done
}

echo "Alta de cliente — Enter acepta el valor entre corchetes."
echo

cliente="${1:-}"
while ! [[ "$cliente" =~ ^[a-z0-9]([a-z0-9-]{0,40}[a-z0-9])?$ ]]; do
    [ -z "$cliente" ] || echo "Solo minúsculas, números y guiones."
    preguntar cliente "Identificador del cliente (ej. cootranshuila)"
done

archivo="$CLIENTES_DIR/${cliente}.env"
[ ! -e "$archivo" ] || fallar "Ya existe $archivo. Edítelo directamente o bórrelo para empezar de nuevo."

nombre_sql="${cliente//-/_}"

preguntar cliente_datos "Nombre del cliente en los datos (RepositorioConfig:Cliente; respeta mayúsculas)" "$cliente"
preguntar dominio "Dominio base" "sigesmas.com"
preguntar sql_database "Nombre de la base SQL" "$nombre_sql"

echo
echo "Facturación electrónica. Proveedores: $PROVEEDORES_FE"
preguntar proveedor "Proveedor" "DATAICO"
proveedor="${proveedor^^}"
[[ " $PROVEEDORES_FE " == *" $proveedor "* ]] || fallar "Proveedor no válido: $proveedor"

fe_url=""; fe_token=""; fe_account=""; fe_correo=""; fe_usuario=""; fe_contrasena=""; fe_nit=""
case "$proveedor" in
    DATAICO|DATATICO)
        preguntar fe_url "URL API Dataico" "https://api.dataico.com/direct/dataico_api/v2/"
        preguntar fe_account "Dataico account id (vacío si va en la resolución)"
        preguntar fe_token "Token Dataico (vacío si va en la resolución)"
        ;;
    *)
        preguntar fe_url "URL API del proveedor"
        preguntar fe_usuario "Usuario del proveedor"
        read -r -s -p "Contraseña del proveedor: " fe_contrasena; echo
        preguntar fe_token "Token / llave del proveedor (si aplica)"
        preguntar fe_correo "Correo de la cuenta (si aplica)"
        preguntar fe_nit "NIT (si aplica)"
        ;;
esac
preguntar fe_resolucion "Número de resolución DIAN"
preguntar fe_prefijo "Prefijo de facturación"
preguntar cod_corriente "Código producto Corriente"
preguntar cod_acpm "Código producto ACPM"
preguntar cod_extra "Código producto Extra"
preguntar cod_gas "Código producto Gas"
preguntar_si_no envia_creditos "¿Factura ventas a crédito?" "n"
preguntar_si_no multiplicar "¿Multiplicar precio por 10 (MultiplicarPorDies)?" "n"

echo
preguntar_si_no usuarios_clientes "¿Activar usuarios cliente en la web?" "s"
preguntar_si_no worker "¿Levantar worker de facturación para este cliente?" "s"

mkdir -p "$CLIENTES_DIR" "$INFRA_DIR/sites"
cp "$DEPLOY_DIR/cliente/cliente.env.example" "$archivo"
chmod 600 "$archivo"

escribir_env CLIENTE "$cliente" "$archivo"
escribir_env CLIENTE_DATOS "$cliente_datos" "$archivo"
escribir_env DOMINIO_BASE "$dominio" "$archivo"
escribir_env SQL_DATABASE "$sql_database" "$archivo"
escribir_env SQL_USER "app_${nombre_sql}" "$archivo"
escribir_env SQL_PASSWORD "$(generar_clave 24)" "$archivo"
escribir_env MONGO_USER "app_${nombre_sql}" "$archivo"
escribir_env MONGO_PASSWORD "$(generar_clave 24)" "$archivo"
escribir_env JWT_SECRET "$(generar_clave 48)" "$archivo"
escribir_env FE_PROVEEDOR "$proveedor" "$archivo"
escribir_env FE_URL "$fe_url" "$archivo"
escribir_env FE_TOKEN "$fe_token" "$archivo"
escribir_env FE_ACCOUNT_ID "$fe_account" "$archivo"
escribir_env FE_CORREO "$fe_correo" "$archivo"
escribir_env FE_USUARIO "$fe_usuario" "$archivo"
escribir_env FE_CONTRASENA "$fe_contrasena" "$archivo"
escribir_env FE_NIT "$fe_nit" "$archivo"
escribir_env FE_RESOLUCION "$fe_resolucion" "$archivo"
escribir_env FE_PREFIJO "$fe_prefijo" "$archivo"
escribir_env FE_COD_CORRIENTE "$cod_corriente" "$archivo"
escribir_env FE_COD_ACPM "$cod_acpm" "$archivo"
escribir_env FE_COD_EXTRA "$cod_extra" "$archivo"
escribir_env FE_COD_GAS "$cod_gas" "$archivo"
escribir_env FE_ENVIA_CREDITOS "$envia_creditos" "$archivo"
escribir_env FE_MULTIPLICAR_POR_DIEZ "$multiplicar" "$archivo"
escribir_env FEATURE_FLAGS "'{\"usuarios-clientes\": $usuarios_clientes}'" "$archivo"
escribir_env WORKER_HABILITADO "$worker" "$archivo"

"$DEPLOY_DIR/scripts/generar-sitio.sh" "$cliente"

echo
"$DEPLOY_DIR/scripts/validar-env.sh" "$cliente"
echo
ok "Cliente creado: $archivo"
echo "   Revise el archivo si necesita ajustar algo y luego despliegue con:"
echo "   deploy/scripts/desplegar.sh up $cliente"
