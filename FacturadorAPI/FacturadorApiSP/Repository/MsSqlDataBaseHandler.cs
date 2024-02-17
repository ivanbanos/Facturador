using Dominio.Entidades;
using FactoradorEstacionesModelo.Fidelizacion;
using FactoradorEstacionesModelo.Objetos;
using FacturadorAPI.Models;
using FacturadorAPI.Repository;
using MachineUtilizationApi.Config;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Data;
using System.Runtime;

namespace MachineUtilizationApi.Repository
{
    public class MsSqlDataBaseHandler : BaseRepository, IDataBaseHandler
    {
        private readonly ILogger<MsSqlDataBaseHandler> _logger;
        private readonly IOptions<RepositoriesConfig> _repositoriesConfig;

        public MsSqlDataBaseHandler(IOptions<ConnectionStringSettings> settings, ILogger<MsSqlDataBaseHandler> logger, IOptions<RepositoriesConfig> repositoriesConfig) : base(settings, logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _repositoriesConfig = repositoriesConfig ?? throw new ArgumentNullException(nameof(repositoriesConfig));
        }

        public async Task<IEnumerable<TipoIdentificacion>> ListarTiposIdentificacion(CancellationToken cancellationToken)
        {
            ConnectionString = _settings.Facturacion;
            var reqDict = new Dictionary<string, object>
            {
            };


            DataTable dt = await LoadDataTableFromStoredProcAsync("ObtenerTipoIdentificaciones", reqDict);
            return dt.ConvertirTipoIdentificacion();
        }

        public async Task<IEnumerable<FormaPagoSiges>> ListarFormasPagoSiges(CancellationToken cancellationToken)
        {
            ConnectionString = _settings.Facturacion;
            var reqDict = new Dictionary<string, object>
            {
            };


            DataTable dt = await LoadDataTableFromStoredProcAsync("BuscarFormasPagosSiges", reqDict);
            return dt.ConvertirFormaPagoSiges();
        }

        public async Task<IEnumerable<CaraSiges>> ListarCarasSigues(CancellationToken cancellationToken)
        {
            ConnectionString = _settings.Facturacion;
            var reqDict = new Dictionary<string, object>
            {
            };
            DataTable dt = await LoadDataTableFromStoredProcAsync("ObtenerCaras", reqDict);
            return dt.ConvertirCarasSiges();
        }

        public async Task ActualizarCanastilla(IEnumerable<Canastilla> canastillas)
        {
            ConnectionString = _settings.Facturacion;
            var ventasIds = new DataTable();
            ventasIds.Columns.Add(new DataColumn("CanastillaId", typeof(int)));
            ventasIds.Columns.Add(new DataColumn("Guid", typeof(string)));
            ventasIds.Columns.Add(new DataColumn("descripcion", typeof(string))
            {
                AllowDBNull = false
            });
            ventasIds.Columns.Add(new DataColumn("unidad", typeof(string))
            {
                AllowDBNull = false
            });
            ventasIds.Columns.Add(new DataColumn("precio", typeof(float))
            {
                AllowDBNull = false
            });
            ventasIds.Columns.Add(new DataColumn("deleted", typeof(bool))
            {
                AllowDBNull = false
            });
            ventasIds.Columns.Add(new DataColumn("iva", typeof(int))
            {
                AllowDBNull = false
            });
            ventasIds.Columns.Add(new DataColumn("cantidad", typeof(float)));
            foreach (var t in canastillas)
            {
                var row = ventasIds.NewRow();
                row["Guid"] = t.guid;
                row["descripcion"] = t.descripcion;
                row["unidad"] = t.unidad;
                row["precio"] = t.precio;
                row["deleted"] = t.deleted;
                row["iva"] = t.iva;
                ventasIds.Rows.Add(row);
            }
            var parameters = new Dictionary<string, object>
            {
                {"@canastillas",ventasIds }
            };
            DataTable dt = await LoadDataTableFromStoredProcAsync( "UpdateOrCreateCanastilla",
                         parameters);
        }

