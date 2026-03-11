using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FacturacionelectronicaCore.Negocio.Resolucion
{
    public interface IResolucionNegocio
    {
        Task<Modelo.Resolucion> HabilitarResolucion(Guid resolucion, DateTime fechaVencimiento);
        Task<Modelo.CreacionResolucion> AddNuevaResolucion(Modelo.CreacionResolucion resolucion);
        Task<IEnumerable<Modelo.Resolucion>> GetResolucionActiva(Guid estacion);
        Task AnularResolucion(Guid resolucion);
    }
}
