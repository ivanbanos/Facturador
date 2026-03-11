using AutoMapper;
using FacturacionelectronicaCore.Negocio.Contabilidad.FacturacionElectronica;
using FacturacionelectronicaCore.Negocio.Modelo;
using FacturacionelectronicaCore.Repositorio.Repositorios;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FacturaConsolidadaEntity = FacturacionelectronicaCore.Repositorio.Entities.FacturaConsolidada;

namespace FacturacionelectronicaCore.Negocio.FacturaConsolidada
{
    public class FacturaConsolidadaNegocio : IFacturaConsolidadaNegocio
    {
        private readonly IFacturaConsolidadaRepository _facturaConsolidadaRepository;
        private readonly IOrdenDeDespachoRepositorio _ordenDeDespachoRepositorio;
        private readonly ITerceroRepositorio _terceroRepositorio;
        private readonly IMapper _mapper;
        private readonly IFacturacionElectronicaFacade _facturacionElectronicaFacade;
        private readonly Alegra _alegraOptions;

        public FacturaConsolidadaNegocio(
            IFacturaConsolidadaRepository facturaConsolidadaRepository,
            IOrdenDeDespachoRepositorio ordenDeDespachoRepositorio,
            ITerceroRepositorio terceroRepositorio,
            IMapper mapper,
            IFacturacionElectronicaFacade facturacionElectronicaFacade,
            IOptions<Alegra> alegraOptions)
        {
            _facturaConsolidadaRepository = facturaConsolidadaRepository;
            _ordenDeDespachoRepositorio = ordenDeDespachoRepositorio;
            _terceroRepositorio = terceroRepositorio;
            _mapper = mapper;
            _facturacionElectronicaFacade = facturacionElectronicaFacade;
            _alegraOptions = alegraOptions.Value;
        }

        public async Task<IEnumerable<Modelo.FacturaConsolidada>> GetFacturasConsolidadas(
            DateTime? fechaDesde = null,
            DateTime? fechaHasta = null,
            string identificacionCliente = null,
            Guid? idEstacion = null,
            string estado = null)
        {
            var facturasEntity = await _facturaConsolidadaRepository.GetFacturasConsolidadas(
                fechaDesde, fechaHasta, identificacionCliente, idEstacion, estado);

            return _mapper.Map<IEnumerable<FacturaConsolidadaEntity>, IEnumerable<Modelo.FacturaConsolidada>>(facturasEntity);
        }

        public async Task<Modelo.FacturaConsolidada> GetFacturaConsolidadaPorGuid(string guid)
        {
            var facturaEntity = await _facturaConsolidadaRepository.GetFacturaConsolidadaPorGuid(guid);
            if (facturaEntity == null) return null;

            return _mapper.Map<FacturaConsolidadaEntity, Modelo.FacturaConsolidada>(facturaEntity);
        }

        public async Task<IEnumerable<Modelo.OrdenDeDespacho>> GetOrdenesPendientesConsolidacion(FiltroOrdenesConsolidacion filtro)
        {
            // Obtener órdenes del cliente en el rango de fechas especificado
            var ordenes = await _ordenDeDespachoRepositorio.GetOrdenesDeDespacho(
                filtro.FechaDesde,
                filtro.FechaHasta,
                filtro.IdentificacionCliente,
                null, // nombreTercero
                filtro.IdEstacion);

            var ordenesModelo = _mapper.Map<IEnumerable<Repositorio.Entities.OrdenDeDespacho>, IEnumerable<Modelo.OrdenDeDespacho>>(ordenes);

            // Filtrar órdenes que:
            // 1. No tengan factura electrónica (o tengan error)
            // 2. No estén ya consolidadas
            // 3. No sean de crédito directo (si está configurado para excluirlas)
            var ordenesPendientes = new List<Modelo.OrdenDeDespacho>();

            foreach (var orden in ordenesModelo)
            {
                // Verificar si ya está consolidada
                var yaConsolidada = await _facturaConsolidadaRepository.OrdenYaConsolidada(orden.guid);
                if (yaConsolidada) continue;

                // Verificar si no tiene factura electrónica o tiene error
                var sinFactura = string.IsNullOrEmpty(orden.idFacturaElectronica) || 
                               orden.idFacturaElectronica.StartsWith("error", StringComparison.OrdinalIgnoreCase);

                // Verificar si no es crédito directo (aplicar la misma lógica que el Worker)
                var esCreditoDirecto = orden.FormaDePago?.Trim().Equals("Crédito Directo", StringComparison.OrdinalIgnoreCase) ?? false;

                if (sinFactura && !esCreditoDirecto)
                {
                    ordenesPendientes.Add(orden);
                }
            }

            return ordenesPendientes.OrderByDescending(x => x.Fecha);
        }