        public async Task<IEnumerable<Canastilla>> GetCanastillas()
        {
            ConnectionString = _settings.Facturacion;
            var reqDict = new Dictionary<string, object>
            {
            };
            DataTable dt = await LoadDataTableFromStoredProcAsync("GetCanastilla", reqDict);


            return dt.ConvertirCanastilla();
        }

        public async Task<IEnumerable<Tercero>> ObtenerTerceroPorIDentificacion(string identificacion, CancellationToken cancellationToken)
        {
            ConnectionString = _settings.Facturacion;
            var reqDict = new Dictionary<string, object>
            {
                    {"@identificacion", identificacion }
            };
            DataTable dt = await LoadDataTableFromStoredProcAsync("ObtenerTercero", reqDict);


            return dt.ConvertirTercero();
        }

        public async Task<FacturaSiges> ObtenerUltimaFacturaPorCara(int idCara, CancellationToken cancellationToken)
        {
            ConnectionString = _settings.Facturacion;
            DataTable dt = await LoadDataTableFromStoredProcAsync("ObtenerFacturaPorVenta",
                            new Dictionary<string, object>{

                    {"@idCara", idCara }
                            });
            return dt.ConvertirFacturasSiges().FirstOrDefault();
        }

        

        public async Task ConvertirAFactura(int idFactura)
        {
            ConnectionString = _settings.Facturacion;
            await LoadDataTableFromStoredProcAsync( "ConvertirAFactura",
            new Dictionary<string, object>{

                    {"@ventaId", idFactura }
                               });
        }

        public async Task ConvertirAOrder(int idFactura)
        {
            ConnectionString = _settings.Facturacion;
            await LoadDataTableFromStoredProcAsync("ConvertirAOrden",
            new Dictionary<string, object>{

                    {"@ventaId", idFactura }
                               });
        }

        public async Task<Factura> GetFacturaPorIdVenta(int idFactura)
        {
            ConnectionString = _settings.Facturacion;
            DataTable dt = await LoadDataTableFromStoredProcAsync("GetFacturaPorIdVenta",
                            new Dictionary<string, object>{

                    {"@idVenta", idFactura }
                            });
            var factura = dt.ConvertirFactura().FirstOrDefault();
            ConnectionString = _settings.Estacion;
            DataTable dt2 = await LoadDataTableFromStoredProcAsync("getVentaPorId",
                           new Dictionary<string, object>{
                {"@CONSECUTIVO",factura.ventaId }
                           });

            factura.Venta = dt2.ConvertirVentaSP().FirstOrDefault();

            factura.Manguera = dt2.ConvertirManguera().FirstOrDefault();
            return factura;
        }

        public async Task ActuralizarFacturasEnviados(List<int> facturas)
        {
            ConnectionString = _settings.Facturacion;
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
            await LoadDataTableFromStoredProcAsync("SetFacturaCanastillaEnviada",
                         parameters);
        }

        public async Task MandarImprimir(int idVenta)
        {
            ConnectionString = _settings.Facturacion;
            await LoadDataTableFromStoredProcAsync("MandarImprimir",
                            new Dictionary<string, object>{

                    {"@ventaId", idVenta }
                            });
        }

