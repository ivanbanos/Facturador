using MediatR;

namespace FacturadorAPI.Application.Commands
{
    public class FidelizarVentaCommand : IRequest
    {
        public FidelizarVentaCommand(string identificacion, int ventaId,
            int facturaPOSId, int terceroId, int formaPago,
            string placa, string kilometraje, string numeroTransaccion,
            int? formaPago2 = null, double? total1 = null, double? total2 = null)
        {
            Identificacion = identificacion;
            VentaId = ventaId;
            FacturaPOSId = facturaPOSId;
            TerceroId = terceroId;
            FormaPago = formaPago;
            Placa = placa;
            Kilometraje = kilometraje;
            NumeroTransaccion = numeroTransaccion;
            FormaPago2 = formaPago2;
            Total1 = total1;
            Total2 = total2;
        }

        public string Identificacion { get; }
        public int VentaId { get; }
        public int FacturaPOSId { get; }
        public int TerceroId { get; }
        public int FormaPago { get; }
        public string Placa { get; }
        public string Kilometraje { get; }
        public string NumeroTransaccion { get; }
        public int? FormaPago2 { get; }
        public double? Total1 { get; }
        public double? Total2 { get; }
    }
}
