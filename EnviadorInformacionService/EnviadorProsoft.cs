using EnviadorInformacionService.Models;
using FacturacionelectronicaCore.Negocio.Contabilidad;
using FacturadorEstacionesRepositorio;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EnviadorInformacionService
{
    internal class EnviadorProsoft : IEnviadorProsoft
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private List<Factura> facturas = new List<Factura>();

        public void EnviarInformacion()
        {
            var _estacionesRepositorio = new EstacionesRepositorioSqlServer();
            var _apiContabilidad = new ApiContabilidad();

            Logger.Info("Iniciando interfaz silog");
            Thread.CurrentThread.CurrentCulture = new CultureInfo("es-ES");
            while (true)
            {
                try
                {
                    if (ConfigurationManager.AppSettings["EnvioAProsoft"] == "true")
                    {
                        Logger.Info("Enviando facturas a prosoft");
                        var facturas = _estacionesRepositorio.BuscarFacturasNoEnviadasFacturacion();

                        var formas = _estacionesRepositorio.BuscarFormasPagos();
                        if (!facturas.Any(x => x.Venta.COD_FOR_PAG == 4 || x.Venta.COD_FOR_PAG == 6))
                        {

                            Logger.Info("No se encontraron facturas");
                            Thread.Sleep(15000);
                            continue;
                        }
                        facturas = facturas.Where(x => x.Venta.COD_FOR_PAG == 4 || x.Venta.COD_FOR_PAG == 6).ToList();


                        var facturasNoEnviadas = facturas;
                        var enviarFacturasEfevtivo = bool.Parse(ConfigurationManager.AppSettings["EnviarFacturasEfevtivo"]);
                        if (!enviarFacturasEfevtivo)
                        {

                            Logger.Info("No se envian facturas efectivo");
                            if (facturas.Any(x => x.Venta.COD_FOR_PAG != 4))
                            {
                                facturas = facturas.Where(x => x.Venta.COD_FOR_PAG != 4).ToList();
                                facturasNoEnviadas = facturas.Where(x => x.Venta.COD_FOR_PAG == 4).ToList();

                                _estacionesRepositorio.ActuralizarFacturasEnviadosFacturacion(facturasNoEnviadas.Select(x => x.ventaId));
                            }
                            else
                            {

                                _estacionesRepositorio.ActuralizarFacturasEnviadosFacturacion(facturas.Select(x => x.ventaId));

                                Thread.Sleep(15000);
                                continue;
                            }
                        }
                        var facturasEnviar = facturas.Select(x => new FacturaProsoft(x, formas.Where(y => y.Id == x.Venta.COD_FOR_PAG).Select(y => y.Descripcion).Single()));


                        Logger.Info($"{facturasEnviar.Count()} facturas encontradas");
                        var idsenviados = _apiContabilidad.EnviarFacturas(facturasEnviar);

                        _estacionesRepositorio.ActuralizarFacturasEnviadosFacturacion(idsenviados);

                        Logger.Info($"{idsenviados.Count()} Facturas recibidas en silog");
                    }


                    Thread.Sleep(15000);
                }
                catch (Exception ex)
                {

                    Logger.Info("Ex" + ex.Message);
                    Logger.Info("Ex" + ex.StackTrace);
                    Thread.Sleep(300000);
                }
            }

        }

        private int ObtenerUltimaFilaOcupada(ExcelWorksheet firstWorksheet)
        {
            int i = 6;
            while (true)
            {
                if (firstWorksheet.Cells[i, 1].Value == null || firstWorksheet.Cells[i, 1].Value.ToString() != "F")
                {
                    return i;
                }
                i++;
            }
        }
    }
}