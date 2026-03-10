using EnviadorInformacionService.Models;
using FactoradorEstacionesModelo.Objetos;
using FacturacionelectronicaCore.Negocio.Modelo;
using FacturadorEstacionesRepositorio;
using Gma.QrCodeNet.Encoding;
using Gma.QrCodeNet.Encoding.Windows.Render;
using Newtonsoft.Json;
using ReporteFacturas;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.IO;
using System.Globalization;
using System.Linq;
using System.Net.NetworkInformation;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace EnviadorInformacionService
{
    public class ImpresionService
    {
        private Dictionary<int, string> carasImpresoras;
        private Dictionary<string, string> islasImpresoras;
        private int imprimiendo = 0;
        private bool ImpresionAutomatica = false;
        private bool impresionFormaDePagoOrdenDespacho = false;
        private string firstMacAddress;
        private readonly IConexionEstacionRemota _conexionEstacionRemota;
        private readonly Guid estacionFuente;
        private readonly bool MultiplicarPor10;
        private readonly IFidelizacion _fidelizacion;


        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly bool generaFacturaElectronica;

        public ImpresionService()
        {
            string GetSetting(string key, string defaultValue = "")
            {
                return ConfigurationManager.AppSettings[key] ?? defaultValue;
            }

            bool GetBool(string key, bool defaultValue = false)
            {
                var value = ConfigurationManager.AppSettings[key];
                return bool.TryParse(value, out var parsed) ? parsed : defaultValue;
            }

            int GetInt(string key, int defaultValue = 0)
            {
                var value = ConfigurationManager.AppSettings[key];
                return int.TryParse(value, out var parsed) ? parsed : defaultValue;
            }

            var estacionFuenteSetting = ConfigurationManager.AppSettings["estacionFuente"];
            if (!Guid.TryParse(estacionFuenteSetting, out estacionFuente))
            {
                estacionFuente = Guid.Empty;
                Logger.Warn("No se encontro 'estacionFuente' valido en AppSettings. Se usara Guid.Empty.");
            }

            MultiplicarPor10 = GetBool("MultiplicarPor10", false);
            _conexionEstacionRemota = new ConexionEstacionRemota();
            firstMacAddress = NetworkInterface
        .GetAllNetworkInterfaces()
        .Where(nic => nic.OperationalStatus == OperationalStatus.Up && nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
        .Select(nic => nic.GetPhysicalAddress().ToString())
        .FirstOrDefault();
            firstMacAddress = firstMacAddress ?? "Mac Unknown";
            Console.WriteLine(GetSetting("Razon"));
            ImpresionAutomatica = GetBool("ImpresionAutomatica", false);
            impresionFormaDePagoOrdenDespacho = GetBool("impresionFormaDePagoOrdenDespacho", false);
            _infoEstacion = new InfoEstacion();
            generaFacturaElectronica = GetBool("GeneraFacturaElectronica", false);
            _infoEstacion.CaracteresPorPagina = GetInt("CaracteresPorPagina", 40);
            _infoEstacion.ImpresionPDA = GetBool("ImpresionPDA", false);
            _infoEstacion.Direccion = GetSetting("Direccion");
            _infoEstacion.Linea1 = GetSetting("Linea1");
            _infoEstacion.Linea2 = GetSetting("Linea2");
            _infoEstacion.Linea3 = GetSetting("Linea3");
            _infoEstacion.Linea4 = GetSetting("Linea4");
            _infoEstacion.NIT = GetSetting("NIT");
            _infoEstacion.Nombre = GetSetting("Nombre");
            _infoEstacion.Razon = GetSetting("Razon");
            _infoEstacion.Rifa = GetBool("Rifa", false);
            if (!decimal.TryParse(ConfigurationManager.AppSettings["MontoMinimoRifa"], NumberStyles.Any, CultureInfo.InvariantCulture, out var montoMinimoRifa)
                && !decimal.TryParse(ConfigurationManager.AppSettings["MontoMinimoRifa"], NumberStyles.Any, CultureInfo.CurrentCulture, out montoMinimoRifa))
            {
                montoMinimoRifa = 10000m;
            }
            _infoEstacion.MontoMinimoRifa = montoMinimoRifa;

            _infoEstacion.Telefono = GetSetting("Telefono");
            _infoEstacion.vecesPermitidasImpresion = GetInt("vecesPermitidasImpresion", 1);


            carasImpresoras = new Dictionary<int, string>();
            islasImpresoras = new Dictionary<string, string>();
            Console.WriteLine("Caras");
            foreach (string nameValueItem in ConfigurationManager.AppSettings)
            {
                if (nameValueItem.Contains("CARA"))
                {
                    Console.WriteLine(nameValueItem);
                    string impresora = ConfigurationManager.AppSettings[nameValueItem];
                    int isla = Int32.Parse(nameValueItem.Split(' ')[1]);
                    carasImpresoras.Add(isla, impresora);
                }
            }
            foreach (string nameValueItem in ConfigurationManager.AppSettings)
            {
                if (nameValueItem.Contains("ISLA"))
                {
                    Console.WriteLine(nameValueItem);
                    string impresora = ConfigurationManager.AppSettings[nameValueItem];
                    string isla = nameValueItem;
                    islasImpresoras.Add(isla, impresora);
                }
            }
            _estacionesRepositorio = new EstacionesRepositorioSqlServer();
            formas = _estacionesRepositorio.BuscarFormasPagos();
            imprimiendo = 0;
            _fidelizacion = new FidelizacionConexionApi();
        }
        public void Execute()
        {
            DateTime lastTimeExec = DateTime.Now.AddMinutes(-5);
            while (true)
            {
                try
                {
                    var objetoImprimir = _estacionesRepositorio.GetObjetoImprimir().FirstOrDefault();
                         
                        if (imprimiendo == 0 && objetoImprimir != null)
                        {
                            var objeto = objetoImprimir.Objeto?.Trim();
                            Logger.Warn($"Objeto a imprimir: '{objeto}' Id: {objetoImprimir.Id}");
                            try
                            {
                                switch (objeto)
                                {
                                    case "Cierre":
                                        {
                                            var turnoimprimir = _estacionesRepositorio.ObtenerTurnoIslaYFecha(objetoImprimir.fecha, objetoImprimir.Isla, objetoImprimir.Numero);
                                            if (turnoimprimir == null)
                                            {
                                                _estacionesRepositorio.ActualizarObjetoImpreso(objetoImprimir.Id);

                                            }
                                            else if (imprimiendo == 0)
                                            {
                                                imprimiendo++;
                                                Logger.Warn($"imprimirnedo {JsonConvert.SerializeObject(turnoimprimir)} ");
                                                ImprimirTurno(turnoimprimir, objetoImprimir.Isla);

                                                _estacionesRepositorio.ActualizarObjetoImpreso(objetoImprimir.Id);
                                            }
                                            else
                                            {
                                                Thread.Sleep(100);
                                            }
                                        }
                                        break;
                                    case "Reimprimir":
                                        {
                                            var turnoimprimir = _estacionesRepositorio.ObtenerTurnoIslaYFecha(objetoImprimir.fecha, objetoImprimir.Isla, objetoImprimir.Numero);
                                            if (turnoimprimir == null)
                                            {
                                                turnoimprimir = _estacionesRepositorio.ObtenerTurnoIslaYFecha(objetoImprimir.fecha.AddDays(-1), objetoImprimir.Isla, objetoImprimir.Numero);

                                            }
                                            if (turnoimprimir == null)
                                            {
                                                _estacionesRepositorio.ActualizarObjetoImpreso(objetoImprimir.Id);

                                            }
                                            else if (imprimiendo == 0)
                                            {
                                                imprimiendo++;
                                                Logger.Warn($"imprimirnedo {JsonConvert.SerializeObject(turnoimprimir)} ");
                                                ImprimirTurno(turnoimprimir, objetoImprimir.Isla);

                                                _estacionesRepositorio.ActualizarObjetoImpreso(objetoImprimir.Id);
                                            }
                                            else
                                            {
                                                Thread.Sleep(100);
                                            }
                                        }
                                        break;
                                    case "Apertura":
                                        {
                                            var turnoimprimir = _estacionesRepositorio.ObtenerTurnoPorIslaFecha(objetoImprimir.fecha, objetoImprimir.Isla);
                                            if (turnoimprimir == null)
                                            {
                                                _estacionesRepositorio.ActualizarObjetoImpreso(objetoImprimir.Id);

                                            }
                                            else if (imprimiendo == 0)
                                            {
                                                imprimiendo++;
                                                Logger.Warn($"imprimirnedo {JsonConvert.SerializeObject(turnoimprimir)} ");
                                                ImprimirTurno(turnoimprimir, objetoImprimir.Isla);

                                                _estacionesRepositorio.ActualizarObjetoImpreso(objetoImprimir.Id);
                                            }
                                            else
                                            {
                                                Thread.Sleep(100);
                                            }
                                        }
                                        break;
                                    case "Bolsa":
                                        {
                                            var bolsaimprimir = _estacionesRepositorio.ObtenerBolsa(objetoImprimir.fecha, objetoImprimir.Isla, objetoImprimir.Numero);
                                            if (bolsaimprimir.Consecutivo == 0)
                                            {
                                                bolsaimprimir = _estacionesRepositorio.ObtenerBolsa(objetoImprimir.fecha.AddDays(-1), objetoImprimir.Isla, objetoImprimir.Numero);

                                            }
                                            if (bolsaimprimir.Consecutivo == 0)
                                            {

                                                _estacionesRepositorio.ActualizarObjetoImpreso(objetoImprimir.Id);
                                            }
                                            else if (imprimiendo == 0)
                                            {
                                                imprimiendo++;
                                                Logger.Warn($"imprimirnedo {JsonConvert.SerializeObject(bolsaimprimir)} ");
                                                ImprimirBolsa(bolsaimprimir);

                                                _estacionesRepositorio.ActualizarObjetoImpreso(objetoImprimir.Id);
                                            }
                                            else
                                            {
                                                Thread.Sleep(100);
                                            }
                                        }
                                        break;
                                    case "CierreCanastilla":
                                    case "ReimprimirCierreCanastilla":
                                    case "CierreCana":
                                        {
                                            // Se espera que objetoImprimir.Isla y objetoImprimir.Numero tengan la isla y numero de turno a imprimir
                                            var isla = objetoImprimir.Isla;
                                            var turnoCerrado = _estacionesRepositorio.ObtenerTurnoIslaYFecha(objetoImprimir.fecha, isla, objetoImprimir.Numero);
                                            if (turnoCerrado == null)
                                            {
                                                Logger.Warn($"No hay turno cerrado para la isla {isla} turno {objetoImprimir.Numero}");
                                                _estacionesRepositorio.ActualizarObjetoImpreso(objetoImprimir.Id);
                                                break;
                                            }
                                            var facturas = _estacionesRepositorio.GetFacturasCanastillaPorIslaTurno(isla.ToString(), turnoCerrado.Numero, turnoCerrado.FechaAperturaJuliana);
                                            if (facturas == null || !facturas.Any())
                                            {
                                                Logger.Warn($"No hay facturas de canastilla para el cierre de la isla {isla} turno {turnoCerrado.Numero}");
                                                _estacionesRepositorio.ActualizarObjetoImpreso(objetoImprimir.Id);
                                                break;
                                            }
                                            imprimiendo++;
                                            ImprimirCierreCanastilla(isla.ToString(), turnoCerrado, facturas);
                                            _estacionesRepositorio.ActualizarObjetoImpreso(objetoImprimir.Id);
                                            break;

                                        }
                                    default:
                                        Logger.Warn($"Objeto no reconocido para imprimir: '{objeto}' (Id: {objetoImprimir.Id})");
                                        _estacionesRepositorio.ActualizarObjetoImpreso(objetoImprimir.Id);
                                        break;

                                }
                            }
                            catch (Exception exObj)
                            {
                                imprimiendo = 0;
                                Logger.Error($"Exception while processing objeto {objetoImprimir.Id}: {exObj.Message}");
                                Logger.Error($"StackTrace: {exObj.StackTrace}");
                                // Mark objeto as processed even though it failed, to prevent blocking other objects
                                _estacionesRepositorio.ActualizarObjetoImpreso(objetoImprimir.Id);
                            }
                        }

                    _estacionesRepositorio.AgregarFacturaDesdeIdVenta();
                    if (imprimiendo == 0)
                    {
                        if (lastTimeExec < DateTime.Now.AddHours(-5))
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
                        //Console.WriteLine("Buscando facturas");
                        var caras = _estacionesRepositorio.getCaras();
                        Console.WriteLine($"Caras {JsonConvert.SerializeObject(caras)}");
                        foreach (var cara in caras)
                        {

                            var facturai = _estacionesRepositorio.getUltimasFacturas(cara.COD_CAR, 1).FirstOrDefault();
                            if (ImpresionAutomatica)
                            {
                                if (facturai != null && facturai.Venta != null && facturai.Venta.CONSECUTIVO != -1
                                    && ((facturai.impresa == 0)))
                                {

                                    Console.WriteLine("Imprimiendo facturas");
                                    imprimiendo++;
                                    Imprimir(facturai);
                                    Console.WriteLine("Fin impresion");
                                    Thread.Sleep(100);
                                    break;
                                }


                            }
                        }

                        var factura = _estacionesRepositorio.getFacturasImprimir();

                        if (imprimiendo == 0 && factura != null && factura.Venta != null && factura.Venta.CONSECUTIVO != -1
                            && ((factura.impresa == 0 && ImpresionAutomatica) || factura.impresa <= -1))
                        {


                            while (true)
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
                                if (factura.impresa == 0)
                                {
                                    break;
                                }
                            }
                            Thread.Sleep(100);
                        }


                        Thread.Sleep(100);

                    }
                    else
                    {
                        Thread.Sleep(100);
                    }
                }
                catch (Exception ex)
                {

                    imprimiendo = 0;
                    Logger.Error("Error " + ex.Message);
                    Logger.Error("Error " + ex.StackTrace);
                    Thread.Sleep(100);
                }
            }
        }

        // --- Cierre Canastilla Printing ---
        private List<LineasImprimir> lineasImprimirCierreCanastilla;
        private void ImprimirCierreCanastilla(string isla, Turno turno, IEnumerable<dynamic> facturas)
        {
            try
            {
                getLineasImprimirCierreCanastilla(isla, turno, facturas);
                printFont = new Font("Console", 9);
                PrintDocument pd = new PrintDocument();
                pd.PrintPage += new PrintPageEventHandler(pd_PrintCierreCanastilla);
                pd.DefaultPageSettings.Margins.Bottom = 20;
                var printerKey = isla;
                if (!islasImpresoras.ContainsKey(printerKey))
                {
                    printerKey = $"ISLA {isla}";
                }
                if (islasImpresoras.ContainsKey(printerKey))
                {
                    Console.WriteLine("Selecionando impresora " + islasImpresoras[printerKey].Trim());
                    pd.PrinterSettings.PrinterName = islasImpresoras[printerKey].Trim();
                }
                else
                {
                    Logger.Warn($"No se encontro impresora configurada para isla {isla}. Se usara impresora por defecto.");
                }
                pd.Print();
            }
            catch (Exception ex)
            {
                imprimiendo = 0;
                Logger.Info("Error " + ex.Message);
                Logger.Info("Error " + ex.StackTrace);
                Thread.Sleep(5000);
            }
        }

        private void getLineasImprimirCierreCanastilla(string isla, Turno turno, IEnumerable<dynamic> facturas)
        {
            lineasImprimirCierreCanastilla = new List<LineasImprimir>();
            var guiones = new StringBuilder();
            guiones.Append('-', _infoEstacion.CaracteresPorPagina > 0 ? _infoEstacion.CaracteresPorPagina : 40);
            lineasImprimirCierreCanastilla.Add(new LineasImprimir(_infoEstacion.Razon, true));
            lineasImprimirCierreCanastilla.Add(new LineasImprimir("NIT             " + _infoEstacion.NIT, false));
            lineasImprimirCierreCanastilla.Add(new LineasImprimir(_infoEstacion.Nombre, false));
            lineasImprimirCierreCanastilla.Add(new LineasImprimir(_infoEstacion.Direccion, false));
            lineasImprimirCierreCanastilla.Add(new LineasImprimir(_infoEstacion.Telefono, false));
            lineasImprimirCierreCanastilla.Add(new LineasImprimir(guiones.ToString(), false));
            lineasImprimirCierreCanastilla.Add(new LineasImprimir($"Turno Canastilla Isla: {isla} Turno: {turno.Numero}Turno: {turno.Numero}", true));
             lineasImprimirCierreCanastilla.Add(new LineasImprimir($"Fecha: {turno.FechaApertura.Date}", true));
            lineasImprimirCierreCanastilla.Add(new LineasImprimir($"Vendedor: {turno.Empleado}", false));
            lineasImprimirCierreCanastilla.Add(new LineasImprimir(guiones.ToString(), false));
            
            // Build summary of canastilla products with quantities and totals
            var canastillasSummary = new Dictionary<string, (double cantidad, double total)>();
            double grandTotal = 0;
            
            foreach (var factura in facturas)
            {
                // Get canastilla details for this invoice
                var detalles = _estacionesRepositorio.getFacturaCanatillaDetalle(factura.FacturasCanastillaId);
                if (detalles != null)
                {
                    foreach (var detalle in detalles)
                    {
                        string canastillaKey = detalle.Canastilla.descripcion;
                        if (canastillasSummary.ContainsKey(canastillaKey))
                        {
                            var current = canastillasSummary[canastillaKey];
                            canastillasSummary[canastillaKey] = (
                                current.cantidad + detalle.cantidad,
                                current.total + detalle.total
                            );
                        }
                        else
                        {
                            canastillasSummary[canastillaKey] = (detalle.cantidad, detalle.total);
                        }
                        grandTotal += detalle.total;
                    }
                }
            }
            
            // Print product details header
            lineasImprimirCierreCanastilla.Add(new LineasImprimir("Producto                    Cantidad    Total", false));
            lineasImprimirCierreCanastilla.Add(new LineasImprimir(guiones.ToString(), false));
            
            // Print each canastilla product with quantity and total
            foreach (var kvp in canastillasSummary)
            {
                string linea = string.Format("{0,-27}{1,10:F0}   {2,12:N2}", 
                    kvp.Key.Length > 15 ? kvp.Key.Substring(0, 12) + "..." : kvp.Key,
                    kvp.Value.cantidad,
                    kvp.Value.total);
                lineasImprimirCierreCanastilla.Add(new LineasImprimir(linea, false));
            }
            
            lineasImprimirCierreCanastilla.Add(new LineasImprimir(guiones.ToString(), false));
            
            // Print total summary
            lineasImprimirCierreCanastilla.Add(new LineasImprimir("", false));
            lineasImprimirCierreCanastilla.Add(new LineasImprimir("RESUMEN DE FACTURAS Y FORMAS DE PAGO", true));
            lineasImprimirCierreCanastilla.Add(new LineasImprimir(guiones.ToString(), false));
            lineasImprimirCierreCanastilla.Add(new LineasImprimir("Factura   Total        Forma de Pago", false));
            lineasImprimirCierreCanastilla.Add(new LineasImprimir(guiones.ToString(), false));
            
            foreach (var f in facturas)
            {
                // Lookup payment method description from formas list
                var formaPago = formas?.FirstOrDefault(x => x.Id == f.codigoFormaPago.Id);
                string formaPagoDescripcion = formaPago?.Descripcion ?? "Sin registro";
                string linea = string.Format("{0,-12}{1,10:N2}   {2}", f.consecutivo, f.total, formaPagoDescripcion);
                lineasImprimirCierreCanastilla.Add(new LineasImprimir(linea, false));
            }
            
            lineasImprimirCierreCanastilla.Add(new LineasImprimir(guiones.ToString(), false));
            lineasImprimirCierreCanastilla.Add(new LineasImprimir(string.Format("TOTAL GENERAL:                        {0,12:N2}", grandTotal), true));
            lineasImprimirCierreCanastilla.Add(new LineasImprimir(guiones.ToString(), false));
            lineasImprimirCierreCanastilla.Add(new LineasImprimir("", false));
            lineasImprimirCierreCanastilla.Add(new LineasImprimir("Fabricado por: SIGES SOLUCIONES SAS ", true));
            lineasImprimirCierreCanastilla.Add(new LineasImprimir("Nit: 901430393-2 ", true));
            lineasImprimirCierreCanastilla.Add(new LineasImprimir("Nombre: Facturador SIGES ", true));
            lineasImprimirCierreCanastilla.Add(new LineasImprimir(formatoTotales("SERIAL MAQUINA: ", firstMacAddress ?? ""), false));
            lineasImprimirCierreCanastilla.Add(new LineasImprimir(".", true));
        }

        private void pd_PrintCierreCanastilla(object sender, PrintPageEventArgs ev)
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
                int caracteresPorPagina = _infoEstacion.CaracteresPorPagina > 0 ? _infoEstacion.CaracteresPorPagina : fonSizeInches * sizePaper / 100;
                foreach (var linea in lineasImprimirCierreCanastilla)
                {
                    count = printLine(linea.linea, ev, count, leftMargin, topMargin, linea.centrada);
                }
                count = printLine(" ", ev, count, leftMargin, topMargin, false);
                count = printLine(" ", ev, count, leftMargin, topMargin, false);
                ev.HasMorePages = false;
                imprimiendo--;
            }
            catch (Exception ex)
            {
                imprimiendo = 0;
                Logger.Info("Error " + ex.Message);
                Logger.Info("Error " + ex.StackTrace);
                Thread.Sleep(5000);
            }
        }

        private List<LineasImprimir> lineasImprimirBolsa;
        private void ImprimirBolsa(Models.Bolsa bolsaimprimir)
        {
            try
            {

                getLineasImprimirTurnoBolsa(bolsaimprimir);
                try
                {
                    printFont = new Font("Console", 9);
                    PrintDocument pd = new PrintDocument();
                    pd.PrintPage += new PrintPageEventHandler(pd_PrintBolsa);
                    pd.DefaultPageSettings.Margins.Bottom = 20;
                    if (bolsaimprimir != null && bolsaimprimir.Isla != null && islasImpresoras.ContainsKey(bolsaimprimir.Isla))
                    {

                        Console.WriteLine("Selecionando impresora " + islasImpresoras[bolsaimprimir.Isla].Trim());
                        pd.PrinterSettings.PrinterName = islasImpresoras[bolsaimprimir.Isla].Trim();
                    }
                    pd.Print();

                }
                catch (Exception ex)
                {

                    printFont = new Font("Console", 9);
                    PrintDocument pd = new PrintDocument();
                    pd.PrintPage += new PrintPageEventHandler(pd_PrintBolsa);
                    pd.DefaultPageSettings.Margins.Bottom = 20;
                    pd.Print();

                }
            }
            catch (Exception ex)
            {
                imprimiendo = 0;
                Logger.Info("Error " + ex.Message);
                Logger.Info("Error " + ex.StackTrace);
                Thread.Sleep(5000);
            }
        }

        private void getLineasImprimirTurnoBolsa(Bolsa bolsaimprimir)
        {
            lineasImprimirBolsa = new List<LineasImprimir>();
            var guiones = new StringBuilder();
            guiones.Append('-', _infoEstacion.CaracteresPorPagina);
            // Iterate over the file, printing each line.
            lineasImprimirBolsa.Add(new LineasImprimir(".", true));
            lineasImprimirBolsa.Add(new LineasImprimir(_infoEstacion.Razon, true));
            lineasImprimirBolsa.Add(new LineasImprimir("NIT             " + _infoEstacion.NIT, false));
            lineasImprimirBolsa.Add(new LineasImprimir(_infoEstacion.Nombre, false));
            lineasImprimirBolsa.Add(new LineasImprimir(_infoEstacion.Direccion, false));
            lineasImprimirBolsa.Add(new LineasImprimir(_infoEstacion.Telefono, false));
            lineasImprimirBolsa.Add(new LineasImprimir(guiones.ToString(), false));

            lineasImprimirBolsa.Add(new LineasImprimir("Isla: " + bolsaimprimir.Isla, false));
            lineasImprimirBolsa.Add(new LineasImprimir("Turno: " + bolsaimprimir.NumeroTurno, false));
            lineasImprimirBolsa.Add(new LineasImprimir("Fecha: " + bolsaimprimir.Fecha, false));
            lineasImprimirBolsa.Add(new LineasImprimir("Empleado: " + bolsaimprimir.Empleado, false));
            lineasImprimirBolsa.Add(new LineasImprimir("Consecutivo: " + bolsaimprimir.Consecutivo, false));
            lineasImprimirBolsa.Add(new LineasImprimir("Bilete: " + bolsaimprimir.Billete, false));
            lineasImprimirBolsa.Add(new LineasImprimir("Moneda: " + bolsaimprimir.Moneda, false));

            lineasImprimirBolsa.Add(new LineasImprimir(guiones.ToString(), false));

            lineasImprimirBolsa.Add(new LineasImprimir("Fabricado por:" + " SIGES SOLUCIONES SAS ", true));
            lineasImprimirBolsa.Add(new LineasImprimir("Nit:" + " 901430393-2 ", true));
            lineasImprimirBolsa.Add(new LineasImprimir("Nombre:" + " Facturador SIGES ", true));
            lineasImprimirBolsa.Add(new LineasImprimir(formatoTotales("SERIAL MAQUINA: ", firstMacAddress ?? ""), false));
            lineasImprimirBolsa.Add(new LineasImprimir(".", true));
        }

        private List<LineasImprimir> lineasImprimirTurno;
        private void ImprimirTurno(Turno turnoimprimir, int isla)
        {

            try
            {

                getLineasImprimirTurno(turnoimprimir, isla);
                try
                {
                    printFont = new Font("Console", 9);
                    PrintDocument pd = new PrintDocument();
                    pd.PrintPage += new PrintPageEventHandler(pd_PrintTurno);
                    pd.DefaultPageSettings.Margins.Bottom = 20;
                    if (turnoimprimir != null && turnoimprimir.Isla != null && islasImpresoras.ContainsKey(turnoimprimir.Isla))
                    {

                        Console.WriteLine("Selecionando impresora " + islasImpresoras[turnoimprimir.Isla].Trim());
                        pd.PrinterSettings.PrinterName = islasImpresoras[turnoimprimir.Isla].Trim();
                    }
                    pd.Print();

                }
                catch (Exception ex)
                {

                    printFont = new Font("Console", 9);
                    PrintDocument pd = new PrintDocument();
                    pd.PrintPage += new PrintPageEventHandler(pd_PrintTurno);
                    pd.DefaultPageSettings.Margins.Bottom = 20;
                    pd.Print();

                }
            }
            catch (Exception ex)
            {
                imprimiendo = 0;
                Logger.Info("Error " + ex.Message);
                Logger.Info("Error " + ex.StackTrace);
                Thread.Sleep(5000);
            }
        }

        private void getLineasImprimirTurno(Turno turnoimprimir, int isla)
        {
            lineasImprimirTurno = new List<LineasImprimir>();
            var guiones = new StringBuilder();
            guiones.Append('-', _infoEstacion.CaracteresPorPagina);
            // Iterate over the file, printing each line.
            lineasImprimirTurno.Add(new LineasImprimir(".", true));
            lineasImprimirTurno.Add(new LineasImprimir(_infoEstacion.Razon, true));
            lineasImprimirTurno.Add(new LineasImprimir("NIT             " + _infoEstacion.NIT, false));
            lineasImprimirTurno.Add(new LineasImprimir(_infoEstacion.Nombre, false));
            lineasImprimirTurno.Add(new LineasImprimir(_infoEstacion.Direccion, false));
            lineasImprimirTurno.Add(new LineasImprimir(_infoEstacion.Telefono, false));
            lineasImprimirTurno.Add(new LineasImprimir(guiones.ToString(), false));
            lineasImprimirTurno.Add(new LineasImprimir("Empleado:       " + turnoimprimir.Empleado, false));
            lineasImprimirTurno.Add(new LineasImprimir("Isla:           " + turnoimprimir.Isla, false));
            lineasImprimirTurno.Add(new LineasImprimir("Numero:           " + turnoimprimir.Numero, false));
            lineasImprimirTurno.Add(new LineasImprimir("Fecha apertura: " + turnoimprimir.FechaApertura.ToString(), false));
            var reporteCierrePorTotal = new List<FactoradorEstacionesModelo.Objetos.Factura>();
            if (turnoimprimir.FechaCierre.HasValue)
            {
                lineasImprimirTurno.Add(new LineasImprimir("Fecha cierre:   " + turnoimprimir.FechaCierre.Value.ToString(), false));
                reporteCierrePorTotal = _estacionesRepositorio.getFacturaPorTurno(isla, turnoimprimir.FechaApertura, turnoimprimir.Numero).ToList();
            }


            var totalCantidad = 0m;
            var totalVenta = 0m;

            lineasImprimirTurno.Add(new LineasImprimir(guiones.ToString(), false));
            foreach (var turnosurtidor in turnoimprimir.turnoSurtidores)
            {
                if (MultiplicarPor10)
                {
                    turnosurtidor.precioCombustible *= 10;
                }
                lineasImprimirTurno.Add(new LineasImprimir(formatoTotales("Manguera :", turnosurtidor.Manguera), false));
                lineasImprimirTurno.Add(new LineasImprimir(formatoTotales("Combustible :", turnosurtidor.Combustible), false));
                lineasImprimirTurno.Add(new LineasImprimir(formatoTotales("Precio :", $"${string.Format("{0:N2}", turnosurtidor.precioCombustible)}"), false));
                lineasImprimirTurno.Add(new LineasImprimir(formatoTotales("Apertura :", turnosurtidor.Apertura.ToString()), false));
                if (turnoimprimir.FechaCierre.HasValue)
                {
                    lineasImprimirTurno.Add(new LineasImprimir(formatoTotales("Cierre :", turnosurtidor.Cierre.ToString()), false));

                    Logger.Info("reporte " + JsonConvert.SerializeObject(reporteCierrePorTotal));
                    Logger.Info("turno surtidor " + JsonConvert.SerializeObject(turnosurtidor));
                    totalCantidad += Convert.ToDecimal(turnosurtidor.Cierre.Value - turnosurtidor.Apertura);
                    totalVenta += Convert.ToDecimal((turnosurtidor.Cierre.Value - turnosurtidor.Apertura) * turnosurtidor.precioCombustible);
                    lineasImprimirTurno.Add(new LineasImprimir(formatoTotales("Cantidad :", string.Format("{0:N2}", turnosurtidor.Cierre - turnosurtidor.Apertura)), false));
                    lineasImprimirTurno.Add(new LineasImprimir(formatoTotales("Total :", $"${string.Format("{0:N2}", (turnosurtidor.Cierre - turnosurtidor.Apertura) * turnosurtidor.precioCombustible)}"), false));

                }

                lineasImprimirTurno.Add(new LineasImprimir(guiones.ToString(), false));
            }
            if (turnoimprimir.FechaCierre.HasValue)
            {
                if (reporteCierrePorTotal != null && reporteCierrePorTotal.Any())
                {

                    //Por forma
                    lineasImprimirTurno.Add(new LineasImprimir($"Resumen por forma de pago", true));
                    lineasImprimirTurno.Add(new LineasImprimir(guiones.ToString(), false));
                    var groupForma = reporteCierrePorTotal.GroupBy(x => x.codigoFormaPago);
                    Logger.Info("facturas turno " + JsonConvert.SerializeObject(groupForma));

                    var cantidadTotalmenosEfectivo = 0m;
                    var ventaTotalmenosEfectivo = 0m;
                    foreach (var forma in groupForma)
                    {
                        if (formas.Any(x => x.Id == forma.Key) && forma.Key != 1)
                        {
                            cantidadTotalmenosEfectivo += forma.Sum(x => x.Venta.CANTIDAD);
                            ventaTotalmenosEfectivo += forma.Sum(x => x.Venta.TOTAL);
                            lineasImprimirTurno.Add(new LineasImprimir(formatoTotales($"{formas.First(x => x.Id == forma.Key).Descripcion.Trim()} :", $"${string.Format("{0:N2}", forma.Sum(x => x.Venta.TOTAL))}"), false));

                        }
                    }

                    lineasImprimirTurno.Add(new LineasImprimir(formatoTotales("Total :", $"${string.Format("{0:N2}", totalVenta)}"), false));


                    lineasImprimirTurno.Add(new LineasImprimir(guiones.ToString(), false));

                    lineasImprimirTurno.Add(new LineasImprimir($"Resumen por Combustibles", true));
                    //Totalizador
                    lineasImprimirTurno.Add(new LineasImprimir(guiones.ToString(), false));
                    var porCombustible = reporteCierrePorTotal.GroupBy(x => x.Venta.Combustible);
                    foreach (var combustible in porCombustible)
                    {
                        var total = combustible.Sum(x => x.Venta.SUBTOTAL);
                        lineasImprimirTurno.Add(new LineasImprimir(formatoTotales("Combustible :", combustible.Key), false));
                        lineasImprimirTurno.Add(new LineasImprimir(formatoTotales("Precio :", $"${string.Format("{0:N2}", combustible.First().Venta.PRECIO_UNI)}"), false));
                        lineasImprimirTurno.Add(new LineasImprimir(formatoTotales("Subtotal :", $"${string.Format("{0:N2}", total)}"), false));
                        lineasImprimirTurno.Add(new LineasImprimir(formatoTotales("Calibracion :", "$0,00"), false));
                        lineasImprimirTurno.Add(new LineasImprimir(formatoTotales("Descuento :", $"${string.Format("{0:N2}", total)}"), false));
                        lineasImprimirTurno.Add(new LineasImprimir(formatoTotales("Total :", $"${string.Format("{0:N2}", total)}"), false));


                        lineasImprimirTurno.Add(new LineasImprimir(guiones.ToString(), false));
                    }

                    lineasImprimirTurno.Add(new LineasImprimir($"Resumen de bolsas", true));
                    //Totalizador
                    lineasImprimirTurno.Add(new LineasImprimir(guiones.ToString(), false));
                    lineasImprimirTurno.Add(new LineasImprimir(formatoTotales("Numero de bolsas :", turnoimprimir.Bolsas.Count().ToString()), false));
                    lineasImprimirTurno.Add(new LineasImprimir(formatoTotales("Total billetes :", turnoimprimir.Bolsas.Sum(x => x.Billete).ToString()), false));
                    lineasImprimirTurno.Add(new LineasImprimir(formatoTotales("Total monedas :", turnoimprimir.Bolsas.Sum(x => x.Moneda).ToString()), false));

                    lineasImprimirTurno.Add(new LineasImprimir(guiones.ToString(), false));
                }

            }

            lineasImprimirTurno.Add(new LineasImprimir("Fabricado por:" + " SIGES SOLUCIONES SAS ", true));
            lineasImprimirTurno.Add(new LineasImprimir("Nit:" + " 901430393-2 ", true));
            lineasImprimirTurno.Add(new LineasImprimir("Nombre:" + " Facturador SIGES ", true));
            lineasImprimirTurno.Add(new LineasImprimir(formatoTotales("SERIAL MAQUINA: ", firstMacAddress ?? ""), false));
            lineasImprimirTurno.Add(new LineasImprimir(".", true));
        }

        private Font printFont;
        private FactoradorEstacionesModelo.Objetos.Factura _factura;
        private Venta _venta;
        private FactoradorEstacionesModelo.Objetos.Tercero _tercero;
        private Manguera _mangueras;
        private InfoEstacion _infoEstacion;
        private int _charactersPerPage;
        private EstacionesRepositorioSqlServer _estacionesRepositorio;
        private List<FormasPagos> formas;
        private List<LineasImprimir> lineasImprimir;

        private IEnumerable<LineasImprimir> getPuntos(int ventaId)
        {
            var fidelizado = _estacionesRepositorio.getFidelizado(ventaId);
            if (fidelizado != null)
            {
                fidelizado = _fidelizacion.GetFidelizados(fidelizado.Documento).Result != null ? _fidelizacion.GetFidelizados(fidelizado.Documento).Result.FirstOrDefault() : fidelizado;
                if (fidelizado != null)
                {
                    return new List<LineasImprimir>() {
                    new LineasImprimir(formatoTotales("Fidelizado:", fidelizado.Nombre??fidelizado.Documento), false)
                , new LineasImprimir(formatoTotales("Puntos:", fidelizado.Puntos.ToString()), false)};
                }
            }
            return new List<LineasImprimir>() { new LineasImprimir("Usuario no fidelizado", false) };


        }
        private void Imprimir(FactoradorEstacionesModelo.Objetos.Factura factura)
        {
            _factura = factura;
            _venta = factura.Venta;
            _tercero = factura.Tercero;
            _mangueras = factura.Manguera;
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
                    if (_venta != null && _venta.COD_CAR != null && carasImpresoras.ContainsKey(_venta.COD_CAR))
                    {

                        Console.WriteLine("Selecionando impresora " + carasImpresoras[_venta.COD_CAR].Trim());
                        pd.PrinterSettings.PrinterName = carasImpresoras[_venta.COD_CAR].Trim();
                    }

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
            var nombreTercero = ObtenerNombreCompletoTercero(_tercero);
            if (_factura.codigoFormaPago == 0)
            {
                _factura.codigoFormaPago = _venta.COD_FOR_PAG;
            }
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

            try
            {
                var intentos = 0;
                do
                {
                    infoTemp = _conexionEstacionRemota.GetInfoFacturaElectronica(_factura.ventaId, estacionFuente, _conexionEstacionRemota.getToken());
                    Thread.Sleep(100);
                } while (infoTemp == null || intentos++ < 3);

                Console.WriteLine("info fac elec " + infoTemp);
                Logger.Info("info fac elec " + infoTemp);
            }
            catch (Exception ex)
            {
                Logger.Info("info fac elec " + ex.Message);
                Logger.Info("info fac elec " + ex.StackTrace);
                Console.WriteLine("info fac elec " + ex.Message);
                Console.WriteLine("info fac elec " + ex.StackTrace);
            }

            if (!string.IsNullOrEmpty(infoTemp))
            {
                infoTemp = infoTemp.Replace("\n\r", " ");

                var facturaElectronica = infoTemp.Split(' ');

                lineasImprimir.Add(new LineasImprimir("Factura Electrónica de Venta " + facturaElectronica[2], true));
                lineasImprimir.Add(new LineasImprimir(facturaElectronica[3], true));
                lineasImprimir.Add(new LineasImprimir(facturaElectronica[4].Substring(0, facturaElectronica[4].Length / 3), true));
                lineasImprimir.Add(new LineasImprimir(facturaElectronica[4].Substring(facturaElectronica[4].Length / 3, facturaElectronica[4].Length * 2 / 3), true));
                lineasImprimir.Add(new LineasImprimir(facturaElectronica[4].Substring(facturaElectronica[4].Length * 2 / 3), true));
                lineasImprimir.Add(new LineasImprimir(guiones.ToString(), false));
                lineasImprimir.Add(new LineasImprimir("Venta: " + _venta.CONSECUTIVO, false));

            }
            else if (_factura.Consecutivo == 0)
            {

                lineasImprimir.Add(new LineasImprimir("Orden de despacho No: " + _venta.CONSECUTIVO, true));
            }
            else
            {
                lineasImprimir.Add(new LineasImprimir("Orden de Servicio Temporal: " + _venta.CONSECUTIVO, true));
            }

            lineasImprimir.Add(new LineasImprimir(guiones.ToString(), false));
            var placa = (!string.IsNullOrEmpty(_factura.Placa) ? _factura.Placa : _venta.PLACA + "").Trim();
            if (_venta.COD_FOR_PAG != 4)
            {

                lineasImprimir.Add(new LineasImprimir(formatoTotales("Vendido a : ", nombreTercero), false));
                lineasImprimir.Add(new LineasImprimir(formatoTotales("Nit/C.C. : ", _tercero.identificacion.Trim()), false));
                lineasImprimir.Add(new LineasImprimir(formatoTotales("Placa : ", placa), false));
                lineasImprimir.Add(new LineasImprimir(formatoTotales("Kilometraje : ", (!string.IsNullOrEmpty(_factura.Kilometraje) ? _factura.Kilometraje : _venta.KILOMETRAJE + "").Trim()), false));
                var codigoInterno = string.IsNullOrEmpty(_factura.Venta.COD_INT) ? _estacionesRepositorio.ObtenerCodigoInterno(placa, _tercero.identificacion.Trim()) : _factura.Venta.COD_INT;
                if (codigoInterno != null)
                {
                    lineasImprimir.Add(new LineasImprimir(formatoTotales("Cod Int : ", codigoInterno), false));
                }
            }
            else
            {
                if (string.IsNullOrEmpty(nombreTercero))
                {
                    lineasImprimir.Add(new LineasImprimir(formatoTotales("Vendido a :", " CONSUMIDOR FINAL".Trim()), false));
                }
                else
                {
                    lineasImprimir.Add(new LineasImprimir(formatoTotales("Vendido a : ", nombreTercero) + "", false));
                }
                if (string.IsNullOrEmpty(_tercero.identificacion))
                {
                    lineasImprimir.Add(new LineasImprimir(formatoTotales("Nit/C.C. : ", "222222222222".Trim()), false));
                }
                else
                {
                    lineasImprimir.Add(new LineasImprimir(formatoTotales("Nit/C.C. : ", _tercero.identificacion.Trim()), false));
                }
                lineasImprimir.Add(new LineasImprimir(formatoTotales("Placa : ", (!string.IsNullOrEmpty(_factura.Placa) ? _factura.Placa : _venta.PLACA + "").Trim()), false));
                lineasImprimir.Add(new LineasImprimir(formatoTotales("Kilometraje : ", (!string.IsNullOrEmpty(_factura.Kilometraje) ? _factura.Kilometraje : _venta.KILOMETRAJE + "").Trim()), false));
                var codigoInterno = _factura.Venta.COD_INT != null ? _factura.Venta.COD_INT : _estacionesRepositorio.ObtenerCodigoInterno(placa, _tercero.identificacion.Trim());
                if (codigoInterno != null)
                {
                    lineasImprimir.Add(new LineasImprimir(formatoTotales("Cod Int : ", codigoInterno), false));
                }
            }

            if (_venta.FECH_PRMA.HasValue && (_mangueras.DESCRIPCION.ToLower().Contains("gn") || _mangueras.DESCRIPCION.ToLower().Contains("gas")))
            {
                lineasImprimir.Add(new LineasImprimir(formatoTotales("Proximo mantenimiento : ", _venta.FECH_PRMA.Value.ToString("dd/MM/yyyy").Trim()), false));
            }

            lineasImprimir.Add(new LineasImprimir(guiones.ToString(), false));
            lineasImprimir.Add(new LineasImprimir(formatoTotales("Fecha : ", _factura.fecha.ToString("dd/MM/yyyy HH:mm:ss")), false));

            lineasImprimir.Add(new LineasImprimir(formatoTotales("Surtidor : ", _venta.COD_SUR + ""), false));
            lineasImprimir.Add(new LineasImprimir(formatoTotales("Cara : ", _venta.COD_CAR + ""), false));
            lineasImprimir.Add(new LineasImprimir(formatoTotales("Manguera : ", _mangueras.COD_MAN + ""), false));
            lineasImprimir.Add(new LineasImprimir(formatoTotales("Vendedor : ", _venta.EMPLEADO.Trim() + ""), false));
            lineasImprimir.Add(new LineasImprimir(guiones.ToString(), false));
            if (MultiplicarPor10)
            {
                _venta.PRECIO_UNI = _venta.PRECIO_UNI * 10;
                _venta.VALORNETO = _venta.VALORNETO * 10;
                _venta.VALORNETO = _venta.TOTAL * 10;
                _venta.Descuento = _venta.Descuento * 10;
            }
            if (_infoEstacion.ImpresionPDA)
            {
                lineasImprimir.Add(new LineasImprimir($"Producto: {_mangueras.DESCRIPCION.Trim()}", false));
                lineasImprimir.Add(new LineasImprimir($"Cantidad: {string.Format("{0:#,0.000}", _venta.CANTIDAD)}", false));
                lineasImprimir.Add(new LineasImprimir($"Precio: {_venta.PRECIO_UNI.ToString("F")}", false));
                lineasImprimir.Add(new LineasImprimir($"Total: {_venta.VALORNETO}", false));

            }
            else
            {
                lineasImprimir.Add(new LineasImprimir(getLienaTarifas("Producto", "   Cant.", "  Precio", "   Total") + "", false));
                lineasImprimir.Add(new LineasImprimir(getLienaTarifas(_mangueras.DESCRIPCION.Trim(), String.Format("{0:#,0.000}", _venta.CANTIDAD), _venta.PRECIO_UNI.ToString("F"), String.Format("{0:#,0.00}", _venta.VALORNETO), true) + "", false));
            }
            lineasImprimir.Add(new LineasImprimir(guiones.ToString() + "", false));
            lineasImprimir.Add(new LineasImprimir("DISCRIMINACION TARIFAS IVA" + "", true));
            //  lineasImprimir.Add(new LineasImprimir(guiones.ToString() + "", false));
            if (_infoEstacion.ImpresionPDA)
            {
                lineasImprimir.Add(new LineasImprimir($"Producto: {_mangueras.DESCRIPCION.Trim()}", false));
                lineasImprimir.Add(new LineasImprimir($"Cantidad: {string.Format("{0:#,0.000}", _venta.CANTIDAD)}", false));
                lineasImprimir.Add(new LineasImprimir($"Tafira: 0 % ", false));
                lineasImprimir.Add(new LineasImprimir($"Total: {_venta.VALORNETO}", false));

            }
            else
            {
                lineasImprimir.Add(new LineasImprimir(getLienaTarifas("Producto", "   Cant.", "  Tafira", "   Total") + "", false));
                lineasImprimir.Add(new LineasImprimir(getLienaTarifas(_mangueras.DESCRIPCION.Trim(), String.Format("{0:#,0.000}", _venta.CANTIDAD), "0%", String.Format("{0:#,0.00}", _venta.VALORNETO), true) + "", false));
            }
            lineasImprimir.Add(new LineasImprimir(guiones.ToString(), false));
            lineasImprimir.Add(new LineasImprimir(formatoTotales("Descuento: ", String.Format("{0:#,0.00}", _venta.Descuento)), false));
            //lineasImprimir.Add(new LineasImprimir(guiones.ToString(), false));
            lineasImprimir.Add(new LineasImprimir(formatoTotales("Subtotal sin IVA : ", String.Format("{0:#,0.00}", _venta.TOTAL)), false));
            //lineasImprimir.Add(new LineasImprimir(guiones.ToString(), false));
            lineasImprimir.Add(new LineasImprimir(formatoTotales("Subtotal IVA :", "0,00"), false));
            //lineasImprimir.Add(new LineasImprimir(guiones.ToString(), false));
            lineasImprimir.Add(new LineasImprimir(formatoTotales("TOTAL : ", String.Format("{0:#,0.00}", _venta.TOTAL)), false));
            //lineasImprimir.Add(new LineasImprimir(guiones.ToString(), false));

            if (_factura.Consecutivo != 0 || impresionFormaDePagoOrdenDespacho)
            {
                var usaMultipago = _factura.codigoFormaPago2.HasValue;
                if (formas.FirstOrDefault(x => x.Id == _factura.codigoFormaPago) != null)
                {
                    var forma = formas.FirstOrDefault(x => x.Id == _factura.codigoFormaPago);
                    if (forma.Descripcion.ToLower().Contains("cr"))
                    {
                        lineasImprimir.Add(new LineasImprimir(formatoTotales("Forma de pago :", " Credito"), false));
                    }
                    else
                    {
                        lineasImprimir.Add(new LineasImprimir(formatoTotales("Forma de pago :", " Contado"), false));

                    }
                    lineasImprimir.Add(new LineasImprimir(formatoTotales("Método de pago : ", forma.Descripcion.Trim()), false));
                    if (usaMultipago && _factura.total1.HasValue)
                    {
                        lineasImprimir.Add(new LineasImprimir(formatoTotales("Valor pago 1 : ", String.Format("{0:#,0.00}", _factura.total1.Value)), false));
                    }

                }
                else
                {
                    lineasImprimir.Add(new LineasImprimir(formatoTotales("Forma de pago :", " Contado"), false));
                    lineasImprimir.Add(new LineasImprimir(formatoTotales("Método de pago :", " Efectivo"), false));
                    if (usaMultipago && _factura.total1.HasValue)
                    {
                        lineasImprimir.Add(new LineasImprimir(formatoTotales("Valor pago 1 : ", String.Format("{0:#,0.00}", _factura.total1.Value)), false));
                    }

                }

                if (usaMultipago)
                {
                    var forma2 = formas.FirstOrDefault(x => x.Id == _factura.codigoFormaPago2.Value);
                    lineasImprimir.Add(new LineasImprimir(formatoTotales("Método pago 2 : ", (forma2?.Descripcion ?? "No definido").Trim()), false));
                    if (_factura.total2.HasValue)
                    {
                        lineasImprimir.Add(new LineasImprimir(formatoTotales("Valor pago 2 : ", String.Format("{0:#,0.00}", _factura.total2.Value)), false));
                    }

                }
                if (!string.IsNullOrEmpty(_factura.numeroTransaccion) && _factura.numeroTransaccion != "NA")
                {
                    lineasImprimir.Add(new LineasImprimir(formatoTotales("N Tran :", _factura.numeroTransaccion), false));
                }

            }

            if (!string.IsNullOrEmpty(infoTemp))
            {

                try
                {
                    var resoluconElectronica = _conexionEstacionRemota.GetResolucionElectronica(_conexionEstacionRemota.getToken());

                    lineasImprimir.Add(new LineasImprimir(guiones.ToString(), false));

                    // Dividir el texto de resolución en líneas de máximo 40 caracteres
                    var lineasResolucion = DividirTextoEnLineas(resoluconElectronica.invoiceText, _charactersPerPage);
                    foreach (var lineaResolucion in lineasResolucion)
                    {
                        lineasImprimir.Add(new LineasImprimir(lineaResolucion, false));
                    }
                }
                catch (Exception)
                {
                    lineasImprimir.Add(new LineasImprimir(guiones.ToString(), false));
                    lineasImprimir.Add(new LineasImprimir("Modalidad Factura Electrónica ", false));
                }

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


            var esCredito = _factura.codigoFormaPago == 6
                || (_factura.codigoFormaPago2.HasValue && _factura.codigoFormaPago2.Value == 6)
                || _venta.COD_FOR_PAG == 6;

            if (_infoEstacion.Rifa && !esCredito && _venta.TOTAL >= _infoEstacion.MontoMinimoRifa)
            {
                lineasImprimir.Add(new LineasImprimir(" ", false));
                lineasImprimir.Add(new LineasImprimir(" ", false));
                lineasImprimir.Add(new LineasImprimir(guiones.ToString(), false));

                lineasImprimir.Add(new LineasImprimir(" ", false));
                lineasImprimir.Add(new LineasImprimir(guiones.ToString(), false));
                lineasImprimir.Add(new LineasImprimir("CÉDULA", false));

                lineasImprimir.Add(new LineasImprimir(" ", false));
                lineasImprimir.Add(new LineasImprimir(guiones.ToString(), false));
                lineasImprimir.Add(new LineasImprimir("CEL/TEL", false));

                lineasImprimir.Add(new LineasImprimir(" ", false));
                lineasImprimir.Add(new LineasImprimir(guiones.ToString(), false));
                lineasImprimir.Add(new LineasImprimir("NRO RECIBO DE TANQUEO ", false));

                lineasImprimir.Add(new LineasImprimir(" ", false));
                lineasImprimir.Add(new LineasImprimir(guiones.ToString(), false));
                lineasImprimir.Add(new LineasImprimir($"FECHA DE TANQUEO {DateTime.Now} ", false));
                lineasImprimir.Add(new LineasImprimir(guiones.ToString(), false));
            }
            lineasImprimir.Add(new LineasImprimir(".", true));
            if (!string.IsNullOrEmpty(infoTemp))
            {

                var facturaElectronica = infoTemp.Split(' ');
                lineasImprimir.Add(new LineasImprimir(guiones.ToString(), false, $"https://catalogo-vpfe.dian.gov.co/User/SearchDocument?DocumentKey={facturaElectronica[4]}"));
            }
        }

        private string ObtenerNombreCompletoTercero(FactoradorEstacionesModelo.Objetos.Tercero tercero)
        {
            var nombre = tercero?.Nombre?.Trim() ?? string.Empty;
            var apellidos = tercero?.Apellidos?.Trim() ?? string.Empty;
            return string.Join(" ", new[] { nombre, apellidos }.Where(x => !string.IsNullOrWhiteSpace(x))).Trim();
        }

        private void pd_PrintPageOnly(object sender, PrintPageEventArgs ev)
        {
            try
            {
                if (_factura.codigoFormaPago == 0)
                {
                    _factura.codigoFormaPago = _venta.COD_FOR_PAG;
                }
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


                _estacionesRepositorio.SetFacturaImpresa(_factura.ventaId);
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


        private void pd_PrintBolsa(object sender, PrintPageEventArgs ev)
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
                if (_infoEstacion.CaracteresPorPagina == 0)
                {
                    _infoEstacion.CaracteresPorPagina = fonSizeInches * sizePaper / 100;
                }
                foreach (var linea in lineasImprimirBolsa)
                {

                    count = printLine(linea.linea, ev, count, leftMargin, topMargin, linea.centrada);
                }
                count = printLine(" ", ev, count, leftMargin, topMargin, false);
                count = printLine(" ", ev, count, leftMargin, topMargin, false);
                if (line != null)
                    ev.HasMorePages = true;
                else
                    ev.HasMorePages = false;


                imprimiendo--;
                if (line != null)
                    ev.HasMorePages = true;
                else
                    ev.HasMorePages = false;
            }
            catch (Exception ex)
            {
                imprimiendo = 0;
                Logger.Info("Error " + ex.Message);
                Logger.Info("Error " + ex.StackTrace);
                Thread.Sleep(5000);
            }

        }

        private void pd_PrintTurno(object sender, PrintPageEventArgs ev)
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
                if (_infoEstacion.CaracteresPorPagina == 0)
                {
                    _infoEstacion.CaracteresPorPagina = fonSizeInches * sizePaper / 100;
                }
                foreach (var linea in lineasImprimirTurno)
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


                imprimiendo--;
                if (line != null)
                    ev.HasMorePages = true;
                else
                    ev.HasMorePages = false;
            }
            catch (Exception ex)
            {
                imprimiendo = 0;
                Logger.Info("Error " + ex.Message);
                Logger.Info("Error " + ex.StackTrace);
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

        private List<string> DividirTextoEnLineas(string texto, int maxCaracteres)
        {
            var lineas = new List<string>();

            if (string.IsNullOrEmpty(texto))
                return lineas;

            // Dividir por palabras para evitar cortar palabras en el medio
            var palabras = texto.Split(' ');
            var palabrasLineaActual = new List<string>();

            foreach (var palabra in palabras)
            {
                // Calcular longitud total si agregamos esta palabra (incluyendo espacios mínimos)
                var longitudConPalabra = palabrasLineaActual.Sum(p => p.Length) +
                                        (palabrasLineaActual.Count > 0 ? palabrasLineaActual.Count : 0) +
                                        palabra.Length;

                // Si agregar la palabra excede el límite de caracteres
                if (longitudConPalabra > maxCaracteres && palabrasLineaActual.Count > 0)
                {
                    // Justificar la línea actual a exactamente maxCaracteres
                    var lineaJustificada = JustificarLinea(palabrasLineaActual, maxCaracteres);
                    lineas.Add(lineaJustificada);
                    palabrasLineaActual.Clear();

                    // Si la palabra sola es más larga que el límite, dividirla
                    if (palabra.Length > maxCaracteres)
                    {
                        for (int i = 0; i < palabra.Length; i += maxCaracteres)
                        {
                            var fragmento = palabra.Substring(i, Math.Min(maxCaracteres, palabra.Length - i));
                            lineas.Add(fragmento.PadRight(maxCaracteres));
                        }
                    }
                    else
                    {
                        palabrasLineaActual.Add(palabra);
                    }
                }
                else
                {
                    palabrasLineaActual.Add(palabra);
                }
            }

            // Justificar la última línea si no está vacía
            if (palabrasLineaActual.Count > 0)
            {
                var ultimaLinea = JustificarLinea(palabrasLineaActual, maxCaracteres);
                lineas.Add(ultimaLinea);
            }

            return lineas;
        }

        private string JustificarLinea(List<string> palabras, int maxCaracteres)
        {
            if (palabras.Count == 0)
                return "";

            if (palabras.Count == 1)
            {
                // Una sola palabra, rellenar con espacios al final
                return palabras[0].PadRight(maxCaracteres);
            }

            // Calcular espacios totales necesarios
            var longitudPalabras = palabras.Sum(p => p.Length);
            var espaciosTotales = maxCaracteres - longitudPalabras;
            var huecos = palabras.Count - 1; // Número de huecos entre palabras

            if (espaciosTotales <= 0 || huecos == 0)
            {
                // Si no hay espacio para distribuir, concatenar con espacios mínimos
                return string.Join(" ", palabras).PadRight(maxCaracteres);
            }

            // Distribuir espacios uniformemente
            var espaciosPorHueco = espaciosTotales / huecos;
            var espaciosExtra = espaciosTotales % huecos;

            var resultado = new StringBuilder();
            for (int i = 0; i < palabras.Count; i++)
            {
                resultado.Append(palabras[i]);

                if (i < palabras.Count - 1) // No agregar espacios después de la última palabra
                {
                    var espaciosAgregar = espaciosPorHueco;
                    if (i < espaciosExtra)
                        espaciosAgregar++; // Distribuir espacios extra en los primeros huecos

                    resultado.Append(' ', espaciosAgregar);
                }
            }

            return resultado.ToString();
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

    public class LineasImprimir
    {
        public LineasImprimir(string linea, bool centrada, string qr = null)
        {
            this.linea = linea;
            this.centrada = centrada;
            this.qr = qr;
        }

        public string linea;
        public bool centrada;
        public string qr;

    }
}