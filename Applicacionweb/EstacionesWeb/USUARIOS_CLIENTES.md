# Usuarios cliente (`usuarios-clientes`)

Resumen de los cambios y guía de pruebas de la funcionalidad de usuarios asociados a clientes (terceros).

> Estado: el backend compila y el frontend pasa lint. **Aún no se ha probado contra una base de datos**; esta guía es la lista de pruebas pendiente.

## 1. Qué hace

- Un usuario puede quedar asociado a un tercero mediante `Usuario.IdentificacionTercero`.
  - Con identificación: rol **Cliente**.
  - Sin identificación (`NULL`): rol **Administrador**. Todos los usuarios existentes quedan como administradores.
- Un cliente solo ve sus propias órdenes de despacho, facturas de canastilla y reporte fiscal.
- Un administrador puede crear usuarios (administradores o clientes) desde la página **Usuarios**.
- Cualquier usuario puede cambiar su contraseña.
- Todo el API exige token, salvo el inicio de sesión. El token dura **1 hora** (antes 5) y no tiene tolerancia de reloj.

## 2. Cambios

### Base de datos

| Archivo | Cambio |
|---|---|
| `MIGRACION_USUARIOS_CLIENTES.sql` | Script idempotente para aplicar todo lo de esta tabla en una base existente |
| `dbo/Tables/Usuario.sql`, `admin/Usuario.sql` | Columna nueva `IdentificacionTercero NVARCHAR(250) NULL` |
| `GetUserGuidByUserNPass`, `GetUserByGuid`, `GetUsuarios` | Devuelven `NombreUsuario` e `IdentificacionTercero`; ya no devuelven la contraseña |
| `CrearUsuario` (nuevo) | Inserta el usuario; devuelve 0 si el nombre de usuario ya existe |
| `CambiarContrasenaUsuario` (nuevo) | Cambia la contraseña solo si la actual coincide |

### API (`FacturacionelectronicaCore.Web`)

| Archivo | Cambio |
|---|---|
| `Startup.cs` | Política global: todo endpoint exige usuario autenticado. Expiración estricta del token. Swagger queda fuera de la política. Registro de `ClienteAccesoFilter` |
| `Authtentication/JWTAuthentication.cs` | Token de 1 hora con claim de rol y, para clientes, `identificacionTercero` |
| `Authtentication/UsuarioClaims.cs` (nuevo) | Roles y utilidades: `EsCliente`, `EsDelCliente`, `RestringirFiltroACliente` |
| `Filters/ClienteAccesoFilter.cs` (nuevo) | Lista de acciones permitidas para clientes; el resto responde 403 |
| `Controllers/UsuariosController.cs` | Nuevos: `POST Login`, `GET` (listar), `GET Me`, `POST Crear`, `POST CambiarContrasena`. `POST` (Save) ya no es anónimo. Las respuestas no incluyen contraseñas |
| `Controllers/UsuariosRequests.cs` (nuevo) | Modelos de petición con validaciones |
| `OrdenesDeDespachoController`, `FacturasCanastillaController` | Para clientes, el filtro se fuerza a su identificación; el detalle de una factura ajena responde 403 |
| `ManejadorInformacionLocalController` | `GetInfoFacturaElectronica` responde 403 si la orden no es del cliente |
| `TercerosController` | Nuevo `GET BuscarPorIdentificacion/{identificacion}` |
| Negocio / Repositorio de `Usuario` y `Tercero` | Métodos `CrearUsuario`, `CambiarContrasena`, `BuscarPorIdentificacion` |

Acciones permitidas a un cliente: consulta de órdenes de despacho, reporte fiscal (`GetConsolidado`), consulta y detalle de facturas de canastilla, `GetInfoFacturaElectronica`, lista de estaciones, inicio de sesión, `Me` y cambio de contraseña.

### Frontend (`EstacionesWeb`)

| Archivo | Cambio |
|---|---|
| `public/config.js` | `FEATURE_FLAGS["usuarios-clientes"]` |
| `services/FeatureFlags.js` (nuevo) | `isFeatureEnabled(nombre)` |
| `services/AuthService.js` (nuevo) | Lectura del token, expiración, rol, rutas permitidas por rol y por bandera |
| `services/GetToken.js` | Inicio de sesión por `POST /Usuarios/Login` con las credenciales en el cuerpo |
| `services/HttpService.js` | Sin sesión válida no llama al API; un 401 cierra la sesión |
| `services/UserService.js` | `listar`, `crear`, `cambiarContrasena` |
| `services/TercerosService.js` | `buscarPorIdentificacionServidor` |
| `views/usuarios/` (nuevo) | `Usuarios.js`, `BuscadorCliente.js`, `CambiarContrasena.js` |
| `components/header/EstacionSelector.js` (nuevo) | Selector de estación para clientes |
| `layout/DefaultLayout.js` | Sin sesión redirige al login; cierre automático al expirar el token |
| `components/AppContent.js`, `AppSidebar.js`, `AppHeader.js`, `_nav.js`, `routes.js` | Rutas y menú filtrados por rol; enlaces a Usuarios y Cambiar contraseña |
| `views/pages/login/Login.js` | Usa `AuthService`; envía con Enter; rechaza clientes si la bandera está apagada |
| `views/Reportes/OrdenesDespacho.js` | El botón "reenviar a facturación" no aparece para clientes |

