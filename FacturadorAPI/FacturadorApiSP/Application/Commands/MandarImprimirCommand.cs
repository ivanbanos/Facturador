using FacturadorAPI.Models;
using MediatR;

namespace FacturadorAPI.Application.Commands
{
    public class MandarImprimirCommand : IRequest<string>
    {

        public MandarImprimirCommand(int facturaPOSId, int terceroId, int formaPago, int ventaId, string placa, string kilometraje, string numeroTransaccion, int impresiones, int? formaPago2 = null, double? total1 = null, double? total2 = null)
        {
            FacturaPOSId = facturaPOSId;
            TerceroId = terceroId;
            FormaPago = formaPago;
            VentaId = ventaId;
            Placa = placa;
            Kilometraje = kilometraje;
            NumeroTransaccion = numeroTransaccion;
            Impresiones = impresiones;
            FormaPago2 = formaPago2;
            Total1 = total1;
            Total2 = total2;
        }

        public int FacturaPOSId { get; }
        public int TerceroId { get; }
        public int FormaPago { get; }
        public int VentaId { get; }
        public string Placa { get; }
        public string Kilometraje { get; }
        public string NumeroTransaccion { get; }
        public int Impresiones { get; }
        public int? FormaPago2 { get; }
        public double? Total1 { get; }
        public double? Total2 { get; }
    }
}
