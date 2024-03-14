using FactoradorEstacionesModelo.Objetos;
using FacturadorAPI.Models;
using FacturadorAPI.Repository.Repo;
using MachineUtilizationApi.Extensions;
using MachineUtilizationApi.Repository;
using MediatR;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.SqlServer.Server;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;

namespace FacturadorAPI.Application.Queries
{
    public class ObtenerUltimaFacturaPorCaraTextoQueryHandler : IRequestHandler<ObtenerUltimaFacturaPorCaraTextoQuery, string>
    {
        private readonly ILogger<ObtenerUltimaFacturaPorCaraTextoQueryHandler> _logger;
        private readonly IDataBaseHandler _databaseHandler;
        private readonly InfoEstacion _infoEstacion;
        private readonly IConexionEstacionRemota _conexionEstacionRemota;

        public ObtenerUltimaFacturaPorCaraTextoQueryHandler(ILogger<ObtenerUltimaFacturaPorCaraTextoQueryHandler> logger,
            IDataBaseHandler databaseHandler,
            IOptions<InfoEstacion> options,
            IConexionEstacionRemota conexionEstacionRemota)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _databaseHandler = databaseHandler ?? throw new ArgumentNullException(nameof(databaseHandler));
            _infoEstacion = options.Value;
            _conexionEstacionRemota = conexionEstacionRemota;
        }

        public async Task<string> Handle(ObtenerUltimaFacturaPorCaraTextoQuery request, CancellationToken cancellationToken)
        {

            var factura = await _databaseHandler.getUltimasFacturas((short)request.IdCara);

            return await getinformacionVenta(factura, cancellationToken);
        }

