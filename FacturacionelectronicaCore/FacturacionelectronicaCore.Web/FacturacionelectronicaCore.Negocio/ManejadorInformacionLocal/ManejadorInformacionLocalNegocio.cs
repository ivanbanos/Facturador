using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using AutoMapper;
using EstacionesServicio.Negocio.Extention;
using FacturacionelectronicaCore.Negocio.Contabilidad;
using FacturacionelectronicaCore.Negocio.Contabilidad.FacturacionElectronica;
using FacturacionelectronicaCore.Negocio.Modelo;
using FacturacionelectronicaCore.Repositorio.Entities;
using FacturacionelectronicaCore.Repositorio.Repositorios;
using Microsoft.Extensions.Options;
using System.Linq.Expressions;

namespace FacturacionelectronicaCore.Negocio.ManejadorInformacionLocal
{
    public class ManejadorInformacionLocalNegocio : IManejadorInformacionLocalNegocio
    {
        private readonly IEstacionesRepository _estacionesRepository;
        private readonly IOrdenDeDespachoRepositorio _ordenDeDespachoRepositorio;
        private readonly ITipoIdentificacionRepositorio _tipoIdentificacionRepositorio;
        private readonly ITerceroRepositorio _terceroRepositorio;
        private readonly IResolucionRepositorio _resolucionRepositorio;
        private readonly IMapper _mapper;
        private readonly IApiContabilidad _apiContabilidad;
        private readonly IFacturacionElectronicaFacade _alegraFacade;
        private readonly IFacturaCanastillaRepository _facturaCanastillaRepository;
        private readonly ICanastillaRepositorio _canastillaRepositorio;
        private readonly IValidadorGuidAFacturaElectronica _validadorGuidAFacturaElectronica;
        private readonly Alegra _alegra;

        // In-memory cache to track orders being processed or sent (IdVentaLocal)
        private static readonly ConcurrentDictionary<string, bool> _ordenesEnviadasCache = new ConcurrentDictionary<string, bool>();

        public ManejadorInformacionLocalNegocio(ITerceroRepositorio tercerosRepositorio, IMapper mapper, IResolucionRepositorio resolucionRepositorio,
                IOrdenDeDespachoRepositorio ordenDeDespachoRepositorio,
                IApiContabilidad apiContabilidad, ITipoIdentificacionRepositorio tipoIdentificacionRepositorio,
                IFacturacionElectronicaFacade alegraFacade, IOptions<Alegra> alegra, IFacturaCanastillaRepository facturaCanastillaRepository, ICanastillaRepositorio canastillaRepositorio, IValidadorGuidAFacturaElectronica validadorGuidAFacturaElectronica, IEstacionesRepository estacionesRepository)
        {
            _terceroRepositorio = tercerosRepositorio;
            _resolucionRepositorio = resolucionRepositorio;
            _ordenDeDespachoRepositorio = ordenDeDespachoRepositorio;
            _apiContabilidad = apiContabilidad;
            _mapper = mapper;

            _alegra = alegra.Value;
            _alegraFacade = alegraFacade;
            _tipoIdentificacionRepositorio = tipoIdentificacionRepositorio;
            _facturaCanastillaRepository = facturaCanastillaRepository;
            _canastillaRepositorio = canastillaRepositorio;
            _validadorGuidAFacturaElectronica = validadorGuidAFacturaElectronica;
            _estacionesRepository = estacionesRepository;
        }

