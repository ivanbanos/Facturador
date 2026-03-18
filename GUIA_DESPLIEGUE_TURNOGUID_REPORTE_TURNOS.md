# Guia paso a paso de despliegue

## 1. Objetivo
Desplegar en produccion la funcionalidad de enlace de turno (`TurnoGuid`) para facturas/ordenes/canastilla y el nuevo reporte de turnos por dia con filtros dinamicos.

## 2. Alcance de componentes

### SQL local de estacion
- `SP/StoreProceduresFacturas.sql`
- `SP/StoreProceduresVentas.sql`
- `SP/Canastilla.sql`

### Servicios de estacion
- `EnviadorInformacionService`

### Backend central
- `FacturacionelectronicaCore/FacturacionelectronicaCore.Web`

### Frontend central
- `EstacionesWeb`

## 3. Ventana y prerrequisitos

1. Definir ventana de mantenimiento fuera de hora pico.
2. Confirmar respaldos:
   - Backup de base local de estacion.
   - Backup de base central y respaldo operativo de colecciones Mongo.
3. Confirmar artefactos compilados:
   - `dotnet build` de `EnviadorInformacionService` en verde.
   - `dotnet build` de `FacturacionelectronicaCore.Web` en verde.
   - `npm run build` de `EstacionesWeb` en verde.
4. Confirmar acceso a:
   - SQL local de cada estacion.
   - Servidor backend central.
   - Hosting del frontend.
   - Logs de enviador y logs de API.

## 3.1 Parametros del entorno (llenar antes de iniciar)

- Ambiente: ______________________
- Ventana inicio: ______________________
- Ventana fin: ______________________
- Responsable tecnico: ______________________
- Responsable funcional: ______________________
- Servidor API (IIS): ______________________
- AppPool API: ______________________
- Sitio/API path fisico: ______________________
- Servidor frontend (IIS o hosting): ______________________
- Sitio frontend path fisico: ______________________
- Estaciones objetivo:
   - ______________________
   - ______________________
- Instancia SQL local por estacion:
   - ______________________
   - ______________________
- Conexion base central (solo lectura para validacion): ______________________
- Ruta de binario EnviadorInformacionService por estacion: ______________________
- Ruta de backup local por estacion: ______________________

## 4. Orden obligatorio de despliegue

1. SQL local de estacion.
2. Backend central.
3. Frontend central.
4. Servicios de estacion.

Este orden evita incompatibilidades de contrato (campos/endpoint nuevos no disponibles).

## 5. Paso a paso operativo

### Paso 1 - Congelamiento de cambios
1. Notificar inicio de ventana.
2. Pausar nuevos despliegues paralelos de otros equipos.
3. Confirmar version objetivo (tag/commit) para cada componente.

### Paso 2 - Ejecutar SQL local (por cada estacion)
1. Conectarse a la base local de la estacion.
2. Ejecutar, en este orden:
   1. `SP/StoreProceduresFacturas.sql`
   2. `SP/StoreProceduresVentas.sql`
   3. `SP/Canastilla.sql`
3. Verificar objetos clave:
   - Columnas `turnoguid` en `FacturasPOS`, `OrdenesDeDespacho`, `FacturasCanastilla`.
   - Procedimiento `PrepararRetroactivoTurnosPendientes` creado.
   - `CrearFactura` recibiendo parametro `@turnoGuid`.
4. Ejecutar reproceso inicial controlado:
   - `PrepararRetroactivoTurnosPendientes`.

Comandos SQL sugeridos de verificacion:

```sql
-- Verificar columnas turnoguid
SELECT TABLE_NAME, COLUMN_NAME
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME IN ('FacturasPOS', 'OrdenesDeDespacho', 'FacturasCanastilla')
   AND COLUMN_NAME = 'turnoguid';

-- Verificar SP retroactivo
SELECT name
FROM sys.procedures
WHERE name = 'PrepararRetroactivoTurnosPendientes';

-- Ejecutar retroactivo controlado
EXEC dbo.PrepararRetroactivoTurnosPendientes;

-- Muestreo rapido post retroactivo
SELECT TOP(20) ventaId, turnoguid, turnoEnviado, fecha
FROM FacturasPOS
ORDER BY ventaId DESC;
```

Checklist rapido de verificacion SQL:
- Existen columnas `turnoguid`: Si/No.
- SP retroactivo creado: Si/No.
- Errores al ejecutar script: Si/No.

### Paso 3 - Desplegar backend central
1. Publicar version objetivo de `FacturacionelectronicaCore.Web`.
2. Reiniciar servicio/app pool correspondiente.
3. Verificar salud de API.
4. Validar endpoint nuevo:
   - `POST /api/Turnos/GetTurnoReporteDia` responde HTTP 200 con payload valido.
5. Validar ingesta de campos nuevos:
   - `TurnoGuid` en factura/canastilla/orden no genera error de serializacion ni mapeo.

Comandos PowerShell sugeridos (IIS):

```powershell
Import-Module WebAdministration

# Parar app pool
Stop-WebAppPool -Name "<APPPOOL_API>"

# Copiar artefactos publicados (ajustar rutas)
Copy-Item "C:\Deploy\FacturacionelectronicaCore.Web\*" "<RUTA_FISICA_API>" -Recurse -Force

# Iniciar app pool
Start-WebAppPool -Name "<APPPOOL_API>"
```

Validacion HTTP sugerida:

```powershell
$body = @{
   fecha = "2026-03-17"
   estacion = "<GUID_ESTACION>"
   empleado = $null
   isla = $null
   numeroTurno = $null
   surtidor = $null
   manguera = $null
   combustible = $null
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://<HOST_API>/api/Turnos/GetTurnoReporteDia" -Method POST -ContentType "application/json" -Body $body
```