        public async Task GenerarFacturaCanastilla(FacturaCanastilla facturaCanastilla, bool imprimir)
        {
            ConnectionString = _settings.Facturacion;
            var ventasIds = new DataTable();
            ventasIds.Columns.Add(new DataColumn("CanastillaId", typeof(int)));
            ventasIds.Columns.Add(new DataColumn("Guid", typeof(string)));
            ventasIds.Columns.Add(new DataColumn("descripcion", typeof(string))
            {
                AllowDBNull = false
            });
            ventasIds.Columns.Add(new DataColumn("unidad", typeof(string))
            {
                AllowDBNull = false
            });
            ventasIds.Columns.Add(new DataColumn("precio", typeof(float))
            {
                AllowDBNull = false
            });
            ventasIds.Columns.Add(new DataColumn("deleted", typeof(bool))
            {
                AllowDBNull = false
            });
            ventasIds.Columns.Add(new DataColumn("iva", typeof(int))
            {
                AllowDBNull = false
            });
            ventasIds.Columns.Add(new DataColumn("cantidad", typeof(float)));
            foreach (var t in facturaCanastilla.canastillas)
            {
                var row = ventasIds.NewRow();
                row["CanastillaId"] = t.canastillaId;
                row["Guid"] = t.canastillaGuid;
                row["descripcion"] = t.descripcion;
                row["unidad"] = t.unidad;
                row["precio"] = t.precio;
                row["deleted"] = t.deleted;
                row["iva"] = t.iva;
                row["cantidad"] = t.cantidad;
                ventasIds.Rows.Add(row);
            }
            var parameters = new Dictionary<string, object>
            {
                {"@canastillaIds",ventasIds},

                {"@terceroId",facturaCanastilla.terceroId},

                {"@COD_FOR_PAG",facturaCanastilla.codigoFormaPago},

                {"@imprimir", imprimir},

                {"@descuento",facturaCanastilla.descuento}
            };
            DataTable dt = await LoadDataTableFromStoredProcAsync( "CrearFacturaCanastilla",
                         parameters);

        }

        public async Task<Tercero> CrearTercero(Tercero tercero)
        {
            ConnectionString = _settings.Facturacion;
            DataTable dt = await LoadDataTableFromStoredProcAsync( "CrearTercero",
                      new Dictionary<string, object>
                      {
                    {"@terceroId", tercero.terceroId },
                    {"@tipoIdentificacion", tercero.tipoIdentificacion??1 },
                    {"@identificacion", tercero.identificacion },
                    {"@nombre", tercero.Nombre },
                    {"@telefono", tercero.Telefono },
                    {"@correo", tercero.Correo },
                    {"@direccion", tercero.Direccion },
                    {"@estado", "AC" },
                    {"@COD_CLI", tercero.COD_CLI },
                      });
            return dt.ConvertirTercero().FirstOrDefault();
        }

        public async Task<IEnumerable<SurtidorSiges>> ListarSurtidoresSigues(CancellationToken cancellationToken)
        {
            ConnectionString = _settings.Facturacion;
            var reqDict = new Dictionary<string, object>
            {
            };
            DataTable dt = await LoadDataTableFromStoredProcAsync("ObtenerSurtidores", reqDict);
            return dt.ConvertirSurtidoresSiges();
        }

        public async Task<IEnumerable<TurnoSiges>> GetTurnosByFechas(DateTime fechaInicio, DateTime fechaFin)
        {
            ConnectionString = _settings.Facturacion;
            DataTable dt = await LoadDataTableFromStoredProcAsync("GetTurnosPorFecha",
                            new Dictionary<string, object>{

                    {"@fechaInicio", fechaInicio },
                    {"@fechaFin", fechaFin }
                            });
            return dt.ConvertirTurnoSiges();
        }

        public async Task<IEnumerable<TurnoSurtidor>> GetTurnoSurtidorInfo(int id)
        {
            ConnectionString = _settings.Facturacion;
            DataTable dt = await LoadDataTableFromStoredProcAsync("GetTurnoSurtidorInfo",
                            new Dictionary<string, object>{

                    {"@Id", id }
                            });
            return dt.ConvertirTurnoSurtidoresSiges();
        }

        public async Task<IEnumerable<FacturaSiges>> GetFacturasPorFechas(DateTime fechaInicio, DateTime fechaFin)
        {
            ConnectionString = _settings.Facturacion;
            DataTable dt = await LoadDataTableFromStoredProcAsync("GetFacturasPorFechas",
                            new Dictionary<string, object>{

                    {"@fechaInicio", fechaInicio },
                    {"@fechaFin", fechaFin }
                            });
            return dt.ConvertirFacturasSiges();
        }

