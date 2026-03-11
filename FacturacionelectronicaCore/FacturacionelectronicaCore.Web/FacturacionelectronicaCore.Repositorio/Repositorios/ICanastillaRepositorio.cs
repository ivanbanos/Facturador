using FacturacionelectronicaCore.Repositorio.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FacturacionelectronicaCore.Repositorio.Repositorios
{
    public interface ICanastillaRepositorio
    {
        Task AddRange(IEnumerable<Canastilla> lists);
        Task<IEnumerable<Canastilla>> GetCanastillas(Guid? estacion = null);
    }
}