        private async Task<string> getinformacionVenta(Factura _factura, CancellationToken cancellationToken)
        {
            var _venta = _factura.Venta;
            var _tercero = _factura.Tercero;
            var _mangueras = _factura.Manguera;
            var formas = await _databaseHandler.ListarFormasPagoSP(cancellationToken);
            var informacionVenta = new StringBuilder();
            var guiones = new StringBuilder();
            guiones.Append('-', 40);
            informacionVenta.Append("." + "\n\r");
            informacionVenta.Append(_infoEstacion.Razon.Centrar() + "\n\r");
            informacionVenta.Append(("NIT " + _infoEstacion.NIT).Centrar() + "\n\r");
            informacionVenta.Append(_infoEstacion.Nombre.Centrar() + "\n\r");
            informacionVenta.Append(_infoEstacion.Direccion.Centrar() + "\n\r");
            informacionVenta.Append(_infoEstacion.Telefono.Centrar() + "\n\r");
            informacionVenta.Append(guiones.ToString() + "\n\r");
            var infoTemp = "";
                try
            {
                var token = await _conexionEstacionRemota.GetToken(cancellationToken);
                infoTemp = await _conexionEstacionRemota.GetInfoFacturaElectronica(_factura.ventaId, Guid.Parse(_infoEstacion.EstacionFuente), token);

                }
                catch (Exception)
                {
                    infoTemp = null;
                }
            if (!string.IsNullOrEmpty(infoTemp))
            {
                infoTemp = infoTemp.Replace("\n\r", " ");

                var facturaElectronica = infoTemp.Split(' ');

                informacionVenta.Append(("Factura Electrónica " + facturaElectronica[2]).Centrar() + "\n\r");
                informacionVenta.Append(facturaElectronica[3].Centrar() + "\n\r");
                informacionVenta.Append(facturaElectronica[4].Substring(0, facturaElectronica[4].Length / 2).Centrar() + "\n\r");
                informacionVenta.Append(facturaElectronica[4].Substring(facturaElectronica[4].Length / 2).Centrar() + "\n\r");
            }
            else if (_factura.Consecutivo == 0)
            {

                informacionVenta.Append(("Orden de despacho No: " + _venta.CONSECUTIVO).Centrar() + "\n\r");
            }
            else
            {
                informacionVenta.Append(("SISTEMA POS No: " + _factura.DescripcionResolucion + "-" + _factura.Consecutivo).Centrar() + "\n\r");
            }

            informacionVenta.Append(guiones.ToString() + "\n\r");
            var placa = (!string.IsNullOrEmpty(_factura.Placa) ? _factura.Placa : _venta.PLACA + "").Trim();
            if (_venta.COD_FOR_PAG != 4)
            {

                informacionVenta.Append(formatoTotales("Vendido a : ", _tercero.Nombre == null ? "" : _tercero.Nombre.Trim()) + "\n\r");
                informacionVenta.Append(formatoTotales("Nit/C.C. : ", _tercero.identificacion.Trim()) + "\n\r");
                informacionVenta.Append(formatoTotales("Placa : ", placa) + "\n\r");
                informacionVenta.Append(formatoTotales("Kilometraje : ", (!string.IsNullOrEmpty(_factura.Kilometraje) ? _factura.Kilometraje : _venta.KILOMETRAJE + "").Trim()) + "\n\r");
                var codigoInterno = _factura.Venta.COD_INT != null ? _factura.Venta.COD_INT : "";
                if (codigoInterno != null)
                {
                    informacionVenta.Append(formatoTotales("Cod Int : ", codigoInterno) + "\n\r");
                }
            }
            else
            {
                if (string.IsNullOrEmpty(_tercero.Nombre))
                {
                    informacionVenta.Append(formatoTotales("Vendido a :", " CONSUMIDOR FINAL".Trim()) + "\n\r");
                }
                else
                {
                    informacionVenta.Append(formatoTotales("Vendido a : ", _tercero.Nombre.Trim()) + "\n\r");
                }
                if (string.IsNullOrEmpty(_tercero.identificacion))
                {
                    informacionVenta.Append(formatoTotales("Nit/C.C. : ", "222222222222".Trim() ) + "\n\r");
                }
                else
                {
                    informacionVenta.Append(formatoTotales("Nit/C.C. : ", _tercero.identificacion.Trim() ) + "\n\r");
                }
                informacionVenta.Append(formatoTotales("Placa : ", (!string.IsNullOrEmpty(_factura.Placa) ? _factura.Placa : _venta.PLACA + "").Trim() + "\n\r"));
                informacionVenta.Append(formatoTotales("Kilometraje : ", (!string.IsNullOrEmpty(_factura.Kilometraje) ? _factura.Kilometraje : _venta.KILOMETRAJE + "").Trim() ) + "\n\r");
                var codigoInterno = _factura.Venta.COD_INT != null ? _factura.Venta.COD_INT : "";
                if (codigoInterno != null)
                {
                    informacionVenta.Append(formatoTotales("Cod Int : ", codigoInterno) + "\n\r");
                }
            }

            if (_venta.FECH_PRMA.HasValue && (_mangueras.DESCRIPCION.ToLower().Contains("gn") || _mangueras.DESCRIPCION.ToLower().Contains("gas")))
            {
                informacionVenta.Append(formatoTotales("Proximo mantenimiento : ", _venta.FECH_PRMA.Value.ToString("dd/MM/yyyy").Trim()));
            }
            informacionVenta.Append("------------------------------------------------" + "\n\r");
            informacionVenta.Append("Fecha : " + _factura.fecha.ToString("dd/MM/yyyy HH:mm:ss") + "\n\r");
            informacionVenta.Append("Surtidor : " + _venta.COD_SUR + "\n\r");
            informacionVenta.Append("Cara : " + _venta.COD_CAR + "\n\r");
            informacionVenta.Append("Manguera : " + _mangueras.COD_MAN + "\n\r");
            informacionVenta.Append("Vendedor : " + _venta.EMPLEADO + "\n\r");
            informacionVenta.Append("------------------------------------------------" + "\n\r");

            informacionVenta.Append("Producto  Cant.     Precio    Total    " + "\n\r");
            informacionVenta.Append(getLienaTarifas(_mangueras.DESCRIPCION.Trim(), String.Format("{0:#,0.000}", _venta.CANTIDAD), _venta.PRECIO_UNI.ToString("F"), String.Format("{0:#,0.00}", _venta.VALORNETO)).Centrar() + "\n\r");
            //informacionVenta.Append("------------------------------------------------" + "\n\r");
            informacionVenta.Append("DISCRIMINACION TARIFAS IVA".Centrar() + "\n\r");
            // informacionVenta.Append("------------------------------------------------" + "\n\r");

            informacionVenta.Append("Producto  Cant.     Tafira    Total    " + "\n\r");
            informacionVenta.Append(getLienaTarifas(_mangueras.DESCRIPCION.Trim(), String.Format("{0:#,0.000}", _venta.CANTIDAD), "0%", String.Format("{0:#,0.00}", _venta.VALORNETO)).Centrar() + "\n\r");
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
                if (formas.FirstOrDefault(x => x.Id == _factura.codigoFormaPago) != null)
                {
                    informacionVenta.Append(formatoTotales("Forma de pago : ", formas.FirstOrDefault(x => x.Id == _factura.codigoFormaPago).Descripcion.Trim()) + "\n\r");

                }
                else
                {
                    informacionVenta.Append(formatoTotales("Forma de pago :", " Efectivo") + "\n\r");

                }
                informacionVenta.Append("------------------------------------------------" + "\n\r");

            }

