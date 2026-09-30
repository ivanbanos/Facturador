using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using FacturacionelectronicaCore.Negocio.FacturaConsolidada;
using FacturacionelectronicaCore.Negocio.Modelo;

namespace FacturacionelectronicaCore.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FacturaConsolidadaController : ControllerBase
    {
        private readonly IFacturaConsolidadaNegocio _facturaConsolidadaNegocio;

        public FacturaConsolidadaController(IFacturaConsolidadaNegocio facturaConsolidadaNegocio)
        {
            _facturaConsolidadaNegocio = facturaConsolidadaNegocio;
        }

        /// <summary>
        /// 1. Listar facturas consolidadas por estación y rango de fechas
        /// </summary>
        [HttpGet("listar")]
        public async Task<ActionResult<IEnumerable<FacturaConsolidada>>> ListarFacturasConsolidadas(
            [FromQuery] string idEstacion,
            [FromQuery] DateTime? fechaInicio,
            [FromQuery] DateTime? fechaFin,
            [FromQuery] string estado = null)
        {
            try
            {
                Guid? estacionGuid = string.IsNullOrEmpty(idEstacion) ? null : Guid.Parse(idEstacion);
                var facturas = await _facturaConsolidadaNegocio.GetFacturasConsolidadas(
                    fechaInicio, fechaFin, null, estacionGuid, estado);
                return Ok(facturas);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error al obtener facturas consolidadas: {ex.Message}");
            }
        }

        /// <summary>
        /// 2. Obtener órdenes pendientes para consolidación
        /// </summary>
        [HttpGet("ordenes-pendientes")]
        public async Task<ActionResult<IEnumerable<OrdenDeDespacho>>> ObtenerOrdenesPendientes(
            [FromQuery] string idEstacion,
            [FromQuery] DateTime? fechaInicio,
            [FromQuery] DateTime? fechaFin,
            [FromQuery] string identificacionCliente = null)
        {
            try
            {
                if (string.IsNullOrEmpty(idEstacion))
                {
                    return BadRequest("IdEstacion es requerido");
                }

                var filtro = new FiltroOrdenesConsolidacion
                {
                    IdEstacion = Guid.Parse(idEstacion),
                    FechaDesde = fechaInicio,
                    FechaHasta = fechaFin,
                    IdentificacionCliente = identificacionCliente
                };

                var ordenes = await _facturaConsolidadaNegocio.GetOrdenesPendientesConsolidacion(filtro);
                return Ok(ordenes);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error al obtener órdenes pendientes: {ex.Message}");
            }
        }

        /// <summary>
        /// 3. Crear factura consolidada
        /// </summary>
        [HttpPost("crear")]
        public async Task<ActionResult<FacturaConsolidada>> CrearFacturaConsolidada(
            [FromBody] CrearFacturaConsolidadaRequest request)
        {
            try
            {
                // Validar la solicitud
                var validacion = await _facturaConsolidadaNegocio.ValidarOrdenesParaConsolidacion(
                    request.GuidsOrdenes, request.IdentificacionCliente);

                if (validacion.StartsWith("error:"))
                {
                    return BadRequest(validacion.Substring(6)); // Remover "error:" del mensaje
                }

                // Crear la factura consolidada
                var facturaConsolidada = await _facturaConsolidadaNegocio.CrearFacturaConsolidada(request);
                return Ok(facturaConsolidada);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error al crear factura consolidada: {ex.Message}");
            }
        }

        /// <summary>
        /// 4. Enviar factura consolidada a facturación electrónica
        /// </summary>
        [HttpPost("{guid}/enviar-facturacion")]
        public async Task<ActionResult<string>> EnviarFacturacionElectronica(string guid)
        {
            try
            {
                var resultado = await _facturaConsolidadaNegocio.EnviarFacturaConsolidadaAFacturacion(guid);
                
                if (resultado.StartsWith("error:"))
                {
                    return BadRequest(resultado.Substring(6)); // Remover "error:" del mensaje
                }

                return Ok(new { mensaje = "Factura enviada exitosamente", resultado = resultado });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error al enviar a facturación electrónica: {ex.Message}");
            }
        }

        /// <summary>
        /// 5. Ver información detallada de una factura consolidada
        /// </summary>
        [HttpGet("{guid}/detalle")]
        public async Task<ActionResult<FacturaConsolidada>> ObtenerDetalleFactura(string guid)
        {
            try
            {
                var detalle = await _facturaConsolidadaNegocio.GetDetalleFacturaConsolidada(guid);
                
                if (detalle == null)
                {
                    return NotFound($"No se encontró la factura consolidada con GUID: {guid}");
                }

                return Ok(detalle);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error al obtener detalle de factura: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtener órdenes disponibles para consolidar agrupadas por cliente
        /// </summary>
        [HttpGet("ordenes-agrupadas")]
        public async Task<ActionResult<object>> ObtenerOrdenesAgrupadasPorCliente(
            [FromQuery] string idEstacion,
            [FromQuery] DateTime? fechaInicio,
            [FromQuery] DateTime? fechaFin)
        {
            try
            {
                if (string.IsNullOrEmpty(idEstacion))
                {
                    return BadRequest("IdEstacion es requerido");
                }

                var filtro = new FiltroOrdenesConsolidacion
                {
                    IdEstacion = Guid.Parse(idEstacion),
                    FechaDesde = fechaInicio,
                    FechaHasta = fechaFin
                };

                var ordenes = await _facturaConsolidadaNegocio.GetOrdenesPendientesConsolidacion(filtro);

                // Agrupar por cliente para facilitar la selección
                var ordenesAgrupadas = ordenes
                    .GroupBy(o => new { o.Identificacion, o.NombreTercero })
                    .Select(g => new
                    {
                        IdentificacionCliente = g.Key.Identificacion,
                        NombreCliente = g.Key.NombreTercero,
                        CantidadOrdenes = g.Count(),
                        TotalConsolidar = g.Sum(x => x.Total),
                        Ordenes = g.Select(o => new
                        {
                            o.guid,
                            o.Fecha,
                            o.Combustible,
                            o.Cantidad,
                            o.Total,
                            o.IdVentaLocal
                        }).ToList()
                    })
                    .OrderBy(x => x.NombreCliente)
                    .ToList();

                return Ok(ordenesAgrupadas);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error al obtener órdenes agrupadas: {ex.Message}");
            }
        }

        /// <summary>
        /// Validar si un grupo de órdenes puede ser consolidado
        /// </summary>
        [HttpPost("validar-consolidacion")]
        public async Task<ActionResult<object>> ValidarConsolidacion(
            [FromBody] List<string> guidsOrdenes,
            [FromQuery] string identificacionCliente)
        {
            try
            {
                var resultado = await _facturaConsolidadaNegocio.ValidarOrdenesParaConsolidacion(
                    guidsOrdenes, identificacionCliente);

                var esValido = !resultado.StartsWith("error:");
                return Ok(new 
                { 
                    esValido = esValido,
                    mensaje = esValido ? "Órdenes válidas para consolidación" : resultado.Substring(6)
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error en validación: {ex.Message}");
            }
        }
    }
}