        public async Task<Modelo.FacturaConsolidada> CrearFacturaConsolidada(CrearFacturaConsolidadaRequest request)
        {
            // Validar las órdenes antes de crear la factura
            var validationResult = await ValidarOrdenesParaConsolidacion(request.GuidsOrdenes, request.IdentificacionCliente);
            if (validationResult != "Ok")
            {
                throw new InvalidOperationException(validationResult);
            }

            // Obtener las órdenes para calcular totales
            var ordenes = new List<Repositorio.Entities.OrdenDeDespacho>();
            foreach (var guid in request.GuidsOrdenes)
            {
                var orden = await _ordenDeDespachoRepositorio.ObtenerOrdenDespachoPorGuid(guid);
                if (orden.Any())
                {
                    ordenes.AddRange(orden);
                }
            }

            // Obtener información del tercero
            var terceroEntity = await _terceroRepositorio.ObtenerTerceroPorIdentificacion(request.IdentificacionCliente);
            var tercero = terceroEntity?.FirstOrDefault();

            if (tercero == null)
            {
                throw new InvalidOperationException($"No se encontró el tercero con identificación {request.IdentificacionCliente}");
            }

            // Crear la factura consolidada
            var facturaConsolidada = new FacturaConsolidadaEntity
            {
                Consecutivo = await _facturaConsolidadaRepository.ObtenerSiguienteConsecutivo(request.IdEstacion),
                FechaDesde = request.FechaDesde,
                FechaHasta = request.FechaHasta,
                IdentificacionCliente = request.IdentificacionCliente,
                NombreCliente = tercero.Nombre,
                Cliente = tercero,
                IdEstacion = request.IdEstacion.ToString(),
                OrdenesConsolidadas = request.GuidsOrdenes,
                UsuarioCreacion = request.UsuarioCreacion,
                Observaciones = request.Observaciones
            };

            // Calcular resúmenes y totales
            CalcularResumenYTotales(facturaConsolidada, ordenes);

            // Guardar en la base de datos
            var facturaCreada = await _facturaConsolidadaRepository.CrearFacturaConsolidada(facturaConsolidada);

            return _mapper.Map<FacturaConsolidadaEntity, Modelo.FacturaConsolidada>(facturaCreada);
        }

