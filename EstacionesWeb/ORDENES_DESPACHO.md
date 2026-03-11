# Órdenes de Despacho - Documentación

## 📦 Funcionalidad Implementada

Se ha creado un sistema completo para la gestión de órdenes de despacho con las siguientes características:

### 🎯 Páginas Principales

#### 1. **Lista de Órdenes de Despacho** (`/OrdenesDespacho`)
- **Filtro automático** del día actual al cargar
- **Filtros personalizables** por rango de fechas
- **Búsqueda en tiempo real** por número de orden, cliente, placa o conductor
- **Paginación** de resultados (10 items por página)
- **Estadísticas visuales** con cards de resumen
- **Tabla responsive** con información completa

#### 2. **Detalle de Orden** (`/OrdenDespachoDetalle/:ordenId`)
- **Información completa** de la orden
- **Detalle de productos** con cantidades y precios
- **Función de impresión** directa
- **Descarga en PDF** con formato profesional
- **Cambio de estado** a despachado con observaciones

### 🛠️ Servicio de API

#### **OrdenesDespachoService** - Actualizado para OpenAPI

**Endpoints OpenAPI compatibles:**
- `POST /api/OrdenesDeDespacho` - Filtro de búsqueda de órdenes
- `POST /api/OrdenesDeDespacho/AddOrdenesImprimir` - Agregar órdenes para imprimir
- `POST /api/OrdenesDeDespacho/AnularOrdenes` - Anular órdenes
- `GET /api/OrdenesDeDespacho/EnviarFacturacion/{ordenGuid}` - Enviar facturación por GUID
- `GET /api/OrdenesDeDespacho/EnviarFacturacion/{idVentaLocal}/{estacion}` - Enviar facturación por ID local
- `POST /api/OrdenesDeDespacho/CrearFacturaOrdenesDeDespacho` - Crear factura
- `GET /api/OrdenesDeDespacho/ObtenerOrdenDespachoPorIdVentaLocal/{idVentaLocal}/{estacion}` - Obtener orden por ID local
- `GET /api/OrdenesDeDespacho/ObtenerOrdenesPorTurno/{turno}` - Obtener órdenes por turno

**Métodos principales:**
```javascript
// Nuevos métodos OpenAPI
- getOrdenesDespacho(filtroBusqueda) - Filtro principal
- addOrdenesImprimir(ordenesGuids) - Agregar para imprimir
- anularOrdenes(ordenesGuids) - Anular órdenes
- enviarFacturacionPorGuid(ordenGuid) - Enviar facturación
- crearFacturaOrdenes(ordenesGuids) - Crear factura
- getOrdenPorIdVentaLocal(idVentaLocal, estacion) - Obtener por ID local
- getOrdenesPorTurno(turno) - Obtener por turno

// Métodos de compatibilidad (actualizados)
- getOrdenById(ordenId) - Busca orden por ID en resultados
- getOrdenDetalle(ordenId) - Obtiene detalle incluido en respuesta
- updateEstadoOrden(ordenId, nuevoEstado) - Usa anularOrdenes
- despacharOrden(ordenId, observaciones) - Usa enviarFacturacionPorGuid
- getOrdenesByDateRange(fechaInicial, fechaFinal) - Método de conveniencia
```

### 🔄 Migración a OpenAPI

#### **Cambios Principales**
1. **Endpoints actualizados** para coincidir con la especificación OpenAPI
2. **Métodos de compatibilidad** mantienen la funcionalidad existente
3. **Uso de estación GUID** desde localStorage en todos los endpoints
4. **Manejo de errores** mejorado con validación de tokens
5. **Filtros de búsqueda** estructurados según OpenAPI

#### **Compatibilidad con Código Existente**
- ✅ `OrdenesDespacho.js` - Funciona sin cambios
- ✅ `OrdenDespachoDetalle.js` - Funciona sin cambios
- ✅ Todos los métodos existentes mantienen su firma
- ✅ Respuestas estructuradas igual que antes

### 🎨 Características de Diseño

#### **Interfaz Moderna**
- **Cards estadísticos** con iconos y colores
- **Tabla responsive** con scroll horizontal
- **Badges de estado** con colores distintivos
- **Botones de acción** con iconos intuitivos
- **Loader states** durante las operaciones

#### **Experiencia de Usuario**
- **Filtros intuitivos** con fechas predeterminadas
- **Búsqueda instantánea** sin necesidad de botón
- **Paginación automática** para mejor rendimiento
- **Navegación fluida** entre páginas
- **Mensajes de estado** claros y precisos

### 📄 Impresión y PDF

