# Implementación de Factura Electrónica en Orden de Despacho

## Funcionalidades Implementadas

### 1. Servicio de Factura Electrónica
- **Archivo**: `src/services/FacturaElectronicaService.js`
- **Funcionalidad**: 
  - Llamada al endpoint `obtenerinformacionfacturaelectronica`
  - Parseo de la respuesta (campos separados por '|')
  - Extracción de consecutivo, fecha y CUFE
  - Generación de URL para QR code

### 2. Componente QR Code
- **Archivo**: `src/components/QRCodeDisplay.js`
- **Funcionalidad**: 
  - Generación de códigos QR usando la librería `qrcode`
  - Renderizado en canvas con tamaño configurable
  - URL del QR: `https://catalogo-vpfe.dian.gov.co/User/SearchDocument?DocumentKey={CUFE}`

### 3. Actualización de Orden de Despacho Detalle
- **Archivo**: `src/views/Reportes/OrdenDespachoDetalle.js`
- **Funcionalidades añadidas**:
  - Estado para manejar información de factura electrónica
  - Carga automática de información cuando `idFacturaElectronica` está presente
  - Sección colapsable para mostrar información de factura electrónica
  - Visualización del código QR para consulta en DIAN

## Estructura de Datos

### Respuesta del Endpoint
```
campo1|campo2|consecutivo|fecha|cufe|...
```

### Datos Extraídos
- **Consecutivo**: Campo 3 (consecutivo con prefijo)
- **Fecha**: Campo 4 (fecha de la factura)
- **CUFE**: Campo 5 (Código Único de Facturación Electrónica)

## Interfaz de Usuario

### Sección de Factura Electrónica
- Solo se muestra si `orden.idFacturaElectronica` existe
- Diseño colapsable con header azul
- Dos columnas:
  - Izquierda: Tabla con información de la factura
  - Derecha: Código QR para consulta en DIAN

### Elementos Visuales
- Botón para expandir/contraer la sección
- Spinner de carga mientras se obtiene la información
- Enlace directo a la consulta en DIAN
- Código QR generado dinámicamente

## Manejo de Errores
- Validación de datos antes del parseo
- Mensajes de error con toast notifications
- Fallback para casos donde no se puede cargar la información
- Estados de carga para mejor UX

## Dependencias Agregadas
- `qrcode`: Para generación de códigos QR

## Uso
1. Al acceder al detalle de una orden con `idFacturaElectronica`
2. Se carga automáticamente la información de factura electrónica
3. El usuario puede expandir la sección para ver:
   - Consecutivo de la factura
   - Fecha de emisión
   - CUFE
   - Código QR para consulta en DIAN
   - Enlace directo a la consulta
