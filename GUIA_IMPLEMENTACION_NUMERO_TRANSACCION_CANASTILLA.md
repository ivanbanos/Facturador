# Guia de implementacion: numeroTransaccion en factura canastilla

## Objetivo
Incorporar el campo `numeroTransaccion` en el flujo completo de factura canastilla:
- Captura en frontend.
- Persistencia en base de datos.
- Transporte por API y servicios.
- Envio a facturacion electronica (Silog2).
- Impresion en ticket de canastilla.

## 1. Base de datos (SQL)
Archivo: `SP/Canastilla.sql`

### Cambios aplicados
1. Se agrego la columna `numeroTransaccion` en la tabla `FacturasCanastilla` dentro del script de creacion:
- Tipo: `VARCHAR(50)`
- Nulable: `NULL`

2. Se agrego migracion defensiva para entornos existentes:
- `IF COL_LENGTH('dbo.FacturasCanastilla', 'numeroTransaccion') IS NULL`
- `ALTER TABLE dbo.FacturasCanastilla ADD numeroTransaccion VARCHAR(50) NULL`

3. Se actualizo el procedimiento almacenado `CrearFacturaCanastilla`:
- Nuevo parametro `@numeroTransaccion VARCHAR(50) = NULL`.
- Se incluye la columna en el `INSERT`.

### Despliegue recomendado
1. Ejecutar el script actualizado en el motor SQL del ambiente destino.
2. Verificar columna:
- `SELECT TOP 1 numeroTransaccion FROM dbo.FacturasCanastilla;`
3. Verificar definicion del SP:
- `sp_helptext 'dbo.CrearFacturaCanastilla';`

## 2. API de facturacion (FacturadorApiSP)
Carpeta: `FacturadorAPI/FacturadorApiSP`

### Cambios aplicados
1. DTO de entrada actualizado:
- `Models/FacturaCanastillaRequest.cs`
- Se agrega propiedad `numeroTransaccion`.

2. Modelo de salida/uso interno actualizado:
- `Models/FacturaCanastilla.cs`
- Se agrega propiedad `numeroTransaccion`.

3. Repositorio SQL actualizado:
- `Repository/MsSqlDataBaseHandler.cs`
- En `GenerarFacturaCanastilla(...)` se envia `@numeroTransaccion` al SP.

4. Convertidor DB -> modelo actualizado:
- `Repository/Convertidor.cs`
- Se mapea `numeroTransaccion` desde DataRow si existe la columna.

## 3. Frontend (captura de canastilla)
Archivo: `facturador-web/src/components/canastilla.js`

### Cambios aplicados
1. Estado inicial del payload:
- Se agrega `numeroTransaccion` en `valorInicialObjetoPostCanastilla`.

2. Handler de formulario:
- Se agrega `handleChangeNumeroTransaccion`.

3. Campo visual:
- Nuevo input `N transaccion` en el formulario de canastilla.

### Validacion funcional
1. Abrir pantalla de canastilla.
2. Ingresar valor en `N transaccion`.
3. Confirmar en el payload enviado al backend que viaje `numeroTransaccion`.

## 4. Servicio local y modelos compartidos
Se actualizo la propagacion del campo en modelos intermedios para que no se pierda en conversiones.

### Archivos impactados
- `EnviadorInformacionService/Models/FacturaCanastilla.cs`
- `FactoradorEstacionesModelo/Objetos/FacturaCanastilla.cs`
- `FacturacionelectronicaCore/.../Negocio/Modelo/FacturaCanastilla.cs`
- `FacturacionelectronicaCore/.../Repositorio/Entities/FacturaCanastilla.cs`
- `FacturacionelectronicaCore.Worker/Worker.cs` (mapeo entity -> modelo)
- `EnviadorInformacionService/Convertidor/Convertidor.cs` (mapeo DataRow -> objeto)

## 5. Envio a Silog2
Archivo: `FacturacionelectronicaCore/.../Contabilidad/FacturacionElectronica/FacturacionSilog2.cs`

### Cambios aplicados
1. En facturas canastilla, el `TransactionReference` del medio de pago usa `factura.numeroTransaccion`.
2. Regla de apellido para NIT aplicada en canastilla y ordenes:
- Si el tipo de identificacion es NIT, `FirstLastName` se envia vacio.
- Si no es NIT, se conserva el apellido.

## 6. Impresion
### Ticket canastilla
Archivo: `EnviadorInformacionService/CanastillaService.cs`

Cambio:
- Se imprime linea `N Tran` cuando `numeroTransaccion` tiene valor.

### Cierre de turno (canastilla)
Archivo: `EnviadorInformacionService/ImpresionService.cs`

Cambio:
- La cantidad de canastilla se imprime con 3 decimales (`F3`) para evitar redondeo a entero.

## 7. Validacion tecnica ejecutada
Compilaciones realizadas:
1. `EnviadorInformacionService.csproj` -> OK (sin errores, con warnings preexistentes).
2. `FacturadorApiSP.csproj` -> OK (sin errores, con warnings de paquetes/vulnerabilidades).
3. `FacturacionelectronicaCore.Web.sln` -> OK (sin errores, con warnings preexistentes).

## 8. Pruebas funcionales sugeridas
1. Crear factura canastilla con `numeroTransaccion` y verificar persistencia en SQL.
2. Confirmar que el ticket impreso incluya `N Tran` y placa.
3. Enviar factura canastilla a Silog2 y validar `TransactionReference`.
4. Probar tercero NIT en canastilla y orden:
- Verificar que `FirstLastName` se envie vacio.
5. Ejecutar cierre de turno con productos canastilla decimales:
- Verificar impresion con 3 decimales.

## 9. Rollback rapido (si se requiere)
1. Ocultar/deshabilitar campo UI `numeroTransaccion`.
2. Retirar parametro en capa API/SP.
3. Mantener columna en BD sin uso (recomendado) para evitar perdida de compatibilidad.
4. Revertir solamente mapeos de envio a proveedor si hay incidencia externa.

## Notas
- El campo se manejo como opcional para no romper flujo actual.
- La compatibilidad se preserva en ambientes que aun no diligencian `numeroTransaccion`.
