# Documento de cambios: soporte para dos formas de pago

## Objetivo
Implementar el soporte para registrar una venta con **dos formas de pago**, manteniendo compatibilidad con escenarios anteriores (una sola forma de pago) y evitando errores cuando los campos adicionales no vienen en el resultado de base de datos.

## Qué se hizo

### 1) Modelos de datos actualizados
Se validó/estructuró el modelo para guardar:
- `codigoFormaPago` (forma principal)
- `codigoFormaPago2` (segunda forma de pago, opcional)
- `total1` (valor asociado a la forma principal)
- `total2` (valor asociado a la segunda forma de pago, opcional)

Esto quedó reflejado en:
- `FacturadorAPI/FacturadorAPI/Models/FacturaSiges.cs`
- `FacturadorAPI/FacturadorApiSP/Models/FacturaSiges.cs`

### 2) Conversión desde DataTable (Repository/Convertidor)
En los convertidores de factura se agregó/aseguró el mapeo de segunda forma de pago con validación defensiva:
- Se revisa si existe la columna `codigoFormaPago2` y si tiene valor antes de asignar.
- Se revisa si existe la columna `total2` y si tiene valor antes de asignar.

Con esto se evita que falle la conversión cuando la consulta SQL no trae aún esos campos o vienen nulos.

Archivos:
- `FacturadorAPI/FacturadorAPI/Repository/Convertidor.cs`
- `FacturadorAPI/FacturadorApiSP/Repository/Convertidor.cs`

### 3) Persistencia hacia base de datos
En la creación de factura canastilla se envían los nuevos parámetros al procedimiento almacenado:
- `@COD_FOR_PAG_2`
- `@total2`

Se conserva además la forma principal y su total (`@COD_FOR_PAG`, `@total1`).

Archivos:
- `FacturadorAPI/FacturadorAPI/Repository/MsSqlDataBaseHandler.cs`
- `FacturadorAPI/FacturadorApiSP/Repository/MsSqlDataBaseHandler.cs`

## Resultado funcional
- El sistema permite manejar ventas con 1 o 2 formas de pago.
- Si no hay segunda forma de pago, el flujo sigue funcionando sin romperse.
- La segunda forma y su valor se propagan desde consulta, modelo y capa de persistencia.

## Comportamiento actual del negocio
- Al **crear** una factura, el flujo inicia con **una sola forma de pago** (forma principal).
- La **segunda forma de pago** se agrega posteriormente en la etapa de **actualización** de la factura.
- En consecuencia, es normal que al momento de creación inicial `codigoFormaPago2` y `total2` vayan nulos y se completen después.

## Consideraciones
- Para aprovechar completamente este soporte, los SP/consultas deben exponer `codigoFormaPago2` y `total2` cuando aplique.
- La UI o el origen de datos que arme la factura debe enviar estos campos para escenarios de pago mixto.
