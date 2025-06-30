using FacturadorAPI.Models;
using MediatR;

namespace FacturadorAPI.Application.Commands
{
    public class MandarImprimirCommand : IRequest<string>
    {

        public MandarImprimirCommand(int facturaPOSId, int terceroId, int formaPago, int ventaId, string placa, string kilometraje)
        {
            FacturaPOSId = facturaPOSId;
            TerceroId = terceroId;
            FormaPago = formaPago;
            VentaId = ventaId;
            Placa = placa;
            Kilometraje = kilometraje; 
        }

        public MandarImprimirCommand(int facturaPOSId, int terceroId, int formaPago, int ventaId, string placa, string kilometraje, string numeroTransaccion, int impresiones) : this(facturaPOSId, terceroId, formaPago, ventaId, placa, kilometraje)
        {
            NumeroTransaccion = numeroTransaccion;
            Impresiones = impresiones;
        }

        public int FacturaPOSId { get; }
        public int TerceroId { get; }
        public int FormaPago { get; }
        public int VentaId { get; }
        public string Placa { get; }
        public string Kilometraje { get; }
        public string NumeroTransaccion { get; }
        public int Impresiones { get; }
    }
}