        public async Task ActualizarFactura(int facturaPOSId, int terceroId, int codigoFormaPago, int idVenta, string placa, string kilometraje)
        {
            ConnectionString = _settings.Facturacion;
            await LoadDataTableFromStoredProcAsync("ActualizarFactura",
                            new Dictionary<string, object>{

                    {"@facturaPOSId", facturaPOSId },
                    {"@Placa", placa },
                    {"@Kilometraje", kilometraje },
                    {"@codigoFormaPago", codigoFormaPago },
                    {"@terceroId", terceroId },
                    {"@ventaId", idVenta }
                            });
        }

        public async Task<Puntos> GetVentaFidelizarAutomaticaPorVenta(int idVenta)
        {
            ConnectionString = _settings.Facturacion;
            DataTable dt = await LoadDataTableFromStoredProcAsync("GetVentaFidelizarAutomaticaPorVenta",
                            new Dictionary<string, object>{

                    {"@idVenta", idVenta },
                            });
            return dt.ConvertirPuntos().FirstOrDefault();
        }

        public async Task AddFidelizado(string documento, float puntos)
        {
            await LoadDataTableFromStoredProcAsync("AddFidelizado",
                            new Dictionary<string, object>{

                {"@documento",documento },
                {"@puntos",puntos }
                            });
        }

        public async Task<Tercero> GetTerceroByQuery(string identificacion)
        {
            ConnectionString = _settings.Facturacion;
            var reqDict = new Dictionary<string, object>
            {
                    {"@identificacion", identificacion }
            };
            DataTable dt = await LoadDataTableFromStoredProcAsync("GetTerceroByQuery", reqDict);


            return dt.ConvertirTercero().FirstOrDefault();
        }

        public async Task AbrirTurno(int isla, string codigo)
        {
            ConnectionString = _settings.Facturacion;
            await LoadDataTableFromStoredProcAsync("AbrirTurno",
                            new Dictionary<string, object>{

                {"@codigo",codigo },
                {"@IdIsla",isla }
                            });
        }

        public async Task CerrarTurno(int isla, string codigo)
        {
            ConnectionString = _settings.Facturacion;
            await LoadDataTableFromStoredProcAsync("CerrarTurno",
                            new Dictionary<string, object>{

                {"@codigo",codigo },
                {"@IdIsla",isla }
                            });
        }

        public async Task MandarImprimirConsecutivo(string consecutivo)
        {
            ConnectionString = _settings.Facturacion;
            DataTable dt = await LoadDataTableFromStoredProcAsync("MandarImprimirConsecutivo",
                            new Dictionary<string, object>{

                    {"@consecutivo", consecutivo }
                            });
        }


        public async Task ReimprimirTurno(DateTime fecha, int idIsla, int posicion)
        {
            ConnectionString = _settings.Facturacion;
            DataTable dt = await LoadDataTableFromStoredProcAsync("ReimprimirTurno",
                            new Dictionary<string, object>{

                    {"@Fecha", fecha },
                    {"@IdIsla", idIsla },
                    {"@posicion", posicion }
                            });
        }

        public async Task<Fidelizado> GetFidelizado(int ventaId)
        {
            ConnectionString = _settings.Facturacion;
            DataTable dt = await LoadDataTableFromStoredProcAsync("GetFidelizado",
                            new Dictionary<string, object>{

                    {"@ventaId", ventaId },
                            });
            return dt.ConvertirFidelizado().FirstOrDefault(); 
        }

        public async Task ActualizarFacturaFidelizada(string identificacion, int ventaId)
        {
            ConnectionString = _settings.Facturacion;
            await LoadDataTableFromStoredProcAsync("ActualizarFacturaFidelizada",
                            new Dictionary<string, object>{

                {"@identificacion",identificacion },
                {"@ventaId",ventaId }
                            });
        }

