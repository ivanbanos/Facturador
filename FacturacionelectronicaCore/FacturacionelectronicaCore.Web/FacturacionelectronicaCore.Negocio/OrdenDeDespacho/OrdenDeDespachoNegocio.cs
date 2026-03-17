using AutoMapper;
using EstacionesServicio.Modelo;
using EstacionesServicio.Negocio.Extention;
using FacturacionelectronicaCore.Negocio.Contabilidad.FacturacionElectronica;
using FacturacionelectronicaCore.Negocio.Extention;
using FacturacionelectronicaCore.Negocio.Modelo;
using FacturacionelectronicaCore.Repositorio.Entities;
using FacturacionelectronicaCore.Repositorio.Repositorios;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using FacturasEntity = EstacionesServicio.Modelo.FacturasEntity;

namespace FacturacionelectronicaCore.Negocio.OrdenDeDespacho
{
    public class OrdenDeDespachoNegocio : IOrdenDeDespachoNegocio
    // ...existing code...
    {
        private readonly IOrdenDeDespachoRepositorio _ordenDeDespachoRepositorio;
        private readonly ITerceroRepositorio _terceroRepositorio;
        private readonly IMapper _mapper;
        private readonly IFacturacionElectronicaFacade _alegraFacade;
        private readonly Alegra _alegra;
        private readonly IValidadorGuidAFacturaElectronica _validadorGuidAFacturaElectronica;
        private readonly ICombustiblesEstacionRepository _combustiblesEstacionRepository;

        public OrdenDeDespachoNegocio(IOrdenDeDespachoRepositorio ordenDeDespachoRepositorio,
                                       IMapper mapper, IFacturacionElectronicaFacade alegraFacade, IOptions<Alegra> alegra, ITerceroRepositorio terceroRepositorio, IValidadorGuidAFacturaElectronica validadorGuidAFacturaElectronica, ICombustiblesEstacionRepository combustiblesEstacionRepository)
        {
            _ordenDeDespachoRepositorio = ordenDeDespachoRepositorio;
            _mapper = mapper;
            _alegraFacade = alegraFacade;
            _alegra = alegra.Value;
            _terceroRepositorio = terceroRepositorio;
            _validadorGuidAFacturaElectronica = validadorGuidAFacturaElectronica;
            _combustiblesEstacionRepository = combustiblesEstacionRepository;
        }

        // Normalize incoming search DateTime using configured ServerTimeOffsetHours.
        // This is applied even when payload values arrive as UTC (with trailing Z),
        // so date-only searches map correctly to stored reporting boundaries.
        private DateTime? ConvertToServerTime(DateTime? input)
        {
            if (!input.HasValue) return null;

            try
            {
                var value = input.Value;

                if (_alegra == null || !_alegra.ServerTimeOffsetHours.HasValue)
                {
                    return value;
                }

                var offset = _alegra.ServerTimeOffsetHours.GetValueOrDefault(0);
                return value.AddHours(offset);
            }
            catch
            {
                // If anything goes wrong, fall back to original value
                return input;
            }
        }

        private DateTime ConvertFromServerTimeForSearchResult(DateTime input)
        {
            try
            {
                var utcValue = input.Kind == DateTimeKind.Unspecified
                    ? DateTime.SpecifyKind(input, DateTimeKind.Utc)
                    : input.ToUniversalTime();

                if (_alegra == null || !_alegra.ServerTimeOffsetHoursSearch.HasValue)
                {
                    return DateTime.SpecifyKind(utcValue, DateTimeKind.Unspecified);
                }

                var adjusted = utcValue.AddHours(_alegra.ServerTimeOffsetHoursSearch.Value);
                return DateTime.SpecifyKind(adjusted, DateTimeKind.Unspecified);
            }
            catch
            {
                return DateTime.SpecifyKind(input, DateTimeKind.Unspecified);
            }
        }

        private bool DebeNormalizarPrecioLegacy(decimal precio)
        {
            return _alegra != null && _alegra.NormalizarPrecioLegacyDiv10 && precio > 20000;
        }

        private bool EsSilog2()
        {
            return string.Equals(_alegra?.Proveedor, "SILOG2", StringComparison.OrdinalIgnoreCase);
        }

        private async Task<int> NormalizarFalsosNegativosSilog2Async(IEnumerable<Repositorio.Entities.OrdenDeDespacho> ordenes)
        {
            if (!EsSilog2() || ordenes == null)
            {
                return 0;
            }

            var normalizadas = 0;
            foreach (var orden in ordenes)
            {
                if (orden == null || string.IsNullOrWhiteSpace(orden.guid))
                {
                    continue;
                }

                if (!TryBuildOkFromSilogFalseNegative(orden.idFacturaElectronica, out var idNormalizado))
                {
                    continue;
                }

                if (string.Equals(orden.idFacturaElectronica, idNormalizado, StringComparison.Ordinal))
                {
                    continue;
                }

                await _ordenDeDespachoRepositorio.SetIdFacturaElectronicaOrdenesdeDespacho(idNormalizado, orden.guid);
                orden.idFacturaElectronica = idNormalizado;
                normalizadas++;
            }

            return normalizadas;
        }

        private static bool TryBuildOkFromSilogFalseNegative(string idFacturaElectronica, out string idNormalizado)
        {
            idNormalizado = null;

            if (string.IsNullOrWhiteSpace(idFacturaElectronica))
            {
                return false;
            }

            if (!idFacturaElectronica.StartsWith("Error:", StringComparison.OrdinalIgnoreCase)
                || idFacturaElectronica.IndexOf("Factura generada exitosamente", StringComparison.OrdinalIgnoreCase) < 0)
            {
                return false;
            }

            var json = ExtractFirstJsonObject(idFacturaElectronica);
            if (string.IsNullOrWhiteSpace(json))
            {
                return false;
            }

            try
            {
                var parsed = JObject.Parse(json);
                var cufe = parsed["cufe"]?.ToString();
                if (string.IsNullOrWhiteSpace(cufe))
                {
                    return false;
                }

                var currentOutput = parsed["message"]?["currentOutput"] as JObject;
                var prefijo = currentOutput?["prefijo"]?.ToString()
                              ?? currentOutput?["prefijoResolucion"]?.ToString()
                              ?? string.Empty;
                var numero = currentOutput?["numero"]?.ToString()
                             ?? currentOutput?["numeroResolucion"]?.ToString()
                             ?? string.Empty;

                var prefijoConsecutivo = string.Concat(prefijo?.Trim(), numero?.Trim());
                if (string.IsNullOrWhiteSpace(prefijoConsecutivo))
                {
                    return false;
                }

                idNormalizado = $"Ok:{prefijoConsecutivo}:{cufe}";
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static string ExtractFirstJsonObject(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            var start = text.IndexOf('{');
            if (start < 0)
            {
                return null;
            }

            var depth = 0;
            var inString = false;
            var escaped = false;

            for (var i = start; i < text.Length; i++)
            {
                var ch = text[i];

                if (inString)
                {
                    if (escaped)
                    {
                        escaped = false;
                    }
                    else if (ch == '\\')
                    {
                        escaped = true;
                    }
                    else if (ch == '"')
                    {
                        inString = false;
                    }

                    continue;
                }

                if (ch == '"')
                {
                    inString = true;
                    continue;
                }

                if (ch == '{')
                {
                    depth++;
                    continue;
                }

                if (ch == '}')
                {
                    depth--;
                    if (depth == 0)
                    {
                        return text.Substring(start, i - start + 1);
                    }
                }
            }

            return null;
        }



        /// <inheritdoc />
        public async Task<IEnumerable<Modelo.OrdenDeDespacho>> GetOrdenesDeDespacho(FiltroBusqueda filtroOrdenDeDespacho)
        {
            try
            {
                // Normalize incoming search dates to the configured server timezone before hitting the repository.
                var fechaInicialServer = ConvertToServerTime(filtroOrdenDeDespacho.FechaInicial);
                var fechaFinalServer = ConvertToServerTime(filtroOrdenDeDespacho.FechaFinal);

                var ordenesDeDespacho = await _ordenDeDespachoRepositorio.GetOrdenesDeDespacho(fechaInicialServer,
                        fechaFinalServer, filtroOrdenDeDespacho.Identificacion, filtroOrdenDeDespacho.NombreTercero, filtroOrdenDeDespacho.Estacion);

                await NormalizarFalsosNegativosSilog2Async(ordenesDeDespacho);

                var ordenes = _mapper.Map<IEnumerable<Repositorio.Entities.OrdenDeDespacho>, IEnumerable<Modelo.OrdenDeDespacho>>(ordenesDeDespacho);

                var nombresPorIdentificacion = new Dictionary<string, string>();
                foreach (var factura in ordenes)
                {
                    factura.Estado = factura.idFacturaElectronica == null ? factura.Estado : "Anulada";
                    factura.Identificacion = factura.Identificacion == null ? "222222222222" : factura.Identificacion;
                    if (!nombresPorIdentificacion.ContainsKey(factura.Identificacion) || string.IsNullOrEmpty(nombresPorIdentificacion[factura.Identificacion]))
                    {
                        var tercero = await _terceroRepositorio.ObtenerTerceroPorIdentificacion(factura.Identificacion);
                        if (tercero.FirstOrDefault() != null)
                        {
                            if (!nombresPorIdentificacion.ContainsKey(factura.Identificacion))
                            {
                                nombresPorIdentificacion.Add(factura.Identificacion, tercero.FirstOrDefault()?.Nombre);

                            }
                            else
                            {
                                nombresPorIdentificacion[factura.Identificacion] = tercero.FirstOrDefault()?.Nombre;

                            }
                        }
                        else
                        {
                            if (!nombresPorIdentificacion.ContainsKey(factura.Identificacion))
                            {
                                nombresPorIdentificacion.Add(factura.Identificacion, " ");

                            }
                        }
                    }
                    if (DebeNormalizarPrecioLegacy(Convert.ToDecimal(factura.Precio)))
                    {

                        factura.Precio /= 10;
                        factura.SubTotal /= 10;
                        factura.Total /= 10;
                        factura.Descuento /= 10;
                    }
                    factura.NombreTercero = nombresPorIdentificacion[factura.Identificacion];
                    factura.Fecha = ConvertFromServerTimeForSearchResult(factura.Fecha);
                }
                return ordenes.OrderByDescending(x => x.IdVentaLocal);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Task<int> AddOrdenesImprimir(IEnumerable<FacturasEntity> ordenDeDespachos)
        {
            try
            {
                return _ordenDeDespachoRepositorio.AddOrdenesImprimir(_mapper.Map<IEnumerable<FacturasEntity>, IEnumerable<Repositorio.Entities.FacturasEntity>>(ordenDeDespachos));
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Task AnularOrdenes(IEnumerable<FacturasEntity> ordenes)
        {
            try
            {
                var ordenesList = _mapper.Map<IEnumerable<FacturasEntity>, IEnumerable<Repositorio.Entities.FacturasEntity>>(ordenes);
                return _ordenDeDespachoRepositorio.AnularOrdenes(ordenesList);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<string> CrearFacturaOrdenesDeDespacho(IEnumerable<OrdenesDeDespachoGuids> ordenesDeDespacho)
        {
            var guids = ordenesDeDespacho.Select(x => x.Guid);
            if (_validadorGuidAFacturaElectronica.FacturasSiendoProceada(guids))
            {
                return "Factura electrónica siendo procesada";
            }
            var ordenes = new List<Modelo.OrdenDeDespacho>();
            foreach (var guid in ordenesDeDespacho)
            {
                var ordenDeDespachoEntity = (await _ordenDeDespachoRepositorio.ObtenerOrdenDespachoPorGuid(guid.Guid)).FirstOrDefault();

                if (ordenDeDespachoEntity == null)
                {
                    _validadorGuidAFacturaElectronica.SacarFacturas(guids);
                    return $"Factura {guid.Guid} no existe";
                }
                if (ordenDeDespachoEntity.idFacturaElectronica != null)
                {
                    _validadorGuidAFacturaElectronica.SacarFacturas(guids);
                    return "Una orden ya tiene factura electrónica existente";
                }

                ordenDeDespachoEntity.Fecha = ordenDeDespachoEntity.Fecha.ToLocalTime();
                ordenes.Add(_mapper.Map<Repositorio.Entities.OrdenDeDespacho, Modelo.OrdenDeDespacho>(ordenDeDespachoEntity));
            }
            if (ordenes.GroupBy(x => x.Identificacion).Count() > 1)
            {
                _validadorGuidAFacturaElectronica.SacarFacturas(guids);
                return $"Las ordenes deben pertenecer al mismo tercero";

            }
            var terceroEntity = (await _terceroRepositorio.ObtenerTerceroPorIdentificacion(ordenes.First().Identificacion)).FirstOrDefault();
            var tercero = _mapper.Map<Repositorio.Entities.Tercero, Modelo.Tercero>(terceroEntity);
            if (tercero.idFacturacion == null)
            {

                _validadorGuidAFacturaElectronica.SacarFacturas(guids);
                return "Tercero no está apto para facturación electrónica";
            }

            var combustibles = new List<string>();
            foreach (var factura in ordenes)
            {
                if (!combustibles.Contains(factura.Combustible))
                {
                    combustibles.Add(factura.Combustible);
                }
            }
            var items = combustibles.Select(x => _alegraFacade.GetItem(x, null).Result);

            try
            {
                var idFacturaElectronica = "error";// await _alegraFacade.GenerarFacturaElectronica(ordenes, tercero, items);
                foreach (var orden in ordenes)
                {
                    await _ordenDeDespachoRepositorio.SetIdFacturaElectronicaOrdenesdeDespacho(idFacturaElectronica, orden.guid);
                }
                _validadorGuidAFacturaElectronica.SacarFacturas(guids);
                return "Ok";
            }
            catch (Exception e)
            {
                _validadorGuidAFacturaElectronica.SacarFacturas(guids);
                return $"Fallo al crear factura electrónica Razón: {e.Message}";
            }
        }

        public async Task<Modelo.OrdenDeDespacho> ObtenerOrdenDespachoPorIdVentaLocal(int idVentaLocal, Guid estacion)
        {


            var ordenDeDespachoEntity = (await _ordenDeDespachoRepositorio.ObtenerOrdenDespachoPorIdVentaLocal(idVentaLocal, estacion)).FirstOrDefault();
            if (ordenDeDespachoEntity == null)
            {
                return null;
            }
            var factura = _mapper.Map<Repositorio.Entities.OrdenDeDespacho, Modelo.OrdenDeDespacho>(ordenDeDespachoEntity);
            if (DebeNormalizarPrecioLegacy(Convert.ToDecimal(factura.Precio)))
            {

                factura.Precio /= 10;
                factura.SubTotal /= 10;
                factura.Total /= 10;
                factura.Descuento /= 10;
            }
            return factura;
        }

        public async Task<IEnumerable<Modelo.OrdenDeDespacho>> ObtenerOrdenesPorTurno(Guid turno)
        {
            var facturasEntity = await _ordenDeDespachoRepositorio.ObtenerOrdenesPorTurno(turno);

            var ordenes = _mapper.Map<IEnumerable<Repositorio.Entities.OrdenDeDespacho>, IEnumerable<Modelo.OrdenDeDespacho>>(facturasEntity);

            foreach (var orden in ordenes)
            {
                if (DebeNormalizarPrecioLegacy(Convert.ToDecimal(orden.Precio)))
                {

                    orden.Precio /= 10;
                    orden.SubTotal /= 10;
                    orden.Total /= 10;
                    orden.Descuento /= 10;
                }
            }
            return ordenes;
        }

        public async Task<string> EnviarAFacturacion(Modelo.OrdenDeDespacho ordenDeDespacho, Guid estacion)
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
                    var response = await _alegraFacade.GenerarFacturaElectronica(ordenDeDespacho, ordenDeDespacho.Tercero, estacion);

                    return response;
                }
                return $"error:Tercero {ordenDeDespacho.Identificacion} no encontrado";
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return $"error:{e.Message}:{e.StackTrace}";
            }
        }

        /// <summary>
        /// Gets dispatch orders without electronic invoice and with 'Crédito Directo' as payment method.
        /// </summary>
        public async Task<IEnumerable<Modelo.OrdenDeDespacho>> GetOrdenesSinFacturaElectronicaCreditoDirecto(FiltroBusqueda filtroOrdenDeDespacho)
        {
            var ordenesDeDespacho = await _ordenDeDespachoRepositorio.GetOrdenesDeDespacho(
                // Normalize search dates to server timezone
                ConvertToServerTime(filtroOrdenDeDespacho.FechaInicial),
                ConvertToServerTime(filtroOrdenDeDespacho.FechaFinal),
                filtroOrdenDeDespacho.Identificacion,
                filtroOrdenDeDespacho.NombreTercero,
                filtroOrdenDeDespacho.Estacion);

            var ordenes = _mapper.Map<IEnumerable<Repositorio.Entities.OrdenDeDespacho>, IEnumerable<Modelo.OrdenDeDespacho>>(ordenesDeDespacho);

            // Filter: no electronic invoice and payment method is 'Crédito Directo'
            var result = ordenes.Where(x => string.IsNullOrEmpty(x.idFacturaElectronica)
                && x.FormaDePago != null
                && x.FormaDePago.Trim().Equals("Crédito Directo", StringComparison.OrdinalIgnoreCase));

            return result;
        }

        /// <summary>
        /// Re-sends electronic invoicing for a list of idVentaLocal and a station Guid.
        /// </summary>
        public async Task<List<string>> ReenviarOrdenesDespachoPorIdVentaLocal(List<int> idVentaLocalList, Guid estacion)
        {
            var resultados = new List<string>();
            var esSilog2 = string.Equals(_alegra?.Proveedor, "SILOG2", StringComparison.OrdinalIgnoreCase);
            foreach (var idVentaLocal in idVentaLocalList)
            {
                var ordenes = await _ordenDeDespachoRepositorio.ObtenerOrdenDespachoPorIdVentaLocal(idVentaLocal, estacion);
                foreach (var orden in ordenes)
                {
                    if (orden != null
                        && (_alegra.EnviaCreditos || (!orden.FormaDePago.ToLower().Contains("dir") && !orden.FormaDePago.ToLower().Contains("calibra") && !orden.FormaDePago.ToLower().Contains("consum") && !orden.FormaDePago.ToLower().Contains("puntos")))
                        && (esSilog2
                            ? (string.IsNullOrWhiteSpace(orden.idFacturaElectronica)
                                || !orden.idFacturaElectronica.StartsWith("Ok", StringComparison.OrdinalIgnoreCase))
                            : (string.IsNullOrEmpty(orden.idFacturaElectronica)
                                || orden.idFacturaElectronica.StartsWith("error")
                                || orden.idFacturaElectronica.Contains("Bad Request"))))
                    {
                        var ordenModelo = _mapper.Map<Repositorio.Entities.OrdenDeDespacho, Modelo.OrdenDeDespacho>(orden);
                        ordenModelo.Tercero = _mapper.Map<Repositorio.Entities.Tercero, Modelo.Tercero>(
                            (await _terceroRepositorio.ObtenerTerceroPorIdentificacion(orden.Identificacion)).FirstOrDefault());

                        var resultado = await EnviarAFacturacion(ordenModelo, estacion);
                        orden.idFacturaElectronica = resultado;

                        resultados.Add($"Orden {idVentaLocal}: {resultado}");
                    }
                    else if (orden != null)
                    {
                        resultados.Add($"Orden {idVentaLocal}: Ya tiene factura electrónica");
                    }
                    else
                    {
                        resultados.Add($"Orden {idVentaLocal}: No encontrada");
                    }
                }
                await _ordenDeDespachoRepositorio.AddRange(ordenes, estacion);
            }
            return resultados;
        }

        public async Task ReenviarFacturas(DateTime fechaInicial, DateTime fechaFinal, Guid estacion)
        {
            // Normalize the provided range to server timezone before querying
            var fechaInicialServer = ConvertToServerTime(fechaInicial);
            var fechaFinalServer = ConvertToServerTime(fechaFinal);
            var esSilog2 = string.Equals(_alegra?.Proveedor, "SILOG2", StringComparison.OrdinalIgnoreCase);
            var ordenes = await _ordenDeDespachoRepositorio.GetOrdenesDeDespacho(fechaInicialServer, fechaFinalServer, null, null, estacion);

            if (ordenes != null)
            {
                foreach (var orden in ordenes)
                {
                    if (esSilog2)
                    {
                        if (string.IsNullOrWhiteSpace(orden.idFacturaElectronica) || !orden.idFacturaElectronica.StartsWith("Ok", StringComparison.OrdinalIgnoreCase))
                        {
                            var ordenModelo = _mapper.Map<Repositorio.Entities.OrdenDeDespacho, Modelo.OrdenDeDespacho>(orden);
                            var terceroModelo = _mapper.Map<Repositorio.Entities.Tercero, Modelo.Tercero>(
                                (await _terceroRepositorio.ObtenerTerceroPorIdentificacion(orden.Identificacion)).FirstOrDefault());

                            orden.idFacturaElectronica = await _alegraFacade.GenerarFacturaElectronica(ordenModelo, terceroModelo, estacion);
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else if (orden.idFacturaElectronica == null)
                    {
                        continue;
                    }
                    else if (orden.idFacturaElectronica.StartsWith("error") || orden.idFacturaElectronica.Contains("Bad Request"))
                    {
                        var ordenModelo = _mapper.Map<Repositorio.Entities.OrdenDeDespacho, Modelo.OrdenDeDespacho>(orden);
                        var terceroModelo = _mapper.Map<Repositorio.Entities.Tercero, Modelo.Tercero>(
                            (await _terceroRepositorio.ObtenerTerceroPorIdentificacion(orden.Identificacion)).FirstOrDefault());

                        orden.idFacturaElectronica = await _alegraFacade.GenerarFacturaElectronica(ordenModelo, terceroModelo, estacion);
                    }
                    else
                    {
                        var ordenModelo = _mapper.Map<Repositorio.Entities.OrdenDeDespacho, Modelo.OrdenDeDespacho>(orden);
                        var terceroModelo = _mapper.Map<Repositorio.Entities.Tercero, Modelo.Tercero>(
                            (await _terceroRepositorio.ObtenerTerceroPorIdentificacion(orden.Identificacion)).FirstOrDefault());

                        orden.idFacturaElectronica = await _alegraFacade.GenerarFacturaElectronica(ordenModelo, terceroModelo, estacion);
                        //orden.idFacturaElectronica = await _alegraFacade.ReenviarFactura(orden, estacion);
                    }

                    if (true)
                    {

                        var ordenesentity = new List<Repositorio.Entities.OrdenDeDespacho>
                    {
                        new Repositorio.Entities.OrdenDeDespacho()
                        {
                        guid = orden.guid.ToString(),
                        Cantidad = orden.Cantidad,
                        Cara = orden.Cara,
                        Combustible = orden.Combustible,
                        Descuento = orden.Descuento,
                        Estado = orden.Estado,
                        Fecha = orden.Fecha,
                        FechaReporte = orden.FechaReporte,
                        FechaProximoMantenimiento = orden.FechaProximoMantenimiento,
                        FormaDePago = orden.FormaDePago,
                        FormaDePago2 = orden.FormaDePago2,
                        Total1 = orden.Total1,
                        Total2 = orden.Total2,
                        Identificacion = orden.Identificacion,
                        IdentificacionTercero = orden.IdentificacionTercero,
                        IdEstacion = orden.IdEstacion,
                        IdEstadoActual = orden.IdEstadoActual,
                        IdFactura = orden.IdFactura,
                        IdInterno = orden.IdInterno,
                        IdLocal = orden.IdLocal,
                        IdTerceroLocal = orden.IdTerceroLocal,
                        IdVentaLocal = orden.IdVentaLocal,
                        Kilometraje = orden.Kilometraje,
                        Manguera = orden.Manguera,
                        NombreTercero = orden.NombreTercero,
                        Placa = orden.Placa,
                        Precio = orden.Precio,
                        SubTotal = orden.SubTotal,
                        Surtidor = orden.Surtidor,
                        Total = orden.Total,
                        idFacturaElectronica = orden.idFacturaElectronica ?? orden?.idFacturaElectronica,
                        Vendedor = orden.Vendedor,
                        }
                    };
                        await _ordenDeDespachoRepositorio.AddRange(ordenesentity, estacion);
                    }


                }
            }
        }

        public async Task<ReporteFiscal> GetReporteFiscal(FiltroBusqueda filtroFactura)
        {
            try
            {
                var ordenes = await GetOrdenesDeDespacho(filtroFactura).ConfigureAwait(true);
                if (!ordenes.Any())
                {
                    return new ReporteFiscal
                    {
                        ConsolidadoOrdenesAnuladas = new List<ConsolidadoCombustible>(),
                        TotalDeOrdenes = 0,
                        ConsolidadosOrdenes = new List<ConsolidadoCombustible>(),
                        consolidadoClienteOrdenes = new List<ConsolidadoCliente>(),
                        TotalOrdenesAnuladas = 0,
                        ConsolidadoFormaPagoOrdenes = new List<ConsolidadoFormaPago>(),
                    };
                }

                var preciosEstacion = await GetPreciosCombustiblesEstacion(filtroFactura?.Estacion ?? Guid.Empty).ConfigureAwait(false);

                var reporte = new ReporteFiscal
                {
                    ConsolidadoOrdenesAnuladas = !ordenes.Any() ? new List<ConsolidadoCombustible>() : GetConsolidadosOrdenes(ordenes.Where(EsOrdenAnuladaParaReporte), preciosEstacion),
                    TotalDeOrdenes = !ordenes.Any() ? 0 : ordenes.Count(),
                    ConsolidadosOrdenes = !ordenes.Any() ? new List<ConsolidadoCombustible>() : GetConsolidadosOrdenes(ordenes.Where(orden => !EsOrdenAnuladaParaReporte(orden)), preciosEstacion),
                    consolidadoClienteOrdenes = !ordenes.Any() ? new List<ConsolidadoCliente>() : GetConsolidadosOrdenesCliente(ordenes),
                    TotalOrdenesAnuladas = !ordenes.Any() ? 0 : ordenes.Count(EsOrdenAnuladaParaReporte),
                    ConsolidadoFormaPagoOrdenes = !ordenes.Any() ? new List<ConsolidadoFormaPago>() : GetConsolidadoFormaPagoOrdenes(ordenes),
                };
                return reporte;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private IEnumerable<ConsolidadoFormaPago> GetConsolidadoFormaPagoOrdenes(IEnumerable<Modelo.OrdenDeDespacho> ordenes)
        {
            return ordenes
                .GroupBy(o => string.IsNullOrWhiteSpace(o.FormaDePago) ? "Sin forma de pago" : o.FormaDePago.Trim(), StringComparer.OrdinalIgnoreCase)
                .Select(g => new ConsolidadoFormaPago
                {
                    FormaPago = g.Key,
                    CantidadFacturas = g.Count(),
                    CantidadCombustible = g.Sum(x => Convert.ToDecimal(x.Cantidad)),
                    Total = g.Sum(x => Convert.ToDecimal(x.Total))
                })
                .ToList();
        }

        private static bool EsOrdenAnuladaParaReporte(Modelo.OrdenDeDespacho orden)
        {
            return !string.IsNullOrWhiteSpace(orden?.idFacturaElectronica);
        }

        private IEnumerable<ConsolidadoCliente> GetConsolidadosOrdenesCliente(IEnumerable<Modelo.OrdenDeDespacho> ordenes)
        {
            var consolidados = new List<ConsolidadoCliente>();
            foreach (var orden in ordenes)
            {
                if (!consolidados.Any(consolidado => consolidado.Cliente == orden.NombreTercero))
                {
                    consolidados.Add(new ConsolidadoCliente
                    {
                        Cliente = orden.NombreTercero,
                        Cantidad = 0,
                        Total = 0
                    });
                }
                var consolidado = consolidados.First(consolidado => consolidado.Cliente == orden.NombreTercero);
                consolidado.Cantidad += Convert.ToDecimal(orden.Cantidad);
                consolidado.Total += Convert.ToDecimal(orden.Total);
            }

            return consolidados;
        }



        private async Task<Dictionary<string, decimal>> GetPreciosCombustiblesEstacion(Guid estacion)
        {
            var precios = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

            if (estacion == Guid.Empty)
            {
                return precios;
            }

            var combustibles = await _combustiblesEstacionRepository.GetCombustiblesEstacion(estacion).ConfigureAwait(false);
            foreach (var combustible in combustibles ?? Enumerable.Empty<FacturacionelectronicaCore.Repositorio.Entities.CombustibleEstacion>())
            {
                var clave = (combustible.Combustible ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(clave) || combustible.Precio <= 0)
                {
                    continue;
                }

                precios[clave] = combustible.Precio;
            }

            return precios;
        }

        private IEnumerable<ConsolidadoCombustible> GetConsolidadosOrdenes(IEnumerable<Modelo.OrdenDeDespacho> ordenes, IDictionary<string, decimal> preciosEstacion)
        {
            var consolidados = new List<ConsolidadoCombustible>();
            foreach (var orden in ordenes)
            {
                var combustible = (orden.Combustible ?? string.Empty).Trim();

                if (!consolidados.Any(consolidado => consolidado.Combustible.Equals(combustible, StringComparison.OrdinalIgnoreCase)))
                {
                    consolidados.Add(new ConsolidadoCombustible
                    {
                        Combustible = combustible,
                        Cantidad = 0,
                        Total = 0,
                        Precio = 0,
                        PrecioActual = 0
                    });
                }
                var consolidado = consolidados.First(consolidado => consolidado.Combustible.Equals(combustible, StringComparison.OrdinalIgnoreCase));
                consolidado.Cantidad += Convert.ToDecimal(orden.Cantidad);
                consolidado.Total += Convert.ToDecimal(orden.Total);

                var precioPromedio = consolidado.Cantidad > 0
                    ? Math.Round(consolidado.Total / consolidado.Cantidad, 3)
                    : 0;

                consolidado.Precio = precioPromedio;

                if (preciosEstacion != null
                    && preciosEstacion.TryGetValue(combustible, out var precioGuardado)
                    && precioGuardado > 0)
                {
                    consolidado.PrecioActual = precioGuardado;
                }
                else
                {
                    consolidado.PrecioActual = precioPromedio;
                }
            }

            return consolidados;
        }


    }
}