        public async Task EnviarOrdenesDespacho(IEnumerable<Modelo.OrdenDeDespacho> ordenDeDespachos, Guid estacion)
        {
            // Guard against null terceros inside the collection to avoid NullReferenceException in EnviarTerceros
            await EnviarTerceros((ordenDeDespachos ?? Enumerable.Empty<Modelo.OrdenDeDespacho>()).Select(x => x?.Tercero).Where(t => t != null));
            Console.WriteLine("EnvioDirecto " + _alegra.EnvioDirecto);
            Console.WriteLine("EnviaCreditos " + _alegra.EnviaCreditos);
            foreach (var x in ordenDeDespachos)
            {
                // In-memory cache check to prevent duplicate sends in this app instance (per station)
                var cacheKey = $"{estacion}:{x.IdVentaLocal}";
                if (!_ordenesEnviadasCache.TryAdd(cacheKey, true))
                {
                    // Already being processed or sent in this instance, skip
                    continue;
                }
                try
                {
                    if (_alegra.MultiplicarPorDies)
                    {
                        x.Precio = _alegra.MultiplicarPorDies ? x.Precio * 10 : x.Precio;
                    }
                    var ordenDeDespachoEntity = (await _ordenDeDespachoRepositorio.ObtenerOrdenDespachoPorIdVentaLocal(x.IdVentaLocal, estacion)).FirstOrDefault();

                    bool envioFactura = (
                        ordenDeDespachoEntity == null
                        || ordenDeDespachoEntity.idFacturaElectronica == null
                        || ordenDeDespachoEntity.idFacturaElectronica.StartsWith("error")
                        || ordenDeDespachoEntity.idFacturaElectronica.StartsWith("Error")
                        // If the stored idFacturaElectronica contains an embedded provider invoice
                        // with an order_reference that doesn't match our local IdVentaLocal, request resend
                        || (ordenDeDespachoEntity != null
                            && !string.IsNullOrEmpty(ordenDeDespachoEntity.idFacturaElectronica)
                            && ordenDeDespachoEntity.idFacturaElectronica.IndexOf("order_reference", StringComparison.OrdinalIgnoreCase) >= 0
                            // If the local IdVentaLocal appears more than once inside the stored payload, treat as suspicious and resend
                            && (ordenDeDespachoEntity.idFacturaElectronica.IndexOf(x.IdVentaLocal.ToString(), StringComparison.OrdinalIgnoreCase)
                                == ordenDeDespachoEntity.idFacturaElectronica.LastIndexOf(x.IdVentaLocal.ToString(), StringComparison.OrdinalIgnoreCase)
                                || ordenDeDespachoEntity.idFacturaElectronica.IndexOf(x.IdVentaLocal.ToString(), StringComparison.OrdinalIgnoreCase) < 0))
                    )
                        && _alegra.EnvioDirecto;

                    if (envioFactura)
                    {
                        // Make FormaDePago checks null-safe
                        x.FormaDePago = (x.FormaDePago ?? "Efectivo");
                        if ((_alegra.EnviaTodo || x.Fecha > DateTime.Now.AddMonths(-1)
                            || (_alegra.EnviaMes && DateTime.Now.AddMonths(-1) < x.Fecha))
                            && (_alegra.EnviaCreditos || (!x.FormaDePago.ToLower().Contains("dir") && !x.FormaDePago.ToLower().Contains("calibra") && !x.FormaDePago.ToLower().Contains("consum") && !x.FormaDePago.ToLower().Contains("puntos"))))
                        {
                           

                            var id = await EnviarAFacturacion(x, estacion);
                            await Task.Delay(2000); // Esperar 2 segundos para no saturar el servicio
                            
                            x.idFacturaElectronica = id;

                            // Persist the new idFacturaElectronica immediately to avoid duplicate sending
                            var ordenesentityUpdate = new List<Repositorio.Entities.OrdenDeDespacho>
                            {
                                new Repositorio.Entities.OrdenDeDespacho()
                                {
                                    guid = x.guid?.ToString() ?? string.Empty,
                                    Cantidad = x.Cantidad,
                                    Cara = x.Cara,
                                    Combustible = x.Combustible,
                                    Descuento = x.Descuento,
                                    Estado = x.Estado,
                                    Fecha = x.Fecha,
                                    FechaReporte = x.FechaReporte,
                                    FechaProximoMantenimiento = x.FechaProximoMantenimiento,
                                    FormaDePago = x.FormaDePago,
                                    FormaDePago2 = x.FormaDePago2,
                                    Total1 = x.Total1,
                                    Total2 = x.Total2,
                                    Identificacion = x.Identificacion,
                                    IdentificacionTercero = x.IdentificacionTercero,
                                    IdEstacion = x.IdEstacion,
                                    IdEstadoActual = x.IdEstadoActual,
                                    IdFactura = x.IdFactura,
                                    IdInterno = x.IdInterno,
                                    IdLocal = x.IdLocal,
                                    IdTerceroLocal = x.IdTerceroLocal,
                                    IdVentaLocal = x.IdVentaLocal,
                                    Kilometraje = x.Kilometraje,
                                    Manguera = x.Manguera,
                                    NombreTercero = x.NombreTercero,
                                    Placa = x.Placa,
                                    Precio = x.Precio,
                                    SubTotal = x.SubTotal,
                                    Surtidor = x.Surtidor,
                                    Total = x.Total,
                                    idFacturaElectronica = x.idFacturaElectronica,
                                    Vendedor = x.Vendedor,
                                }
                            };
                            await _ordenDeDespachoRepositorio.AddRange(ordenesentityUpdate, estacion);
                            // After this, the next check will see the updated value
                            continue; // Skip to next iteration, as we've already persisted
                        }
                    }

                    // Only add/update if not already handled above
                    if (ordenDeDespachoEntity == null
                        || ordenDeDespachoEntity.idFacturaElectronica == null
                        || ordenDeDespachoEntity.idFacturaElectronica.Contains("error") || !_alegra.Desactivado)
                    {
                        var ordenesentity = new List<Repositorio.Entities.OrdenDeDespacho>
                        {
                            new Repositorio.Entities.OrdenDeDespacho()
                            {
                                guid = x.guid?.ToString() ?? string.Empty,
                                Cantidad = x.Cantidad,
                                Cara = x.Cara,
                                Combustible = x.Combustible,
                                Descuento = x.Descuento,
                                Estado = x.Estado,
                                Fecha = x.Fecha,
                                FechaReporte = x.FechaReporte,
                                FechaProximoMantenimiento = x.FechaProximoMantenimiento,
                                FormaDePago = x.FormaDePago,
                                FormaDePago2 = x.FormaDePago2,
                                Total1 = x.Total1,
                                Total2 = x.Total2,
                                Identificacion = x.Identificacion,
                                IdentificacionTercero = x.IdentificacionTercero,
                                IdEstacion = x.IdEstacion,
                                IdEstadoActual = x.IdEstadoActual,
                                IdFactura = x.IdFactura,
                                IdInterno = x.IdInterno,
                                IdLocal = x.IdLocal,
                                IdTerceroLocal = x.IdTerceroLocal,
                                IdVentaLocal = x.IdVentaLocal,
                                Kilometraje = x.Kilometraje,
                                Manguera = x.Manguera,
                                NombreTercero = x.NombreTercero,
                                Placa = x.Placa,
                                Precio = x.Precio,
                                SubTotal = x.SubTotal,
                                Surtidor = x.Surtidor,
                                Total = x.Total,
                                idFacturaElectronica = x.idFacturaElectronica ?? ordenDeDespachoEntity?.idFacturaElectronica,
                                Vendedor = x.Vendedor,
                            }
                        };
                        await _ordenDeDespachoRepositorio.AddRange(ordenesentity, estacion);
                    }
                }
                finally
                {
                    // Remove from cache after processing (success or fail)
                    bool removed;
                    _ordenesEnviadasCache.TryRemove(cacheKey, out removed);
                }
            }
        }