### Bandera `usuarios-clientes`

```js
// public/config.js
var FEATURE_FLAGS = {
  "usuarios-clientes": true,
};
```

- `true`: funcionalidad completa.
- `false` o ausente: se ocultan Usuarios y Cambiar contraseña (menú, encabezado y rutas) y los clientes no pueden iniciar sesión en la web.
- La bandera solo afecta al frontend. El API aplica las restricciones siempre.

## 3. Preparación para probar

1. **Base de datos**: ejecutar `FacturacionelectronicaCore.Database/MIGRACION_USUARIOS_CLIENTES.sql` en la base de pruebas. Se puede ejecutar varias veces.
2. **API**: levantar `FacturacionelectronicaCore.Web` (perfil local: `https://localhost:5001`).
3. **Frontend**: en `public/config.js`, apuntar `SERVER_URL` al API de pruebas y dejar la bandera en `true`. Luego `npm start`.
4. **Datos**: tener a mano
   - un usuario existente (será administrador),
   - dos terceros con movimientos en el rango de fechas a consultar (cliente A y cliente B),
   - idealmente más de una estación.

## 4. Guía de pruebas

Marcar cada caso al completarlo.

### A. Sesión y token

| # | Pasos | Resultado esperado |
|---|---|---|
| A1 | Iniciar sesión con un usuario existente | Entra al Dashboard; el nombre aparece en el encabezado |
| A2 | Usuario o contraseña incorrectos | Mensaje de error; no entra |
| A3 | Dejar un campo vacío y enviar | "Ingrese usuario y contraseña"; no se llama al API |
| A4 | Escribir credenciales y presionar Enter | Inicia sesión igual que con el botón |
| A5 | Sin sesión, abrir `#/dashboard` directamente | Redirige al login |
| A6 | Con sesión, borrar `token` de localStorage y navegar a otra página | Vuelve al login |
| A7 | Esperar 1 hora con la sesión abierta | La sesión se cierra sola y vuelve al login |
| A8 | Llamar cualquier endpoint sin token (p. ej. `GET /api/Estaciones`) | 401 |
| A9 | Abrir `/swagger` | Carga sin pedir token |

### B. Gestión de usuarios (administrador)

| # | Pasos | Resultado esperado |
|---|---|---|
| B1 | Abrir **Usuarios** | Lista con nombre, usuario, rol y cliente; los usuarios anteriores figuran como Administrador |
| B2 | Crear usuario tipo Administrador | Aviso de creado; aparece en la lista como Administrador |
| B3 | Tipo Cliente: buscar la identificación del cliente A | Con un resultado se selecciona solo; con varios se elige de la lista |
| B4 | Buscar una identificación inexistente | "No se encontró ningún cliente con esa identificación" |
| B5 | Crear el usuario cliente A | Aparece como Cliente con su identificación |
| B6 | Crear otro usuario para el cliente B | Igual que B5 |
| B7 | Tipo Cliente sin seleccionar cliente | "Seleccione el cliente al que pertenece el usuario" |
| B8 | Repetir un nombre de usuario existente | "El usuario '…' ya existe" |
| B9 | Usuario de menos de 3 caracteres, o con espacio o `/ \ ? # %` | Mensaje de validación; no se crea |
| B10 | Contraseña de menos de 6 caracteres, o confirmación distinta | Mensaje de validación; no se crea |
| B11 | Quitar el cliente seleccionado con la X | Vuelve el buscador |

### C. Cambio de contraseña

| # | Pasos | Resultado esperado |
|---|---|---|
| C1 | Contraseña actual correcta y una nueva válida | "Contraseña actualizada correctamente"; el formulario se limpia |
| C2 | Cerrar sesión y entrar con la nueva | Entra; con la anterior ya no |
| C3 | Contraseña actual incorrecta | "La contraseña actual es incorrecta" |
| C4 | Nueva igual a la actual | Mensaje de validación |
| C5 | Nueva y confirmación distintas | "Las contraseñas nuevas no coinciden" |
| C6 | Cambiar a una contraseña que contenga `/` (p. ej. `abc/123`) y volver a entrar | Entra correctamente |

