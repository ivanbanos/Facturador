#!/bin/bash
# Publica /dacpac/schema.dacpac en la base del cliente. Es idempotente: sqlpackage compara el esquema
# actual con el dacpac y solo aplica las diferencias. Nunca borra objetos que no estén en el dacpac
# y se detiene si un cambio pudiera perder datos.
set -euo pipefail

DACPAC=/dacpac/schema.dacpac
: "${SQL_HOST:?Falta SQL_HOST}"
: "${SQL_DATABASE:?Falta SQL_DATABASE}"
: "${SQL_USER:?Falta SQL_USER}"
: "${SQL_PASSWORD:?Falta SQL_PASSWORD}"

if [ ! -f "$DACPAC" ]; then
    echo "AVISO: no existe deploy/db/schema.dacpac; no se aplica esquema."
    echo "       Bases restauradas desde backup funcionan igual. Para clientes nuevos extraiga el esquema"
    echo "       con deploy/scripts/extraer-esquema.sh antes de desplegar."
    exit 0
fi

echo "Aplicando esquema a ${SQL_DATABASE} en ${SQL_HOST}..."
sqlpackage /Action:Publish \
    /SourceFile:"$DACPAC" \
    /TargetServerName:"$SQL_HOST" \
    /TargetDatabaseName:"$SQL_DATABASE" \
    /TargetUser:"$SQL_USER" \
    /TargetPassword:"$SQL_PASSWORD" \
    /TargetTrustServerCertificate:True \
    /p:CreateNewDatabase=False \
    /p:BlockOnPossibleDataLoss=True \
    /p:DropObjectsNotInSource=False \
    /p:AllowIncompatiblePlatform=True \
    /p:IgnorePermissions=True \
    /p:IgnoreRoleMembership=True \
    /p:IgnoreLoginSids=True \
    /p:ExcludeObjectTypes="Users;Logins;RoleMembership;Permissions;Credentials;DatabaseScopedCredentials" \
    /p:ScriptDatabaseOptions=False

echo "Esquema aplicado."
