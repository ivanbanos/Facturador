# Controlador Estacion Web - Proyectos Nuevos

Se crearon dos proyectos nuevos sin modificar los existentes:

- `ControladorEstacion.WebApi` (ASP.NET Core .NET 8)
- `controlador-estacion-web` (React + STOMP)

## 1. Backend .NET 8

Ruta: `ControladorEstacion.WebApi`

### Endpoints
- `GET /api/v1/controlador/surtidores`
- `POST /api/v1/controlador/reportes/lecturas`
- `POST /api/v1/controlador/reportes/ventas`
- `GET /api/v1/controlador/health`

### Configuracion
Editar `ControladorEstacion.WebApi/appsettings.Development.json`:

```json
{
  "LegacyApi": {
    "BaseUrl": "https://localhost:5001"
  },
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:3000"
    ]
  }
}
```

`LegacyApi:BaseUrl` debe apuntar al API legado (`FacturadorApiSP`) si quieres reusar datos reales.

### Ejecutar
```bash
cd ControladorEstacion.WebApi
dotnet run
```

## 2. Frontend React

Ruta: `controlador-estacion-web`

### Configuracion runtime
Editar `controlador-estacion-web/public/config.js`:

```javascript
window.ControladorApiUrl = "https://localhost:7106";
window.RabbitWebSocket = "wss://TU_BROKER:15674/ws";
window.RabbitStompUser = "TU_USUARIO";
window.RabbitStompPassword = "TU_PASSWORD";
window.RabbitStompDestination = "controlador";
```

### Ejecutar
```bash
cd controlador-estacion-web
npm start
```

## 3. Flujo funcional
1. React consulta surtidores al API .NET 8.
2. React se suscribe por STOMP al destino configurado.
3. Al recibir mensajes, actualiza estado de cada surtidor en dashboard.
4. Pagina de reportes consulta lecturas/ventas via API .NET 8.

## 4. Notas
- El API nuevo tiene rate limiting para endpoints de reportes.
- El API nuevo valida rango de fechas (sin fechas futuras, max 365 dias).
- No se dejaron credenciales hardcodeadas en el frontend.