        public async Task<IEnumerable<Modelo.Tercero>> GetTercerosActualizados(Guid estacion)
        {
            try
            {
                var terceros = await _terceroRepositorio.GetTercerosActualizados(estacion);
                return _mapper.Map<IEnumerable<Repositorio.Entities.Tercero>, IEnumerable<Modelo.Tercero>>(terceros);
            }
            catch (Exception)
            {

                throw;
            }
        }


        public async Task<IEnumerable<Modelo.Tercero>> GetTercerosActualizados()
        {
            try
            {
                var terceros = await _terceroRepositorio.GetTercerosActualizados();
                return _mapper.Map<IEnumerable<Repositorio.Entities.Tercero>, IEnumerable<Modelo.Tercero>>(terceros);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<IEnumerable<string>> GetTipos()
        {
            return await _tipoIdentificacionRepositorio.GetTipos();
        }

        public async Task<string> GetInfoFacturaElectronica(int idVentaLocal, Guid estacion)
        {

            var ordenDeDespachoEntity = (await _ordenDeDespachoRepositorio.ObtenerOrdenDespachoPorIdVentaLocal(idVentaLocal, estacion))?.FirstOrDefault();
            if (ordenDeDespachoEntity != null)
            {
                var idFactura = ordenDeDespachoEntity.idFacturaElectronica;

                Console.WriteLine("idFactura: " + idFactura);
                Console.WriteLine("_alegra.Proveedor: " + _alegra.Proveedor);
                Console.WriteLine("startswith: " + (idFactura.StartsWith("Error:") || idFactura.StartsWith("error:")));

                if (!string.IsNullOrEmpty(idFactura))
                {
                    if (_alegra.Proveedor == "DATAICO" || _alegra.Proveedor == "DATATICO")
                    {
                        try
                        {
                            // Attempt to find any JSON blob(s) inside the idFactura string and parse them.
                            var text = idFactura;
                            for (int i = 0; i < text.Length; i++)
                            {
                                if (text[i] != '{') continue;
                                int depth = 0;
                                int end = -1;
                                for (int j = i; j < text.Length; j++)
                                {
                                    if (text[j] == '{') depth++;
                                    else if (text[j] == '}') depth--;
                                    if (depth == 0)
                                    {
                                        end = j;
                                        break;
                                    }
                                }
                                if (end <= 0) continue;
                                var candidate = text.Substring(i, end - i + 1);
                                try
                                {
                                    var obj = Newtonsoft.Json.Linq.JObject.Parse(candidate);
                                    // Try to get number and cufe from multiple possible locations
                                    var number = obj.SelectToken("number")?.ToString()
                                                 ?? obj.SelectToken("invoice.number")?.ToString();
                                    var cufe = obj.SelectToken("cufe")?.ToString()
                                               ?? obj.SelectToken("dian.cufe")?.ToString()
                                               ?? obj.SelectToken("respuesta.cufe")?.ToString();
                                    var issuedate = obj.SelectToken("issue_date")?.ToString()
                                                    ?? obj.SelectToken("invoice.issue_date")?.ToString()
                                                    ?? obj.SelectToken("invoice.payment_date")?.ToString();
                                    if (string.IsNullOrEmpty(issuedate))
                                    {
                                        issuedate = ordenDeDespachoEntity.Fecha.ToString("dd/MM/yyyy HH:mm:ss");
                                    }
                                    if (!string.IsNullOrEmpty(number) && !string.IsNullOrEmpty(cufe))
                                    {
                                        return $"Factura electrónica\n\r{number}\n\rCUFE:\n\r{cufe}\n\rFecha emisión: {issuedate}";
                                    }
                                }
                                catch
                                {
                                    // ignore parse errors for this candidate, try next
                                }
                                // move i forward to end to avoid re-parsing inside the same object
                                i = end;
                            }
                            // fallback: try the classic colon-split pattern if no JSON gave the info
                            var parts = idFactura.Split(':');
                            if (parts.Length >= 3 && !(idFactura.StartsWith("Error:") || idFactura.StartsWith("error:")))
                            {
                                var prefijoConsecutivo = parts[1];
                                var cufe = parts[2];
                                var issuedate = ordenDeDespachoEntity.Fecha.ToString("dd/MM/yyyy HH:mm:ss");
                                return $"Factura electrónica\n\r{prefijoConsecutivo}\n\rCUFE:\n\r{cufe}\n\rFecha emisión: {issuedate}";
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error parsing JSON: " + ex.Message);
                        }
                    }
                    else
                    {
                        var parts = idFactura.Split(':');
                        if (parts.Length >= 3 && !(idFactura.StartsWith("Error:") || idFactura.StartsWith("error:")))
                        {
                            var prefijoConsecutivo = parts[1];
                            var cufe = parts[2];
                            var issuedate = ordenDeDespachoEntity.Fecha.ToString("dd/MM/yyyy HH:mm:ss");
                            return $"Factura electrónica\n\r{prefijoConsecutivo}\n\rCUFE:\n\r{cufe}\n\rFecha emisión: {issuedate}";
                        }
                        if (_alegra.Proveedor == "SIIGO")
                        {
                            return await _alegraFacade.GetFacturaElectronica(idFactura, estacion);
                        }
                    }
                }
                return null;
            }
            return null;
        }

        public async Task<int> AddFacturaCanastilla(IEnumerable<Modelo.FacturaCanastilla> facturas, Guid estacion)
        {
            try
            {

                await EnviarTerceros(facturas.Select(x => x.terceroId));
                // Get all canastillas from repo for this estacion
                var canastillasRepo = await _canastillaRepositorio.GetCanastillas(estacion);
                foreach (var factura in facturas)
                {
                    try
                    {
                        // Replace campoextra in each factura.canastillas from repo
                        foreach (var canastilla in factura.canastillas ?? Enumerable.Empty<Modelo.CanastillaFactura>())
                        {
                            if (canastilla.Canastilla != null)
                            {
                                var repoCanastilla = canastillasRepo.FirstOrDefault(c => c.guid == canastilla.Canastilla.guid);
                                if (repoCanastilla != null)
                                {
                                    canastilla.Canastilla.campoextra = repoCanastilla.campoextra;
                                }
                            }
                        }

                        var facturaCanastilla = await _facturaCanastillaRepository.GetFacturaPorIdCanastilla(factura.FacturasCanastillaId, estacion);
                        var idFactruraElectronica = "";
                        var idExistente = facturaCanastilla?.idFacturaElectronica;
                        var facturaElectronicaEnviada = !string.IsNullOrEmpty(idExistente)
                            && !idExistente.StartsWith("error", StringComparison.OrdinalIgnoreCase);

                        if (facturaElectronicaEnviada)
                        {
                            factura.idFacturaElectronica = idExistente;
                        }
                        else if (facturaCanastilla == null
                            || facturaCanastilla.idFacturaElectronica == null
                            || facturaCanastilla.idFacturaElectronica.StartsWith("error")
                            || facturaCanastilla.idFacturaElectronica.StartsWith("Error"))
                        {
                            var terceroEntity = (await _terceroRepositorio.ObtenerTerceroPorIdentificacion(factura.terceroId.Identificacion)).FirstOrDefault();
                            if (terceroEntity != null)
                            {
                                var tercero = _mapper.Map<Repositorio.Entities.Tercero, Modelo.Tercero>(terceroEntity);
                                if (_alegra.ValidaTercero && tercero.idFacturacion == null)
                                {
                                    idFactruraElectronica = "error:Tercero no está apto para facturación electrónica";
                                }
                                else if (_alegra.EnvioDirecto
                                    && (_alegra.EnviaTodo || factura.fecha > DateTime.Now.AddMonths(-1)
                                        || (_alegra.EnviaMes && DateTime.Now.AddMonths(-1) < factura.fecha))
                                    && (_alegra.EnviaCreditos || (!(factura.codigoFormaPago?.Descripcion ?? "").ToLower().Contains("dir")
                                        && !(factura.codigoFormaPago?.Descripcion ?? "").ToLower().Contains("calibra")
                                        && !(factura.codigoFormaPago?.Descripcion ?? "").ToLower().Contains("consum")
                                        && !(factura.codigoFormaPago?.Descripcion ?? "").ToLower().Contains("puntos"))))
                                {
                                    // Retry mechanism for newly created terceros in Alegra
                                    var maxRetries = 3;
                                    var retryDelay = 3000; // 3 seconds
                                    
                                    for (int attempt = 1; attempt <= maxRetries; attempt++)
                                    {
                                        try
                                        {
                                            var response = await _alegraFacade.GenerarFacturaElectronica(factura, tercero, estacion);
                                            
                                            // If response contains specific errors related to tercero not found, retry
                                            if (response != null && 
                                                (response.Contains("Producto canastilla no creado") || 
                                                 response.Contains("contact not found") ||
                                                 response.Contains("tercero") ||
                                                 (response.StartsWith("error:") && attempt < maxRetries)))
                                            {
                                                Console.WriteLine($"Canastilla attempt {attempt} failed for tercero {tercero.Identificacion}: {response}. Retrying in {retryDelay}ms...");
                                                await Task.Delay(retryDelay);
                                                continue;
                                            }
                                            
                                            idFactruraElectronica = response;
                                            break;
                                        }
                                        catch (Exception ex) when (attempt < maxRetries)
                                        {
                                            Console.WriteLine($"Canastilla attempt {attempt} failed for tercero {tercero.Identificacion}: {ex.Message}. Retrying in {retryDelay}ms...");
                                            await Task.Delay(retryDelay);
                                        }
                                    }
                                    
                                    // If still null after retries, try one final attempt
                                    if (string.IsNullOrEmpty(idFactruraElectronica))
                                    {
                                        idFactruraElectronica = await _alegraFacade.GenerarFacturaElectronica(factura, tercero, estacion);
                                    }
                                }
                                
                                await Task.Delay(2000);
                                factura.idFacturaElectronica = idFactruraElectronica;
                            }
                            else
                            {
                                if (_alegra.EnvioDirecto
                                    && (_alegra.EnviaTodo || factura.fecha > DateTime.Now.AddMonths(-1)
                                        || (_alegra.EnviaMes && DateTime.Now.AddMonths(-1) < factura.fecha))
                                    && (_alegra.EnviaCreditos || (!(factura.codigoFormaPago?.Descripcion ?? "").ToLower().Contains("dir") && !(factura.codigoFormaPago?.Descripcion ?? "").ToLower().Contains("calibra") && !(factura.codigoFormaPago?.Descripcion ?? "").ToLower().Contains("consum") && !(factura.codigoFormaPago?.Descripcion ?? "").ToLower().Contains("puntos"))))
                                {
                                    // Retry mechanism for newly created terceros in Alegra
                                    var maxRetries = 3;
                                    var retryDelay = 3000; // 3 seconds
                                    
                                    for (int attempt = 1; attempt <= maxRetries; attempt++)
                                    {
                                        try
                                        {
                                            var response = await _alegraFacade.GenerarFacturaElectronica(factura, factura.terceroId, estacion);
                                            
                                            // If response contains specific errors related to tercero not found, retry
                                            if (response != null && 
                                                (response.Contains("Producto canastilla no creado") || 
                                                 response.Contains("contact not found") ||
                                                 response.Contains("tercero") ||
                                                 (response.StartsWith("error:") && attempt < maxRetries)))
                                            {
                                                Console.WriteLine($"Canastilla direct tercero attempt {attempt} failed: {response}. Retrying in {retryDelay}ms...");
                                                await Task.Delay(retryDelay);
                                                continue;
                                            }
                                            
                                            idFactruraElectronica = response;
                                            break;
                                        }
                                        catch (Exception ex) when (attempt < maxRetries)
                                        {
                                            Console.WriteLine($"Canastilla direct tercero attempt {attempt} failed: {ex.Message}. Retrying in {retryDelay}ms...");
                                            await Task.Delay(retryDelay);
                                        }
                                    }
                                    
                                    // If still null after retries, try one final attempt
                                    if (string.IsNullOrEmpty(idFactruraElectronica))
                                    {
                                        idFactruraElectronica = await _alegraFacade.GenerarFacturaElectronica(factura, factura.terceroId, estacion);
                                    }
                                    
                                    await Task.Delay(2000);
                                    factura.idFacturaElectronica = idFactruraElectronica;
                                }
                            }
                        }

                        var facturaRepo = _mapper.Map<Modelo.FacturaCanastilla, Repositorio.Entities.FacturaCanastilla>(factura);
                        await _facturaCanastillaRepository.Add(facturaRepo, facturaRepo.canastillas, estacion);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        Console.WriteLine(ex.StackTrace);
                        factura.idFacturaElectronica = ex.Message + ex.StackTrace;
                    }
                }
                return 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                return -1;
            }
        }

        public async Task<ResolucionElectronica> GetResolucionElectronica()
        {
            var estacion = await _estacionesRepository.GetEstaciones();
            return await _alegraFacade.GetResolucionElectronica(estacion.First().guid.ToString());
        }

        public async Task<string> JsonFacturaElectronica(int idVentaLocal, Guid estacion)
        {
            var ordenDeDespachoEntity = (await _ordenDeDespachoRepositorio.ObtenerOrdenDespachoPorIdVentaLocal(idVentaLocal, estacion))?.FirstOrDefault();
            var ordenesDeDespacho = _mapper.Map<Repositorio.Entities.OrdenDeDespacho, Modelo.OrdenDeDespacho>(ordenDeDespachoEntity);

            if (ordenDeDespachoEntity == null)
            {
                return "error:Venta no existe en el sistema";
            }
            var terceroEntity = (await _terceroRepositorio.ObtenerTerceroPorIdentificacion(ordenesDeDespacho.Identificacion))?.FirstOrDefault();
            var tercero = _mapper.Map<Repositorio.Entities.Tercero, Modelo.Tercero>(terceroEntity);
            if (_alegra.ValidaTercero && tercero.idFacturacion == null)
            {

                return "error:Tercero no está apto para facturación electrónica";
            }
            ordenesDeDespacho.Tercero = tercero;
            return await _alegraFacade.getJson(ordenesDeDespacho, estacion) + " respuesta " + (ordenDeDespachoEntity.idFacturaElectronica ?? "No generada");
        }


        public async Task<string> JsonFacturaElectronicaCanastilla(int idVentaLocal, Guid estacion)
        {
            var facturaCanastillaEntity = await _facturaCanastillaRepository.GetFacturaPorIdCanastilla(idVentaLocal, estacion);
            var facturaCanastilla = _mapper.Map<Repositorio.Entities.FacturaCanastilla, Modelo.FacturaCanastilla>(facturaCanastillaEntity);

            if (facturaCanastillaEntity == null)
            {
                return "error:Venta no existe en el sistema";
            }
            var terceroEntity = (await _terceroRepositorio.ObtenerTerceroPorIdentificacion(facturaCanastilla.terceroId.Identificacion))?.FirstOrDefault();
            var tercero = _mapper.Map<Repositorio.Entities.Tercero, Modelo.Tercero>(terceroEntity);
            if (_alegra.ValidaTercero && tercero.idFacturacion == null)
            {

                return "error:Tercero no está apto para facturación electrónica";
            }
            facturaCanastilla.terceroId = tercero;
            return await _alegraFacade.getJsonCanastilla(facturaCanastilla, estacion) + " respuesta " + (facturaCanastillaEntity.idFacturaElectronica ?? "No generada");
        }

        public async Task<string> GetInfoFacturaElectronicaCanastilla(int consecutivo, Guid estacion)
        {
            var ordenDeDespachoEntity = (await _facturaCanastillaRepository.GetFacturaPorIdCanastilla(consecutivo, estacion));
            if (ordenDeDespachoEntity != null)
            {
                var idFactura = ordenDeDespachoEntity.idFacturaElectronica;

                Console.WriteLine("idFactura: " + idFactura);
                Console.WriteLine("_alegra.Proveedor: " + _alegra.Proveedor);
                Console.WriteLine("startswith: " + (idFactura.StartsWith("Error:") || idFactura.StartsWith("error:")));

                if (!string.IsNullOrEmpty(idFactura))
                {
                    if ((_alegra.Proveedor == "DATAICO" || _alegra.Proveedor == "DATATICO") && (idFactura.StartsWith("Error:") || idFactura.StartsWith("error:")))
                    {
                        try
                        {
                            var jsonStart = idFactura.IndexOf(":") + 1;
                            var json = idFactura.Substring(jsonStart).Trim();
                            // Sometimes there are two JSONs concatenated, take only the first
                            int end = json.IndexOf("}{");
                            if (end > 0)
                                json = json.Substring(0, end + 1);
                            Console.WriteLine("JSON: " + json);
                            var obj = Newtonsoft.Json.Linq.JObject.Parse(json);
                            var number = obj["number"]?.ToString();
                            var cufe = obj["cufe"]?.ToString();
                            Console.WriteLine($"Number: {number}, CUFE: {cufe}");
                            if (!string.IsNullOrEmpty(number) && !string.IsNullOrEmpty(cufe))
                                return $"Factura electrónica\n\r{number}\n\rCUFE:\n\r{cufe}";
                        }
                        catch (Exception ex) { Console.WriteLine("Error parsing JSON: " + ex.Message); }
                    }
                    else
                    {
                        var parts = idFactura.Split(':');
                        if (parts.Length >= 3 && (idFactura.StartsWith("Ok:") || idFactura.ToLower().StartsWith("dian") || idFactura.StartsWith("ok:")))
                        {
                            var prefijoConsecutivo = parts[1];
                            var cufe = parts[2];
                            return $"Factura electrónica\n\r{prefijoConsecutivo}\n\rCUFE:\n\r{cufe}";
                        }
                        if (_alegra.Proveedor == "SIIGO")
                        {
                            return await _alegraFacade.GetFacturaElectronica(idFactura, estacion);
                        }
                    }
                }
                return null;
            }
            return null;
        }

        public async Task AgregarFechaReporteFactura(IEnumerable<Modelo.FacturaFechaReporte> facturaFechaReporte, Guid estacion)
        {
            var facturas = _mapper.Map<IEnumerable<Modelo.FacturaFechaReporte>, IEnumerable<Repositorio.Entities.FacturaFechaReporte>>(facturaFechaReporte);
            await _ordenDeDespachoRepositorio.AgregarFechaReporteFactura(facturas, estacion);
        }

        public async Task EnviarTerceros(IEnumerable<Modelo.Tercero> terceros)
        {
            try
            {
                terceros = terceros.Where(x => !string.IsNullOrEmpty(x.Identificacion) && !string.IsNullOrEmpty(x.DescripcionTipoIdentificacion));
                await _terceroRepositorio.AddOrUpdate(_mapper.Map<IEnumerable<TerceroInput>>(terceros));
                
                if (_alegra.Proveedor == "ALEGRA")
                {
                    var newTercerosCreated = false;
                    
                    foreach (var tercero in terceros)
                    {
                        if (tercero.idFacturacion != null)
                        {
                            await _alegraFacade.ActualizarTercero(tercero, tercero.idFacturacion);
                        }
                        else
                        {
                            var idFacturacion = await _alegraFacade.GenerarTercero(tercero);
                            await _terceroRepositorio.SetIdFacturacion(tercero.Guid, idFacturacion);
                            newTercerosCreated = true;
                        }
                    }
                    
                    // If new terceros were created, add a delay to allow Alegra to process them
                    if (newTercerosCreated)
                    {
                        Console.WriteLine("New terceros created in Alegra. Waiting 5 seconds for processing...");
                        await Task.Delay(5000); // 5 second delay
                    }
                }
            }
            catch (Exception) { }
        }

        public async Task<IEnumerable<Modelo.OrdenDeDespacho>> GetOrdenesDeDespacho(Guid estacion)
        {
            try
            {
                IEnumerable<Repositorio.Entities.OrdenDeDespacho> ordenes = await _ordenDeDespachoRepositorio.GetOrdenesImprimir(estacion);
                var ordenesDeDespacho = _mapper.Map<IEnumerable<Repositorio.Entities.OrdenDeDespacho>, IEnumerable<Modelo.OrdenDeDespacho>>(ordenes);
                return ordenesDeDespacho;
            }
            catch (Exception)
            {
                throw;
            }
        }
        private async Task<string> EnviarAFacturacion(Modelo.OrdenDeDespacho ordenDeDespacho, Guid estacion)
        {
            try
            {
                var terceroEntity = (await _terceroRepositorio.ObtenerTerceroPorIdentificacion(ordenDeDespacho.Identificacion)).FirstOrDefault();
                if (terceroEntity != null)
                {
                    var tercero = _mapper.Map<Repositorio.Entities.Tercero, Modelo.Tercero>(terceroEntity);
                    if (_alegra.ValidaTercero && tercero.idFacturacion == null)
                    {
                        return "error:Tercero no está apto para facturación electrónica";
                    }
                    ordenDeDespacho.Tercero = tercero;
                    
                    // Retry mechanism for newly created terceros in Alegra
                    var maxRetries = 3;
                    var retryDelay = 3000; // 3 seconds
                    
                    for (int attempt = 1; attempt <= maxRetries; attempt++)
                    {
                        try
                        {
                            var response = await _alegraFacade.GenerarFacturaElectronica(ordenDeDespacho, ordenDeDespacho.Tercero, estacion);
                            
                            // If response contains specific errors related to tercero not found, retry
                            if (response != null && 
                                (response.Contains("Combustible no creado") || 
                                 response.Contains("contact not found") ||
                                 response.Contains("tercero") ||
                                 (response.StartsWith("error:") && attempt < maxRetries)))
                            {
                                Console.WriteLine($"Attempt {attempt} failed for tercero {tercero.Identificacion}: {response}. Retrying in {retryDelay}ms...");
                                await Task.Delay(retryDelay);
                                continue;
                            }
                            
                            return response;
                        }
                        catch (Exception ex) when (attempt < maxRetries)
                        {
                            Console.WriteLine($"Attempt {attempt} failed for tercero {tercero.Identificacion}: {ex.Message}. Retrying in {retryDelay}ms...");
                            await Task.Delay(retryDelay);
                        }
                    }
                    
                    // Final attempt without retry
                    return await _alegraFacade.GenerarFacturaElectronica(ordenDeDespacho, ordenDeDespacho.Tercero, estacion);
                }
                return $"error:Tercero {ordenDeDespacho.Identificacion} no encontrado";
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return $"error:{e.Message}:{e.StackTrace}";
            }
        }
    }
}
