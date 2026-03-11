# FacturacionelectronicaCore.Worker - Windows Service Manual

## Build and Publish

1. Open a terminal in the solution root.
2. Run:
   ```powershell
   dotnet publish FacturacionelectronicaCore.Worker/FacturacionelectronicaCore.Worker.csproj -c Release -o C:\Path\To\Publish
   ```
   Replace `C:\Path\To\Publish` with your desired output folder.

## Install as Windows Service

1. Open PowerShell as Administrator.
2. Register the service:
   ```powershell
   sc.exe create FacturacionelectronicaCoreWorker binPath= "C:\Path\To\Publish\FacturacionelectronicaCore.Worker.exe"
   ```
3. Start the service:
   ```powershell
   sc.exe start FacturacionelectronicaCoreWorker
   ```

## Stop and Remove the Service

- Stop:
  ```powershell
  sc.exe stop FacturacionelectronicaCoreWorker
  ```
- Delete:
  ```powershell
  sc.exe delete FacturacionelectronicaCoreWorker
  ```

## Alternative: Using PowerShell Cmdlet

```powershell
New-Service -Name "FacturacionelectronicaCoreWorker" -BinaryPathName "C:\Path\To\Publish\FacturacionelectronicaCore.Worker.exe"
Start-Service -Name "FacturacionelectronicaCoreWorker"
```

## Notes
- The service will run in the background and start automatically with Windows (unless set otherwise).
- To update, stop the service, overwrite the published files, and start the service again.
- Check Windows Event Viewer for logs if needed.
