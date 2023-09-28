using FacturadorAPI.Models;
using MachineUtilizationApi.Repository;
using MediatR;
using Microsoft.Extensions.Options;
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
            informacionVenta.Append("------------------------------------------------" + "\n\r");
            if (factura.Consecutivo == 0)
            {

                informacionVenta.Append("Orden de despacho No:" + factura.Consecutivo + "\n\r");
            }
            else
            {
                informacionVenta.Append("Factura de venta P.O.S No: " + factura.DescripcionResolucion + "-" + factura.Consecutivo + "\n\r");
            }
            informacionVenta.Append("------------------------------------------------" + "\n\r");
            if (factura.codigoFormaPago != 1)
            {
                informacionVenta.Append("Vendido a : " + factura.Tercero.Nombre + "\n\r");
                informacionVenta.Append("Nit/C.C. : " + factura.Tercero.identificacion + "\n\r");
                informacionVenta.Append("Placa : PLACA\n\r");
                informacionVenta.Append("Kilometraje : KILOMETRAJE\n\r");
                informacionVenta.Append("Cod Int : " + factura.CodigoInterno + "\n\r");
            }
            else
            {
                informacionVenta.Append("Vendido a : CONSUMIDOR FINAL\n\r");
                informacionVenta.Append("Nit/C.C. : 222222222222\n\r");
                informacionVenta.Append("Placa : PLACA\n\r");
                informacionVenta.Append("Kilometraje : KILOMETRAJE\n\r");
            }
            if (factura.fechaUltimaActualizacion.HasValue && factura.Manguera.Descripcion.ToLower().Contains("gn"))
            {
                informacionVenta.Append("Proximo mantenimiento : " + factura.fechaUltimaActualizacion.Value.ToString("dd/MM/yyyy") + "\n\r");
            }
            informacionVenta.Append("------------------------------------------------" + "\n\r");
            informacionVenta.Append("Fecha : " + factura.fecha.ToString("dd/MM/yyyy HH:mm:ss") + "\n\r");
            informacionVenta.Append("Surtidor : " + factura.Surtidor + "\n\r");
            informacionVenta.Append("Cara : " + factura.Cara + "\n\r");
            informacionVenta.Append("Manguera : " + factura.Mangueras + "\n\r");
            informacionVenta.Append("Vendedor : " + factura.Empleado + "\n\r");
            informacionVenta.Append("------------------------------------------------" + "\n\r");

            informacionVenta.Append("Producto  Cant.     Precio    Total    " + "\n\r");
            informacionVenta.Append(getLienaTarifas(factura.Combustible, String.Format("{0:#,0.000}", factura.Cantidad), factura.Precio.ToString("F"), String.Format("{0:#,0.00}", factura.Total)) + "\n\r");
            //informacionVenta.Append("------------------------------------------------" + "\n\r");
            informacionVenta.Append("DISCRIMINACION TARIFAS IVA" + "\n\r");
            // informacionVenta.Append("------------------------------------------------" + "\n\r");

            informacionVenta.Append("Producto  Cant.     Tafira    Total    " + "\n\r");
            informacionVenta.Append(getLienaTarifas(factura.Combustible, String.Format("{0:#,0.000}", factura.Cantidad), "0%", String.Format("{0:#,0.00}", factura.Total)) + "\n\r");
            informacionVenta.Append("------------------------------------------------" + "\n\r");
            informacionVenta.Append("Subtotal sin IVA : " + String.Format("{0:#,0.00}", factura.Total) + "\n\r");
            //informacionVenta.Append("------------------------------------------------" + "\n\r");
            informacionVenta.Append("Descuento : " + String.Format("{0:#,0.00}", factura.Descuento) + "\n\r");
            //informacionVenta.Append("------------------------------------------------" + "\n\r");
            informacionVenta.Append("Subtotal IVA : 0,00 \n\r");
            // informacionVenta.Append("------------------------------------------------" + "\n\r");
            informacionVenta.Append("TOTAL : " + String.Format("{0:#,0.00}", factura.Total) + "\n\r");
            //informacionVenta.Append("------------------------------------------------" + "\n\r");
            if (factura.Consecutivo != 0)
            {
                informacionVenta.Append("Forma de pago : FORMA DE PAGO\n\r");
                informacionVenta.Append("------------------------------------------------" + "\n\r");

            }

            return informacionVenta.ToString();
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
