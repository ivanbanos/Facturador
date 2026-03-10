using EnviadorInformacionService.Models;
using FactoradorEstacionesModelo.Objetos;
using FacturadorEstacionesRepositorio;
using Gma.QrCodeNet.Encoding.Windows.Render;
using Gma.QrCodeNet.Encoding;
using ReporteFacturas;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Microsoft.IO.RecyclableMemoryStreamManager;

namespace EnviadorInformacionService
{
    public class CanastillaService : ICanastillaService
    {
        private Dictionary<int, string> carasImpresoras;
        private int imprimiendo = 0;
        private bool ImpresionAutomatica = false;
        private bool impresionFormaDePagoOrdenDespacho = false;
        private string firstMacAddress;
        private readonly IConexionEstacionRemota _conexionEstacionRemota;
        private readonly Guid estacionFuente;

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly bool generaFacturaElectronica;
        private Font printFont;
        private FacturaCanastilla _factura;
        private InfoEstacion _infoEstacion;
        private int _charactersPerPage;
        private EstacionesRepositorioSqlServer _estacionesRepositorio;
        private List<FormasPagos> formasDePago;
        private List<LineasImprimir> lineasImprimir;
        private string impresora;
        private int vecesImpresionCanastilla;
        public CanastillaService()
        {
            estacionFuente = new Guid(ConfigurationManager.AppSettings["estacionFuente"]);
            impresora = ConfigurationManager.AppSettings["IMPRESORACanastilla"];
            vecesImpresionCanastilla = int.Parse(ConfigurationManager.AppSettings["vecesImpresionCanastilla"]);
            _conexionEstacionRemota = new ConexionEstacionRemota();
            firstMacAddress = NetworkInterface
        .GetAllNetworkInterfaces()
        .Where(nic => nic.OperationalStatus == OperationalStatus.Up && nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
        .Select(nic => nic.GetPhysicalAddress().ToString())
        .FirstOrDefault();

            //Logger.Info(ConfigurationManager.AppSettings["Razon"].ToString());
            ImpresionAutomatica = bool.Parse(ConfigurationManager.AppSettings["ImpresionAutomatica"]);
            impresionFormaDePagoOrdenDespacho = bool.Parse(ConfigurationManager.AppSettings["impresionFormaDePagoOrdenDespacho"]);
            _infoEstacion = new InfoEstacion();
            generaFacturaElectronica = bool.Parse(ConfigurationManager.AppSettings["GeneraFacturaElectronica"].ToString());
            _infoEstacion.CaracteresPorPagina = int.Parse(ConfigurationManager.AppSettings["CaracteresPorPagina"].ToString());
            _infoEstacion.ImpresionPDA = bool.Parse(ConfigurationManager.AppSettings["ImpresionPDA"].ToString());
            _infoEstacion.Direccion = ConfigurationManager.AppSettings["Direccion"].ToString();
            _infoEstacion.Linea1 = ConfigurationManager.AppSettings["Linea1"].ToString();
            _infoEstacion.Linea2 = ConfigurationManager.AppSettings["Linea2"].ToString();
            _infoEstacion.Linea3 = ConfigurationManager.AppSettings["Linea3"].ToString();
            _infoEstacion.Linea4 = ConfigurationManager.AppSettings["Linea4"].ToString();
            _infoEstacion.NIT = ConfigurationManager.AppSettings["NIT"].ToString();
            _infoEstacion.Nombre = ConfigurationManager.AppSettings["Nombre"].ToString();
            _infoEstacion.Razon = ConfigurationManager.AppSettings["Razon"].ToString();

            _infoEstacion.Telefono = ConfigurationManager.AppSettings["Telefono"].ToString();
            _infoEstacion.vecesPermitidasImpresion = int.Parse(ConfigurationManager.AppSettings["vecesImpresionCanastilla"].ToString());


            carasImpresoras = new Dictionary<int, string>();
            foreach (string nameValueItem in ConfigurationManager.AppSettings)
            {
                if (nameValueItem.Contains("CARA"))
                {
                    string impresora = ConfigurationManager.AppSettings[nameValueItem];
                    int isla = Int32.Parse(nameValueItem.Split(' ')[1]);
                    carasImpresoras.Add(isla, impresora);
                }
            }
            _estacionesRepositorio = new EstacionesRepositorioSqlServer();

            formasDePago = _estacionesRepositorio.BuscarFormasPagos();
            imprimiendo = 0;
        }
        public void ProcesoCanastilla()
        {
            while (true)
            {
                DateTime lastTimeExec = DateTime.Now;

                try
                {
                    if (imprimiendo == 0)
                    {
                        if (lastTimeExec < DateTime.Now.AddHours(-6))
                        {
                            lastTimeExec = DateTime.Now;
                            try
                            {
                                var infoTemp = _conexionEstacionRemota.getInfoEstacion(estacionFuente, _conexionEstacionRemota.getToken());
                                _infoEstacion = infoTemp;
                            }
                            catch (Exception e)
                            {
                                _infoEstacion = new InfoEstacion
                                {
                                    CaracteresPorPagina = int.Parse(ConfigurationManager.AppSettings["CaracteresPorPagina"].ToString()),
                                    Direccion = ConfigurationManager.AppSettings["Direccion"].ToString(),
                                    Linea1 = ConfigurationManager.AppSettings["Linea1"].ToString(),
                                    Linea2 = ConfigurationManager.AppSettings["Linea2"].ToString(),
                                    Linea3 = ConfigurationManager.AppSettings["Linea3"].ToString(),
                                    Linea4 = ConfigurationManager.AppSettings["Linea4"].ToString(),
                                    NIT = ConfigurationManager.AppSettings["NIT"].ToString(),
                                    Nombre = ConfigurationManager.AppSettings["Nombre"].ToString(),
                                    Razon = ConfigurationManager.AppSettings["Razon"].ToString(),

                                    Telefono = ConfigurationManager.AppSettings["Telefono"].ToString(),
                                    vecesPermitidasImpresion = int.Parse(ConfigurationManager.AppSettings["vecesPermitidasImpresion"].ToString())
                                };

                            }
                        }
                        if (_estacionesRepositorio.HayFacturasCanastillaPorImprimir())
                        {
                            var factura = _estacionesRepositorio.getFacturasCanastillaImprimir(); if (imprimiendo == 0 && factura != null
                             && ((factura.impresa == 0 && ImpresionAutomatica) || factura.impresa <= -1 || factura.enviada))
                            {


                                factura.impresa = 0 - vecesImpresionCanastilla;
                                while (factura.impresa != 0)
                                {
                                    if (imprimiendo == 0)
                                    {
                                        imprimiendo++;
                                        Imprimir(factura);
                                        factura.impresa++;
                                    }
                                    else
                                    {
                                        Thread.Sleep(100);
                                    }
                                }
                                Thread.Sleep(1000);
                            }


                        }


                        Thread.Sleep(1000);

                    }
                    else
                    {
                        Thread.Sleep(1000);
                    }
                }
                catch (Exception ex)
                {

                    imprimiendo = 0;
                    Logger.Error("Error " + ex.Message);
                    Logger.Error("Error " + ex.StackTrace);
                    Thread.Sleep(5000);
                }

                Thread.Sleep(5000);
            }

        }


        public void WebCanastilla()
        {
            while (true)
            {
                try
                {
                    var facturas = _estacionesRepositorio.BuscarFacturasNoEnviadasCanastilla();
                    var token = _conexionEstacionRemota.getToken();
                    if (facturas.Any())
                    {
                        var formas = _estacionesRepositorio.BuscarFormasPagos();
                        foreach (var factura in facturas)
                        {
                            factura.codigoFormaPago = formasDePago.FirstOrDefault(x => x.Id == factura.codigoFormaPago.Id);
                        }
                        var okFacturas = _conexionEstacionRemota.EnviarFacturasCanastilla(facturas, estacionFuente, token);
                        if (okFacturas)
                        {

                            _estacionesRepositorio.ActuralizarFacturasEnviadosCanastilla(facturas.Select(x => x.FacturasCanastillaId));

                        }
                        else
                        {

                            Logger.Error("No subieron facturas");
                        }
                    }

                    var consecutivo = 0;// _conexionEstacionRemota.ObtenerParaImprimir(estacionFuente, token);

                    if (consecutivo != 0)
                    {
                        var facturaReal = _estacionesRepositorio.BuscarFacturaCanastillaPorConsecutivo(consecutivo);
                        facturaReal.impresa = 0 - vecesImpresionCanastilla;
                        while (facturaReal.impresa != 0)
                        {
                            if (imprimiendo == 0)
                            {
                                imprimiendo++;
                                Imprimir(facturaReal);
                                facturaReal.impresa++;
                            }
                            else
                            {
                                Thread.Sleep(200);
                            }
                        }
                    }

                }
                catch (Exception ex)
                {

                    Logger.Error($"Error en proceso {ex.Message}. {ex.StackTrace} ");
                }
                Thread.Sleep(3000);
            }

        }


        private void Imprimir(FacturaCanastilla factura)
        {
            _factura = factura;
            getLineasImprimir();
            //imprimir
            try
            {

                try
                {
                    printFont = new Font("Console", 9);
                    PrintDocument pd = new PrintDocument();
                    pd.PrintPage += new PrintPageEventHandler(pd_PrintPageOnly);
                    pd.DefaultPageSettings.Margins.Bottom = 20;
                    // Print the document.

                    pd.PrinterSettings.PrinterName = impresora;

                    pd.Print();

                }
                catch (Exception ex)
                {

                    printFont = new Font("Console", 9);
                    PrintDocument pd = new PrintDocument();
                    pd.PrintPage += new PrintPageEventHandler(pd_PrintPageOnly);
                    pd.DefaultPageSettings.Margins.Bottom = 20;
                    pd.Print();

                }
            }
            catch (Exception ex)
            {
                imprimiendo = 0;
                Logger.Error("Error " + ex.Message);
                Logger.Error("Error " + ex.StackTrace);
                Thread.Sleep(5000);
            }
        }

        private void getLineasImprimir()
        {
            _charactersPerPage = _infoEstacion.CaracteresPorPagina;

            if (_charactersPerPage == 0)
            {
                _charactersPerPage = 40;
            }
            lineasImprimir = new List<LineasImprimir>();
            var guiones = new StringBuilder();
            guiones.Append('-', _charactersPerPage);
            // Iterate over the file, printing each line.
            lineasImprimir.Add(new LineasImprimir(".", true));
            lineasImprimir.Add(new LineasImprimir(_infoEstacion.Razon, true));
            lineasImprimir.Add(new LineasImprimir("NIT " + _infoEstacion.NIT, true));
            lineasImprimir.Add(new LineasImprimir(_infoEstacion.Nombre, true));
            lineasImprimir.Add(new LineasImprimir(_infoEstacion.Direccion, true));
            lineasImprimir.Add(new LineasImprimir(_infoEstacion.Telefono, true));
            lineasImprimir.Add(new LineasImprimir(guiones.ToString(), false));
            var infoTemp = "";
            if (generaFacturaElectronica)
            {
                try
                {
                    var intentos = 0;
                    do
                    {
                        infoTemp = _conexionEstacionRemota.GetInfoFacturaElectronicaCanastilla(_factura.FacturasCanastillaId, estacionFuente, _conexionEstacionRemota.getToken());
                        if (!string.IsNullOrEmpty(infoTemp))
                        {
                            break;
                        }

                        if (!_factura.enviada)
                        {
                            var ok = _conexionEstacionRemota.EnviarFacturasCanastilla(new List<FacturaCanastilla> { _factura }, estacionFuente, _conexionEstacionRemota.getToken());
                            if (ok)
                            {
                                _estacionesRepositorio.ActuralizarFacturasEnviadosCanastilla(new List<int> { _factura.FacturasCanastillaId });
                            }
                        }

                        Thread.Sleep(5000);
                    } while (string.IsNullOrEmpty(infoTemp) && intentos++ < 3);

                    Console.WriteLine("info fac elec " + infoTemp);
                    Logger.Info("info fac elec " + infoTemp);
                    Logger.Info("id " + _factura.FacturasCanastillaId);
                }
                catch (Exception ex)
                {
                    Logger.Info("info fac elec " + ex.Message);
                    Logger.Info("info fac elec " + ex.StackTrace);
                    Console.WriteLine("info fac elec " + ex.Message);
                    Console.WriteLine("info fac elec " + ex.StackTrace);
                }
            }
            if (!string.IsNullOrEmpty(infoTemp))
            {
                infoTemp = infoTemp.Replace("\n\r", " ");

                var facturaElectronica = infoTemp.Split(' ');

                lineasImprimir.Add(new LineasImprimir("Factura Electrónica de Venta " + facturaElectronica[2], true));
                lineasImprimir.Add(new LineasImprimir(facturaElectronica[3], true));
                lineasImprimir.Add(new LineasImprimir(facturaElectronica[4].Substring(0, facturaElectronica[4].Length / 2), true));
                lineasImprimir.Add(new LineasImprimir(facturaElectronica[4].Substring(facturaElectronica[4].Length / 2), true));
                lineasImprimir.Add(new LineasImprimir(guiones.ToString(), false));
                lineasImprimir.Add(new LineasImprimir("Venta: " + _factura.consecutivo, false));
            }
            else
            {
                lineasImprimir.Add(new LineasImprimir("Orden de Servicio Temporal: " + _factura.FacturasCanastillaId, true));
            }

            lineasImprimir.Add(new LineasImprimir(guiones.ToString(), false));
            lineasImprimir.Add(new LineasImprimir(formatoTotales("Vendido a : ", _factura.terceroId.Nombre == null ? "" : _factura.terceroId.Nombre.Trim()), false));
            lineasImprimir.Add(new LineasImprimir(formatoTotales("Nit/C.C. : ", _factura.terceroId.identificacion.Trim()), false));



            lineasImprimir.Add(new LineasImprimir(guiones.ToString(), false));
            lineasImprimir.Add(new LineasImprimir(formatoTotales("Fecha : ", _factura.fecha.ToString("dd/MM/yyyy HH:mm:ss")), false));

            lineasImprimir.Add(new LineasImprimir(formatoTotales("Isla : ", _factura.Isla + ""), false));
            lineasImprimir.Add(new LineasImprimir(formatoTotales("Vendedor : ", _factura.Empleado?.Trim() + ""), false));
            if (!string.IsNullOrWhiteSpace(_factura.Placa))
            {
                lineasImprimir.Add(new LineasImprimir(formatoTotales("Placa : ", _factura.Placa.Trim()), false));
            }
            lineasImprimir.Add(new LineasImprimir(guiones.ToString(), false));
            if (_infoEstacion.ImpresionPDA)
            {
                foreach (var canastilla in _factura.canastillas)
                {
                    lineasImprimir.Add(new LineasImprimir($"Producto: {canastilla.Canastilla.descripcion.Trim()}", false));
                    lineasImprimir.Add(new LineasImprimir($"Cantidad: {string.Format("{0:#,0.00}", canastilla.cantidad)}", false));
                    lineasImprimir.Add(new LineasImprimir($"Precio: {canastilla.precioBruto.ToString("F")}", false));
                    lineasImprimir.Add(new LineasImprimir($"Subtotal: {string.Format("{0:#,0.00}", canastilla.subtotal)}", false));
                }

            }
            else
            {
                lineasImprimir.Add(new LineasImprimir(getLienaTarifas("Producto", "   Cant.", "  Precio", "   subtotal") + "", false));
                foreach (var canastilla in _factura.canastillas)
                {
                    lineasImprimir.Add(new LineasImprimir(getLienaTarifas(canastilla.Canastilla.descripcion.Trim(), String.Format("{0:#,0.000}", canastilla.cantidad), canastilla.precioBruto.ToString("F"), String.Format("{0:#,0.00}", canastilla.subtotal), true) + "", false));
                }
            }
            lineasImprimir.Add(new LineasImprimir(guiones.ToString() + "", false));
            lineasImprimir.Add(new LineasImprimir("DISCRIMINACION TARIFAS IVA" + "", true));
            //  lineasImprimir.Add(new LineasImprimir(guiones.ToString() + "", false));
            if (_infoEstacion.ImpresionPDA)
            {
                foreach (var canastilla in _factura.canastillas)
                {
                    lineasImprimir.Add(new LineasImprimir($"Producto: {canastilla.Canastilla.descripcion.Trim()}", false));
                    lineasImprimir.Add(new LineasImprimir($"Cantidad: {string.Format("{0:#,0.000}", canastilla.cantidad)}", false));
                    lineasImprimir.Add(new LineasImprimir($"Iva: {string.Format("{0:#,0.00}", canastilla.iva)}  ", false));
                    lineasImprimir.Add(new LineasImprimir($"Total: {string.Format("{0:#,0.00}", canastilla.total)}", false));
                }
            }
            else
            {
                lineasImprimir.Add(new LineasImprimir(getLienaTarifas("Producto", "   Cant.", "  Iva", "   Total") + "", false));
                foreach (var canastilla in _factura.canastillas)
                {
                    lineasImprimir.Add(new LineasImprimir(getLienaTarifas(canastilla.Canastilla.descripcion.Trim(), String.Format("{0:#,0.000}", canastilla.cantidad), $"{canastilla.iva}", String.Format("{0:#,0.00}", canastilla.total), true) + "", false));
                }
            }
            lineasImprimir.Add(new LineasImprimir(guiones.ToString(), false));
            lineasImprimir.Add(new LineasImprimir(formatoTotales("Descuento: ", String.Format("{0:#,0.00}", _factura.descuento)), false));
            //lineasImprimir.Add(new LineasImprimir(guiones.ToString(), false));
            lineasImprimir.Add(new LineasImprimir(formatoTotales("Subtotal sin IVA : ", String.Format("{0:#,0.00}", _factura.subtotal)), false));
            //lineasImprimir.Add(new LineasImprimir(guiones.ToString(), false));
            lineasImprimir.Add(new LineasImprimir(formatoTotales("Subtotal IVA :", $"{string.Format("{0:#,0.00}", _factura.iva)}"), false));
            //lineasImprimir.Add(new LineasImprimir(guiones.ToString(), false));
            lineasImprimir.Add(new LineasImprimir(formatoTotales("TOTAL : ", String.Format("{0:#,0.00}", _factura.total)), false));
            //lineasImprimir.Add(new LineasImprimir(guiones.ToString(), false));

            var forma = formasDePago.FirstOrDefault(x => x.Id == _factura.codigoFormaPago.Id);
            lineasImprimir.Add(new LineasImprimir(formatoTotales("Forma de pago : ", forma?.Descripcion?.Trim()), false));



            try
            {
                var resoluconElectronica = _conexionEstacionRemota.GetResolucionElectronica(_conexionEstacionRemota.getToken());

                lineasImprimir.Add(new LineasImprimir(guiones.ToString(), false));
                if (resoluconElectronica.invoiceText.Contains("desde"))
                {

                    lineasImprimir.Add(new LineasImprimir(resoluconElectronica.invoiceText.Substring(0, resoluconElectronica.invoiceText.IndexOf("desde")), false));
                    lineasImprimir.Add(new LineasImprimir(resoluconElectronica.invoiceText.Substring(resoluconElectronica.invoiceText.IndexOf("desde"), resoluconElectronica.invoiceText.IndexOf("Valido")), false));
                    lineasImprimir.Add(new LineasImprimir(resoluconElectronica.invoiceText.Substring(resoluconElectronica.invoiceText.IndexOf("Valido")), false));
                }
                else
                {

                    lineasImprimir.Add(new LineasImprimir(resoluconElectronica.invoiceText, false));
                }
            }
            catch (Exception)
            {
                lineasImprimir.Add(new LineasImprimir(guiones.ToString(), false));
                lineasImprimir.Add(new LineasImprimir("Modalidad Factura Electrónica ", false));
            }

            if (!String.IsNullOrEmpty(_infoEstacion.Linea1))
            {
                lineasImprimir.Add(new LineasImprimir(_infoEstacion.Linea1, false));
            }
            if (!String.IsNullOrEmpty(_infoEstacion.Linea2))
            {
                lineasImprimir.Add(new LineasImprimir(_infoEstacion.Linea2, false));
            }
            if (!String.IsNullOrEmpty(_infoEstacion.Linea3))
            {
                lineasImprimir.Add(new LineasImprimir(_infoEstacion.Linea3, false));
            }
            if (!String.IsNullOrEmpty(_infoEstacion.Linea4))
            {
                lineasImprimir.Add(new LineasImprimir(_infoEstacion.Linea4, false));
            }
            lineasImprimir.Add(new LineasImprimir("Fabricado por:" + " SIGES SOLUCIONES SAS ", true));
            lineasImprimir.Add(new LineasImprimir("Nit:" + " 901430393-2 ", true));
            lineasImprimir.Add(new LineasImprimir("Nombre:" + " Facturador SIGES ", true));
            lineasImprimir.Add(new LineasImprimir(formatoTotales("SERIAL MAQUINA: ", firstMacAddress), false));
            lineasImprimir.Add(new LineasImprimir(".", true));
            if (!string.IsNullOrEmpty(infoTemp))
            {

                var facturaElectronica = infoTemp.Split(' ');
                lineasImprimir.Add(new LineasImprimir(guiones.ToString(), false, $"https://catalogo-vpfe.dian.gov.co/User/SearchDocument?DocumentKey={facturaElectronica[4]}"));
            }

        }

        private void pd_PrintPageOnly(object sender, PrintPageEventArgs ev)
        {
            try
            {
                float yPos = 0;
                int count = 0;
                float leftMargin = 5;
                float topMargin = 10;
                String line = null;
                int sizePaper = ev.PageSettings.PaperSize.Width;
                int fonSizeInches = 72 / 9;
                _charactersPerPage = _infoEstacion.CaracteresPorPagina;
                if (_charactersPerPage == 0)
                {
                    _charactersPerPage = fonSizeInches * sizePaper / 100;
                }
                foreach (var linea in lineasImprimir)
                {
                    if (!string.IsNullOrEmpty(linea.qr))
                    {

                        count = printLine(linea.qr, ev, count, leftMargin, topMargin, false, isQr: true);
                    }
                    else
                    {

                        count = printLine(linea.linea, ev, count, leftMargin, topMargin, linea.centrada);
                    }
                }
                count = printLine(" ", ev, count, leftMargin, topMargin, false);
                count = printLine(" ", ev, count, leftMargin, topMargin, false);
                if (line != null)
                    ev.HasMorePages = true;
                else
                    ev.HasMorePages = false;


                _estacionesRepositorio.SetFacturaCanastillaImpresa(_factura.FacturasCanastillaId);
                imprimiendo--;
                if (line != null)
                    ev.HasMorePages = true;
                else
                    ev.HasMorePages = false;
            }
            catch (Exception ex)
            {
                imprimiendo = 0;
                Logger.Error("Error " + ex.Message);
                Logger.Error("Error " + ex.StackTrace);
                Thread.Sleep(5000);
            }

        }

        private string formatoTotales(string v1, string v2)
        {
            var result = v1;
            var tabs = new StringBuilder();
            tabs.Append(v1);
            var whitespaces = _charactersPerPage - v1.Length - v2.Length;
            whitespaces = whitespaces < 0 ? 0 : whitespaces;
            tabs.Append(' ', whitespaces);

            tabs.Append(v2);
            return tabs.ToString();
        }

        private string getLienaTarifas(string v1, string v2, string v3, string v4, bool after = false)
        {
            var spacesInPage = _charactersPerPage / 4;
            var tabs = new StringBuilder();
            if (_charactersPerPage == 40)
            {
                tabs.Append(v1.Substring(0, v1.Length < 12 ? v1.Length : 12));
                var whitespaces = 12 - v1.Length;
                whitespaces = whitespaces < 0 ? 0 : whitespaces;
                tabs.Append(' ', whitespaces);


                if (after)
                {
                    whitespaces = 8 - v2.Length;
                    whitespaces = whitespaces < 0 ? 0 : whitespaces;
                    tabs.Append(' ', whitespaces);
                    tabs.Append(v2.Substring(0, v2.Length < 8 ? v2.Length : 8));

                    whitespaces = 8 - v3.Length;
                    whitespaces = whitespaces < 0 ? 0 : whitespaces;
                    tabs.Append(' ', whitespaces);
                    tabs.Append(v3.Substring(0, v3.Length < 8 ? v3.Length : 8));

                    whitespaces = 12 - v4.Length;
                    whitespaces = whitespaces < 0 ? 0 : whitespaces;
                    tabs.Append(' ', whitespaces);
                    tabs.Append(v4.Substring(0, v4.Length < 12 ? v4.Length : 12));
                }
                else
                {
                    tabs.Append(v2.Substring(0, v2.Length < 8 ? v2.Length : 8));
                    whitespaces = 8 - v2.Length;
                    whitespaces = whitespaces < 0 ? 0 : whitespaces;
                    tabs.Append(' ', whitespaces);

                    tabs.Append(v3.Substring(0, v3.Length < 8 ? v3.Length : 8));
                    whitespaces = 8 - v3.Length;
                    whitespaces = whitespaces < 0 ? 0 : whitespaces;
                    tabs.Append(' ', whitespaces);

                    tabs.Append(v4.Substring(0, v4.Length < 12 ? v4.Length : 12));
                    whitespaces = 12 - v4.Length;
                    whitespaces = whitespaces < 0 ? 0 : whitespaces;
                    tabs.Append(' ', whitespaces);


                }
                return tabs.ToString();
            }
            else
            {
                tabs.Append(v1.Substring(0, v1.Length < spacesInPage ? v1.Length : spacesInPage));
                var whitespaces = spacesInPage - v1.Length;
                whitespaces = whitespaces < 0 ? 0 : whitespaces;
                tabs.Append(' ', whitespaces);


                if (after)
                {
                    whitespaces = spacesInPage - v2.Length;
                    whitespaces = whitespaces < 0 ? 0 : whitespaces;
                    tabs.Append(' ', whitespaces);
                    tabs.Append(v2.Substring(0, v2.Length < spacesInPage ? v2.Length : spacesInPage));

                    whitespaces = spacesInPage - v3.Length;
                    whitespaces = whitespaces < 0 ? 0 : whitespaces;
                    tabs.Append(' ', whitespaces);
                    tabs.Append(v3.Substring(0, v3.Length < spacesInPage ? v3.Length : spacesInPage));

                    whitespaces = spacesInPage - v4.Length;
                    whitespaces = whitespaces < 0 ? 0 : whitespaces;
                    tabs.Append(' ', whitespaces);
                    tabs.Append(v4.Substring(0, v4.Length < spacesInPage ? v4.Length : spacesInPage));
                }
                else
                {
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


                }
                return tabs.ToString();
            }
        }

        private int printLine(string text, PrintPageEventArgs ev, int count, float leftMargin, float topMargin, bool center = false, bool isQr = false)
        {
            if (center)
            {
                var whitespaces = (_charactersPerPage - text.Length) / 2;
                var tabs = new StringBuilder();
                whitespaces = whitespaces < 0 ? 0 : whitespaces;
                tabs.Append(' ', whitespaces);
                text = tabs.ToString() + text;
            }
            float yPos = topMargin + (count * printFont.GetHeight(ev.Graphics));
            if (isQr)
            {
                GenerateQRCode(text, 160);
                Image newImage = Image.FromFile($"{AppContext.BaseDirectory}/file.bmp");

                RectangleF srcRect = new RectangleF(0, 0, 160F, 160F);
                GraphicsUnit units = GraphicsUnit.Pixel;
                ev.Graphics.DrawImage(newImage, leftMargin, yPos, srcRect, units);
            }
            else
            {
                ev.Graphics.DrawString(text, printFont, Brushes.Black, leftMargin, yPos, new StringFormat() { });
            }
            count++;
            return count;
        }

        private void GenerateQRCode(string content, int size)
        {
            QrEncoder encoder = new QrEncoder(ErrorCorrectionLevel.H);
            QrCode qrCode;
            encoder.TryEncode(content, out qrCode);

            GraphicsRenderer gRenderer = new GraphicsRenderer(new FixedModuleSize(4, QuietZoneModules.Two), System.Drawing.Brushes.Black, System.Drawing.Brushes.White);
            //Graphics g = gRenderer.Draw(qrCode.Matrix);

            MemoryStream ms = new MemoryStream();
            gRenderer.WriteToStream(qrCode.Matrix, ImageFormat.Bmp, ms);

            var imageTemp = new Bitmap(ms);

            var image = new Bitmap(imageTemp, new System.Drawing.Size(new System.Drawing.Point(size, size)));

            image.Save($"{AppContext.BaseDirectory}/file.bmp", ImageFormat.Bmp);

        }
    }
}