            if (!string.IsNullOrEmpty(infoTemp))
            {
                try
                {
                    var token = await _conexionEstacionRemota.GetToken(cancellationToken);

                    var resoluconElectronica = await _conexionEstacionRemota.GetResolucionElectronica(token, cancellationToken);

                    informacionVenta.Append(guiones.ToString()+ "\n\r");
                    informacionVenta.Append(resoluconElectronica.invoiceText+ "\n\r");
                }
                catch (Exception)
                {
                    informacionVenta.Append(guiones.ToString()+ "\n\r");
                    informacionVenta.Append("Modalidad Factura Electrónica "+ "\n\r");
                }

            }

            else if (_factura.Consecutivo != 0)
            {


                informacionVenta.Append(guiones.ToString()+ "\n\r");
                informacionVenta.Append("Resolucion de Facturacion No. "+ "\n\r");
                informacionVenta.Append(_factura.Autorizacion + " de " + _factura.FechaInicioResolucion.ToString("dd/MM/yyyy") + " "+ "\n\r");
                var numeracion = "Numeracion Autorizada por la DIAN";
                if (_factura.habilitada)
                {
                    numeracion = "Numeracion Habilitada por la DIAN";
                }
                informacionVenta.Append(numeracion + " "+ "\n\r");
                informacionVenta.Append("Del " + _factura.DescripcionResolucion + "-" + _factura.Inicio + " al " + _factura.DescripcionResolucion + "-" + _factura.Final + ""+ "\n\r");

            }
            if (!String.IsNullOrEmpty(_infoEstacion.Linea1))
            {
                informacionVenta.Append(_infoEstacion.Linea1+ "\n\r");
            }
            if (!String.IsNullOrEmpty(_infoEstacion.Linea2))
            {
                informacionVenta.Append(_infoEstacion.Linea2+ "\n\r");
            }
            if (!String.IsNullOrEmpty(_infoEstacion.Linea3))
            {
                informacionVenta.Append(_infoEstacion.Linea3+ "\n\r");
            }
            if (!String.IsNullOrEmpty(_infoEstacion.Linea4))
            {
                informacionVenta.Append(_infoEstacion.Linea4+ "\n\r");
            }
            informacionVenta.Append("Fabricado por: SIGES SOLUCIONES SAS ".Centrar() + "\n\r");
            informacionVenta.Append("Nit: 901430393-2 ".Centrar() + "\n\r");
            informacionVenta.Append("Nombre:  Facturador SIGES ".Centrar() + "\n\r");
            var firstMacAddress = NetworkInterface
        .GetAllNetworkInterfaces()
        .Where(nic => nic.OperationalStatus == OperationalStatus.Up && nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
        .Select(nic => nic.GetPhysicalAddress().ToString())
        .FirstOrDefault();
            firstMacAddress = firstMacAddress ?? "Mac Unknown";
            informacionVenta.Append(formatoTotales("SERIAL MAQUINA: ", firstMacAddress).Centrar() + "\n\r");
            informacionVenta.Append("."+ "\n\r");
            return  informacionVenta.ToString();
        }

        private string formatoTotales(string v1, string v2)
        {
            var result = v1;
            var tabs = new StringBuilder();
            tabs.Append(v1);
            var whitespaces = 40 - v1.Length - v2.Length;
            whitespaces = whitespaces < 0 ? 0 : whitespaces;
            tabs.Append(' ', whitespaces);

            tabs.Append(v2);
            return tabs.ToString();
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