#### **Formato Profesional**
- **Encabezado corporativo** con información de la estación
- **Datos de la orden** estructurados y legibles
- **Tabla de productos** con cantidades y precios
- **Totales calculados** automáticamente
- **Pie de página** con información de contacto

#### **Funcionalidades**
- **Vista previa** antes de imprimir
- **Descarga directa** en formato PDF
- **Impresión optimizada** para papel A4
- **Formateo automático** de moneda y fechas

### 🔐 Seguridad y Validación

#### **Autenticación**
- **Validación de tokens** en todos los endpoints
- **Redirección automática** al login si no hay token
- **Manejo de sesiones** expiradas
- **Seguridad en las rutas** protegidas

#### **Validación de Datos**
- **Filtros de fecha** validados
- **Campos requeridos** verificados
- **Tipos de datos** correctos
- **Manejo de errores** robusto

### 📱 Experiencia de Usuario

#### **Responsive Design**
- **Adaptación automática** a diferentes tamaños de pantalla
- **Navegación táctil** optimizada para móviles
- **Botones grandes** para facilitar el uso
- **Scroll horizontal** en tablas en dispositivos pequeños

#### **Feedback Visual**
- **Loaders** durante las operaciones
- **Mensajes de éxito/error** con colores distintivos
- **Estados de carga** claros
- **Confirmaciones** antes de acciones críticas

### 🎯 Estados de Órdenes

#### **Estados Disponibles**
- **Pendiente** - Orden creada pero no despachada
- **En Proceso** - Orden siendo preparada
- **Despachada** - Orden completada y enviada
- **Anulada** - Orden cancelada

#### **Flujo de Estados**
1. **Creación** - Orden se crea en estado "Pendiente"
2. **Preparación** - Cambio manual a "En Proceso"
3. **Despacho** - Cambio a "Despachada" con observaciones
4. **Anulación** - Posible desde cualquier estado previo

### 📊 Métricas y Estadísticas

#### **Dashboard de Estadísticas**
- **Total de órdenes** en el período
- **Órdenes pendientes** por despachar
- **Órdenes en proceso** de preparación
- **Órdenes despachadas** completadas
- **Total monetario** de todas las órdenes

#### **Filtros Inteligentes**
- **Filtro de fecha inicial** y final
- **Búsqueda por texto** en múltiples campos
- **Paginación automática** para rendimiento
- **Contador de resultados** en tiempo real

### 🔧 Configuración Técnica

#### **Dependencias**
- **React 18+** con hooks modernos
- **CoreUI React** para componentes UI
- **React Router** para navegación
- **PDFMake** para generación de documentos
- **localStorage** para configuración de estación

#### **Rutas Configuradas**
```javascript
{ path: '/OrdenesDespacho', name: 'Órdenes de Despacho', element: OrdenesDespacho }
{ path: '/OrdenDespachoDetalle/:ordenId', name: 'Detalle de Orden', element: OrdenDespachoDetalle }
```

#### **Navegación**
- **Menú lateral** con acceso directo
- **Breadcrumbs** para contexto
- **Botones de navegación** entre páginas
- **URLs amigables** con parámetros

### 🚀 Características Avanzadas

#### **Impresión Optimizada**
- **Vista previa** antes de imprimir
- **Formato profesional** para documentos oficiales
- **Compatibilidad** con impresoras estándar
- **Tamaño de papel** configurable

#### **Gestión de Estados**
- **Modal de confirmación** para cambios críticos
- **Observaciones** en cambios de estado
- **Historial** de cambios (preparado para implementar)
- **Validación** de flujo de estados

#### **Rendimiento**
- **Lazy loading** de componentes
- **Paginación** para conjuntos grandes de datos
- **Filtros optimizados** sin recargas innecesarias
- **Cache** de datos del servicio

### 📋 Casos de Uso

#### **Operador de Estación**
1. **Consulta diaria** de órdenes pendientes
2. **Búsqueda específica** por cliente o placa
3. **Impresión** de órdenes para despacho
4. **Cambio de estado** cuando completa el despacho

#### **Supervisor**
1. **Revisión de estadísticas** del día
2. **Seguimiento** de órdenes en proceso
3. **Anulación** de órdenes problemáticas
4. **Generación de reportes** en PDF

#### **Administrador**
1. **Análisis** de tendencias de órdenes
2. **Configuración** de filtros automáticos
3. **Supervisión** de operaciones
4. **Auditoría** de cambios de estado

### 🐛 Manejo de Errores

#### **Validaciones**
- **Campos requeridos** verificados antes de envío
- **Formato de fechas** validado
- **Rango de fechas** lógico
- **Autenticación** verificada constantemente

