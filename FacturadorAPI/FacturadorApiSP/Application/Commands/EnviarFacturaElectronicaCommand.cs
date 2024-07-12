using MediatR;

namespace FacturadorAPI.Application.Commands
{
    public class EnviarFacturaElectronicaCommand : IRequest<string>
    {

        public EnviarFacturaElectronicaCommand(int idFactura)
        {
            IdFactura = idFactura;
        }

        public EnviarFacturaElectronicaCommand(int idFactura, int terceroId, int formaPago, int ventaId, string placa, string kilometraje, string numeroTransaccion) : this(idFactura)
        {
            TerceroId = terceroId;
            FormaPago = formaPago;
            VentaId = ventaId;
            Placa = placa;
            Kilometraje = kilometraje;
            NumeroTransaccion = numeroTransaccion;
        }

        public int IdFactura { get; }
        public int TerceroId { get; }
        public int FormaPago { get; }
        public int VentaId { get; }
        public string Placa { get; }
        public string Kilometraje { get; }
        public string NumeroTransaccion { get; }
    }
}
