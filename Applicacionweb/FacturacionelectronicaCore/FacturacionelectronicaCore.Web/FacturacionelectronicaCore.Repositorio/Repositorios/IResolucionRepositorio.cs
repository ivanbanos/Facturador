using FacturacionelectronicaCore.Repositorio.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FacturacionelectronicaCore.Repositorio.Repositorios
{
    public interface IResolucionRepositorio
    {
        Task UpdateConsecutivoResolucion(int consecutivo);
        Task<CreacionResolucion> AddNuevaResolucion(CreacionResolucion resolucionEntity);
        Task<IEnumerable<Resolucion>> GetResolucionActiva(Guid estacion);
        Task<Resolucion> HabilitarResolucion(Guid estacion, DateTime fechaVencimiento);
        Task AnularResolucion(Guid resolucion);
        Task<ResolucionFacturaElectronica> GetFacturaelectronicaPorPRefijo(string prefijo);
        Task SetFacturaelectronicaPorPRefijo(string prefijo, int numeroActual);
    }
}