#### **Mensajes de Error**
- **Errores de conexión** - Mensaje claro sobre problemas de red
- **Errores de autenticación** - Redirección automática al login
- **Errores de datos** - Mensajes específicos sobre el problema
- **Errores del servidor** - Información general para el usuario

### 📈 Mejoras Futuras

#### **Funcionalidades Planeadas**
- **Historial de cambios** de estado
- **Notificaciones push** para nuevas órdenes
- **Filtros avanzados** por tipo de producto
- **Integración con inventario** en tiempo real
- **Reportes avanzados** con gráficos
- **Exportación** a Excel
- **Firma digital** para confirmación de despacho

#### **Optimizaciones**
- **Cache inteligente** de consultas frecuentes
- **Actualización automática** de datos
- **Compresión** de respuestas grandes
- **Indexación** para búsquedas rápidas

## 🎯 Resumen de Funcionalidades

✅ **Lista de órdenes** con filtro del día actual
✅ **Búsqueda avanzada** por múltiples criterios
✅ **Detalle completo** de cada orden
✅ **Impresión profesional** en formato A4
✅ **Descarga en PDF** para archivo
✅ **Cambio de estado** con observaciones
✅ **Estadísticas visuales** en tiempo real
✅ **Navegación fluida** entre páginas
✅ **Responsive design** para móviles
✅ **Autenticación robusta** con tokens
✅ **Validación completa** de datos
✅ **Manejo de errores** elegante
✅ **Compatibilidad OpenAPI** completa
✅ **Migración sin breaking changes**

*Sistema completamente funcional y listo para producción.*

---

**Archivos Actualizados:**
- `src/services/OrdenesDespachoService.js` - Servicio actualizado con OpenAPI
- `src/views/Reportes/OrdenesDespacho.js` - Lista de órdenes (sin cambios)
- `src/views/Reportes/OrdenDespachoDetalle.js` - Detalle de órdenes (sin cambios)

**Cambios Principales:**
- ✅ Endpoints actualizados para OpenAPI
- ✅ Métodos de compatibilidad mantenidos
- ✅ Uso correcto de estación GUID
- ✅ Manejo de errores mejorado
- ✅ Validación de tokens en todos los métodos
- ✅ Filtros de búsqueda estructurados

*Documentación actualizada: Enero 2025*

### 🎨 Características de Diseño

#### **Lista de Órdenes**
- **Cards de estadísticas** con totales por estado
- **Badges de estado** con colores distintivos:
  - 🟡 Pendiente (Warning)
  - 🔵 En Proceso (Info)
  - 🟢 Despachado (Success)
  - 🔴 Cancelado (Danger)
- **Filtros intuitivos** con calendarios
- **Búsqueda instantánea** sin necesidad de botón
- **Paginación elegante** para grandes volúmenes

#### **Detalle de Orden**
- **Layout profesional** dividido en secciones
- **Información del cliente** y vehículo organizadas
- **Tabla de productos** con totales calculados
- **Sección de totales** con subtotal, descuentos e IVA
- **Observaciones** destacadas cuando existen

### 📄 Impresión y PDF

#### **Formato de Impresión**
- **Encabezado oficial** con datos de la estación
- **Numeración clara** de la orden
- **Información completa** del cliente y vehículo
- **Tabla detallada** de productos
- **Totales destacados** con formato monetario
- **Secciones de firmas** para autorización y recepción

#### **Características del PDF**
- **Tamaño A4** optimizado para impresión
- **Márgenes profesionales** (40pt)
- **Estilos consistentes** con la marca
- **Tablas bien estructuradas** con bordes y colores
- **Información fiscal** completa

### 🔐 Seguridad y Validación

#### **Autenticación**
- **Token Bearer** requerido en todas las operaciones
- **Validación automática** de sesión
- **Redirección al login** cuando expira el token
- **Filtro por estación** activa

#### **Validación de Datos**
- **Campos requeridos** validados en tiempo real
- **Fechas válidas** verificadas antes del envío
- **Estados consistentes** según el flujo de negocio
- **Manejo de errores** con mensajes claros

### 📱 Experiencia de Usuario

#### **Flujo de Trabajo**
1. **Carga automática** de órdenes del día actual
2. **Filtrado opcional** por fechas específicas
3. **Búsqueda rápida** por múltiples criterios
4. **Navegación fluida** entre lista y detalle
5. **Acciones directas** (imprimir, despachar)

#### **Estados Visuales**
- **Loading spinners** durante cargas
- **Toast notifications** para feedback
- **Badges informativos** para estados
- **Cards estadísticas** para resúmenes rápidos

