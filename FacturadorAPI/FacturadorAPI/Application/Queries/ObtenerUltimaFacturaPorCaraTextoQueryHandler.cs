using FacturadorAPI.Models;
using MachineUtilizationApi.Repository;
using MediatR;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.SqlServer.Server;
using System.Text;

namespace FacturadorAPI.Application.Queries
{
    public class ObtenerUltimaFacturaPorCaraTextoQueryHandler : IRequestHandler<ObtenerUltimaFacturaPorCaraTextoQuery, string>
    {
        private readonly ILogger<ObtenerUltimaFacturaPorCaraTextoQueryHandler> _logger;
        private readonly IDataBaseHandler _databaseHandler;
        private readonly InfoEstacion _infoEstacion;

        public ObtenerUltimaFacturaPorCaraTextoQueryHandler(ILogger<ObtenerUltimaFacturaPorCaraTextoQueryHandler> logger, 
            IDataBaseHandler databaseHandler,
            IOptions<InfoEstacion> options)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _databaseHandler = databaseHandler ?? throw new ArgumentNullException(nameof(databaseHandler));
            _infoEstacion = options.Value;
        }

        public async Task<string> Handle(ObtenerUltimaFacturaPorCaraTextoQuery request, CancellationToken cancellationToken)
        {

            var factura = await _databaseHandler.ObtenerUltimaFacturaPorCara(request.IdCara, cancellationToken);

            return getLineasImprimir(factura);
        }

        private string getLineasImprimir(FacturaSiges factura)
        {

            var informacionVenta = new StringBuilder();
            informacionVenta.Append("------------------------------------------------" + "\n");
            if (factura.Consecutivo == 0)
            {

                informacionVenta.Append("Orden de despacho No:" + factura.Consecutivo + "\n");
            }
            else
            {
                informacionVenta.Append("Factura de venta P.O.S No: " + factura.DescripcionResolucion + "-" + factura.Consecutivo + "\n");
            }
            informacionVenta.Append("------------------------------------------------" + "\n");
            if (factura.codigoFormaPago != 1)
            {
                informacionVenta.Append("Vendido a : " + factura.Tercero.Nombre + "\n");
                informacionVenta.Append("Nit/C.C. : " + factura.Tercero.identificacion + "\n");
                informacionVenta.Append("Placa : PLACA\n");
                informacionVenta.Append("Kilometraje : KILOMETRAJE\n");
                informacionVenta.Append("Cod Int : " + factura.CodigoInterno + "\n");
            }
            else
            {
                informacionVenta.Append("Vendido a : CONSUMIDOR FINAL\n");
                informacionVenta.Append("Nit/C.C. : 222222222222\n");
                informacionVenta.Append("Placa : PLACA\n");
                informacionVenta.Append("Kilometraje : KILOMETRAJE\n");
            }
            if (factura.fechaUltimaActualizacion.HasValue && factura.Manguera.Descripcion.ToLower().Contains("gn"))
            {
                informacionVenta.Append("Proximo mantenimiento : " + factura.fechaUltimaActualizacion.Value.ToString("dd/MM/yyyy") + "\n");
            }
            informacionVenta.Append(getPuntos(factura.ventaId) + "\n");
            informacionVenta.Append("------------------------------------------------" + "\n");
            informacionVenta.Append("Fecha : " + factura.fecha.ToString("dd/MM/yyyy HH:mm:ss") + "\n");
            informacionVenta.Append("Surtidor : " + factura.Surtidor + "\n");
            informacionVenta.Append("Cara : " + factura.Cara + "\n");
            informacionVenta.Append("Manguera : " + factura.Mangueras + "\n");
            informacionVenta.Append("Vendedor : " + factura.Empleado + "\n");
            informacionVenta.Append("------------------------------------------------" + "\n");

            informacionVenta.Append("Producto  Cant.     Precio    Total    " + "\n");
            informacionVenta.Append(getLienaTarifas(factura.Combustible, String.Format("{0:#,0.000}", factura.Cantidad), factura.Precio.ToString("F"), String.Format("{0:#,0.00}", factura.Total)) + "\n");
            //informacionVenta.Append("------------------------------------------------" + "\n");
            informacionVenta.Append("DISCRIMINACION TARIFAS IVA" + "\n");
            // informacionVenta.Append("------------------------------------------------" + "\n");

            informacionVenta.Append("Producto  Cant.     Tafira    Total    " + "\n");
            informacionVenta.Append(getLienaTarifas(factura.Combustible, String.Format("{0:#,0.000}", factura.Cantidad), "0%", String.Format("{0:#,0.00}", factura.Total)) + "\n");
            informacionVenta.Append("------------------------------------------------" + "\n");
            informacionVenta.Append("Subtotal sin IVA : " + String.Format("{0:#,0.00}", factura.Total) + "\n");
            //informacionVenta.Append("------------------------------------------------" + "\n");
            informacionVenta.Append("Descuento : " + String.Format("{0:#,0.00}", factura.Descuento) + "\n");
            //informacionVenta.Append("------------------------------------------------" + "\n");
            informacionVenta.Append("Subtotal IVA : 0,00 \n");
            // informacionVenta.Append("------------------------------------------------" + "\n");
            informacionVenta.Append("TOTAL : " + String.Format("{0:#,0.00}", factura.Total) + "\n");
            //informacionVenta.Append("------------------------------------------------" + "\n");
            if (factura.Consecutivo != 0)
            {
                informacionVenta.Append("Forma de pago : FORMA DE PAGO\n");
                informacionVenta.Append("------------------------------------------------" + "\n");

            }

            return  informacionVenta.ToString();
        }
        private async Task<string> getPuntos(int ventaId)
        {
            var fidelizado = await  _databaseHandler.GetFidelizado(ventaId);
            if (fidelizado != null)
            {
                return "Puntos: " + fidelizado.Puntos;
            }
            else
            {
                return "Usuario no fidelizado";
            }
        }
        private string getLienaTarifas(string v1, string v2, string v3, string v4)
        {
            var spacesInPage = _infoEstacion.CaracteresPorPagina / 4;
            var tabs = new StringBuilder();
            tabs.Append(v1.Substring(0, v1.Length < spacesInPage ? v1.Length : spacesInPage));
            var whitespaces = spacesInPage - v1.Length;
            whitespaces = whitespaces < 0 ? 0 : whitespaces;
            tabs.Append(' ', whitespaces);

            tabs.Append(v2.Substring(0, v2.Length < spacesInPage ? v2.Length : spacesInPage));
            whitespaces = spacesInPage - v2.Length;
            whitespaces = whitespaces < 0 ? 0 : whitespaces;
            tabs.Append(' ', whitespaces);

            tabs.Append(v3.Substring(0, v3.Length < spacesInPage ? v3.Length : spacesInPage));
            whitespaces = spacesInPage - v3.Length;
            whitespaces = whitespaces < 0 ? 0 : whitespaces;
            tabs.Append(' ', whitespaces);

            tabs.Append(v4.Substring(0, v4.Length < spacesInPage ? v4.Length : spacesInPage));
            whitespaces = spacesInPage - v4.Length;
            whitespaces = whitespaces < 0 ? 0 : whitespaces;
            tabs.Append(' ', whitespaces);
            return tabs.ToString();
        }

    }
}
