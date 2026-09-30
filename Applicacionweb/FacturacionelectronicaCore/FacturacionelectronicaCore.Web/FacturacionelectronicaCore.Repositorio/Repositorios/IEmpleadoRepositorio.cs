using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FacturacionelectronicaCore.Repositorio.Repositorios
{
    public interface IEmpleadoRepositorio
    {
        Task<string> GetEmpleadoByName(string vendedor);
    }
}
