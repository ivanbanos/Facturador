# FacturasCanastilla Component

Este componente proporciona una interfaz completa para ver y reportar facturas de canastilla con funcionalidades avanzadas de filtrado y generación de reportes.

## Características

### Visualización de Datos
- **Vista de Tabla**: Muestra todas las facturas de canastilla en una tabla responsiva
- **Filtros por Fecha**: Filtrado por rango de fechas (fecha inicial y final)
- **Búsqueda en Tiempo Real**: Filtro de texto por consecutivo, tercero, identificación o fecha
- **Resumen Automático**: Cálculo automático de totales (cantidad, subtotal, descuento, IVA, total)

### Reportes Avanzados
- **Reporte Completo**: Utiliza el endpoint `/api/FacturasCanastilla/GetFacturasReporte` para obtener datos completos
- **Detalle por Forma de Pago**: Accordion desplegable con consolidado por formas de pago
- **Detalle por Artículo**: Accordion desplegable con consolidado por artículos de canastilla
- **Exportación PDF**: Generación de reportes PDF completos con todas las secciones

### Interfaz de Usuario
- **Modal de Detalle**: Ver información completa de cada factura incluyendo items
- **Estados Visuales**: Badges para mostrar el estado de las facturas (Activa/Anulada)
- **Responsive Design**: Funciona en desktop y dispositivos móviles
- **Loading States**: Indicadores de carga durante las operaciones

## Estructura de Datos

### FacturaCanastilla
Cada factura contiene:
- **id/facturasCanastillaId** (string): Identificador único
- **consecutivo** (int): Número consecutivo de la factura
- **fecha** (DateTime): Fecha de la factura
- **terceroId** (object): Información del tercero (nombre, identificación)
- **subtotal** (float): Subtotal de la factura
- **descuento** (float): Descuento aplicado
- **iva** (float): IVA calculado
- **total** (float): Total de la factura
- **estado** (string): Estado de la factura
- **canastillas** (array): Items de la factura

### ReporteFacturasCanastilla
El reporte completo incluye:
- **facturas** (array): Lista de facturas
- **detalleFormaPago** (array): Consolidado por forma de pago
- **detalleArticulo** (array): Consolidado por artículo

## Uso

### Navegación
Acceder vía: `/FacturasCanastilla` o a través del menú principal "Facturas de Canastilla"

### Operaciones Básicas

#### Cargar Facturas
1. Seleccionar fecha inicial y final (por defecto: último mes)
2. Hacer clic en "Filtrar" para cargar los datos
3. Los datos se obtienen usando el filtro de búsqueda configurado automáticamente

#### Búsqueda y Filtrado
- **Filtro por Fechas**: Campos obligatorios para definir el rango de consulta
- **Búsqueda de Texto**: Campo opcional para filtrar dentro de los resultados cargados
- **Criterios de Búsqueda**: Consecutivo, nombre del tercero, identificación, fecha

#### Ver Detalle de Factura
1. Hacer clic en el ícono del ojo en cualquier fila de la tabla
2. Se abre un modal con información completa
3. Incluye la lista de items de canastilla si está disponible

#### Generar Reporte PDF
1. Cargar facturas con los filtros deseados
2. Hacer clic en "Exportar PDF"
3. Se genera un reporte completo con:
   - Encabezado con información de la estación
   - Resumen general con totales
   - Tabla detallada de facturas
   - Secciones adicionales (formas de pago, artículos) si existen datos

## Detalles Técnicos

### Servicios Utilizados
- **FacturasCanastillasService**: Servicio principal para operaciones CRUD
- **Toast**: Para notificaciones al usuario
- **PDFMake**: Para generación de reportes PDF

### Endpoints de API Utilizados
- `POST /api/FacturasCanastilla/GetFacturasReporte` - Obtener reporte completo
- `GET /api/FacturasCanastilla/{id}` - Obtener factura específica
- `GET /api/FacturasCanastilla/detalle/{id}` - Obtener detalle de factura

### Filtro de Búsqueda
El componente utiliza el objeto `FiltroBusqueda` con:
- **fechaInicial** (DateTime): Fecha de inicio del rango
- **fechaFinal** (DateTime): Fecha final del rango
- **identificacion** (string): Filtro por identificación (opcional)
- **nombreTercero** (string): Filtro por nombre (opcional)
- **estacion** (Guid): GUID de la estación (automático desde localStorage)

### Formato de Moneda
- Utiliza `Intl.NumberFormat` para formato de peso colombiano (COP)
- Muestra valores sin decimales para mejor legibilidad

### Generación de PDF
- **Encabezado**: Nombre y NIT de la estación
- **Marca de Agua**: "SIGES Soluciones"
- **Pie de Página**: Información de la estación y paginación
- **Estilos**: Tablas responsivas con alineación apropiada
- **Secciones Dinámicas**: Solo incluye secciones con datos disponibles

## Configuración

### Variables Requeridas en localStorage
- `estacionGuid`: GUID de la estación actual
- `estacionNombre`: Nombre de la estación (para reportes)
- `estacionNit`: NIT de la estación (para reportes)

### Dependencias
- React 18+
- CoreUI React
- PDFMake
- React Router DOM

## Estados del Componente

### Estados Principales
- `reporteData`: Datos completos del reporte obtenidos del API
- `facturas`: Array de facturas para mostrar en la tabla
- `loading`: Estado de carga durante las operaciones
- `totalesCalculados`: Objeto con totales calculados automáticamente

### Estados de Filtrado
- `fechaInicial/fechaFinal`: Rango de fechas para el filtro
- `searchTerm`: Término de búsqueda para filtrado local

### Estados de Modal
- `showDetailModal`: Controla la visibilidad del modal de detalle
- `selectedFactura`: Factura seleccionada para mostrar en detalle
- `loadingDetail`: Estado de carga del detalle de la factura

## Validaciones

### Filtros de Fecha
- Ambas fechas son obligatorias para realizar la consulta
- La fecha inicial no puede ser mayor que la fecha final
- Se valida antes de enviar la petición al servidor

### Manejo de Errores
- Errores de autenticación redirigen al login
- Errores de red se muestran como notificaciones toast
- Estados de carga previenen múltiples peticiones simultáneas

## Notas de Implementación

- El componente está optimizado para grandes volúmenes de datos
- La búsqueda local evita consultas innecesarias al servidor
- Los reportes PDF se generan del lado del cliente para mejor rendimiento
- Compatible con diferentes resoluciones de pantalla
- Soporta temas claros y oscuros de CoreUI
