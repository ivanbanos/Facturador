using FacturadorAPI.Models;
using FacturadorAPI.Repository.Repo;
using MachineUtilizationApi.Repository;
using MediatR;
using Microsoft.Extensions.Options;
using System.Net.NetworkInformation;
using System.Text;

namespace FacturadorAPI.Application.Commands
{
    public class AgregarFacturaCanastillaCommandHandler : IRequestHandler<AgregarFacturaCanastillaCommand, string>
    {
        private readonly ILogger<AgregarFacturaCanastillaCommandHandler> _logger;
        private readonly IDataBaseHandler _databaseHandler;
        private readonly InfoEstacion _infoEstacion;
        private readonly IConexionEstacionRemota _conexionEstacionRemota;

        public AgregarFacturaCanastillaCommandHandler(ILogger<AgregarFacturaCanastillaCommandHandler> logger,
            IDataBaseHandler databaseHandler,
            IOptions<InfoEstacion> options,
            IConexionEstacionRemota conexionEstacionRemota)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _infoEstacion = options.Value;
            _databaseHandler = databaseHandler ?? throw new ArgumentNullException(nameof(databaseHandler));
            _conexionEstacionRemota = conexionEstacionRemota ?? throw new ArgumentNullException(nameof(conexionEstacionRemota));
        }