Checklist rapido backend:
- API arriba: Si/No.
- Endpoint diario de turnos operativo: Si/No.
- Logs sin errores criticos de ingesta: Si/No.

### Paso 4 - Desplegar frontend central
1. Publicar build de `EstacionesWeb`.
2. Limpiar cache/CDN segun plataforma.
3. Abrir modulo de Reportes > Turnos.
4. Confirmar que la UI usa selector de un solo dia y filtros dinamicos.
5. Validar exportes PDF y Excel desde la vista filtrada.

Comandos sugeridos:

```powershell
# En servidor de build
Set-Location "C:\Users\ivana\Documents\GitHub\Facturador\EstacionesWeb"
npm ci
npm run build

# Copiar build al hosting (ajustar rutas)
Copy-Item ".\build\*" "<RUTA_FISICA_FRONTEND>" -Recurse -Force
```

Checklist rapido frontend:
- UI dia unico visible: Si/No.
- Filtros aplican sin recargar modulo: Si/No.
- Exportes funcionales: Si/No.

### Paso 5 - Actualizar servicios de estacion
1. Publicar binarios de `EnviadorInformacionService`.
2. Reiniciar servicio en cada estacion.
3. Verificar en logs:
   - Sincronizacion de turnos del mes en curso.
   - Ejecucion de retroactivo de turnos pendientes.
   - No falsos positivos de `turnoEnviado` cuando falla asociacion.

Comandos PowerShell sugeridos (servicio Windows):

```powershell
# Ajustar nombre real del servicio
Stop-Service -Name "EnviadorInformacionService" -Force

# Copiar binarios desplegados
Copy-Item "C:\Deploy\EnviadorInformacionService\*" "<RUTA_SERVICIO_ESTACION>" -Recurse -Force

Start-Service -Name "EnviadorInformacionService"

# Verificacion estado
Get-Service -Name "EnviadorInformacionService"
```

Checklist rapido enviador:
- Servicio arriba: Si/No.
- Ciclo de sincronizacion ejecutado: Si/No.
- Errores bloqueantes: Si/No.

### Paso 6 - Smoke test post-despliegue (obligatorio)
1. Generar 1 factura combustible con isla y validar `TurnoGuid` extremo a extremo.
2. Generar 1 factura canastilla con isla y validar `TurnoGuid` extremo a extremo.
3. Ejecutar reenvio de pendientes y validar que no se pierda el enlace de turno.
4. Consultar reporte de turnos por un dia historico, filtrar y exportar PDF/Excel.

Si cualquiera de estas 4 pruebas falla, no cerrar la ventana sin decision de contingencia.

## 6. Validaciones tecnicas recomendadas

### SQL local
1. Muestreo de registros recientes en `FacturasPOS` y `OrdenesDeDespacho` con `turnoguid` no nulo.
2. Muestreo de `FacturasCanastilla` con `isla` y `turnoguid`.

Consultas SQL recomendadas:

```sql
SELECT TOP(20) ventaId, turnoguid, fecha, turnoEnviado
FROM FacturasPOS
WHERE turnoguid IS NOT NULL
ORDER BY ventaId DESC;

SELECT TOP(20) ventaId, turnoguid, fecha, turnoEnviado
FROM OrdenesDeDespacho
WHERE turnoguid IS NOT NULL
ORDER BY ventaId DESC;

SELECT TOP(20) facturacanastillaId, isla, turnoguid, fecha
FROM FacturasCanastilla
ORDER BY facturacanastillaId DESC;
```

### Backend/Mongo
1. Verificar en documentos nuevos que `TurnoGuid` se persiste en:
   - Factura.
   - FacturaCanastilla.
   - OrdenDeDespacho.

Validacion operativa sugerida:
- Revisar logs de ingesta de API en la ventana del despliegue.
- Validar al menos 1 documento nuevo por coleccion con `TurnoGuid` no vacio.

### Frontend
1. Validar que el total exportado corresponda al total visible en pantalla para una combinacion de filtros.

## 7. Criterio de salida de despliegue
Se considera exitoso cuando:
1. SQL aplicado sin errores en estaciones objetivo.
2. API y frontend operativos.
3. Enviador sincronizando sin errores bloqueantes.
4. Smoke test (4 de 4) en verde.

## 8. Plan de rollback

### Nivel 1 - Frontend
1. Revertir despliegue de `EstacionesWeb` a build anterior.

### Nivel 2 - Backend
1. Revertir `FacturacionelectronicaCore.Web` a version previa estable.
2. Reiniciar app pool/servicio.

### Nivel 3 - Servicios de estacion
1. Volver binario previo de `EnviadorInformacionService`.
2. Reiniciar servicio.

### Nivel 4 - SQL local
1. Restaurar backup local de estacion tomado antes de ventana.
2. Reejecutar validaciones basicas de facturacion.

Nota: rollback SQL se ejecuta solo si hay impacto operativo real que no pueda mitigarse en caliente.

## 9. Registro de ejecucion

Fecha:
Responsable tecnico:
Ambiente:

Estaciones incluidas:
-

Resultado por etapa:
- Paso 2 SQL local:
- Paso 3 Backend:
- Paso 4 Frontend:
- Paso 5 Enviador:
- Paso 6 Smoke test:

Estado final:
- Exitoso / Parcial / Revertido

Observaciones:
-

## 10. Cierre de ventana (checklist final)

1. SQL aplicado en el 100% de estaciones objetivo.
2. API estable por al menos 15 minutos sin errores criticos.
3. Frontend estable y reporte diario operando.
4. Servicio enviador estable en estaciones objetivo.
5. Smoke test 4/4 exitoso.
6. Aprobacion funcional registrada.
7. Comunicacion de cierre enviada a interesados.