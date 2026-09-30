using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FacturacionelectronicaCore.Repositorio.Repositorios
{
    public interface ITipoIdentificacionRepositorio
    {
        int GetIdByTexto(string descripcionTipoIdentificacion);
        Task<IEnumerable<string>> GetTipos();
    }
}
