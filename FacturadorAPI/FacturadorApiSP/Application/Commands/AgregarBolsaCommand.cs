using MediatR;

namespace FacturadorApiSP.Application.Commands
{
    public class AgregarBolsaCommand : IRequest
    {
        public AgregarBolsaCommand(int isla, string codigo, string cantidad)
        {
            Isla = isla;
            Codigo = codigo;
            Cantidad = cantidad;
        }

        public AgregarBolsaCommand(int isla, string codigo, string cantidad, string moneda, string numero) : this(isla, codigo, cantidad)
        {
            Numero = numero;
            Moneda = moneda;
        }

        public int Isla { get; }
        public string Codigo { get; }
        public string Cantidad { get; }
        public string Moneda { get; }
        public string Numero { get; }
    }
}
