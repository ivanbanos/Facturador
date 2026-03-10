using EnviadorInformacionService.Models;
using FactoradorEstacionesModelo.Convertidor;
using FactoradorEstacionesModelo.Objetos;
using FacturacionelectronicaCore.Negocio.Modelo;
using FacturacionelectronicaCore.Repositorio.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FacturadorEstacionesRepositorio
{
    public class EstacionesRepositorioSqlServer : IEstacionesRepositorio
    { 
        private readonly ConnectionStrings _connectionString;

        private Convertidor _convertidor;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="estacionesRepositorio"></param>
        public EstacionesRepositorioSqlServer()
        {
            _convertidor = new Convertidor();
            _connectionString = new ConnectionStrings()
            {
                estacion = ConfigurationManager.ConnectionStrings["estacion"].ConnectionString,
                Facturacion = ConfigurationManager.ConnectionStrings["Facturacion"].ConnectionString
            };
        }




        public virtual async Task<DataTable> LoadDataTableFromStoredProcAsync(string procName, IDictionary<string, object> parameters, int? timeout = null)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(_connectionString.estacion))
            using (SqlCommand cmd = GetSqlStoredProcCommand(procName, conn))
            {
                try
                {
                    if (timeout != null)
                    {
                        cmd.CommandTimeout = timeout.Value;
                    }

                    SetParameters(cmd, parameters);

                    await conn.OpenAsync();

                    using (IDataReader reader = await ExecuteActionWithLogAsync<IDataReader>(cmd, async c => await c.ExecuteReaderAsync()))
                    {
                        dt.Load(reader, LoadOption.OverwriteChanges);
                    }

                    }
                catch (Exception e)
                {
                   throw e;
                }
            }

            return dt;
        }

        public virtual DataTable LoadDataTableFromStoredProc(string connectionString, string procName, IDictionary<string, object> parameters, int? timeout = null)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = GetSqlStoredProcCommand(procName, conn))
            {
                try
                {
                    if (timeout != null)
                    {
                        cmd.CommandTimeout = timeout.Value;
                    }

                    SetParameters(cmd, parameters);

                    conn.Open();

                    using (IDataReader reader = cmd.ExecuteReader())
                    {
                        dt.Load(reader, LoadOption.OverwriteChanges);
                    }

                }
                catch (SqlException e)
                {
                    throw e;
                }
            }

            return dt;
        }


        public virtual DataSet LoadDataSetFromStoredProc(string connectionString, string procName, IDictionary<string, object> parameters, int? timeout = null)
        {
            DataSet ds = new DataSet();
            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = GetSqlStoredProcCommand(procName, conn))
            {
                try
                {
                    if (timeout != null)
                    {
                        cmd.CommandTimeout = timeout.Value;
                    }

                    SetParameters(cmd, parameters);

                    conn.Open();

                    var adapter = new SqlDataAdapter(cmd);

                    adapter.Fill(ds);

                }
                catch (SqlException e)
                {
                    throw e;
                }
            }

            return ds;
        }

        private SqlCommand GetSqlStoredProcCommand(string commandText, SqlConnection conn)
        {
            SqlCommand command = new SqlCommand(commandText, conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            return command;
        }

        public FactoradorEstacionesModelo.Objetos.Factura getFacturasImprimir()
        {
            var parameters = new Dictionary<string, object>
            {
            };
            DataTable dt2 = LoadDataTableFromStoredProc(_connectionString.Facturacion, "getFacturaImprimir",
                         parameters);
            var facturas = _convertidor.ConvertirFactura(dt2);




            Thread.Sleep(3000);
            foreach (var factura in facturas)
            {
                DataTable dt = LoadDataTableFromStoredProc(_connectionString.estacion, "getVentaPorId",
                           new Dictionary<string, object>{
                {"@CONSECUTIVO",factura.ventaId }
                           });

                var ventas = _convertidor.ConvertirVenta(dt);
                var manguera = _convertidor.ConvertirManguera(dt).Single();
                factura.Venta = ventas.FirstOrDefault();
                factura.Manguera = manguera;
            }
            return facturas.FirstOrDefault();
        }

        private void SetParameters(SqlCommand cmd, IDictionary<string, object> parameters)
        {
            if (parameters != null)
            {
                foreach (KeyValuePair<string, object> param in parameters)
                {
                    if (param.Value is SqlParameter)
                    {
                        cmd.Parameters.Add(param.Value);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                    }
                }
            }
        }

        protected virtual async Task<T> ExecuteActionWithLogAsync<T>(SqlCommand cmd, Func<SqlCommand, Task<T>> action)
        {
                T retVal = await action(cmd);
                return retVal;
        }


        

        public FactoradorEstacionesModelo.Objetos.Resolucion BuscarResolucionActiva(IEnumerable<FactoradorEstacionesModelo.Objetos.Resolucion> resolucionesRemota)
        {
            var combustible = resolucionesRemota.FirstOrDefault(x => x.Tipo == 0);
            var canastilla = resolucionesRemota.FirstOrDefault(x => x.Tipo == 1);
            DataTable dt = LoadDataTableFromStoredProc(_connectionString.Facturacion, "BuscarResolucionActiva",
                      new Dictionary<string, object>
                      {
                {"@DescripcionResolucion",combustible?.DescripcionResolucion },
                {"@FechaFinalResolucion",combustible?.FechaFinalResolucion },
                {"@FechaInicioResolucion",combustible?.FechaInicioResolucion },
                {"@ConsecutivoInicial",combustible?.ConsecutivoInicial },
                {"@ConsecutivoFinal",combustible?.ConsecutivoFinal },
                {"@ConsecutivoActual",combustible?.ConsecutivoActual },
                {"@Autorizacion",combustible?.Autorizacion },
                {"@Habilitada",combustible?.Habilitada },
                {"@Tipo",combustible?.Tipo },
                {"@DescripcionResolucionCanastilla",canastilla?.DescripcionResolucion },
                {"@FechaFinalResolucionCanastilla",canastilla?.FechaFinalResolucion },
                {"@FechaInicioResolucionCanastilla",canastilla?.FechaInicioResolucion },
                {"@ConsecutivoInicialCanastilla",canastilla?.ConsecutivoInicial },
                {"@ConsecutivoFinalCanastilla",canastilla?.ConsecutivoFinal },
                {"@ConsecutivoActualCanastilla",canastilla?.ConsecutivoActual },
                {"@AutorizacionCanastilla",canastilla?.Autorizacion },
                {"@HabilitadaCanastilla",canastilla?.Habilitada },
                {"@TipoCanastilla",canastilla?.Tipo },
        });

            return _convertidor.ConvertirResolucion(dt).FirstOrDefault();
        }

        internal bool HayFacturasCanastillaPorImprimir()
        {
            DataTable dt = LoadDataTableFromStoredProc(_connectionString.Facturacion, "FacturasCanastillaPorImprimir",
                         new Dictionary<string, object>
                         {
                         });
           return dt.AsEnumerable().Count() > 0;
        }

        public List<FactoradorEstacionesModelo.Objetos.Tercero> BuscarTercerosNoEnviados()
        {
            DataTable dt = LoadDataTableFromStoredProc(_connectionString.Facturacion, "BuscarTercerosNoEnviados",
                      new Dictionary<string, object>
                      {
                      });

            return _convertidor.ConvertirTercero(dt)?.ToList();
        }


        public void CambiarConsecutivoActual(int consecutivoActual)
        {

            var parameters = new Dictionary<string, object>
            {
                {"@consecutivoActual",consecutivoActual }
            };
            DataTable dt2 = LoadDataTableFromStoredProc(_connectionString.Facturacion, "CambiarConsecutivoActual",
                         parameters);
        }

        public List<FactoradorEstacionesModelo.Objetos.Factura> BuscarFacturasNoEnviadas()
        {
            var parameters = new Dictionary<string, object>
            {
            };
            DataTable dt2 = LoadDataTableFromStoredProc(_connectionString.Facturacion, "getFacturaSinEnviar",
                         parameters);
            var facturas = _convertidor.ConvertirFactura(dt2);
            foreach (var factura in facturas)
            {
                DataTable dt = LoadDataTableFromStoredProc(_connectionString.estacion, "getVentaPorId",
                           new Dictionary<string, object>{
                {"@CONSECUTIVO",factura.ventaId }
                           });

                var ventas = _convertidor.ConvertirVenta(dt);
                var manguera = _convertidor.ConvertirManguera(dt).FirstOrDefault();
                factura.Venta = ventas.FirstOrDefault();
                factura.Manguera = manguera;
            }
            return facturas.ToList();
        }

        internal void ActualizarCanastilla(Canastilla canastilla)
        {
            throw new NotImplementedException();
        }

        internal FacturaCanastilla BuscarFacturaCanastillaPorConsecutivo(int consecutivo)
        {
            var parameters = new Dictionary<string, object>
            {
                {"@consecutivo",consecutivo }
            };
            DataTable dt = LoadDataTableFromStoredProc(_connectionString.Facturacion, "BuscarFacturaCanastillaPorConsecutivo",
                         parameters);
            var facturas = _convertidor.ConvertirFacturaCanastilla(dt);
            List<FacturaCanastilla> facturasEnviar = new List<FacturaCanastilla>();
            foreach (var factura in facturas)
            {


                var parameters2 = new Dictionary<string, object>
            {
                {"@FacturaCanastillaId",factura.FacturasCanastillaId }
            };
                DataTable dt2 = LoadDataTableFromStoredProc(_connectionString.Facturacion, "getFacturaCanatillaDetalle",
                             parameters2);

                factura.canastillas = _convertidor.ConvertirFacturaCanastillaDEtalle(dt2);
                facturasEnviar.Add(factura);
            }
            return facturasEnviar.FirstOrDefault();
        }

        /// <summary>
        /// Obtiene el detalle de una factura canastilla
        /// </summary>
        /// <param name="facturaCanastillaId">ID de la factura canastilla</param>
        /// <returns>Lista de detalles de la factura canastilla</returns>
        public IEnumerable<CanastillaFactura> getFacturaCanatillaDetalle(int facturaCanastillaId)
        {
            var parameters = new Dictionary<string, object>
            {
                {"@FacturaCanastillaId", facturaCanastillaId }
            };
            DataTable dt = LoadDataTableFromStoredProc(_connectionString.Facturacion, "getFacturaCanatillaDetalle", parameters);
            var detalle = _convertidor.ConvertirFacturaCanastillaDEtalle(dt);
            return detalle;
        }

        internal object ActualizarResolucionCanastilla(object resolucionRemota)
        {
            throw new NotImplementedException();
        }

        public List<FormasPagos> BuscarFormasPagos()
        {
            DataTable dt = LoadDataTableFromStoredProc(_connectionString.estacion, "ObtenerFormasPago",
                   new Dictionary<string, object>
                   {
                   });

            return _convertidor.ConvertirFormasPagos(dt).ToList();
        }

        internal IEnumerable<FacturaCanastilla> BuscarFacturasNoEnviadasCanastilla()
        {
            var parameters = new Dictionary<string, object>
            {
            };
            DataTable dt = LoadDataTableFromStoredProc(_connectionString.Facturacion, "getFacturaEnviarCanastilla",
                         parameters);
            var facturas = _convertidor.ConvertirFacturaCanastilla(dt);
            List<FacturaCanastilla> facturasEnviar = new List<FacturaCanastilla>();
            foreach(var factura in facturas)
            {


                var parameters2 = new Dictionary<string, object>
            {
                {"@FacturaCanastillaId",factura.FacturasCanastillaId }
            };
                DataTable dt2 = LoadDataTableFromStoredProc(_connectionString.Facturacion, "getFacturaCanatillaDetalle",
                             parameters2);

                factura.canastillas = _convertidor.ConvertirFacturaCanastillaDEtalle(dt2);
                facturasEnviar.Add(factura);
            }
            return facturasEnviar;
        }

        internal void SetFacturaCanastillaEnviada(int facturasCanastillaId)
        {
            var parameters = new Dictionary<string, object>
            {
                {"@facturaCanastillaId",facturasCanastillaId }
            };
            DataTable dt2 = LoadDataTableFromStoredProc(_connectionString.Facturacion, "SetFacturaCanastillaEnviada",
                         parameters);
        }

        public void ActuralizarTercerosEnviados(IEnumerable<int> terceros)
        {
            var ventasIds = new DataTable();
            ventasIds.Columns.Add(new DataColumn("ventaId", typeof(long))
            {
                AllowDBNull = false
            });
            foreach (var t in terceros)
            {
                var row = ventasIds.NewRow();
                row["ventaId"] = t;
                ventasIds.Rows.Add(row);
            }
            var parameters = new Dictionary<string, object>
            {
                {"@terceros",ventasIds }
            };
            DataTable dt2 = LoadDataTableFromStoredProc(_connectionString.Facturacion, "CambiarEstadoTerceroEnviado",
                         parameters);
        }

        internal void ActuralizarFacturasEnviadosCanastilla(IEnumerable<int> facturas)
        {
            var ventasIds = new DataTable();
            ventasIds.Columns.Add(new DataColumn("ventaId", typeof(long))
            {
                AllowDBNull = false
            });
            foreach (var t in facturas)
            {
                var row = ventasIds.NewRow();
                row["ventaId"] = t;
                ventasIds.Rows.Add(row);
            }
            var parameters = new Dictionary<string, object>
            {
                {"@facturas",ventasIds }
            };
            DataTable dt2 = LoadDataTableFromStoredProc(_connectionString.Facturacion, "SetFacturaCanastillaEnviada",
                         parameters);
        }

        public void ActuralizarFacturasEnviados(IEnumerable<int> facturas)
        {
            var ventasIds = new DataTable();
            ventasIds.Columns.Add(new DataColumn("ventaId", typeof(long))
            {
                AllowDBNull = false
            });
            foreach (var t in facturas)
            {
                var row = ventasIds.NewRow();
                row["ventaId"] = t;
                ventasIds.Rows.Add(row);
            }
            var parameters = new Dictionary<string, object>
            {
                {"@facturas",ventasIds }
            };
            DataTable dt2 = LoadDataTableFromStoredProc(_connectionString.Facturacion, "CambiarEstadoFactursEnviada",
                         parameters);
        }

        public List<TipoIdentificacion> getTiposIdentifiaciones()
        {
            DataTable dt = LoadDataTableFromStoredProc(_connectionString.Facturacion, "ObtenerTipoIdentificaciones",
                   new Dictionary<string, object>
                   {
                   });

            return _convertidor.ConvertirTipoIdentificacion(dt).ToList();
        }

        public void ActuralizarTerceros(FactoradorEstacionesModelo.Objetos.Tercero tercero)
        {
            List<TipoIdentificacion> tipos = getTiposIdentifiaciones();
            tercero.tipoIdentificacion = tipos.Where(x => x.Descripcion.ToLower() == tercero.tipoIdentificacionS.ToLower()).FirstOrDefault().TipoIdentificacionId;
            DataTable dt = LoadDataTableFromStoredProc(_connectionString.Facturacion, "CrearTercero",
                         new Dictionary<string, object>
                         {
                    {"@terceroId", tercero.terceroId },
                    {"@tipoIdentificacion", tercero.tipoIdentificacion },
                    {"@identificacion", tercero.identificacion },
                    {"@nombre", tercero.Nombre },
                    {"@apellidos", tercero.Apellidos },
                    {"@telefono", tercero.Telefono },
                    {"@correo", tercero.Correo },
                    {"@direccion", tercero.Direccion },
                    {"@estado", "AC" },
                    {"@COD_CLI", tercero.COD_CLI },
                         });
        }
        public void MandarImprimir(int ventaId)
        {
            LoadDataTableFromStoredProc(_connectionString.Facturacion, "MandarImprimir",
                            new Dictionary<string, object>{

                    {"@ventaId", ventaId }
                            });
        }
        public List<Cara> getCaras()
        {
            DataTable dt = LoadDataTableFromStoredProc(_connectionString.estacion, "ObtenerCaras",
                   new Dictionary<string, object>
                   {
                   });

            return _convertidor.ConvertirCara(dt).ToList();
        }

        public List<FactoradorEstacionesModelo.Objetos.Factura> getUltimasFacturas(short cOD_CAR, int v)
        {
            DataTable dt = LoadDataTableFromStoredProc(_connectionString.estacion, "ObtenerVentaPorCara",
                         new Dictionary<string, object>{

                    {"@COD_CAR", cOD_CAR }
                         });

            var venta = _convertidor.ConvertirVenta(dt).FirstOrDefault();
            if (venta == null)
            {
                return new List<FactoradorEstacionesModelo.Objetos.Factura>() { new FactoradorEstacionesModelo.Objetos.Factura() { Venta = new Venta() { CONSECUTIVO = -1 } } };
            }
            DataTable dt2 = LoadDataTableFromStoredProc(_connectionString.Facturacion, "ObtenerFacturaPorVenta",
                         new Dictionary<string, object>{

                    {"@ventaId", venta.CONSECUTIVO }
                         });
            var factura = _convertidor.ConvertirFactura(dt2).FirstOrDefault();
            if (factura == null)
            {
                try
                {

                    DataTable dt3 = LoadDataTableFromStoredProc(_connectionString.estacion, "AgregarFacturaPorIdVenta",
                             new Dictionary<string, object>{

                    {"@ventaId", venta.CONSECUTIVO }
                             });
                    dt2 = LoadDataTableFromStoredProc(_connectionString.Facturacion, "ObtenerFacturaPorVenta",
                             new Dictionary<string, object>{

                    {"@ventaId", venta.CONSECUTIVO }
                             });
                    factura = _convertidor.ConvertirFactura(dt2).FirstOrDefault();
                }
                catch (Exception ex)
                {

                }
            }
            if (factura == null)
            {
                return new List<FactoradorEstacionesModelo.Objetos.Factura>();
            }
            factura.Manguera = _convertidor.ConvertirManguera(dt).FirstOrDefault();
            factura.Venta = venta;
            return new List<FactoradorEstacionesModelo.Objetos.Factura>() { factura };
        }


        public void SetFacturaImpresa(int facturaPOSId)
        {

            var parameters = new Dictionary<string, object>
            {
                {"@ventaid",facturaPOSId }
            };
            DataTable dt2 = LoadDataTableFromStoredProc(_connectionString.Facturacion, "SetFacturaImpresa",
                         parameters);
        }

        public void AgregarFacturaDesdeIdVenta()
        {
            DataTable dt = LoadDataTableFromStoredProc(_connectionString.estacion, "AgregarFacturaDesdeIdVenta",
                     new Dictionary<string, object>
                     {
                     });
        }



        public List<FactoradorEstacionesModelo.Objetos.Factura> BuscarFacturasNoEnviadasFacturacion()
        {
            var parameters = new Dictionary<string, object>
            {
            };
            DataTable dt2 = LoadDataTableFromStoredProc(_connectionString.Facturacion, "getFacturaSinEnviadaFacturacion",
                         parameters);
            var facturas = _convertidor.ConvertirFactura(dt2);





            foreach (var factura in facturas)
            {
                DataTable dt = LoadDataTableFromStoredProc(_connectionString.estacion, "getVentaPorId",
                           new Dictionary<string, object>{
                {"@CONSECUTIVO",factura.ventaId }
                           });

                var ventas = _convertidor.ConvertirVenta(dt);
                var manguera = _convertidor.ConvertirManguera(dt).Single();
                factura.Venta = ventas.FirstOrDefault();
                factura.Manguera = manguera;
            }
            return facturas.ToList();
        }


        public void ActuralizarFacturasEnviadosFacturacion(IEnumerable<int> facturas)
        {
            var ventasIds = new DataTable();
            ventasIds.Columns.Add(new DataColumn("ventaId", typeof(long))
            {
                AllowDBNull = false
            });
            foreach (var t in facturas)
            {
                var row = ventasIds.NewRow();
                row["ventaId"] = t;
                ventasIds.Rows.Add(row);
            }
            var parameters = new Dictionary<string, object>
            {
                {"@facturas",ventasIds }
            };
            DataTable dt2 = LoadDataTableFromStoredProc(_connectionString.Facturacion, "CambiarEstadoFactursEnviadaFacturacion",
                         parameters);
        }

        public List<FacturaFechaReporte> BuscarFechasReportesNoEnviadas()
        {
            var parameters = new Dictionary<string, object>
            {
            };
            DataTable dt2 = LoadDataTableFromStoredProc(_connectionString.estacion, "BuscarFechasReportesNoEnviadas",
                         parameters);
            return _convertidor.ConvertirFacturaFechaReporte(dt2);
        }

        public string ObtenerCodigoInterno(string placa, string identificacion)
        {
            var parameters = new Dictionary<string, object>
            {{"@PLACA",placa },
            {"@identificacion",identificacion }
            };
            DataTable dt2 = LoadDataTableFromStoredProc(_connectionString.estacion, "ObtenerCodigoInterno",
                         parameters);
            return dt2.AsEnumerable().Select(dr => dr.Field<string>("COD_INT")).FirstOrDefault();
        }

        public void ActuralizarFechasReportesEnviadas(IEnumerable<int> facturas)
        {
            var ventasIds = new DataTable();
            ventasIds.Columns.Add(new DataColumn("ventaId", typeof(long))
            {
                AllowDBNull = false
            });
            foreach (var t in facturas)
            {
                var row = ventasIds.NewRow();
                row["ventaId"] = t;
                ventasIds.Rows.Add(row);
            }
            var parameters = new Dictionary<string, object>
            {
                {"@facturas",ventasIds }
            };
            DataTable dt2 = LoadDataTableFromStoredProc(_connectionString.Facturacion, "ActuralizarFechasReportesEnviadas",
                         parameters);
        }


        public IEnumerable<FactoradorEstacionesModelo.Objetos.Factura> getFacturasSiigo()
        {
            var parameters = new Dictionary<string, object>
            {
            };
            DataTable dt2 = LoadDataTableFromStoredProc(_connectionString.Facturacion, "getFacturaSiigo",
                         parameters);
            var facturas = _convertidor.ConvertirFactura(dt2);




            Thread.Sleep(3000);
            foreach (var factura in facturas)
            {
                DataTable dt = LoadDataTableFromStoredProc(_connectionString.estacion, "getVentaPorId",
                           new Dictionary<string, object>{
                {"@CONSECUTIVO",factura.ventaId }
                           });

                var ventas = _convertidor.ConvertirVenta(dt);
                var manguera = _convertidor.ConvertirManguera(dt).Single();
                factura.Venta = ventas.FirstOrDefault();
                factura.Manguera = manguera;
            }
            return (IEnumerable<FactoradorEstacionesModelo.Objetos.Factura>)facturas;
        }

        internal void SetFacturaCanastillaImpresa(int facturasCanastillaId)
        {
            var parameters = new Dictionary<string, object>
            {
                {"@facturaCanastillaId",facturasCanastillaId }
            };
            DataTable dt2 = LoadDataTableFromStoredProc(_connectionString.Facturacion, "SetFacturaCanastillaImpresa",
                         parameters);
        }


        internal FacturaCanastilla getFacturasCanastillaImprimir()
        {
            var parameters = new Dictionary<string, object>
            {
            };
            DataTable dt = LoadDataTableFromStoredProc(_connectionString.Facturacion, "getFacturaImprimirCanastilla",
                         parameters);
            var facturas = _convertidor.ConvertirFacturaCanastilla(dt);
            if (!facturas.Any())
            {
                return null;
            }
            var factura = facturas.First();

            var parameters2 = new Dictionary<string, object>
            {
                {"@FacturaCanastillaId",factura.FacturasCanastillaId }
            };
            DataTable dt2 = LoadDataTableFromStoredProc(_connectionString.Facturacion, "getFacturaCanatillaDetalle",
                         parameters2);

            factura.canastillas = _convertidor.ConvertirFacturaCanastillaDEtalle(dt2);
            return factura;
        }

        public List<FactoradorEstacionesModelo.Objetos.Factura> GetFacturaSinEnviarTurno()
        {
            var parameters = new Dictionary<string, object>
            {
            };
            DataTable dt2 = LoadDataTableFromStoredProc(_connectionString.Facturacion, "getFacturaSinEnviarTurno",
                         parameters);
            var facturas = _convertidor.ConvertirFactura(dt2);
            foreach (var factura in facturas)
            {
                DataTable dt = LoadDataTableFromStoredProc(_connectionString.estacion, "getVentaPorId",
                           new Dictionary<string, object>{
                {"@CONSECUTIVO",factura.ventaId }
                           });

                var ventas = _convertidor.ConvertirVenta(dt);
                var manguera = _convertidor.ConvertirManguera(dt).FirstOrDefault();
                factura.Venta = ventas.FirstOrDefault();
                factura.Manguera = manguera;
            }
            return facturas.ToList();
        }

        public void ActuralizarFacturasEnviadosTurno(int factura)
        {
            var ventasIds = new DataTable();
            ventasIds.Columns.Add(new DataColumn("ventaId", typeof(long))
            {
                AllowDBNull = false
            });
            var row = ventasIds.NewRow();
            row["ventaId"] = factura;
            ventasIds.Rows.Add(row);
            var parameters = new Dictionary<string, object>
            {
                {"@facturas",ventasIds }
            };
            DataTable dt2 = LoadDataTableFromStoredProc(_connectionString.Facturacion, "ActuralizarTurnoEnviadas",
                         parameters);
        }

        public FacturacionelectronicaCore.Negocio.Modelo.Turno ObtenerTurnoIslaPorVenta(int ventaId)
        {
            var ds = LoadDataSetFromStoredProc(_connectionString.estacion, "ObtenerTurnoIslaPorVenta",
                       new Dictionary<string, object>{
                {"@ventaId",ventaId }
                       });

            return _convertidor.ConvertirTurno(ds);
        }

        public IEnumerable<ObjetoImprimir> GetObjetoImprimir()
        {
            var ds = LoadDataTableFromStoredProc(_connectionString.Facturacion, "GetObjetoImprimir",
                       new Dictionary<string, object>{
                       });

            return _convertidor.ConvertirObjetoImprimir(ds);
        }

        internal void ActualizarObjetoImpreso(int id)
        {
            var ds = LoadDataTableFromStoredProc(_connectionString.Facturacion, "SetObjetoImpreso",
                       new Dictionary<string, object>
                       {
                {"@Id",id }
                       });

        }

        public Bolsa ObtenerBolsa(DateTime fecha, int isla, int numero)
        {
            var dt = LoadDataTableFromStoredProc(_connectionString.estacion, "getBolsanumero",
                         new Dictionary<string, object>{

                    {"@IdIsla", isla },
                    {"@fecha", fecha },
                    {"@numero", numero }
                         });

            return _convertidor.ConvertirBolsa(dt);
        }

        public Turno ObtenerTurnoCerradoPorIslaFecha(DateTime fecha, int isla)
        {
            var ds = LoadDataSetFromStoredProc(_connectionString.estacion, "ObtenerTurnoIsla",
                      new Dictionary<string, object>{
                {"@IdIsla",isla }
                      });

            return _convertidor.ConvertirTurno(ds);
        }

        public Turno ObtenerTurnoPorIslaFecha(DateTime fecha, int isla)
        {
            var ds = LoadDataSetFromStoredProc(_connectionString.estacion, "ObtenerTurnoIsla",
                       new Dictionary<string, object>{
                {"@IdIsla",isla }
                       });

            return _convertidor.ConvertirTurno(ds);
        }

        public Turno ObtenerTurnoIslaYFecha(DateTime fecha, int isla, int numero)
        {
            var ds = LoadDataSetFromStoredProc(_connectionString.estacion, "ObtenerTurnoIslaYFecha",
                       new Dictionary<string, object>{
                {"@IdIsla",isla },
                {"@num_tur",numero },
                {"@fecha",fecha }
                       });

            return _convertidor.ConvertirTurno(ds);
        }

        public CuposRequest GetInfoCupos ()
        {
            var ds = LoadDataSetFromStoredProc(_connectionString.estacion, "GetInfoCupos",
                       new Dictionary<string, object>{
                       });

            return _convertidor.ConvertirInfoCupos(ds);
        }
        public IEnumerable<FactoradorEstacionesModelo.Objetos.Factura> getFacturaPorTurno(int isla, DateTime fecha, int num)
        {

            DataTable dt2 =  LoadDataTableFromStoredProc(_connectionString.estacion, "getFacturaPorTurno",
                         new Dictionary<string, object>{

                    {"@IdIsla", isla },
                    {"@num_tur", num },
                    {"@fecha", fecha }
                         });
            var facturas = _convertidor.ConvertirFactura(dt2);
            foreach (var factura in facturas)
            {

                DataTable dt = LoadDataTableFromStoredProc(_connectionString.estacion, "getVentaPorId",
                             new Dictionary<string, object>{

                    {"@CONSECUTIVO", factura.ventaId }
                             });

                var venta = _convertidor.ConvertirVenta(dt).FirstOrDefault();
                factura.Manguera = _convertidor.ConvertirManguera(dt).FirstOrDefault();
                factura.Venta = venta;
            }
            return facturas;

        }

        public Fidelizado getFidelizado(int ventaId)
        {
DataTable dt = LoadDataTableFromStoredProc(_connectionString.estacion, "GetFidelizado",
                            new Dictionary<string, object>{

                    {"@ventaId", ventaId }
                            });
            return _convertidor.ConvertirFidelizado(dt).FirstOrDefault();
    }

        public List<FactoradorEstacionesModelo.Objetos.Tercero> BuscarTercerosNoEnviadosASiesa()
        {
            DataTable dt = LoadDataTableFromStoredProc(_connectionString.Facturacion, "BuscarTercerosNoEnviadosSiesa",
                      new Dictionary<string, object>
                      {
                      });

            return _convertidor.ConvertirTercero(dt)?.ToList();
        }

        internal void MarcarTercerosEnviadosASiesa(IEnumerable<int> terceros)
        {
            var ventasIds = new DataTable();
            ventasIds.Columns.Add(new DataColumn("ventaId", typeof(long))
            {
                AllowDBNull = false
            });
            foreach (var t in terceros)
            {
                var row = ventasIds.NewRow();
                row["ventaId"] = t;
                ventasIds.Rows.Add(row);
            }
            var parameters = new Dictionary<string, object>
            {
                {"@terceros",ventasIds }
            };
            DataTable dt2 = LoadDataTableFromStoredProc(_connectionString.Facturacion, "CambiarEstadoTerceroEnviadoSiesa",
                         parameters);
        }

        public IEnumerable<FactoradorEstacionesModelo.Objetos.Factura> BuscarFacturasNoEnviadasSiesa(DateTime? fechaMin = null, DateTime? fechaMax = null)
        {
            var parameters = new Dictionary<string, object>
            {
                {"@fechaMin", (object)fechaMin ?? DBNull.Value },
                {"@fechaMax", (object)fechaMax ?? DBNull.Value }
            };
            DataTable dt2 = LoadDataTableFromStoredProc(_connectionString.Facturacion, "getFacturaSinEnviarSiesa",
                         parameters);
            var facturas = _convertidor.ConvertirFactura(dt2);
            foreach (var factura in facturas)
            {
                DataTable dt = LoadDataTableFromStoredProc(_connectionString.estacion, "getVentaPorId",
                           new Dictionary<string, object>{
                {"@CONSECUTIVO",factura.ventaId }
                           });

                var ventas = _convertidor.ConvertirVenta(dt);
                var manguera = _convertidor.ConvertirManguera(dt).FirstOrDefault();
                factura.Venta = ventas.FirstOrDefault();
                factura.Manguera = manguera;
            }
            return facturas.ToList();
        }

        public void ActuralizarFacturasEnviadosSiesa(IEnumerable<int> facturas)
        {
            var ventasIds = new DataTable();
            ventasIds.Columns.Add(new DataColumn("ventaId", typeof(long))
            {
                AllowDBNull = false
            });
            foreach (var t in facturas)
            {
                var row = ventasIds.NewRow();
                row["ventaId"] = t;
                ventasIds.Rows.Add(row);
            }
            var parameters = new Dictionary<string, object>
            {
                {"@facturas",ventasIds }
            };
            DataTable dt2 = LoadDataTableFromStoredProc(_connectionString.Facturacion, "ActuralizarEnviadasSiesa",
                         parameters);
        }

        public string ObtenerAuxiliarContable(int codigoFormaPago, string combustible, bool factura, bool cruce)
        {
            var parameters = new Dictionary<string, object>
            {
                {"@codigoformapago",codigoFormaPago },
                {"@combustible",combustible },
                {"@factura",factura },
                {"@cruce",cruce }
            };
            DataTable dt2 = LoadDataTableFromStoredProc(_connectionString.Facturacion, "BuscarAuxiliar",
                         parameters);

            return dt2.AsEnumerable().FirstOrDefault()?.Field<string>("auxiliar");

        }

        /// <summary>
        /// Obtiene el último turno cerrado por isla.
        /// </summary>
        /// <param name="isla">Identificador de la isla</param>
        /// <returns>Turno cerrado o null</returns>
        public Turno ObtenerUltimoTurnoCerradoPorIsla(string isla)
        {
            var parameters = new Dictionary<string, object>
            {
                {"@IdIsla", isla }
            };
            var dt = LoadDataSetFromStoredProc(_connectionString.estacion, "ObtenerTurnoIslaCerrado", parameters);
            var turno = _convertidor.ConvertirTurno(dt);
            return turno;
        }

        /// <summary>
        /// Obtiene las facturas de canastilla por isla y turno.
        /// </summary>
        /// <param name="isla">Identificador de la isla</param>
        /// <param name="turno">Número de turno</param>
        /// <returns>Lista de facturas canastilla</returns>
        public List<FacturaCanastilla> GetFacturasCanastillaPorIslaTurno(string isla, int turno, int fechaTurno)
        {
            var parameters = new Dictionary<string, object>
            {
                {"@isla", isla },
                {"@turno", turno },
                {"@fechaTurno", fechaTurno }
            };
            DataTable dt = LoadDataTableFromStoredProc(_connectionString.Facturacion, "GetFacturasCanastillaIslaTurno", parameters);
            var facturas = _convertidor.ConvertirFacturaCanastilla(dt);
            return facturas?.ToList() ?? new List<FacturaCanastilla>();
        }

        /// <summary>
        /// Busca facturas canastilla no enviadas a Siesa.
        /// </summary>
        /// <returns>Lista de facturas canastilla</returns>
        public IEnumerable<FacturaCanastilla> BuscarFacturasCanastillaNoEnviadasSiesa()
        {
            var dataSet = LoadDataSetFromStoredProc(_connectionString.Facturacion, "getFacturaCanastillaSinEnviarSiesa", null);

            if (dataSet.Tables.Count == 0)
            {
                return new List<FacturaCanastilla>();
            }

            var facturas = _convertidor.ConvertirFacturaCanastilla(dataSet.Tables[0]);
            return facturas;
        }

        /// <summary>
        /// Marca facturas canastilla como enviadas a Siesa.
        /// </summary>
        /// <param name="facturas">IDs de facturas canastilla</param>
        public void ActualizarFacturasCanastillaEnviadasSiesa(IEnumerable<int> facturas)
        {
            var ventasIds = new DataTable();
            ventasIds.Columns.Add("ventaId", typeof(int));

            foreach (var factura in facturas)
            {
                ventasIds.Rows.Add(factura);
            }

            var parameters = new Dictionary<string, object>
            {
                {"@facturas", ventasIds }
            };

            DataTable dt = LoadDataTableFromStoredProc(_connectionString.Facturacion, "ActualizarEnviadasSiesaCanastilla", parameters);
        }
    }
}