        public async Task<string> Handle(AgregarFacturaCanastillaCommand request, CancellationToken cancellationToken)
        {

            try
            {

              var facturaId =   await _databaseHandler.GenerarFacturaCanastilla(request.FacturaCanastilla, false);
                var _factura = await _databaseHandler.BuscarFacturaCanastillaPorConsecutivo(facturaId);
                var _charactersPerPage = 40;

                var formas = await _databaseHandler.ListarFormasPagoSP(cancellationToken);
                if (_charactersPerPage == 0)
                {
                    _charactersPerPage = 40;
                }
                var informacionVenta = new StringBuilder();
                var guiones = new StringBuilder();
                guiones.Append('-', 40);
                // Iterate over the file, printing each line.
                 informacionVenta.Append(".");
                 informacionVenta.Append(_infoEstacion.Razon);
                 informacionVenta.Append("NIT " + _infoEstacion.NIT);
                 informacionVenta.Append(_infoEstacion.Nombre);
                 informacionVenta.Append(_infoEstacion.Direccion);
                 informacionVenta.Append(_infoEstacion.Telefono);
                 informacionVenta.Append(guiones.ToString());
                var infoTemp = "";
                //if (generaFacturaElectronica)
                //{
                //    try
                //    {
                //        infoTemp = _conexionEstacionRemota.GetInfoFacturaElectronica(_factura.ventaId, estacionFuente, _conexionEstacionRemota.getToken());

                //    }
                //    catch (Exception)
                //    {
                //        infoTemp = null;
                //    }
                //}
                if (!string.IsNullOrEmpty(infoTemp))
                {
                    infoTemp = infoTemp.Replace("\n\r", " ");

                    var facturaElectronica = infoTemp.Split(' ');

                     informacionVenta.Append("Factura Electrónica" + facturaElectronica[2]);
                     informacionVenta.Append(facturaElectronica[3]);
                     informacionVenta.Append(facturaElectronica[4]);
                }

                 informacionVenta.Append("SISTEMA POS CANASTILLA No: " + _factura.resolucion.DescripcionResolucion + "-" + _factura.consecutivo);


                 informacionVenta.Append(guiones.ToString());
                 informacionVenta.Append(formatoTotales("Vendido a : ", _factura.terceroId.Nombre == null ? "" : _factura.terceroId.Nombre.Trim()));
                 informacionVenta.Append(formatoTotales("Nit/C.C. : ", _factura.terceroId.identificacion.Trim()));



                 informacionVenta.Append(guiones.ToString());
                 informacionVenta.Append(formatoTotales("Fecha : ", _factura.fecha.ToString("dd/MM/yyyy HH:mm:ss")));
                 informacionVenta.Append(guiones.ToString());
                if (_infoEstacion.ImpresionPDA)
                {
                    foreach (var canastilla in _factura.canastillas)
                    {
                         informacionVenta.Append($"Producto: {canastilla.Canastilla.descripcion.Trim()}");
                         informacionVenta.Append($"Cantidad: {string.Format("{0:#,0.000}", canastilla.cantidad)}");
                         informacionVenta.Append($"Precio: {canastilla.precio.ToString("F")}");
                         informacionVenta.Append($"Subtotal: {canastilla.subtotal}");
                    }

                }
                else
                {
                     informacionVenta.Append(getLienaTarifas("Producto", "   Cant.", "  Precio", "   subtotal") + "");
                    foreach (var canastilla in _factura.canastillas)
                    {
                         informacionVenta.Append(getLienaTarifas(canastilla.Canastilla.descripcion.Trim(), String.Format("{0:#,0.000}", canastilla.cantidad), canastilla.precio.ToString("F"), String.Format("{0:#,0.00}", canastilla.subtotal) + ""));
                    }
                }
                 informacionVenta.Append(guiones.ToString() + "");
                 informacionVenta.Append("DISCRIMINACION TARIFAS IVA" + "");
                //   informacionVenta.Append(guiones.ToString() + "");
                if (_infoEstacion.ImpresionPDA)
                {
                    foreach (var canastilla in _factura.canastillas)
                    {
                         informacionVenta.Append($"Producto: {canastilla.Canastilla.descripcion.Trim()}");
                         informacionVenta.Append($"Cantidad: {string.Format("{0:#,0.000}", canastilla.cantidad)}");
                         informacionVenta.Append($"Iva: {canastilla.iva}  ");
                         informacionVenta.Append($"Total: {canastilla.total}");
                    }
                }
                else
                {
                     informacionVenta.Append(getLienaTarifas("Producto", "   Cant.", "  Iva", "   Total") + "");
                    foreach (var canastilla in _factura.canastillas)
                    {
                         informacionVenta.Append(getLienaTarifas(canastilla.Canastilla.descripcion.Trim(), String.Format("{0:#,0.000}", canastilla.cantidad), $"{canastilla.iva}", String.Format("{0:#,0.00}", canastilla.total) + ""));
                    }
                }
                 informacionVenta.Append(guiones.ToString());
                 informacionVenta.Append(formatoTotales("Descuento: ", String.Format("{0:#,0.00}", _factura.descuento)));
                // informacionVenta.Append(guiones.ToString());
                 informacionVenta.Append(formatoTotales("Subtotal sin IVA : ", String.Format("{0:#,0.00}", _factura.subtotal)));
                // informacionVenta.Append(guiones.ToString());
                 informacionVenta.Append(formatoTotales("Subtotal IVA :", $"{_factura.iva}"));
                // informacionVenta.Append(guiones.ToString());
                 informacionVenta.Append(formatoTotales("TOTAL : ", String.Format("{0:#,0.00}", _factura.total)));
                // informacionVenta.Append(guiones.ToString());

                var forma = formas.FirstOrDefault(x => x.Id == _factura.codigoFormaPago.Id);
                 informacionVenta.Append(formatoTotales("Forma de pago : ", forma?.Descripcion?.Trim()));


                if (!string.IsNullOrEmpty(infoTemp))
                {
                     informacionVenta.Append(guiones.ToString());
                     informacionVenta.Append("Resolucion de Facturacion No. ");
                     informacionVenta.Append("18764013579016 de 2021-05-24");
                     informacionVenta.Append("Modalidad Factura Electrónica ");
                     informacionVenta.Append("Desde N° FEE1 hasta FEE1000000");
                     informacionVenta.Append("Vigencia hasta 2021-11-24");

                }

                else if (_factura.consecutivo != 0)
                {


                     informacionVenta.Append(guiones.ToString());
                     informacionVenta.Append("Resolucion de Facturacion No. ");
                     informacionVenta.Append(_factura.resolucion.Autorizacion + " de " + _factura.resolucion.FechaInicioResolucion.ToString("dd/MM/yyyy") + " ");
                    var numeracion = "Numeracion Autorizada por la DIAN";
                    if (_factura.resolucion.Habilitada)
                    {
                        numeracion = "Numeracion Habilitada por la DIAN";
                    }
                     informacionVenta.Append(numeracion + " ");
                     informacionVenta.Append("Del " + _factura.resolucion.DescripcionResolucion + "-" + _factura.resolucion.ConsecutivoInicial + " al " + _factura.resolucion.DescripcionResolucion + "-" + _factura.resolucion.ConsecutivoFinal + "");

                }
                if (!String.IsNullOrEmpty(_infoEstacion.Linea1))
                {
                     informacionVenta.Append(_infoEstacion.Linea1);
                }
                if (!String.IsNullOrEmpty(_infoEstacion.Linea2))
                {
                     informacionVenta.Append(_infoEstacion.Linea2);
                }
                if (!String.IsNullOrEmpty(_infoEstacion.Linea3))
                {
                     informacionVenta.Append(_infoEstacion.Linea3);
                }
                if (!String.IsNullOrEmpty(_infoEstacion.Linea4))
                {
                     informacionVenta.Append(_infoEstacion.Linea4);
                }
                 informacionVenta.Append("Fabricado por:" + " SIGES SOLUCIONES SAS ");
                 informacionVenta.Append("Nit:" + " 901430393-2 ");
                 informacionVenta.Append("Nombre:" + " Facturador SIGES ");
                var firstMacAddress = NetworkInterface
        .GetAllNetworkInterfaces()
        .Where(nic => nic.OperationalStatus == OperationalStatus.Up && nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
        .Select(nic => nic.GetPhysicalAddress().ToString())
        .FirstOrDefault();
                firstMacAddress = firstMacAddress ?? "Mac Unknown";
                informacionVenta.Append(formatoTotales("SERIAL MAQUINA: ", firstMacAddress));
                 informacionVenta.Append(".");
            }
            catch (Exception ex)
            {
            }

            return "";

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
            var fidelizado = await _databaseHandler.GetFidelizado(ventaId);
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