        public async Task<string> EnviarFacturaConsolidadaAFacturacion(string guidFactura)
        {
            var facturaEntity = await _facturaConsolidadaRepository.GetFacturaConsolidadaPorGuid(guidFactura);
            if (facturaEntity == null)
            {
                return "error:Factura consolidada no encontrada";
            }

            if (facturaEntity.Estado == "Enviada" || facturaEntity.Estado == "Exitosa")
            {
                return "error:La factura ya ha sido enviada previamente";
            }

            try
            {
                // Obtener las órdenes consolidadas
                var ordenes = new List<Repositorio.Entities.OrdenDeDespacho>();
                foreach (var guidOrden in facturaEntity.OrdenesConsolidadas)
                {
                    var orden = await _ordenDeDespachoRepositorio.ObtenerOrdenDespachoPorGuid(guidOrden);
                    if (orden.Any())
                    {
                        ordenes.AddRange(orden);
                    }
                }

                if (!ordenes.Any())
                {
                    return "error:No se encontraron órdenes para consolidar";
                }

                // Convertir a modelo de negocio
                var ordenesModelo = _mapper.Map<IEnumerable<Repositorio.Entities.OrdenDeDespacho>, IEnumerable<Modelo.OrdenDeDespacho>>(ordenes);
                var terceroModelo = _mapper.Map<Repositorio.Entities.Tercero, Modelo.Tercero>(facturaEntity.Cliente);

                // Crear una orden "virtual" que represente la consolidación
                var ordenConsolidada = CrearOrdenVirtualConsolidada(facturaEntity, ordenesModelo.ToList(), terceroModelo);

                // Enviar a facturación electrónica
                var resultado = await _facturacionElectronicaFacade.GenerarFacturaElectronica(
                    ordenConsolidada, 
                    terceroModelo, 
                    Guid.Parse(facturaEntity.IdEstacion));

                // Actualizar la factura con el resultado
                facturaEntity.IdFacturaElectronica = resultado;
                facturaEntity.InfoFacturacionElectronica = resultado;
                facturaEntity.Estado = resultado.StartsWith("error") ? "Error" : "Exitosa";

                await _facturaConsolidadaRepository.ActualizarFacturaConsolidada(facturaEntity);

                return resultado;
            }
            catch (Exception ex)
            {
                // Actualizar estado a error
                facturaEntity.Estado = "Error";
                facturaEntity.InfoFacturacionElectronica = $"error:{ex.Message}";
                await _facturaConsolidadaRepository.ActualizarFacturaConsolidada(facturaEntity);

                return $"error:{ex.Message}";
            }
        }

        public async Task<Modelo.FacturaConsolidada> GetDetalleFacturaConsolidada(string guid)
        {
            var facturaEntity = await _facturaConsolidadaRepository.GetFacturaConsolidadaPorGuid(guid);
            if (facturaEntity == null) return null;

            var facturaModelo = _mapper.Map<FacturaConsolidadaEntity, Modelo.FacturaConsolidada>(facturaEntity);

            // Cargar las órdenes detalladas
            foreach (var guidOrden in facturaEntity.OrdenesConsolidadas)
            {
                var ordenes = await _ordenDeDespachoRepositorio.ObtenerOrdenDespachoPorGuid(guidOrden);
                var ordenesModelo = _mapper.Map<IEnumerable<Repositorio.Entities.OrdenDeDespacho>, IEnumerable<Modelo.OrdenDeDespacho>>(ordenes);
                facturaModelo.OrdenesDetalle.AddRange(ordenesModelo);
            }

            return facturaModelo;
        }

        public async Task<string> ValidarOrdenesParaConsolidacion(List<string> guidsOrdenes, string identificacionCliente)
        {
            if (!guidsOrdenes.Any())
            {
                return "error:Debe seleccionar al menos una orden";
            }

            var identificacionesDistintas = new HashSet<string>();

            foreach (var guid in guidsOrdenes)
            {
                // Verificar que la orden existe
                var ordenes = await _ordenDeDespachoRepositorio.ObtenerOrdenDespachoPorGuid(guid);
                if (!ordenes.Any())
                {
                    return $"error:Orden {guid} no encontrada";
                }

                var orden = ordenes.First();

                // Verificar que pertenece al cliente especificado
                if (orden.Identificacion != identificacionCliente)
                {
                    return $"error:La orden {guid} no pertenece al cliente {identificacionCliente}";
                }

                // Verificar que no esté ya consolidada
                var yaConsolidada = await _facturaConsolidadaRepository.OrdenYaConsolidada(guid);
                if (yaConsolidada)
                {
                    return $"error:La orden {guid} ya está consolidada";
                }

                // Verificar que no tenga factura electrónica exitosa
                if (!string.IsNullOrEmpty(orden.idFacturaElectronica) && 
                    !orden.idFacturaElectronica.StartsWith("error", StringComparison.OrdinalIgnoreCase))
                {
                    return $"error:La orden {guid} ya tiene factura electrónica";
                }

                identificacionesDistintas.Add(orden.Identificacion);
            }

            // Verificar que todas las órdenes pertenezcan al mismo cliente
            if (identificacionesDistintas.Count > 1)
            {
                return "error:Todas las órdenes deben pertenecer al mismo cliente";
            }

            return "Ok";
        }

