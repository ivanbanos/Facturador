using EstacionesServicio.Modelo;
using System;
using System.Collections.Generic;

namespace EstacionesServicio.Negocio.Extention
{
    public interface IValidadorGuidAFacturaElectronica
    {
        bool FacturaSiendoProceada(string ordenGuid);
        void SacarFactura(string ordenGuid);
        bool FacturasSiendoProceada(IEnumerable<string> facturasGuids);
        void SacarFacturas(IEnumerable<string> facturasGuids);
        string? ObtenerColaImpresionCanastilla(Guid idEstacion);
        void AgregarAColaImpresionCanastilla(string guid, Guid idEstacion);
    }
}