        public async Task<IEnumerable<IslaSiges>> getIslas()
        {
            ConnectionString = _settings.Estacion;
            DataTable dt = await LoadDataTableFromStoredProcAsync( "ObtenerIslas",
                new Dictionary<string, object>
                {

                });

            return dt.ConvertirIsla();
        }

        public async Task<FacturaSiges> GetVenta(int idVenta)
        {
            ConnectionString = _settings.Estacion;
            DataTable dt = await LoadDataTableFromStoredProcAsync("getVentaPorId",
                           new Dictionary<string, object>{
                {"@CONSECUTIVO",idVenta }
                           });

            return dt.ConvertirVenta().FirstOrDefault();
        }

        public async Task<IEnumerable<FormaPagoSiges>> ListarFormasPagoSP(CancellationToken cancellationToken)
        {
            ConnectionString = _settings.Estacion;
            DataTable dt = await LoadDataTableFromStoredProcAsync( "ObtenerFormasPago",
                       new Dictionary<string, object>
                       {
                       });

                return dt.ConvertirFormasPagosSP().ToList();
        }

        public async Task<IEnumerable<CaraSiges>> ListarCarasSp(CancellationToken cancellationToken)
        {
            ConnectionString = _settings.Estacion;
            DataTable dt = await LoadDataTableFromStoredProcAsync( "ObtenerCaras",
                   new Dictionary<string, object>
                   {
                   });

            return dt.ConvertirCaraSP();
        }


        public async Task<IEnumerable<SurtidorSiges>> ListarSurtidoresSP(CancellationToken cancellationToken)
        {
            ConnectionString = _settings.Estacion;

            var reqDict = new Dictionary<string, object>
            {
            };
            DataTable dt = await LoadDataTableFromStoredProcAsync("ObtenerSurtidores", reqDict);
            return dt.ConvertirSurtidoresSiges();
        }

        public async Task<TurnoSiges> ObtenerTurnoPorIsla(int idIsla, CancellationToken cancellationToken)
        {
            ConnectionString = _settings.Estacion;
            DataTable dt = await LoadDataTableFromStoredProcAsync("ObtenerTurnoIsla",
                            new Dictionary<string, object>{

                    {"@idIsla", idIsla }
                            });
            return dt.ConvertirTurnoSiges().FirstOrDefault();
        }

        public async Task<Factura> getUltimasFacturas(short cOD_CAR)
        {
            ConnectionString = _settings.Estacion;
            DataTable dt  = await LoadDataTableFromStoredProcAsync("ObtenerVentaPorCara",
                         new Dictionary<string, object>{

                    {"@COD_CAR", cOD_CAR }
                         });

            var venta = dt.ConvertirVentaSP().FirstOrDefault();
            if (venta == null)
            {
                return new Factura() { Venta = new Venta() { CONSECUTIVO = -1 }  };
            }
            ConnectionString = _settings.Facturacion;
            DataTable dt2 = await LoadDataTableFromStoredProcAsync("ObtenerFacturaPorVenta",
                         new Dictionary<string, object>{

                    {"@ventaId", venta.CONSECUTIVO }
                         });
            var factura = dt2.ConvertirFactura().FirstOrDefault();
            if (factura == null)
            {
                ConnectionString = _settings.Estacion;
                DataTable dt3 = await LoadDataTableFromStoredProcAsync("AgregarFacturaPorIdVenta",
                         new Dictionary<string, object>{

                    {"@ventaId", venta.CONSECUTIVO }
                         });

                ConnectionString = _settings.Facturacion;
                dt2 = await LoadDataTableFromStoredProcAsync("ObtenerFacturaPorVenta",
                         new Dictionary<string, object>{

                    {"@ventaId", venta.CONSECUTIVO }
                         });
                factura = dt2.ConvertirFactura().FirstOrDefault();
            }
            if (factura == null)
            {
                return null;
            }
            factura.Manguera = dt.ConvertirManguera().FirstOrDefault();
            factura.Venta = venta;
            return factura;
        }
    }
}