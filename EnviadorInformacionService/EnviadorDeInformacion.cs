
using EnviadorInformacionService;
using FacturacionelectronicaCore.Negocio.Contabilidad;
using FacturacionelectronicaCore.Negocio.Modelo;
using FacturadorEstacionesRepositorio;
using System;
using System.Configuration;
using System.Linq;
using System.Threading;

namespace EnviadorInformacion
{
    public class EnviadorDeInformacion : IEnviadorDeInformacion
    {
        private readonly IEstacionesRepositorio _estacionesRepositorio;
        private readonly IConexionEstacionRemota _conexionEstacionRemota;
        private readonly IApiContabilidad _apiContabilidad;
        private readonly Guid estacionFuente;
        private DateTime? stanByTime;

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        public EnviadorDeInformacion()
        {
            _estacionesRepositorio = new EstacionesRepositorioSqlServer();
            _conexionEstacionRemota = new ConexionEstacionRemota();
            _apiContabilidad = new ApiContabilidad();
            estacionFuente = new Guid(ConfigurationManager.AppSettings["estacionFuente"]);
        }

        public void EnviarInformacion()
        {
            while (true)
            {
                try
                {
                    EnviarFacturas();
                    Thread.Sleep(60000);
                }
                catch (Exception ex)
                {

                    Logger.Error("Ex" + ex.Message);
                    Logger.Error("Ex" + ex.StackTrace);
                    Thread.Sleep(60000);
                }
            }
        }



        private void EnviarFacturas()
        {
            string token = _conexionEstacionRemota.getToken();
            var facturas = _estacionesRepositorio.BuscarFacturasNoEnviadas();
            if (facturas.Any(x=>x.Manguera!=null))
            {
                var formas = _estacionesRepositorio.BuscarFormasPagos();

                var okFacturas = _conexionEstacionRemota.EnviarFacturas(facturas.Where(x=>x.Manguera!=null), formas, estacionFuente, token);
                if (okFacturas)
                {
                    _estacionesRepositorio.ActuralizarFacturasEnviados(facturas.Select(x => x.ventaId));
                }
                else
                {

                    Logger.Info("No subieron facturas");
                }
            }



            var facturasFechas = _estacionesRepositorio.BuscarFechasReportesNoEnviadas();
            if (facturasFechas.Any())
            {

                var okFacturasFechas = _conexionEstacionRemota.AgregarFechaReporteFactura(facturasFechas, estacionFuente, token);
                if (okFacturasFechas)
                {
                    _estacionesRepositorio.ActuralizarFechasReportesEnviadas(facturasFechas.Select(x => x.IdVentaLocal));
                }
                else
                {

                    Logger.Warn("No subieron facturas");
                }
            }

            try
            {

                var tercerosRecibidos = _conexionEstacionRemota.RecibirTercerosActualizados(estacionFuente, token);
                foreach (var tercero in tercerosRecibidos)
                {

                    _estacionesRepositorio.ActuralizarTerceros(tercero);

                    Logger.Info($"Tercero {tercero.identificacion} agregado");
                }
                var facturasIdImprimir = _conexionEstacionRemota.RecibirFacturasImprimir(estacionFuente, token);
                var ordenesIdImprimir = _conexionEstacionRemota.RecibirOrdenesImprimir(estacionFuente, token);


                foreach (var orden in ordenesIdImprimir)
                {
                    _estacionesRepositorio.MandarImprimir(orden.IdVentaLocal);
                }
                foreach (var factura in facturasIdImprimir)
                {
                    _estacionesRepositorio.MandarImprimir(factura.IdVentaLocal);
                }
            }catch(Exception ex)
            {
                Logger.Warn($"No subieron facturas {ex.Message}");
            }

            if(!stanByTime.HasValue || stanByTime.Value < DateTime.Now.AddHours(-2))
            {
                try
                {
                    var cuposInfo = _estacionesRepositorio.GetInfoCupos();

                _conexionEstacionRemota.SubirInfoCupos(cuposInfo, estacionFuente, token);
                stanByTime = DateTime.Now;
                }
                catch (Exception ex)
                {
                    Logger.Warn($"No subieron cupos {ex.Message}");
                }
            }
            var facturasPorturno = _estacionesRepositorio.GetFacturaSinEnviarTurno();
            if (facturasPorturno.Any(x => x.Manguera != null))
            {
                foreach (var factura in facturasPorturno.Where(x => x.Manguera != null))
                {
                    try
                    {
                        var turno = _estacionesRepositorio.ObtenerTurnoIslaPorVenta(factura.ventaId);
                        if (turno != null)
                        {
                            var okFacturas = _conexionEstacionRemota.SetTurnoFactura(factura.ventaId, turno.FechaApertura, turno.Isla, turno.Numero, estacionFuente, token);
                            if (okFacturas)
                            {
                                _estacionesRepositorio.ActuralizarFacturasEnviadosTurno(factura.ventaId);
                            }
                            else
                            {
                                _conexionEstacionRemota.SubirTurno(turno, estacionFuente, token);
                                okFacturas = _conexionEstacionRemota.SetTurnoFactura(factura.ventaId, turno.FechaApertura, turno.Isla, turno.Numero, estacionFuente, token);
                                if (okFacturas)
                                {
                                    _estacionesRepositorio.ActuralizarFacturasEnviadosTurno(factura.ventaId);
                                }
                                else
                                {
                                    _estacionesRepositorio.ActuralizarFacturasEnviadosTurno(factura.ventaId);
                                    Logger.Info("No subieron facturas");
                                }
                            }
                        }else
                        {
                            //_estacionesRepositorio.ActuralizarFacturasEnviadosTurno(factura.ventaId);
                        }
                    }
                    catch (Exception ex)
                    {
                        _estacionesRepositorio.ActuralizarFacturasEnviadosTurno(factura.ventaId);
                        Logger.Warn($"No subieron turnos {ex.Message}");
                    }

                }
            }


        }
         }

}