### 🎯 Estados de Órdenes

#### **Flujo de Estados**
1. **Pendiente** → Orden creada, esperando despacho
2. **En Proceso** → Orden siendo preparada
3. **Despachado** → Orden completada y entregada
4. **Cancelado** → Orden anulada

#### **Acciones por Estado**
- **Pendiente**: Puede despacharse, imprimirse
- **En Proceso**: Puede despacharse, imprimirse
- **Despachado**: Solo impresión/consulta
- **Cancelado**: Solo consulta

### 📊 Métricas y Estadísticas

#### **Dashboard de Estadísticas**
- **Total de órdenes** en el período
- **Órdenes pendientes** por despachar
- **Órdenes en proceso** de preparación
- **Órdenes despachadas** completadas
- **Total monetario** de todas las órdenes

#### **Filtros Inteligentes**
- **Filtro de fecha inicial** y final
- **Búsqueda por texto** en múltiples campos
- **Paginación automática** para rendimiento
- **Contador de resultados** en tiempo real

### 🔧 Configuración Técnica

#### **Dependencias**
- **React 18+** con hooks modernos
- **CoreUI React** para componentes UI
- **React Router** para navegación
- **PDFMake** para generación de documentos
- **localStorage** para configuración de estación

#### **Rutas Configuradas**
```javascript
{ path: '/OrdenesDespacho', name: 'Órdenes de Despacho', element: OrdenesDespacho }
{ path: '/OrdenDespachoDetalle/:ordenId', name: 'Detalle de Orden', element: OrdenDespachoDetalle }
```

#### **Navegación**
- **Menú lateral** con acceso directo
- **Breadcrumbs** para contexto
- **Botones de navegación** entre páginas
- **URLs amigables** con parámetros

### 🚀 Características Avanzadas

#### **Impresión Optimizada**
- **Vista previa** antes de imprimir
- **Formato profesional** para documentos oficiales
- **Compatibilidad** con impresoras estándar
- **Tamaño de papel** configurable

#### **Gestión de Estados**
- **Modal de confirmación** para cambios críticos
- **Observaciones** en cambios de estado
- **Historial** de cambios (preparado para implementar)
- **Validación** de flujo de estados

#### **Rendimiento**
- **Lazy loading** de componentes
- **Paginación** para conjuntos grandes de datos
- **Filtros optimizados** sin recargas innecesarias
- **Cache** de datos del servicio

### 📋 Casos de Uso

#### **Operador de Despacho**
1. Consulta órdenes pendientes del día
2. Filtra por cliente o placa específica
3. Revisa detalle de productos a despachar
4. Imprime orden para el proceso físico
5. Marca como despachada al completar

#### **Supervisor**
1. Revisa estadísticas del día
2. Consulta órdenes por período
3. Verifica órdenes despachadas
4. Genera reportes en PDF
5. Audita observaciones de despacho

#### **Cliente/Transportador**
1. Recibe orden impresa
2. Verifica productos listados
3. Confirma cantidades y precios
4. Firma recepción de mercancía

### 🐛 Manejo de Errores

#### **Errores Comunes y Soluciones**
- **Token expirado** → Redirección automática al login
- **Orden no encontrada** → Mensaje informativo y retorno a lista
- **Error de red** → Toast con mensaje de error
- **Campos vacíos** → Validación visual con mensajes

#### **Estados de Error**
- **Sin conexión** → Mensaje de conectividad
- **Datos incompletos** → Validación de formularios
- **Permisos insuficientes** → Redirección de seguridad
- **Errores de servidor** → Logs detallados en consola

---

## 🎯 Resumen de Funcionalidades

✅ **Lista de órdenes** con filtro del día actual
✅ **Búsqueda avanzada** por múltiples criterios
✅ **Detalle completo** de cada orden
✅ **Impresión profesional** en formato A4
✅ **Descarga en PDF** para archivo
✅ **Cambio de estado** con observaciones
✅ **Estadísticas visuales** en tiempo real
✅ **Navegación fluida** entre páginas
✅ **Responsive design** para móviles
✅ **Autenticación robusta** con tokens
✅ **Validación completa** de datos
✅ **Manejo de errores** elegante

*Sistema completamente funcional y listo para producción.*

---

**Archivos Creados:**
- `src/services/OrdenesDespachoService.js`
- `src/views/Reportes/OrdenesDespacho.js`
- `src/views/Reportes/OrdenDespachoDetalle.js`
- Rutas y navegación configuradas

*Documentación actualizada: Julio 2025*
