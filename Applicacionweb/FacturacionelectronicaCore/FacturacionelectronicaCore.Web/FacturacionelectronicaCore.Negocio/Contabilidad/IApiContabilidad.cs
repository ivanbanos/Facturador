using FacturacionelectronicaCore.Negocio.Contabilidad.FacturacionElectronica;
using System;
using System.Collections.Generic;
using System.Text;

namespace FacturacionelectronicaCore.Negocio.Contabilidad
{
    public interface IApiContabilidad
    {
        void EnviarFacturas(IEnumerable<FacturaSilog> facturas);
    }
}