### D. Usuario cliente

Iniciar sesión con el usuario del cliente A.

| # | Pasos | Resultado esperado |
|---|---|---|
| D1 | Iniciar sesión | Llega a Órdenes de Despacho; el enlace del encabezado dice "Inicio" |
| D2 | Revisar el menú lateral | Solo Órdenes de Despacho, Reporte Fiscal, Facturas de Canastilla y Cambiar contraseña |
| D3 | Abrir `#/dashboard`, `#/Usuarios` o `#/terceros` a mano | Redirige a Órdenes de Despacho |
| D4 | Consultar órdenes de despacho | Solo órdenes del cliente A |
| D5 | Con órdenes sin facturar en pantalla | No aparece el botón de reenviar a facturación |
| D6 | Abrir el detalle de una orden y su factura electrónica | Se muestra |
| D7 | Consultar Reporte Fiscal | Solo datos del cliente A |
| D8 | Consultar Facturas de Canastilla y abrir un detalle | Solo facturas del cliente A |
| D9 | Con más de una estación | Aparece el selector de estación; al cambiarlo la página se recarga con la nueva estación |
| D10 | Con una sola estación | No aparece el selector y las consultas funcionan |
| D11 | Cambiar la contraseña | Funciona igual que en C1 |
| D12 | Entrar con el usuario del cliente B y repetir D4, D7 y D8 | Solo datos del cliente B |

### E. Restricciones en el API (cliente)

Con el token del cliente A (copiarlo de localStorage), llamar al API directamente:

| # | Petición | Resultado esperado |
|---|---|---|
| E1 | `POST /api/OrdenesDeDespacho` enviando en el filtro la identificación del cliente B | Devuelve solo órdenes del cliente A |
| E2 | `POST /api/OrdenesDeDespacho/GetConsolidado` con la identificación del cliente B | Solo datos del cliente A |
| E3 | `POST /api/FacturasCanastilla/GetFactura` con la identificación del cliente B | Solo facturas del cliente A |
| E4 | `GET /api/FacturasCanastilla/{id}` y `/detalle/{id}` de una factura del cliente B | 403 |
| E5 | `GET /api/ManejadorInformacionLocal/GetInfoFacturaElectronica/{idVentaLocal}/estacion/{estacion}` de una orden del cliente B | 403 |
| E6 | `GET /api/Usuarios`, `POST /api/Usuarios/Crear`, `GET /api/Terceros`, `POST /api/OrdenesDeDespacho/AnularOrdenes` | 403 con "El usuario cliente no tiene acceso a este recurso" |
| E7 | `GET /api/Usuarios/Me` | Datos del propio usuario, sin contraseña |

### F. Bandera apagada

Poner `"usuarios-clientes": false` en `public/config.js` y recargar.

| # | Pasos | Resultado esperado |
|---|---|---|
| F1 | Entrar como administrador | No aparecen Usuarios ni Cambiar contraseña en el menú, ni el candado en el encabezado |
| F2 | Abrir `#/Usuarios` o `#/CambiarContrasena` a mano | Redirige al Dashboard |
| F3 | Entrar como cliente | "El acceso para clientes no está habilitado"; no entra |
| F4 | Resto de páginas del administrador | Funcionan como siempre |
| F5 | Borrar la línea de la bandera (o todo `FEATURE_FLAGS`) | Se comporta igual que en `false` |

### G. Regresión

| # | Pasos | Resultado esperado |
|---|---|---|
| G1 | Como administrador, recorrer Dashboard, Terceros, Canastilla, Turnos, Cupos y reportes | Todo carga; ninguna página devuelve 401 o 403 |
| G2 | Servicios de estación (`EnviadorInformacionService` / `SigesServicio`) contra este API durante más de una hora | Siguen enviando facturas y recibiendo impresiones; sin errores 401 en sus logs |
| G3 | `GET /api/Usuarios/{usuario}/{contrasena}` (ruta que usan los servicios de estación) | Sigue devolviendo el token |

## 5. Pendientes y limitaciones

- Las contraseñas se guardan y comparan en texto plano, igual que antes de este cambio.
- No hay edición ni eliminación de usuarios desde la web; solo creación.
- La ruta `GET /api/Usuarios/{usuario}/{contrasena}` se conserva para los servicios de estación y no admite contraseñas con `/`. El usuario que usan esos servicios debe ser administrador y no tener `/` en la contraseña.
- No hay pruebas automatizadas para esta funcionalidad.
- Un cliente ve la lista completa de estaciones en el selector.