        #region Métodos Privados

        private void CalcularResumenYTotales(FacturaConsolidadaEntity factura, List<Repositorio.Entities.OrdenDeDespacho> ordenes)
        {
            factura.ResumenCombustibles = new List<Repositorio.Entities.ResumenCombustible>();
            factura.Totales = new Repositorio.Entities.TotalesFactura();

            // Agrupar por tipo de combustible
            var gruposPorCombustible = ordenes.GroupBy(x => x.Combustible);

            foreach (var grupo in gruposPorCombustible)
            {
                var resumen = new Repositorio.Entities.ResumenCombustible
                {
                    TipoCombustible = grupo.Key,
                    CantidadTotal = (double)grupo.Sum(x => x.Cantidad),
                    SubTotal = (double)grupo.Sum(x => x.SubTotal),
                    Descuento = (double)grupo.Sum(x => x.Descuento),
                    Iva = (double)grupo.Sum(x => (double)x.Total - (double)x.SubTotal), // Aproximación del IVA
                    Total = (double)grupo.Sum(x => x.Total),
                    NumeroOrdenes = grupo.Count()
                };

                factura.ResumenCombustibles.Add(resumen);
            }

            // Calcular totales generales
            factura.Totales.SubTotal = factura.ResumenCombustibles.Sum(x => x.SubTotal);
            factura.Totales.DescuentoTotal = factura.ResumenCombustibles.Sum(x => x.Descuento);
            factura.Totales.IvaTotal = factura.ResumenCombustibles.Sum(x => x.Iva);
            factura.Totales.Total = factura.ResumenCombustibles.Sum(x => x.Total);
            factura.Totales.TotalOrdenes = ordenes.Count;
            factura.Totales.CantidadTotalCombustible = factura.ResumenCombustibles.Sum(x => x.CantidadTotal);
        }

        private Modelo.OrdenDeDespacho CrearOrdenVirtualConsolidada(
            FacturaConsolidadaEntity factura, 
            List<Modelo.OrdenDeDespacho> ordenes,
            Modelo.Tercero tercero)
        {
            // Crear una orden "virtual" que represente toda la consolidación
            var ordenVirtual = new Modelo.OrdenDeDespacho
            {
                guid = factura.Guid,
                IdVentaLocal = factura.Consecutivo,
                Fecha = factura.FechaCreacion,
                Identificacion = factura.IdentificacionCliente,
                NombreTercero = factura.NombreCliente,
                SubTotal = (decimal)factura.Totales.SubTotal,
                Descuento = (decimal)factura.Totales.DescuentoTotal,
                Total = (double)factura.Totales.Total,
                Cantidad = (double)factura.Totales.CantidadTotalCombustible,
                Combustible = "CONSOLIDADO", // Tipo especial para identificar consolidaciones
                FormaDePago = "Consolidada",
                Tercero = tercero,
                IdEstacion = int.Parse(factura.IdEstacion),
                Precio = factura.Totales.CantidadTotalCombustible > 0 
                    ? (double)((double)factura.Totales.SubTotal / factura.Totales.CantidadTotalCombustible)
                    : 0
            };

            return ordenVirtual;
        }

        #endregion
    }
}