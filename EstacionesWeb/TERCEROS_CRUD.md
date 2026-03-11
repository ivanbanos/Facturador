# Módulo de Gestión de Terceros - CRUD Completo

## Descripción General

El módulo de gestión de terceros proporciona un sistema completo de CRUD (Crear, Leer, Actualizar, Eliminar) para la administración de terceros en el sistema de estaciones de servicio. Este módulo está completamente integrado con la API REST y sigue las especificaciones OpenAPI proporcionadas.

## Componentes Creados

### 1. TercerosService.js
**Ubicación:** `src/services/TercerosService.js`

Servicio que implementa todos los endpoints de la API para terceros:

#### Endpoints de la API implementados:
- `GET /api/Terceros` - Obtiene todos los terceros
- `GET /api/Terceros/{guid}` - Obtiene un tercero por GUID
- `POST /api/Terceros` - Crea o actualiza terceros
- `GET /api/Terceros/GetIsTerceroValidoPorIdentificacion/{identificacion}` - Valida identificación
- `POST /api/Terceros/SincronizarTerceros` - Sincroniza terceros
- `GET /api/ManejadorInformacionLocal/GetTercerosActualizados/{estacion}` - Terceros actualizados por estación
- `GET /api/ManejadorInformacionLocal/GetTercerosActualizados` - Todos los terceros actualizados
- `POST /api/ManejadorInformacionLocal/EnviarTerceros` - Envía terceros al sistema

#### Métodos de conveniencia:
- `crearTercero(tercero)` - Crea un nuevo tercero
- `actualizarTercero(tercero)` - Actualiza un tercero existente
- `buscarPorIdentificacion(identificacion)` - Busca por identificación
- `buscarPorNombre(nombre)` - Busca por nombre
- `buscarTerceros(filtros)` - Búsqueda con múltiples criterios
- `validarTercero(tercero)` - Validación de datos
- `validarEmail(email)` - Validación de formato de email

### 2. Terceros.js
**Ubicación:** `src/views/terceros/Terceros.js`

Página principal de gestión de terceros con las siguientes características:

#### Funcionalidades:
- **Lista paginada** de terceros con información clave
- **Búsqueda en tiempo real** por nombre, identificación o correo
- **Filtros avanzados** por tipo de persona, tipo de identificación, municipio y departamento
- **Paginación** configurable
- **Contador de registros** y badges informativos
- **Botones de acción** para ver, editar y crear terceros
- **Sincronización** de terceros con el servidor
- **Interfaz responsive** y moderna

#### Características técnicas:
- Carga asíncrona de datos
- Manejo de estados de loading y error
- Filtrado local eficiente
- Navegación mediante React Router
- Tooltips informativos
- Alertas de éxito y error

### 3. TerceroForm.js
**Ubicación:** `src/views/terceros/TerceroForm.js`

Formulario completo para crear y editar terceros:

#### Funcionalidades:
- **Formulario organizado en secciones**:
  - Información Básica
  - Información de Contacto
  - Información de Ubicación
  - Información Adicional

#### Características principales:
- **Validación en tiempo real** de identificación con el servidor
- **Validación de formato** de emails
- **Campos requeridos** claramente marcados
- **Autodetección** de modo edición vs creación
- **Mensajes de validación** específicos por campo
- **Tipos de datos** correctos según la API (números para tipos, etc.)
- **Interfaz intuitiva** con iconos y agrupación lógica

#### Validaciones implementadas:
- Identificación requerida y única
- Nombre requerido
- Tipo de persona requerido
- Tipo de identificación requerido
- Formato de email válido
- Validación en servidor para identificación

### 4. TerceroDetalle.js
**Ubicación:** `src/views/terceros/TerceroDetalle.js`

Página de visualización detallada de un tercero:

#### Características:
- **Vista organizada en cards** para mejor legibilidad
- **Avatar con iniciales** del tercero
- **Información completa** organizada por categorías:
  - Información Básica
  - Información de Contacto
  - Información de Ubicación
  - Información Técnica
- **Links funcionales** para correos y teléfonos
- **Badges de estado** y tipo de persona
- **Navegación fácil** hacia edición
- **Información técnica** como GUIDs e IDs

## Rutas Configuradas

```javascript
// Rutas agregadas a routes.js
{ path: '/terceros', name: 'Terceros', element: Terceros },
{ path: '/terceros/nuevo', name: 'Nuevo Tercero', element: TerceroForm },
{ path: '/terceros/editar/:guid', name: 'Editar Tercero', element: TerceroForm },
{ path: '/terceros/ver/:guid', name: 'Detalle de Tercero', element: TerceroDetalle },
```

## Navegación

Se agregó el ítem de navegación en `_nav.js`:
```javascript
{
  component: CNavItem,
  name: 'Terceros',
  to: '/terceros',
  icon: <CIcon icon={cilPeople} customClassName="nav-icon" />,
  level: 1,
}
```

## Estructura de Datos

### Modelo de Tercero (según OpenAPI):
```javascript
{
  id: number,
  guid: string (uuid),
  nombre: string,
  segundo: string,
  apellidos: string,
  municipio: string,
  departamento: string,
  direccion: string,
  tipoPersona: number, // 1: Natural, 2: Jurídica
  responsabilidadTributaria: number,
  pais: string,
  codigoPostal: string,
  celular: string,
  telefono: string,
  telefono2: string,
  correo: string,
  correo2: string,
  vendedor: string,
  comentarios: string,
  tipoIdentificacion: number, // 1: CC, 2: CE, 3: NIT, 4: Pasaporte, 5: TI
  identificacion: string,
  descripcionTipoIdentificacion: string,
  idLocal: number,
  idContable: number,
  idFacturacion: string
}
```

## Características Técnicas

### Tecnologías Utilizadas:
- **React** con hooks (useState, useEffect)
- **React Router** para navegación
- **CoreUI React** para componentes UI
- **CoreUI Icons** para iconografía
- **Async/Await** para manejo de promesas
- **Try/Catch** para manejo de errores

### Patrón de Diseño:
- **Separación de responsabilidades** (Service, View, Navigation)
- **Componentes reutilizables**
- **Estado local** para manejo de UI
- **Validación en cliente y servidor**
- **Manejo de errores** consistente

### Accesibilidad:
- **Labels apropiados** en todos los inputs
- **Feedback visual** para estados de carga y error
- **Navegación por teclado** compatible
- **Contraste adecuado** en colores
- **Iconos descriptivos** con tooltips

## Próximos Pasos

El módulo está completamente funcional y listo para usar. Algunas mejoras futuras podrían incluir:

1. **Exportación a PDF/Excel** de la lista de terceros
2. **Importación masiva** de terceros desde archivo
3. **Historial de cambios** en terceros
4. **Búsqueda avanzada** con más criterios
5. **Integración con mapas** para direcciones
6. **Validación de NIT** con algoritmo de verificación

## Testing

Para probar el módulo:
1. Navegar a `/terceros` para ver la lista
2. Crear un nuevo tercero con `/terceros/nuevo`
3. Editar un tercero existente desde la lista
4. Ver detalles de un tercero desde la lista
5. Probar filtros y búsquedas
6. Verificar validaciones en el formulario
