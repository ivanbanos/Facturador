using FactoradorEstacionesModelo.Objetos;
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

            var factura = await _databaseHandler.getUltimasFacturas((short)request.IdCara);

            return getLineasImprimir(factura);
        }

        private string getLineasImprimir(Factura _factura)
        {
            var _venta = _factura.Venta;
            var _tercero = _factura.Tercero;
            var _mangueras = _factura.Manguera;
            var informacionVenta = new StringBuilder();
            informacionVenta.Append("------------------------------------------------" + "\n\r");
            if (_factura.Consecutivo == 0)
            {

                informacionVenta.Append("Orden de despacho No:" + _venta.CONSECUTIVO + "\n\r");
            }
            else
            {
                informacionVenta.Append("Factura de venta P.O.S No: " + _factura.DescripcionResolucion + "-" + _factura.Consecutivo + "\n\r");
            }
            informacionVenta.Append("------------------------------------------------" + "\n\r");
            if (_venta.COD_FOR_PAG != 4)
            {
                informacionVenta.Append("Vendido a : " + _tercero.Nombre + "\n\r");
                informacionVenta.Append("Nit/C.C. : " + _tercero.identificacion + "\n\r");
                informacionVenta.Append("Placa : PLACA\n\r");
                informacionVenta.Append("Kilometraje : KILOMETRAJE\n\r");
                informacionVenta.Append("Cod Int : " + _venta.COD_INT + "\n\r");
            }
            else
            {
                informacionVenta.Append("Vendido a : CONSUMIDOR FINAL\n\r");
                informacionVenta.Append("Nit/C.C. : 222222222222\n\r");
                informacionVenta.Append("Placa : PLACA\n\r");
                informacionVenta.Append("Kilometraje : KILOMETRAJE\n\r");
            }
            if (_venta.FECH_ULT_ACTU.HasValue && _mangueras.DESCRIPCION.ToLower().Contains("gn"))
            {
                informacionVenta.Append("Proximo mantenimiento : " + _venta.FECH_ULT_ACTU.Value.ToString("dd/MM/yyyy") + "\n\r");
            }
            informacionVenta.Append("------------------------------------------------" + "\n\r");
            informacionVenta.Append("Fecha : " + _factura.fecha.ToString("dd/MM/yyyy HH:mm:ss") + "\n\r");
            informacionVenta.Append("Surtidor : " + _venta.COD_SUR + "\n\r");
            informacionVenta.Append("Cara : " + _venta.COD_CAR + "\n\r");
            informacionVenta.Append("Manguera : " + _mangueras.COD_MAN + "\n\r");
            informacionVenta.Append("Vendedor : " + _venta.EMPLEADO + "\n\r");
            informacionVenta.Append("------------------------------------------------" + "\n\r");

            informacionVenta.Append("Producto  Cant.     Precio    Total    " + "\n\r");
            informacionVenta.Append(getLienaTarifas(_mangueras.DESCRIPCION.Trim(), String.Format("{0:#,0.000}", _venta.CANTIDAD), _venta.PRECIO_UNI.ToString("F"), String.Format("{0:#,0.00}", _venta.VALORNETO)) + "\n\r");
            //informacionVenta.Append("------------------------------------------------" + "\n\r");
            informacionVenta.Append("DISCRIMINACION TARIFAS IVA" + "\n\r");
            // informacionVenta.Append("------------------------------------------------" + "\n\r");

            informacionVenta.Append("Producto  Cant.     Tafira    Total    " + "\n\r");
            informacionVenta.Append(getLienaTarifas(_mangueras.DESCRIPCION.Trim(), String.Format("{0:#,0.000}", _venta.CANTIDAD), "0%", String.Format("{0:#,0.00}", _venta.VALORNETO)) + "\n\r");
            informacionVenta.Append("------------------------------------------------" + "\n\r");
            informacionVenta.Append("Subtotal sin IVA : " + String.Format("{0:#,0.00}", _venta.TOTAL) + "\n\r");
            //informacionVenta.Append("------------------------------------------------" + "\n\r");
            informacionVenta.Append("Descuento : " + String.Format("{0:#,0.00}", _venta.Descuento) + "\n\r");
            //informacionVenta.Append("------------------------------------------------" + "\n\r");
            informacionVenta.Append("Subtotal IVA : 0,00 \n\r");
            // informacionVenta.Append("------------------------------------------------" + "\n\r");
            informacionVenta.Append("TOTAL : " + String.Format("{0:#,0.00}", _venta.TOTAL) + "\n\r");
            //informacionVenta.Append("------------------------------------------------" + "\n\r");
            if (_factura.Consecutivo != 0)
            {
                informacionVenta.Append("Forma de pago : FORMA DE PAGO\n\r");
                informacionVenta.Append("------------------------------------------------" + "\n\r");

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
