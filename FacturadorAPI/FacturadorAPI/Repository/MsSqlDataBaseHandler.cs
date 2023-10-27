using Dominio.Entidades;
using FactoradorEstacionesModelo.Fidelizacion;
using FacturadorAPI.Models;
using FacturadorAPI.Repository;
using MachineUtilizationApi.Config;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Data;

namespace MachineUtilizationApi.Repository
{
    public class MsSqlDataBaseHandler : BaseRepository, IDataBaseHandler
    {
        private readonly ILogger<MsSqlDataBaseHandler> _logger;
        private readonly IOptions<RepositoriesConfig> _repositoriesConfig;
        public override string ConnectionString => Settings.EstacionSiges;

        public MsSqlDataBaseHandler(IOptions<ConnectionStringSettings> settings, ILogger<MsSqlDataBaseHandler> logger, IOptions<RepositoriesConfig> repositoriesConfig) : base(settings, logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _repositoriesConfig = repositoriesConfig ?? throw new ArgumentNullException(nameof(repositoriesConfig));
        }

        public async Task<IEnumerable<TipoIdentificacion>> ListarTiposIdentificacion(CancellationToken cancellationToken)
        {
            var reqDict = new Dictionary<string, object>
            {
            };


            DataTable dt = await LoadDataTableFromStoredProcAsync("ObtenerTipoIdentificaciones", reqDict);
            return dt.ConvertirTipoIdentificacion();
        }

        public async Task<IEnumerable<FormaPagoSiges>> ListarFormasPagoSiges(CancellationToken cancellationToken)
        {
            var reqDict = new Dictionary<string, object>
            {
            };


            DataTable dt = await LoadDataTableFromStoredProcAsync("BuscarFormasPagosSiges", reqDict);
            return dt.ConvertirFormaPagoSiges();
        }

        public async Task<IEnumerable<CaraSiges>> ListarCarasSigues(CancellationToken cancellationToken)
        {
            var reqDict = new Dictionary<string, object>
            {
            };
            DataTable dt = await LoadDataTableFromStoredProcAsync("ObtenerCaras", reqDict);
            return dt.ConvertirCarasSiges();
        }

        public async Task ActualizarCanastilla(IEnumerable<Canastilla> canastillas)
        {
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
            var reqDict = new Dictionary<string, object>
            {
            };
            DataTable dt = await LoadDataTableFromStoredProcAsync("GetCanastilla", reqDict);


            return dt.ConvertirCanastilla();
        }

        public async Task<IEnumerable<Tercero>> ObtenerTerceroPorIDentificacion(string identificacion, CancellationToken cancellationToken)
        {
            var reqDict = new Dictionary<string, object>
            {
                    {"@identificacion", identificacion }
            };
            DataTable dt = await LoadDataTableFromStoredProcAsync("ObtenerTercero", reqDict);


            return dt.ConvertirTercero();
        }

        public async Task<FacturaSiges> ObtenerUltimaFacturaPorCara(int idCara, CancellationToken cancellationToken)
        {
            DataTable dt = await LoadDataTableFromStoredProcAsync("ObtenerFacturaPorVenta",
                            new Dictionary<string, object>{

                    {"@idCara", idCara }
                            });
            return dt.ConvertirFacturasSiges().FirstOrDefault();
        }

        public async Task<TurnoSiges> ObtenerTurnoPorIsla(int idIsla, CancellationToken cancellationToken)
        {
            DataTable dt = await LoadDataTableFromStoredProcAsync("ObtenerTurnoIsla",
                            new Dictionary<string, object>{

                    {"@idIsla", idIsla }
                            });
            return dt.ConvertirTurnoSiges().FirstOrDefault();
        }

        public async Task ConvertirAFactura(int idFactura)
        {
            await LoadDataTableFromStoredProcAsync( "ConvertirAFactura",
            new Dictionary<string, object>{

                    {"@ventaId", idFactura }
                               });
        }

        public async Task ConvertirAOrder(int idFactura)
        {
            await LoadDataTableFromStoredProcAsync("ConvertirAOrden",
            new Dictionary<string, object>{

                    {"@ventaId", idFactura }
                               });
        }

