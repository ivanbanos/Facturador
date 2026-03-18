
using EnviadorInformacionService;
using FactoradorEstacionesModelo.Siges;
using FacturacionelectronicaCore.Negocio.Contabilidad;
using FacturacionelectronicaCore.Negocio.Modelo;
using FacturadorEstacionesRepositorio;
using System;
using System.Collections.Generic;
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
            try
            {
                var combustibles = _conexionEstacionRemota.GetCombustiblesEstacion(estacionFuente, token);
                _estacionesRepositorio.ActualizarPreciosCombustibles(combustibles);
            }
            catch (Exception ex)
            {
                Logger.Warn($"No fue posible sincronizar combustibles: {ex.Message}");
            }

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

            SincronizarTurnosMesEnCurso(token);

            try
            {
                _estacionesRepositorio.PrepararRetroactivoTurnosPendientes();
            }
            catch (Exception ex)
            {
                Logger.Warn($"No fue posible preparar retroactivo de turnos pendientes: {ex.Message}");
            }

            var facturasPorturno = _estacionesRepositorio.GetFacturaSinEnviarTurno();
            if (facturasPorturno.Any(x => x.Manguera != null))
            {
                foreach (var factura in facturasPorturno.Where(x => x.Manguera != null))
                {
                    try
                    {
                        var turnoGuid = GetFacturaTurnoGuid(factura);
                        if (!string.IsNullOrWhiteSpace(turnoGuid))
                        {
                            _estacionesRepositorio.ActuralizarFacturasEnviadosTurno(factura.ventaId);
                            continue;
                        }

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
                                SincronizarTurno(turno, token);
                                okFacturas = _conexionEstacionRemota.SetTurnoFactura(factura.ventaId, turno.FechaApertura, turno.Isla, turno.Numero, estacionFuente, token);
                                if (okFacturas)
                                {
                                    _estacionesRepositorio.ActuralizarFacturasEnviadosTurno(factura.ventaId);
                                }
                                else
                                {
                                    Logger.Warn($"No se pudo actualizar turno retroactivo para venta {factura.ventaId}");
                                }
                            }
                        }else
                        {
                            //_estacionesRepositorio.ActuralizarFacturasEnviadosTurno(factura.ventaId);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Warn($"No subieron turnos {ex.Message}");
                    }

                }
            }


        }

        private void SincronizarTurnosMesEnCurso(string token)
        {
            var inicioMes = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var ahora = DateTime.Now;
            var inicioRango = DateTime.Now.Day == 1
                ? inicioMes.AddDays(-1)
                : inicioMes;

            IEnumerable<TurnoSiges> turnosMes;
            try
            {
                turnosMes = _estacionesRepositorio.GetTurnosByFechas(inicioRango, ahora).ToList();
            }
            catch (Exception ex)
            {
                Logger.Warn($"No fue posible consultar turnos del mes en curso: {ex.Message}");
                return;
            }

            foreach (var turnoSiges in turnosMes)
            {
                try
                {
                    var turnosSurtidor = _estacionesRepositorio.ObtenerTurnoInfo(turnoSiges.Id).ToList();
                    var turno = ConvertirTurnoParaSincronizacion(turnoSiges, turnosSurtidor);
                    SincronizarTurno(turno, token);
                }
                catch (Exception ex)
                {
                    Logger.Warn($"No fue posible sincronizar turno {turnoSiges.Id}: {ex.Message}");
                }
            }
        }

        private void SincronizarTurno(Turno turno, string token)
        {
            if (turno == null)
            {
                return;
            }

            turno.Isla = string.IsNullOrWhiteSpace(turno.Isla) ? turno.Isla : turno.Isla.Trim();
            _conexionEstacionRemota.SubirTurno(turno, estacionFuente, token);
        }

        private static Turno ConvertirTurnoParaSincronizacion(TurnoSiges turnoSiges, IEnumerable<FactoradorEstacionesModelo.Siges.TurnoSurtidor> turnosSurtidor)
        {
            var turno = new Turno
            {
                Empleado = turnoSiges.Empleado,
                FechaApertura = turnoSiges.FechaApertura,
                FechaCierre = turnoSiges.FechaCierre,
                IdEstado = turnoSiges.IdEstado,
                Isla = turnoSiges.Isla,
                Numero = turnoSiges.Numero,
                turnoSurtidores = new List<FacturacionelectronicaCore.Negocio.Modelo.TurnoSurtidor>()
            };

            if (turnosSurtidor == null)
            {
                return turno;
            }

            turno.turnoSurtidores = turnosSurtidor.Select(x => new FacturacionelectronicaCore.Negocio.Modelo.TurnoSurtidor
            {
                Apertura = x.Apertura,
                Cierre = x.Cierre,
                Manguera = x.Manguera?.Descripcion ?? x.Manguera?.Ubicacion,
                Surtidor = x.Manguera?.Ubicacion,
                Combustible = x.Combustible?.Descripcion,
                precioCombustible = Convert.ToSingle(x.Combustible?.Precio ?? 0)
            }).ToList();

            return turno;
        }

        private static string GetFacturaTurnoGuid(object factura)
        {
            if (factura == null)
            {
                return null;
            }

            var prop = factura.GetType().GetProperty("TurnoGuid") ?? factura.GetType().GetProperty("turnoguid");
            if (prop == null)
            {
                return null;
            }

            return prop.GetValue(factura, null) as string;
        }
    }

}

