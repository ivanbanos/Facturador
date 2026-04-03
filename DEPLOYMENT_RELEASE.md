# Deployment Guide - Release Build

**Build Date**: April 3, 2026  
**Configuration**: Release (Production-Optimized)

## 📦 Build Artifacts

### Backend: ControladorEstacion.WebApi
- **Location**: `ControladorEstacion.WebApi/bin/Release/publish/`
- **Files**: ~50+ files including executable, DLLs, and dependencies
- **Size**: ~500 MB with all dependencies
- **Framework**: .NET 8

**Key Executable**:
```
ControladorEstacion.WebApi.exe
```

**Configuration Files**:
- `appsettings.json` - Application settings
- `appsettings.Development.json` - Development overrides

### Frontend: controlador-estacion-web
- **Location**: `controlador-estacion-web/build/`
- **Files**: 40+ static files (HTML, CSS, JS)
- **Total Size**: ~150 KB (minified)
- **Files**:
  - `index.html` - Main entry point
  - `static/js/main.c0ba92cd.js` (90.43 kB gzipped)
  - `static/css/main.81366b51.css` (32.09 kB gzipped)

---

## 🚀 Deployment Methods

### Method 1: Local Windows Machine (Development/Testing)

#### Start the Backend API:
```powershell
cd "C:\Users\ivana\Documents\GitHub\Facturador\ControladorEstacion.WebApi\bin\Release\publish"
.\ControladorEstacion.WebApi.exe
```

**Expected Output**:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:5001
      Now listening on: http://localhost:5000
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to exit.
```

#### Start the Frontend:
```powershell
cd "C:\Users\ivana\Documents\GitHub\Facturador\controlador-estacion-web"
npm install -g serve
serve -s build -l 3000
```

**Access the application**:
- Frontend: http://localhost:3000
- API: http://localhost:5000
- Swagger: https://localhost:5001/swagger

---

### Method 2: IIS Deployment (Windows Server)

#### Backend (ControladorEstacion.WebApi):
1. Copy entire `publish\` folder to server
2. Create IIS Application Pool (.NET CLR v4.0 - but targets .NET 8)
3. Create IIS Website pointing to the publish folder
4. Configure Application Pool to use ApplicationHost.config with proper handlers
5. Create HTTPS bindings if required
6. Set environment variables in `web.config` or app configuration

#### Frontend (controlador-estacion-web):
1. Copy entire `build\` folder to server
2. Create IIS Website pointing to the build folder
3. Configure default document: `index.html`
4. Add URL Rewrite rule to handle client-side routing:
```xml
<rule name="React Routes" stopProcessing="true">
  <match url=".*" />
  <conditions logicalGrouping="MatchList">
    <add input="{REQUEST_FILENAME}" matchType="IsFile" negate="true" />
    <add input="{REQUEST_FILENAME}" matchType="IsDirectory" negate="true" />
  </conditions>
  <action type="Rewrite" url="index.html" />
</rule>
```

---

### Method 3: Docker Deployment

#### Create Dockerfile for Backend:
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0-nanoserver-ltsc2022

WORKDIR /app
COPY publish/ .

EXPOSE 5000 5001
ENV ASPNETCORE_URLS=http://+:5000;https://+:5001

ENTRYPOINT ["dotnet", "ControladorEstacion.WebApi.dll"]
```

#### Create Dockerfile for Frontend:
```dockerfile
FROM node:20-alpine AS builder
WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . .
RUN npm run build

FROM nginx:alpine
COPY --from=builder /app/build /usr/share/nginx/html
COPY nginx.conf /etc/nginx/nginx.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
```

---

## ⚙️ Configuration

### Environment Variables Required

**Backend (ControladorEstacion.WebApi)**:
```
ConnectionStrings__DefaultConnection=<your_database_connection>
RabbitMQ__Host=localhost
RabbitMQ__Username=guest
RabbitMQ__Password=guest
LegacyApi__BaseUrl=http://legacy-api:5000
```

**Frontend (controlador-estacion-web)**:
- Configured in `public/config.js`
- Runtime configuration loads based on environment

---

## ✅ Pre-Deployment Checklist

- [ ] Database migrations completed on target server
- [ ] RabbitMQ service is running and accessible
- [ ] Legacy Facturador API endpoint is configured
- [ ] SSL/TLS certificates are installed (if using HTTPS)
- [ ] API endpoint URL configured in frontend config
- [ ] Firewall rules allow traffic on required ports (5000, 5001, 3000)
- [ ] Application has appropriate read/write permissions
- [ ] Logging is configured and directories exist
- [ ] Health checks pass before going live

---

## 🔍 Verification Steps

### Test Backend:
```powershell
curl http://localhost:5000/health
curl -X GET http://localhost:5000/api/controlador
```

### Test Frontend:
```
Open browser to http://localhost:3000
Verify dashboard loads without errors
Check browser console for any JavaScript errors
```

### Check Logs:
- Backend: Check console output or Windows Event Viewer
- Frontend: Check browser developer console (F12)

---

## 📊 Build Information

| Component | Version | Framework | Status |
|-----------|---------|-----------|--------|
| Backend API | 1.0.0 | .NET 8 | ✅ Built |
| Frontend | 0.1.0 | React 18 | ✅ Built |
| Build Date | Apr 3, 2026 | - | - |

---

## 🆘 Troubleshooting

**Backend won't start**:
- Check if ports 5000/5001 are already in use
- Verify .NET 8 runtime is installed
- Check `appsettings.json` for correct configuration

**Frontend shows blank page**:
- Check browser console for errors (F12)
- Verify API endpoint is correct in config
- Clear browser cache

**API connection errors**:
- Verify backend is running on configured port
- Check CORS settings in `Program.cs`
- Verify network connectivity between frontend and backend

---

## 📝 Next Steps

1. Choose deployment method above
2. Follow the corresponding deployment steps
3. Run verification tests
4. Monitor application logs for any errors
5. Set up monitoring/alerts for production