        public async Task<FacturaSiges> GetFacturaPorIdVenta(int idFactura)
        {
            DataTable dt = await LoadDataTableFromStoredProcAsync("GetFacturaPorIdVenta",
                            new Dictionary<string, object>{

                    {"@idVenta", idFactura }
                            });
            return dt.ConvertirFacturasSiges().FirstOrDefault();
        }

        public async Task ActuralizarFacturasEnviados(List<int> facturas)
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
            await LoadDataTableFromStoredProcAsync("SetFacturaCanastillaEnviada",
                         parameters);
        }

        public async Task MandarImprimir(int idVenta)
        {
            await LoadDataTableFromStoredProcAsync("MandarImprimir",
                            new Dictionary<string, object>{

                    {"@ventaId", idVenta }
                            });
        }

        public async Task GenerarFacturaCanastilla(FacturaCanastilla facturaCanastilla, bool imprimir)
        {
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
            var reqDict = new Dictionary<string, object>
            {
            };
            DataTable dt = await LoadDataTableFromStoredProcAsync("ObtenerSurtidores", reqDict);
            return dt.ConvertirSurtidoresSiges();
        }

        public async Task<IEnumerable<TurnoSiges>> GetTurnosByFechas(DateTime fechaInicio, DateTime fechaFin)
        {
            DataTable dt = await LoadDataTableFromStoredProcAsync("GetTurnosPorFecha",
                            new Dictionary<string, object>{

                    {"@fechaInicio", fechaInicio },
                    {"@fechaFin", fechaFin }
                            });
            return dt.ConvertirTurnoSiges();
        }

        public async Task<IEnumerable<TurnoSurtidor>> GetTurnoSurtidorInfo(int id)
        {
            DataTable dt = await LoadDataTableFromStoredProcAsync("GetTurnoSurtidorInfo",
                            new Dictionary<string, object>{

                    {"@Id", id }
                            });
            return dt.ConvertirTurnoSurtidoresSiges();
        }

        public async Task<IEnumerable<FacturaSiges>> GetFacturasPorFechas(DateTime fechaInicio, DateTime fechaFin)
        {
            DataTable dt = await LoadDataTableFromStoredProcAsync("GetFacturasPorFechas",
                            new Dictionary<string, object>{

                    {"@fechaInicio", fechaInicio },
                    {"@fechaFin", fechaFin }
                            });
            return dt.ConvertirFacturasSiges();
        }

        public async Task ActualizarFactura(int facturaPOSId, int terceroId, int codigoFormaPago, int idVenta, string placa, string kilometraje)
        {
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
            var reqDict = new Dictionary<string, object>
            {
                    {"@identificacion", identificacion }
            };
            DataTable dt = await LoadDataTableFromStoredProcAsync("GetTerceroByQuery", reqDict);


            return dt.ConvertirTercero().FirstOrDefault();
        }

        public async Task AbrirTurno(int isla, string codigo)
        {
            await LoadDataTableFromStoredProcAsync("AbrirTurno",
                            new Dictionary<string, object>{

                {"@codigo",codigo },
                {"@IdIsla",isla }
                            });
        }

        public async Task CerrarTurno(int isla, string codigo)
        {
            await LoadDataTableFromStoredProcAsync("CerrarTurno",
                            new Dictionary<string, object>{

                {"@codigo",codigo },
                {"@IdIsla",isla }
                            });
        }

        public async Task MandarImprimirConsecutivo(string consecutivo)
        {
            DataTable dt = await LoadDataTableFromStoredProcAsync("MandarImprimirConsecutivo",
                            new Dictionary<string, object>{

                    {"@consecutivo", consecutivo }
                            });
        }


        public async Task ReimprimirTurno(DateTime fecha, int idIsla, int posicion)
        {
            DataTable dt = await LoadDataTableFromStoredProcAsync("ReimprimirTurno",
                            new Dictionary<string, object>{

                    {"@Fecha", fecha },
                    {"@IdIsla", idIsla },
                    {"@posicion", posicion }
                            });
        }

        public async Task<Fidelizado> GetFidelizado(string identificacion)
        {
            DataTable dt = await LoadDataTableFromStoredProcAsync("GetFidelizado",
                            new Dictionary<string, object>{

                    {"@documento", identificacion },
                            });
            return dt.ConvertirFidelizado().FirstOrDefault(); 
        }
    }
}