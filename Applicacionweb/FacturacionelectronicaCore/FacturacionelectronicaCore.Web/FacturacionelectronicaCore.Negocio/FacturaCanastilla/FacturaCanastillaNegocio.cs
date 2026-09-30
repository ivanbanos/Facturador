using EstacionesServicio.Negocio.Extention;
using FacturacionelectronicaCore.Negocio.Modelo;
using FacturacionelectronicaCore.Repositorio.Entities;
using FacturacionelectronicaCore.Repositorio.Repositorios;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FacturaCanastilla = FacturacionelectronicaCore.Repositorio.Entities.FacturaCanastilla;
using Microsoft.Extensions.Options;
using FacturacionelectronicaCore.Negocio.Contabilidad.FacturacionElectronica;

namespace FacturacionelectronicaCore.Negocio.FacturaCanastillaNegocio
{
    public class FacturaCanastillaNegocio : IFacturaCanastillaNegocio
    {
        private readonly IFacturaCanastillaRepository _facturaCanastillaRepository;
        private readonly IValidadorGuidAFacturaElectronica _validadorGuidAFacturaElectronica;
        private readonly Alegra _alegra;

        public FacturaCanastillaNegocio(IFacturaCanastillaRepository facturaCanastillaRepository, IValidadorGuidAFacturaElectronica validadorGuidAFacturaElectronica, IOptions<Alegra> alegra)
        {
            _facturaCanastillaRepository = facturaCanastillaRepository;
            _validadorGuidAFacturaElectronica = validadorGuidAFacturaElectronica;
            _alegra = alegra.Value;
        }

        // Convert a nullable incoming DateTime by adding the configured hour offset from _alegra.ServerTimeOffsetHours.
        // Behavior:
        //  - If ServerTimeOffsetHours is null, returns the original value (no offset).
        //  - Adds the configured number of hours (can be negative) to the provided DateTime.
        private DateTime? ConvertToServerTime(DateTime? input)
        {
            if (!input.HasValue) return null;
            if (_alegra == null || !_alegra.ServerTimeOffsetHours.HasValue) return input;

            try
            {
                var offset = _alegra.ServerTimeOffsetHours.GetValueOrDefault(0);
                return input.Value.AddHours(offset);
            }
            catch
            {
                // If anything goes wrong, fall back to original value
                return input;
            }
        }
        public async Task<IEnumerable<FacturaCanastilla>> GetDetalleFactura(string idFactura)
        {
            return null;// await _facturaCanastillaRepository.GetDetalleFactura(idFactura);
        }

        public async Task<FacturaCanastilla> GetFactura(string idFactura)
        {
            return (await _facturaCanastillaRepository.GetFactura(idFactura))?.FirstOrDefault();
        }

        public async Task<IEnumerable<FacturaCanastilla>> GetFacturas(DateTime? fechaInicial, DateTime? fechaFinal, string identificacionTercero, string nombreTercero, Guid estacion)
        {
            // Normalize incoming search dates to the configured server timezone before hitting the repository.
            var fechaInicialServer = ConvertToServerTime(fechaInicial);
            var fechaFinalServer = ConvertToServerTime(fechaFinal);
            
            return await _facturaCanastillaRepository.GetFacturas(fechaInicialServer, fechaFinalServer, identificacionTercero, nombreTercero, estacion);
        }


        public void ColocarEspera(string guid, Guid idEstacion)
        {
            _validadorGuidAFacturaElectronica.AgregarAColaImpresionCanastilla(guid, idEstacion);
        }
        public async Task<int> ObtenerParaImprimir(Guid idEstacion)
        {
            var guid = _validadorGuidAFacturaElectronica.ObtenerColaImpresionCanastilla(idEstacion);
            if (!string.IsNullOrEmpty(guid))
            {
                var factura = await GetFactura(guid);
               
                return factura.consecutivo;
            }
            return 0;
        }

        public async Task<FacturaCanastillaReporte> GetFacturasReporte(DateTime? fechaInicial, DateTime? fechaFinal, string identificacion, string nombreTercero, Guid estacion)
        {
            // Normalize incoming search dates to the configured server timezone before hitting the repository.
            var fechaInicialServer = ConvertToServerTime(fechaInicial);
            var fechaFinalServer = ConvertToServerTime(fechaFinal);
            
            var facturas = await _facturaCanastillaRepository.GetFacturas(fechaInicialServer, fechaFinalServer, identificacion, nombreTercero, estacion);
            var detalleArticulos = new List<DetalleArticulo>();
            var cantidad = 0;
            var subtotal = 0d;
            var iva = 0d;
            var total = 0d;
            foreach (var factura in facturas)
            {
                foreach (var detalle in factura.canastillas)
                {

                    if (!detalleArticulos.Any(x => x.Descripcion == detalle.Canastilla.descripcion))
                    {
                        detalleArticulos.Add(new DetalleArticulo() { Descripcion = detalle.Canastilla.descripcion, Cantidad=0, Subtotal = 0, Iva = 0, Total = 0 });
                    }
                    detalleArticulos.First(x => x.Descripcion == detalle.Canastilla.descripcion).Cantidad += 1;
                    detalleArticulos.First(x => x.Descripcion == detalle.Canastilla.descripcion).Subtotal += detalle.subtotal;
                    detalleArticulos.First(x => x.Descripcion == detalle.Canastilla.descripcion).Iva += detalle.iva;
                    detalleArticulos.First(x => x.Descripcion == detalle.Canastilla.descripcion).Total += detalle.total;
                    cantidad += 1;
                    subtotal += detalle.subtotal;
                    iva += detalle.iva;
                    total += detalle.total;
                }
            }
            detalleArticulos.Add(new DetalleArticulo() { Descripcion = "Total", Cantidad = cantidad, Subtotal = subtotal, Iva = iva, Total = total });

            var facturaCanastillaReporte = new FacturaCanastillaReporte();
            facturaCanastillaReporte.Facturas = facturas;
            facturaCanastillaReporte.DetalleArticulo = detalleArticulos;
            List<DetalleFormaPago> formas = facturas.GroupBy(x => x.codigoFormaPago.Descripcion)
                .Select(x => new DetalleFormaPago()
                {
                    FormaDePago = x.Key,
                    Cantidad = x.Count(),
                    Precio = x.Sum(x => x.total),
                }).ToList();
            formas.Add(new DetalleFormaPago() { FormaDePago="Total", Cantidad = cantidad , Precio = total });
            facturaCanastillaReporte.DetalleFormaPago = formas;
            return facturaCanastillaReporte;
        }

    }
}
