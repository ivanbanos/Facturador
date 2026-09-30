using FacturacionelectronicaCore.Repositorio.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FacturacionelectronicaCore.Repositorio.Repositorios
{
    public interface IFacturaCanastillaRepository
    {
        Task<int> Add(FacturaCanastilla factura, IEnumerable<CanastillaFactura> detalleFactura, Guid estacion);
        Task<IEnumerable<FacturaCanastilla>> GetFacturas(DateTime? fechaInicial, DateTime? fechaFinal, string identificacionTercero, string nombreTercero, Guid estacion);

        Task<IEnumerable<FacturaCanastilla>> GetFactura(string idFactura);

        Task<IEnumerable<FacturaCanastillaDetalleResponse>> GetDetalleFactura(string idFactura);

        Task SetIdFacturaElectronicaFactura(string idFacturaElectronica, string guid);
Task<bool> FacturaGenerada(int facturasCanastillaId, Guid estacion);
        Task<FacturaCanastilla> GetFacturaPorIdCanastilla(int consecutivo, Guid estacion);
    }
}
