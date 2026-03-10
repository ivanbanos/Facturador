using Dominio.Entidades;
using EnviadorInformacionService.Models;
using FactoradorEstacionesModelo.Convertidor;
using FactoradorEstacionesModelo.Extensions;
using FactoradorEstacionesModelo.Fidelizacion;
using FactoradorEstacionesModelo.Objetos;
using FactoradorEstacionesModelo.Siges;
using FacturacionelectronicaCore.Repositorio.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FacturadorEstacionesRepositorio
{
    public class EstacionesRepositorioSqlServer : IEstacionesRepositorio
    {
        private readonly ILogger<EstacionesRepositorioSqlServer> _logger;

        private readonly ConnectionStrings _connectionString;

        private readonly Convertidor _convertidor;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="estacionesRepositorio"></param>
        public EstacionesRepositorioSqlServer(ILogger<EstacionesRepositorioSqlServer> logger,
            IOptions<ConnectionStrings> options)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _convertidor = new Convertidor();
            _connectionString = options.Value;
        }


        public List<Isla> getIslas()
        {
            DataTable dt = LoadDataTableFromStoredProc(_connectionString.estacion, "ObtenerIslas",
                new Dictionary<string, object>
                {

                });

            return _convertidor.ConvertirIsla(dt).ToList();
        }

        public List<Surtidor> getSurtidores(int idIsla)
        {
            DataTable dt = LoadDataTableFromStoredProc(_connectionString.estacion, "ObtenerSurtidores",
                new Dictionary<string, object>
                {
                    {"@COD_ISL", idIsla }
                });

            return _convertidor.ConvertirSurtidor(dt).ToList();
        }


        public List<Cara> getCaras()
        {
            DataTable dt = LoadDataTableFromStoredProc(_connectionString.estacion, "ObtenerCaras",
                   new Dictionary<string, object>
                   {
                   });

            return _convertidor.ConvertirCara(dt).ToList();
        }


        public List<FormasPagos> BuscarFormasPagos()
        {
            DataTable dt = LoadDataTableFromStoredProc(_connectionString.estacion, "ObtenerFormasPago",
                   new Dictionary<string, object>
                   {
                   });

            return _convertidor.ConvertirFormasPagos(dt).ToList();
        }

        public Tercero getTercero(string identificacion)
        {
            DataTable dt = LoadDataTableFromStoredProc(_connectionString.Facturacion, "ObtenerTercero",
                   new Dictionary<string, object>
                   {
                    {"@identificacion", identificacion }
                   });

            return _convertidor.ConvertirTercero(dt).FirstOrDefault();
        }

        public Tercero crearTercero(int terceroId, TipoIdentificacion tipoIdentificacion, string identificacion, string nombre, string telefono, string correo,
            string direccion,
            string COD_CLI)
        {
            DataTable dt = LoadDataTableFromStoredProc(_connectionString.Facturacion, "CrearTercero",
                      new Dictionary<string, object>
                      {
                    {"@terceroId", terceroId },
                    {"@tipoIdentificacion", tipoIdentificacion.TipoIdentificacionId },
                    {"@identificacion", identificacion },
                    {"@nombre", nombre },
                    {"@telefono", telefono },
                    {"@correo", correo },
                    {"@direccion", direccion },
                    {"@estado", "AC" },
                    {"@COD_CLI", COD_CLI },
                      });
            return _convertidor.ConvertirTercero(dt).FirstOrDefault();
        }

        public List<TipoIdentificacion> getTiposIdentifiaciones()
        {
            DataTable dt = LoadDataTableFromStoredProc(_connectionString.Facturacion, "ObtenerTipoIdentificaciones",
                   new Dictionary<string, object>
                   {
                   });

            return _convertidor.ConvertirTipoIdentificacion(dt).ToList();
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
                    _logger.LogError(e, $"LoadDataTableFromStoredProcAsync: Proc '{procName}' with exception. {e.Message} ");
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
                catch (Exception e)
                {
                    _logger.LogError(e, $"LoadDataTableFromStoredProcAsync: Proc '{procName}' with exception. {e.Message} ");
                    throw e;
                }
            }

            return dt;
        }

        private SqlCommand GetSqlStoredProcCommand(string commandText, SqlConnection conn)
        {
            SqlCommand command = new SqlCommand(commandText, conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            return command;
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

        public List<Factura> getUltimasFacturas(short cOD_CAR, int v)
        {
            DataTable dt = LoadDataTableFromStoredProc(_connectionString.estacion, "ObtenerVentaPorCara",
                         new Dictionary<string, object>{

                    {"@COD_CAR", cOD_CAR }
                         });

            var venta = _convertidor.ConvertirVenta(dt).FirstOrDefault();
            if(venta == null)
            {
                return new List<Factura>() { new Factura() { Venta = new Venta() { CONSECUTIVO = -1 } } };
            }
            DataTable dt2 = LoadDataTableFromStoredProc(_connectionString.Facturacion, "ObtenerFacturaPorVenta",
                         new Dictionary<string, object>{

                    {"@ventaId", venta.CONSECUTIVO }
                         });
            var factura = _convertidor.ConvertirFactura(dt2).FirstOrDefault();
            if(factura == null)
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
            if (factura == null)
            {
                return new List<Factura>();
            }
            factura.Manguera = _convertidor.ConvertirManguera(dt).FirstOrDefault();
            factura.Venta = venta;
            return new List<Factura>() { factura };
        }

        public void ActualizarFactura(int facturaPOSId, string placa, string kilometraje, int formaPago, int terceroId, int ventaId)
        {
            DataTable dt = LoadDataTableFromStoredProc(_connectionString.Facturacion, "ActualizarFactura",
                            new Dictionary<string, object>{

                    {"@facturaPOSId", facturaPOSId },

                    {"@Placa", placa },

                    {"@Kilometraje", kilometraje },

                    {"@codigoFormaPago", formaPago },

                    {"@terceroId", terceroId },
                                {"@ventaId", ventaId }
                            });
        }

        public void MandarImprimir(int ventaId)
        {
            LoadDataTableFromStoredProc(_connectionString.Facturacion, "MandarImprimir",
                            new Dictionary<string, object>{

                    {"@ventaId", ventaId }
                            });
        }
        public void enviarFacturacionSiigo(int ventaId)
        {
            LoadDataTableFromStoredProc(_connectionString.Facturacion, "enviarFacturacionSiigo",
                            new Dictionary<string, object>{

                    {"@ventaId", ventaId }
                            });
        }

        public void ConvertirAFactura(int ventaId)
        {
            LoadDataTableFromStoredProc(_connectionString.Facturacion, "ConvertirAFactura",
                               new Dictionary<string, object>{

                    {"@ventaId", ventaId }
                               });
        }
        public void ConvertirAOrden(int ventaId)
        {
            LoadDataTableFromStoredProc(_connectionString.Facturacion, "ConvertirAOrden",
                               new Dictionary<string, object>{

                    {"@ventaId", ventaId }
                               });
        }


        public List<MangueraSiges> GetMangueras(int id)
        {
            DataTable dt = LoadDataTableFromStoredProc(_connectionString.EstacionSiges, "ObtenerMangueras",
                   new Dictionary<string, object>
                   {{"@IdCara", id }
                   });

            return _convertidor.ConvertirManguerasSiges(dt);
        }

        public void AgregarVenta(int idManguera, double cantidadventa, string iButton)
        {
            LoadDataTableFromStoredProc(_connectionString.EstacionSiges, "AgregarVenta",
                               new Dictionary<string, object>{

                    {"@IdManguera", idManguera },
                    {"@cantidad", cantidadventa },
                    {"@IButton", iButton }
                               });
        }

        List<CaraSiges> IEstacionesRepositorio.GetCarasSiges()
        {
            DataTable dt = LoadDataTableFromStoredProc(_connectionString.EstacionSiges, "ObtenerCaras",
                   new Dictionary<string, object>
                   {
                   });

            return _convertidor.ConvertirCarasSiges(dt);
        }

        public List<SurtidorSiges> GetSurtidoresSiges()
        {
            DataTable dt = LoadDataTableFromStoredProc(_connectionString.EstacionSiges, "ObtenerSurtidores",
                   new Dictionary<string, object>
                   {
                   });

            return _convertidor.ConvertirSurtidoresSiges(dt);
        }

        public List<FormaPagoSiges> BuscarFormasPagosSiges()
        {
            DataTable dt = LoadDataTableFromStoredProc(_connectionString.EstacionSiges, "BuscarFormasPagosSiges",
                   new Dictionary<string, object>
                   {
                   });

            return _convertidor.ConvertirFormaPagoSiges(dt);
        }

        public List<FacturaSiges> getUltimasFacturasSiges(int idCara, int cantidad)
        {
            DataTable dt = LoadDataTableFromStoredProc(_connectionString.EstacionSiges, "ObtenerFacturaPorVenta",
                            new Dictionary<string, object>{

                    {"@idCara", idCara }
                            });
            return _convertidor.ConvertirFacturasSiges(dt);
        }

        public FacturaSiges getFacturasImprimir()
        {
            var parameters = new Dictionary<string, object>
            {
            };
            DataTable dt2 = LoadDataTableFromStoredProc(_connectionString.Facturacion, "getFacturaImprimir",
                         parameters);
            var facturas = _convertidor.ConvertirFacturasSiges(dt2);
            return facturas.FirstOrDefault();
        }

        public void SetFacturaImpresa(int ventaId)
        {
            var parameters = new Dictionary<string, object>
            {
                {"@ventaid",ventaId }
            };
            DataTable dt2 = LoadDataTableFromStoredProc(_connectionString.Facturacion, "SetFacturaImpresa",
                         parameters);
        }

        public void CerrarTurno(Isla isla, string codigo, float totalizador)
        {
            var parameters = new Dictionary<string, object>
            {
                {"@codigo",codigo },
                {"@IdIsla",isla.idIsla }
            };
            DataTable dt2 = LoadDataTableFromStoredProc(_connectionString.Facturacion, "CerrarTurno",
                         parameters);
        }

        public void AbrirTurno(Isla isla, string codigo, float totalizador)
        {
            var parameters = new Dictionary<string, object>
            {
                {"@codigo",codigo },
                {"@IdIsla",isla.idIsla }
            };
            DataTable dt2 = LoadDataTableFromStoredProc(_connectionString.Facturacion, "AbrirTurno",
                         parameters);
        }

        public TurnoSiges ObtenerTurnoSurtidor(int id)
        {
            DataTable dt = LoadDataTableFromStoredProc(_connectionString.EstacionSiges, "ObtenerTurnoSurtidor",
                            new Dictionary<string, object>{

                    {"@idSurtidor", id }
                            });
            var turno = _convertidor.ConvertirTurnoSiges(dt).FirstOrDefault();
            if (turno != null)
            {
                var parameters2 = new Dictionary<string, object>
                {
                 {"@Id",turno.Id },
                };
                DataTable dt3 = LoadDataTableFromStoredProc(_connectionString.Facturacion, "GetTurnoSurtidorInfo",
                             parameters2);
                turno.turnoSurtidores = _convertidor.ConvertirTurnoSurtidoresSiges(dt3);
            }

            return turno;
        }

        public void EnviarTotalizadorCierre(int idSurtidor, int? idTurno, int idManguera, double total)
        {
            var parameters = new Dictionary<string, object>
            {
                {"@idSurtidor",idSurtidor },
                {"@IdTurno",idTurno },
                {"@idManguera",idManguera },
                {"@totalizador",total }
            };
            DataTable dt2 = LoadDataTableFromStoredProc(_connectionString.Facturacion, "SetTotalizadorTurnoSurtidorCierrre",
                         parameters);
        }

        public void EnviarTotalizadorApertura(int idSurtidor, int? idTurno, int idManguera, double total)
        {
            var parameters = new Dictionary<string, object>
            {
                 {"@idSurtidor",idSurtidor },
                {"@IdTurno",idTurno },
                {"@idManguera",idManguera },
                {"@totalizador",total }
            };
            DataTable dt2 = LoadDataTableFromStoredProc(_connectionString.Facturacion, "SetTotalizadorTurnoSurtidorApertura",
                         parameters);
        }

        public List<FacturaSiges> getVentaSinSubirSICOM()
        {
            var parameters = new Dictionary<string, object>
            {
            };
            DataTable dt2 = LoadDataTableFromStoredProc(_connectionString.Facturacion, "GetVentasSinSubir",
                         parameters);
            var facturas = _convertidor.ConvertirVentasSiges(dt2);
            return facturas;
        }

        public void actualizarVentaSubidaSicom(int ventaId)
        {
            var parameters = new Dictionary<string, object>
            {
                 {"@Id",ventaId },
            };
            DataTable dt2 = LoadDataTableFromStoredProc(_connectionString.Facturacion, "ActualizarVentaSubidaSicom",
                         parameters);
        }

        public void ActualizarCarros(List<VehiculoSuic> vehiculos)
        {
            var table = CommonExtensions.ConvertToDataTable(vehiculos.ToArray());

            var parameters = new Dictionary<string, object>
            {
                 {"@VehiculosType",table },
            };
            DataTable dt2 = LoadDataTableFromStoredProc(_connectionString.Facturacion, "ActualizarCarros",
                         parameters);
        }

        public TurnoSiges getTurnosSinImprimir()
        {
            var parameters = new Dictionary<string, object>
            {
            };
            DataTable dt2 = LoadDataTableFromStoredProc(_connectionString.Facturacion, "GetTurnoImprimir",
                         parameters);
            var turno = _convertidor.ConvertirTurnoSiges(dt2).FirstOrDefault();
            if(turno!= null)
            {
                var parameters2 = new Dictionary<string, object>
                {
                 {"@Id",turno.Id },
                };
                DataTable dt3 = LoadDataTableFromStoredProc(_connectionString.Facturacion, "GetTurnoSurtidorInfo",
                             parameters2);
                turno.turnoSurtidores = _convertidor.ConvertirTurnoSurtidoresSiges(dt3);
            }
            return turno;
        }

        public VehiculoSuic GetVehiculoSuic(string iButton)
        {
            var parameters = new Dictionary<string, object>
            {
                 {"@idrom",iButton },
            };
            DataTable dt2 = LoadDataTableFromStoredProc(_connectionString.Facturacion, "GetVehiculoSuic",
                         parameters);
            return _convertidor.ConvertirVehiculoSiges(dt2).FirstOrDefault();
        }

        public void ActualizarTurnoImpreso(int id)
        {
            var parameters = new Dictionary<string, object>
            {
                 {"@Id",id },
            };
            DataTable dt2 = LoadDataTableFromStoredProc(_connectionString.Facturacion, "ActualizarTurnoImpreso",
                         parameters);
        }

        public Resolucion BuscarResolucionActiva(IEnumerable<Resolucion> resolucionesRemota)
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

        public bool HayFacturasCanastillaPorImprimir()
        {
            DataTable dt = LoadDataTableFromStoredProc(_connectionString.Facturacion, "FacturasCanastillaPorImprimir",
                         new Dictionary<string, object>
                         {
                         });
            return dt.AsEnumerable().Count() > 0;
        }

        public List<Tercero> BuscarTercerosNoEnviados()
        {
            DataTable dt = LoadDataTableFromStoredProc(_connectionString.Facturacion, "BuscarTercerosNoEnviados",
                      new Dictionary<string, object>
                      {
                      });

            return _convertidor.ConvertirTercero(dt).ToList();
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

        public List<Factura> BuscarFacturasNoEnviadas()
        {
            var parameters = new Dictionary<string, object>
            {
            };
            DataTable dt2 = LoadDataTableFromStoredProc(_connectionString.Facturacion, "getFacturaSinEnviar",
                         parameters);
            var facturas = _convertidor.ConvertirFactura(dt2);
            foreach (Factura factura in facturas)
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


        public FacturaCanastilla BuscarFacturaCanastillaPorConsecutivo(int consecutivo)
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

        
        public IEnumerable<FacturaCanastilla> BuscarFacturasNoEnviadasCanastilla()
        {
            var parameters = new Dictionary<string, object>
            {
            };
            DataTable dt = LoadDataTableFromStoredProc(_connectionString.Facturacion, "getFacturaEnviarCanastilla",
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
            return facturasEnviar;
        }

        public void SetFacturaCanastillaEnviada(int facturasCanastillaId)
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

        public void ActuralizarFacturasEnviadosCanastilla(IEnumerable<int> facturas)
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

        public void ActuralizarTerceros(Tercero tercero)
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
                    {"@telefono", tercero.Telefono },
                    {"@correo", tercero.Correo },
                    {"@direccion", tercero.Direccion },
                    {"@estado", "AC" },
                    {"@COD_CLI", tercero.COD_CLI },
                         });
        }
       
        public void AgregarFacturaDesdeIdVenta()
        {
            DataTable dt = LoadDataTableFromStoredProc(_connectionString.estacion, "AgregarFacturaDesdeIdVenta",
                     new Dictionary<string, object>
                     {
                     });
        }



        public List<Factura> BuscarFacturasNoEnviadasFacturacion()
        {
            var parameters = new Dictionary<string, object>
            {
            };
            DataTable dt2 = LoadDataTableFromStoredProc(_connectionString.Facturacion, "getFacturaSinEnviadaFacturacion",
                         parameters);
            var facturas = _convertidor.ConvertirFactura(dt2);





            foreach (Factura factura in facturas)
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


        public List<FacturaFechaReporte> BuscarFechasReportesNoEnviadasSiges()
        {
            var parameters = new Dictionary<string, object>
            {
            };
            DataTable dt2 = LoadDataTableFromStoredProc(_connectionString.estacion, "BuscarFechasReportesNoEnviadasSiges",
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


        public IEnumerable<Factura> getFacturasSiigo()
        {
            var parameters = new Dictionary<string, object>
            {
            };
            DataTable dt2 = LoadDataTableFromStoredProc(_connectionString.Facturacion, "getFacturaSiigo",
                         parameters);
            var facturas = _convertidor.ConvertirFactura(dt2);




            Thread.Sleep(3000);
            foreach (Factura factura in facturas)
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
            return facturas;
        }

        public void SetFacturaCanastillaImpresa(int facturasCanastillaId)
        {
            var parameters = new Dictionary<string, object>
            {
                {"@facturaCanastillaId",facturasCanastillaId }
            };
            DataTable dt2 = LoadDataTableFromStoredProc(_connectionString.Facturacion, "SetFacturaCanastillaImpresa",
                         parameters);
        }


        public FacturaCanastilla getFacturasCanastillaImprimir()
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

        public List<FacturaSiges> BuscarFacturasNoEnviadasSiges()
        {
            var parameters = new Dictionary<string, object>
            {
            };
            DataTable dt2 = LoadDataTableFromStoredProc(_connectionString.Facturacion, "getFacturaSinEnviar",
                         parameters);
            return _convertidor.ConvertirFacturasSiges(dt2);
        }

        public void MarcarTercerosEnviadosASiesa(IEnumerable<int> ids)
        {
            var table = new DataTable();
            table.Columns.Add(new DataColumn("ventaId", typeof(int)));
            foreach (var id in ids)
            {
                var row = table.NewRow();
                row["ventaId"] = id;
                table.Rows.Add(row);
            }
            var parameters = new Dictionary<string, object>
            {
                {"@terceros", table }
            };
            LoadDataTableFromStoredProc(_connectionString.Facturacion, "CambiarEstadoTerceroEnviadoSiesaSiges", parameters);
        }

        public string ObtenerAuxiliarContable(int codigoFormaPago, string combustible, bool contable, bool cruce)
        {
            var parameters = new Dictionary<string, object>
            {
                {"@codigoFormaPago", codigoFormaPago },
                {"@combustible", combustible },
                {"@factura", contable },
                {"@cruce", cruce }
            };
            DataTable dt = LoadDataTableFromStoredProc(_connectionString.Facturacion, "BuscarAuxiliar", parameters);
            return dt.AsEnumerable().Select(dr => dr.Field<string>("auxiliar")).FirstOrDefault();
        }

        public void ActualizarFacturaSiesa(int ventaId)
        {
            var parameters = new Dictionary<string, object>
            {
                {"@ventaId", ventaId }
            };
            LoadDataTableFromStoredProc(_connectionString.Facturacion, "ActualizarFacturaSiesa", parameters);
        }

        public TurnoSiges ObtenerTurnoIsla(int idIsla)
        {
            DataTable dt = LoadDataTableFromStoredProc(_connectionString.EstacionSiges, "ObtenerTurnoIsla",
                            new Dictionary<string, object>{

                    {"@idIsla", idIsla }
                            });
            var turno = _convertidor.ConvertirTurnoSiges(dt).FirstOrDefault();
            if (turno != null)
            {
                var parameters2 = new Dictionary<string, object>
                {
                 {"@Id",turno.Id },
                };
                DataTable dt3 = LoadDataTableFromStoredProc(_connectionString.Facturacion, "GetTurnoSurtidorInfo",
                             parameters2);
                turno.turnoSurtidores = _convertidor.ConvertirTurnoSurtidoresSiges(dt3);
            }

            return turno;
        }

        public void AddFidelizado(string documento, float? puntos)
        {
            var parameters = new Dictionary<string, object>
            {
                {"@documento", documento },
                {"@puntos", puntos }
            };
            LoadDataTableFromStoredProc(_connectionString.Facturacion, "AddFidelizado", parameters);
        }

        public Puntos GetVentaFidelizarAutomatica(int id)
        {
            DataTable dt = LoadDataTableFromStoredProc(_connectionString.estacion, "GetVentaFidelizarAutomatica",
                                        new Dictionary<string, object>{

                    {"@idManguera", id }
                                        });
            return _convertidor.ConvertirPuntos(dt).FirstOrDefault();
        }

        public Fidelizado getFidelizado(int ventaId)
        {
            DataTable dt = LoadDataTableFromStoredProc(_connectionString.estacion, "GetFidelizado",
                                        new Dictionary<string, object>{

                    {"@ventaId", ventaId }
                                        });
            return _convertidor.ConvertirFidelizado(dt).FirstOrDefault();
        }

        public List<FacturaSiges> GetReporteCierrePorTotal(int id)
        {
            var parameters = new Dictionary<string, object>
            {
                {"@idTurno", id }
            };
            DataTable dt = LoadDataTableFromStoredProc(_connectionString.Facturacion, "GetReporteCierrePorTotal", parameters);
            return _convertidor.ConvertirFacturasSiges(dt);
        }

        public IEnumerable<FacturaSiges> GetFacturasPorFechas(DateTime desde, DateTime hasta)
        {
            var parameters = new Dictionary<string, object>
            {
                {"@fechaInicio", desde },
                {"@fechaFin", hasta }
            };
            DataTable dt = LoadDataTableFromStoredProc(_connectionString.Facturacion, "GetFacturasPorFechas", parameters);
            return _convertidor.ConvertirFacturasSiges(dt);
        }

        public IEnumerable<TurnoSiges> GetTurnosByFechas(DateTime desde, DateTime hasta)
        {
            var parameters = new Dictionary<string, object>
            {
                {"@fechaInicio", desde },
                {"@fechaFin", hasta }
            };
            DataTable dt = LoadDataTableFromStoredProc(_connectionString.Facturacion, "GetTurnosPorFechas", parameters);
            return _convertidor.ConvertirTurnoSiges(dt);
        }

        public IEnumerable<TurnoSurtidor> ObtenerTurnoInfo(int id)
        {
            var parameters = new Dictionary<string, object>
            {
                {"@Id", id }
            };
            DataTable dt = LoadDataTableFromStoredProc(_connectionString.Facturacion, "GetTurnoSurtidorInfo", parameters);
            return _convertidor.ConvertirTurnoSurtidoresSiges(dt);
        }

        public IEnumerable<FacturaSiges> BuscarFacturasNoEnviadasSiesa()
        {
            var parameters = new Dictionary<string, object>
            {
            };
            DataTable dt2 = LoadDataTableFromStoredProc(_connectionString.Facturacion, "getFacturaSinEnviarSiesaSiges",
                         parameters);
            return _convertidor.ConvertirFacturasSiges(dt2);
        }

        public void ActuralizarFacturasEnviadosSiesa(List<int> facturasEnviadas)
        {
            var ventasIds = new DataTable();
            ventasIds.Columns.Add(new DataColumn("ventaId", typeof(long))
            {
                AllowDBNull = false
            });
            foreach (var t in facturasEnviadas)
            {
                var row = ventasIds.NewRow();
                row["ventaId"] = t;
                ventasIds.Rows.Add(row);
            }
            var parameters = new Dictionary<string, object>
            {
                {"@facturas",ventasIds }
            };
            DataTable dt2 = LoadDataTableFromStoredProc(_connectionString.Facturacion, "ActuralizarEnviadasSiesaSiges",
                         parameters);
        }
    }
}
