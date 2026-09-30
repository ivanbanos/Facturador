using FacturacionelectronicaCore.Negocio.Modelo;
using FacturacionelectronicaCore.Repositorio.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FacturaCanastilla = FacturacionelectronicaCore.Repositorio.Entities.FacturaCanastilla;

namespace FacturacionelectronicaCore.Negocio.FacturaCanastillaNegocio
{
    public interface IFacturaCanastillaNegocio
    {
        Task<IEnumerable<FacturaCanastilla>> GetFacturas(DateTime? fechaInicial, DateTime? fechaFinal, string identificacionTercero, string nombreTercero, Guid estacion);

        Task<FacturaCanastilla> GetFactura(string idFactura);

        Task<IEnumerable<FacturaCanastilla>> GetDetalleFactura(string idFactura);
        void ColocarEspera(string guid, Guid idEstacion);
        Task<int> ObtenerParaImprimir(Guid idEstacion);
        Task<FacturaCanastillaReporte> GetFacturasReporte(DateTime? fechaInicial, DateTime? fechaFinal, string identificacion, string nombreTercero, Guid estacion);
    }
}
