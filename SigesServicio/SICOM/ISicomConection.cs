using FactoradorEstacionesModelo.Siges;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ManejadorSurtidor.SICOM
{
    public interface ISicomConection
    {
        Task<VehiculoSuic> validateIButton(string iButton);
        Task<bool> enviarVenta(string iButton, float consumo, DateTime fecha);
        Task<string> GetInfoCarros();
    }
}
