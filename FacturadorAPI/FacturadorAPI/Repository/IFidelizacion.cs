using Dominio.Entidades;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FacturadorEstacionesRepositorio
{
    public interface IFidelizacion
    {
        Task<IEnumerable<Fidelizado>> GetFidelizados();
        Task<bool> SubirPuntops(float total, string documentoFidelizado, string factura);
    }
}
