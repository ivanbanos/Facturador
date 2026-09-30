using Dominio.Entidades;
using EnviadorInformacionService.Models;
using FactoradorEstacionesModelo.Fidelizacion;
using FactoradorEstacionesModelo.Objetos;
using FactoradorEstacionesModelo.Siges;
using FacturacionelectronicaCore.Repositorio.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using SurtidorSiges = FactoradorEstacionesModelo.Siges.SurtidorSiges;

namespace FactoradorEstacionesModelo.Convertidor
{
    public class Convertidor
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, byte> ColumnasFaltantesReportadas =
            new System.Collections.Concurrent.ConcurrentDictionary<string, byte>(StringComparer.OrdinalIgnoreCase);

        public IEnumerable<T> Convertir<T>(DataTable dt)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Lee una columna de forma tolerante: si el SP no la retorna, viene en NULL
        /// o con un tipo numérico distinto (ej. decimal vs double), devuelve el valor convertido
        /// o <paramref name="valorPorDefecto"/> en lugar de lanzar excepción.
        /// </summary>
        private static T Columna<T>(DataRow dr, string columna, T valorPorDefecto = default)
        {
            if (!dr.Table.Columns.Contains(columna))
            {
                if (ColumnasFaltantesReportadas.TryAdd(columna, 0))
                    Logger.Warn($"El resultado del SP no contiene la columna '{columna}'; se usa valor por defecto. Revise si el SP está desactualizado.");
                return valorPorDefecto;
            }
            if (dr.IsNull(columna))
            {
                return valorPorDefecto;
            }

            var valor = dr[columna];
            if (valor is T tipado)
            {
                return tipado;
            }

            try
            {
                var destino = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
                if (destino == typeof(Guid))
                {
                    return (T)(object)Guid.Parse(valor.ToString());
                }
                return (T)Convert.ChangeType(valor, destino, CultureInfo.InvariantCulture);
            }
            catch (Exception ex) when (ex is InvalidCastException || ex is FormatException || ex is OverflowException)
            {
                Logger.Warn($"No se pudo convertir la columna '{columna}' ({valor.GetType().Name}) a {typeof(T).Name}; se usa valor por defecto. {ex.Message}");
                return valorPorDefecto;
            }
        }

        public IEnumerable<Isla> ConvertirIsla(DataTable dt)
        {
            List<Isla> response = new List<Isla>();

            response.AddRange(
                dt.AsEnumerable().Select(dr => new Isla()
                {
                    idIsla = dr.Field<int>("Id"),
                    descripcion = dr.Field<string>("descripcion")
                })
            );
            return response;
        }

        public IEnumerable<Surtidor> ConvertirSurtidor(DataTable dt)
        {
            List<Surtidor> response = new List<Surtidor>();

            response.AddRange(
                dt.AsEnumerable().Select(dr => new Surtidor()
                {
                    COD_SUR = dr.Field<short>("COD_SUR"),
                    MARCA = dr.Field<string>("MARCA")
                })
            );
            return response;
        }

        public IEnumerable<Cara> ConvertirCara(DataTable dt)
        {
            List<Cara> response = new List<Cara>();

            response.AddRange(
                dt.AsEnumerable().Select(dr => new Cara()
                {
                    COD_CAR = dr.Field<short>("COD_CAR"),
                    POS = dr.Field<byte>("POS"),
                    DESCRIPCION = dr.Field<string>("DESCRIPCION"),
                    NUM_POS = dr.Field<short?>("NUM_POS"),
                    COD_SUR = dr.Field<short>("COD_SUR")
                })
            );
            return response;
        }

        public IEnumerable<Manguera> ConvertirManguera(DataTable dt)
        {
            List<Manguera> response = new List<Manguera>();

            response.AddRange(
                dt.AsEnumerable().Select(dr => new Manguera()
                {
                    COD_MAN = dr.Field<short>("COD_MAN"),
                    COD_TANQ = dr.Field<short>("COD_TANQ"),
                    DESCRIPCION = dr.Field<string>("DESCRIPCION"),
                    DS_ROM = dr.Field<string>("DS_ROM")
                })
            );
            return response;
        }

        public IEnumerable<Venta> ConvertirVenta(DataTable dt)
        {
            List<Venta> response = new List<Venta>();

            response.AddRange(
                dt.AsEnumerable().Select(dr =>

                    CrearVenta(dr)
                )
            );
            return response;
        }

        private static Venta CrearVenta(DataRow dr)
        {
            var venta = new Venta();

            venta.CONSECUTIVO = Columna<int>(dr, "CONSECUTIVO", 0);
            venta.COD_CLI = Columna<string>(dr, "COD_CLI", "");
            venta.PLACA = Columna<string>(dr, "PLACA", "");
            venta.KILOMETRAJE = Columna<decimal?>(dr, "KIL_ACT", 0);
            venta.CANTIDAD = Columna<decimal>(dr, "CANTIDAD", 0);
            venta.PRECIO_UNI = Columna<decimal>(dr, "PRECIO_UNI", 0);
            venta.IVA = Columna<int>(dr, "IVA", 0);
            venta.SUBTOTAL = Columna<decimal>(dr, "SUBTOTAL", 0);
            venta.TOTAL = Columna<decimal>(dr, "TOTAL", 0);
            venta.VALORNETO = Columna<decimal>(dr, "VALORNETO", 0);
            venta.NOMBRE = Columna<string>(dr, "NOMBRE", "");
            venta.TIPO_NIT = Columna<string>(dr, "TIPO_NIT", "");
            venta.NIT = Columna<string>(dr, "NIT", "");
            venta.DIR_OFICINA = Columna<string>(dr, "DIR_OFICINA", "");
            venta.TEL_OFICINA = Columna<string>(dr, "TEL_OFICINA", "");
            venta.IMP_NOM = Columna<string>(dr, "IMP_NOM", "");

            venta.COD_CAR = Columna<short>(dr, "COD_CAR");
            venta.COD_SUR = Columna<short>(dr, "COD_SUR");
            venta.COD_INT = Columna<string>(dr, "COD_INT", "");
            // Se conserva la regla previa: sin KIL_ACT la forma de pago queda en 0.
            venta.COD_FOR_PAG = Columna<decimal?>(dr, "KIL_ACT") == null ? 0 : Columna<int>(dr, "COD_FOR_PAG");
            venta.FECH_ULT_ACTU = Columna<DateTime?>(dr, "FECH_ULT_ACTU", null);

            venta.Combustible = Columna<string>(dr, "DESCRIPCION", "");
            venta.Descuento = Columna<decimal>(dr, "DESCUENTO", 0);
            venta.EMPLEADO = Columna<string>(dr, "VENDEDOR", "");

            var suma = Convert.ToInt32(venta.PRECIO_UNI * venta.CANTIDAD);
            var sumaTotal = Convert.ToInt32((venta.PRECIO_UNI * venta.CANTIDAD) - venta.Descuento);
            if (suma >= 1000000 && venta.VALORNETO < 1000000)
            {
                venta.VALORNETO = suma;
            }
            if (sumaTotal >= 1000000 && venta.TOTAL < 1000000)
            {
                venta.TOTAL = sumaTotal;
            }

            return venta;
        }

        private long getNumericValue(object dbValue)
        {
            if (dbValue.GetType() == typeof(int))
            {
                return (int)dbValue;
            }
            else if (dbValue.GetType() == typeof(long))
            {
                return (long)dbValue;
            }
            else if (dbValue.GetType() == typeof(short))
            {
                return (short)dbValue;
            }
            else
            {
                return 0;
            }
        }

        public IEnumerable<FormasPagos> ConvertirFormasPagos(DataTable dt)
        {
            List<FormasPagos> response = new List<FormasPagos>();

            response.AddRange(
                dt.AsEnumerable().Select(dr => new FormasPagos()
                {
                    Id = dr.Field<short>("COD_FOR_PAG"),
                    Descripcion = dr.Field<string>("DESCRIPCION")
                })
            );
            return response;
        }

        public IEnumerable<Factura> ConvertirFactura(DataTable dt)
        {
            List<Factura> response = new List<Factura>();

            response.AddRange(
                dt.AsEnumerable().Select(dr => new Factura()
                {
                    facturaPOSId = Columna<int>(dr, "facturaPOSId"),
                    ventaId = Columna<int>(dr, "ventaId"),
                    TurnoGuid = dt.Columns.Contains("turnoguid") ? Columna<string>(dr, "turnoguid") : null,
                    Consecutivo = Columna<int>(dr, "CONSECUTIVO"),
                    DescripcionResolucion = Columna<string>(dr, "descripcionRes"),
                    Autorizacion = Columna<string>(dr, "autorizacion"),
                    Placa = Columna<string>(dr, "Placa"),
                    Kilometraje = Columna<string>(dr, "Kilometraje"),
                    fecha = Columna<DateTime>(dr, "fecha"),
                    Final = Columna<int>(dr, "consecutivoFinal"),
                    Inicio = Columna<int>(dr, "consecutivoInicio"),
                    FechaFinalResolucion = Columna<DateTime>(dr, "fechafinal"),
                    FechaInicioResolucion = Columna<DateTime>(dr, "fechaInicio"),
                    habilitada = Columna<bool>(dr, "habilitada"),
                    impresa = Columna<int>(dr, "impresa"),
                    Estado = Columna<string>(dr, "estado"),
                    codigoFormaPago = Columna<int>(dr, "codigoFormaPago"),

                    Tercero = new Tercero()
                    {
                        COD_CLI = Columna<string>(dr, "COD_CLI"),
                        Direccion = Columna<string>(dr, "direccion"),
                        Nombre = Columna<string>(dr, "Nombre"),
                        Apellidos = dt.Columns.Contains("apellidos") ? Columna<string>(dr, "apellidos") : null,
                        Telefono = Columna<string>(dr, "Telefono"),
                        identificacion = Columna<string>(dr, "identificacion"),

                        Correo = Columna<string>(dr, "correo"),
                        terceroId = Columna<int>(dr, "terceroId"),
                        tipoIdentificacion = Columna<int?>(dr, "tipoIdentificacion"),
                        tipoIdentificacionS = Columna<string>(dr, "descripcion"),
                    },
                })
            );
            return response;
        }

        public IEnumerable<Tercero> ConvertirTercero(DataTable dt)
        {
            List<Tercero> response = new List<Tercero>();

            response.AddRange(
                dt.AsEnumerable().Select(dr => new Tercero()
                {
                    COD_CLI = dr.Field<string>("COD_CLI"),
                    Direccion = dr.Field<string>("direccion"),
                    Nombre = dr.Field<string>("Nombre"),
                    Apellidos = dt.Columns.Contains("apellidos") ? dr.Field<string>("apellidos") : null,
                    Telefono = dr.Field<string>("Telefono"),
                    identificacion = dr.Field<string>("identificacion"),

                    Correo = dr.Field<string>("correo"),
                    terceroId = dr.Field<int>("terceroId"),
                    tipoIdentificacion = dr.Field<int?>("tipoIdentificacion"),
                    tipoIdentificacionS = dr.Field<string>("descripcion"),
                })
            );
            return response;
        }

        public IEnumerable<TipoIdentificacion> ConvertirTipoIdentificacion(DataTable dt)
        {
            List<TipoIdentificacion> response = new List<TipoIdentificacion>();

            response.AddRange(
                dt.AsEnumerable().Select(dr => new TipoIdentificacion()
                {
                    CodigoDian = dr.Field<short>("CodigoDian"),
                    Descripcion = dr.Field<string>("Descripcion"),
                    TipoIdentificacionId = dr.Field<int>("TipoIdentificacionId"),
                })
            );
            return response;
        }

        public List<string> ConvertirIds(DataTable dt)
        {
            List<string> response = new List<string>();
            return dt.AsEnumerable().Select(dr => dr.Field<string>("consecutivo")).ToList();
        }

        public List<int> ConvertirIntIds(DataTable dt1)
        {
            List<int> response = new List<int>();
            return dt1.AsEnumerable().Select(dr => dr.Field<int>("ventaId")).ToList();
        }

        public List<SurtidorSiges> ConvertirSurtidoresSiges(DataTable dt)
        {
            List<SurtidorSiges> response = new List<SurtidorSiges>();
            foreach(var dr in dt.AsEnumerable())
            {
                if (!response.Any(x => x.Numero == dr.Field<int>("Numero")))
                {
                    response.Add(new SurtidorSiges()
                    {
                        caras = new List<CaraSiges>(),
                        Descripcion = dr.Field<string>("Surtidor"),
                        Id = dr.Field<int>("IdSurtidor"),
                        Puerto = dr.Field<string>("puerto"),
                        Numero = dr.Field<int>("Numero"),
                        PuertoIButton = dr.Field<string>("PuertoIButton"),
                    });
                }
                response.Find(x=>x.Numero == dr.Field<int>("Numero")).caras.Add(
                new CaraSiges()
                {
                    Id = dr.Field<int>("Id"),
                    Descripcion = dr.Field<string>("descripcion"),
                    Impresora = dr.Field<string>("Impresora"),
                    Isla = dr.Field<string>("Isla")
                });
            }
            return response;
        }

        public List<MangueraSiges> ConvertirManguerasSiges(DataTable dt)
        {
            List<MangueraSiges> response = new List<MangueraSiges>();

            response.AddRange(
                dt.AsEnumerable().Select(dr => new MangueraSiges()
                {
                    Id = dr.Field<int>("Id"),
                    Descripcion = dr.Field<string>("descripcion"),
                    Ubicacion = dr.Field<string>("ubicacion")
                })
            );
            return response;
        }

        public List<CaraSiges> ConvertirCarasSiges(DataTable dt)
        {
            List<CaraSiges> response = new List<CaraSiges>();
            foreach (var dr in dt.AsEnumerable())
            {
                response.Add(
                new CaraSiges()
                {
                    Id = dr.Field<int>("Id"),
                    Descripcion = dr.Field<string>("descripcion"),
                    Isla = dr.Field<string>("Isla"),
                    IdIsla = dr.Field<int>("IdIsla")
                });
            }
            return response;
        }

        public List<FormaPagoSiges> ConvertirFormaPagoSiges(DataTable dt)
        {
            List<FormaPagoSiges> response = new List<FormaPagoSiges>();
            foreach (var dr in dt.AsEnumerable())
            {
                response.Add(
                new FormaPagoSiges()
                {
                    Id = dr.Field<int>("Id"),
                    Descripcion = dr.Field<string>("descripcion")
                });
            }
            return response;
        }

        public List<FacturaSiges> ConvertirFacturasSiges(DataTable dt)
        {
            List<FacturaSiges> response = new List<FacturaSiges>();

            response.AddRange(
                dt.AsEnumerable().Select(dr =>
                {
                    
                    return new FacturaSiges()
                    {
                        facturaPOSId = Columna<int>(dr, "facturaPOSId"),
                        ventaId = Columna<int>(dr, "ventaId"),
                        Consecutivo = Columna<int>(dr, "CONSECUTIVO"),
                        DescripcionResolucion = Columna<string>(dr, "descripcionRes"),
                        Autorizacion = Columna<string>(dr, "autorizacion"),
                        Placa = Columna<string>(dr, "Placa"),
                        Kilometraje = Columna<string>(dr, "Kilometraje"),
                        fecha = Columna<DateTime>(dr, "fecha"),
                        Final = Columna<int>(dr, "consecutivoFinal"),
                        Inicio = Columna<int>(dr, "consecutivoInicio"),
                        FechaFinalResolucion = Columna<DateTime>(dr, "fechafinal"),
                        FechaInicioResolucion = Columna<DateTime>(dr, "fechaInicio"),
                        habilitada = Columna<bool>(dr, "habilitada"),
                        impresa = Columna<int>(dr, "impresa"),
                        Estado = Columna<string>(dr, "estado"),
                        codigoFormaPago = Columna<int>(dr, "codigoFormaPago"),
                        codigoFormaPago2 = dt.Columns.Contains("codigoFormaPago2") ? Columna<int?>(dr, "codigoFormaPago2") : null,
                        total1 = dt.Columns.Contains("total1") ? Columna<double?>(dr, "total1") : null,
                        total2 = dt.Columns.Contains("total2") ? Columna<double?>(dr, "total2") : null,
                        Combustible = Columna<string>(dr, "Combustible"),
                        Surtidor = Columna<string>(dr, "Surtidor"),
                        Cara = Columna<string>(dr, "Cara"),
                        Mangueras = Columna<string>(dr, "Manguera"),
                        Cantidad = Columna<double>(dr, "cantidad"),
                        Precio = Columna<double>(dr, "precio"),
                        Total = Columna<double>(dr, "total"),
                        Subtotal = Columna<double>(dr, "subtotal"),
                        Descuento = Columna<double>(dr, "descuento"),
                        Empleado = Columna<string>(dr, "Empleado"),
                        fechaProximoMantenimiento = Columna<DateTime?>(dr, "fechaProximoMantenimiento"),

                        Tercero = new Tercero()
                        {
                            COD_CLI = Columna<string>(dr, "COD_CLI"),
                            Direccion = Columna<string>(dr, "direccion"),
                            Nombre = Columna<string>(dr, "Nombre"),
                            Apellidos = dt.Columns.Contains("apellidos") ? Columna<string>(dr, "apellidos") : null,
                            Telefono = Columna<string>(dr, "Telefono"),
                            identificacion = Columna<string>(dr, "identificacion"),

                            Correo = Columna<string>(dr, "correo"),
                            terceroId = Columna<int>(dr, "terceroId"),
                            tipoIdentificacion = Columna<int?>(dr, "tipoIdentificacion"),
                            tipoIdentificacionS = Columna<string>(dr, "descripcion"),
                            EnviadoSiesa = dt.Columns.Contains("enviadoSiesa") ? Columna<bool?>(dr, "enviadoSiesa") : null,
                        },
                    };
                })
            );

            return response;
        }

        public IEnumerable<TurnoSiges> ConvertirTurnoSiges(DataTable dt)
        {
            List<TurnoSiges> response = new List<TurnoSiges>();
            foreach (var dr in dt.AsEnumerable())
            {
                response.Add(
                new TurnoSiges()
                {
                    Id = dr.Field<int>("Id"),
                    Empleado = dr.Field<string>("Nombre"),
                    Isla = dr.Field<string>("Isla"),
                    IdEstado = dr.Field<int>("IdEstado"),
                    FechaApertura = dr.Field<DateTime>("FechaApertura"),
                    FechaCierre = dr.Field<DateTime?>("FechaCierre"),
                    Numero = dt.Columns.Contains("Numero") ? Convert.ToInt32(dr["Numero"]) : 0
                });
            }
            return response;
        }

        public List<FacturaSiges> ConvertirVentasSiges(DataTable dt)
        {
            List<FacturaSiges> response = new List<FacturaSiges>();

            response.AddRange(
                dt.AsEnumerable().Select(dr => new FacturaSiges()
                {
                    ventaId = dr.Field<int>("Id"),

                    Cantidad = dr.Field<double>("cantidad"),


                    IButton = dr.Field<string>("Ibutton"),
                    fecha = dr.Field<DateTime>("fecha"),
                })
            );
            return response;
        }

        public List<TurnoSurtidor> ConvertirTurnoSurtidoresSiges(DataTable dt)
        {
            List<TurnoSurtidor> response = new List<TurnoSurtidor>();

            response.AddRange(
                dt.AsEnumerable().Select(dr => new TurnoSurtidor()
                {
                    Apertura = dr.Field<double>("Apertura"),

                    Cierre = dr.Field<double?>("Cierre"),

                    Combustible = new Combustible()
                    {
                        Id = dr.Field<int>("IdCombustible"),
                        Descripcion = dr.Field<string>("combustible"),
                        Precio = dr.Field<double>("precio")
                    },
                    Manguera = new MangueraSiges()
                    {
                        Id = dr.Field<int>("Id"),
                        Descripcion = dr.Field<string>("descripcion"),
                        Ubicacion = dr.Field<string>("ubicacion")
                    },
                })
            );
            return response;
        }

        public List<VehiculoSuic> ConvertirVehiculoSiges(DataTable dt)
        {
            List<VehiculoSuic> response = new List<VehiculoSuic>();

            response.AddRange(
                dt.AsEnumerable().Select(dr => new VehiculoSuic()
                {
                    estado = dr.Field<int>("estado"),
                    capacidad = dr.Field<string>("capacidad"),
                    fechaFin = dr.Field<DateTime>("fechaFin"),
                    fechaInicio = dr.Field<DateTime>("fechaInicio"),
                    idrom = dr.Field<string>("idrom"),
                    motivo = dr.Field<string>("motivo"),
                    motivoTexto = dr.Field<string>("motivoTexto"),
                    placa = dr.Field<string>("placa"),
                    servicio = dr.Field<string>("servicio"),
                    vin = dr.Field<string>("vin"),
                })
            );
            return response;
        }

        private static DateTime? JulianToDateTime(int julianDate)
        {
            try
            {
                int day = julianDate % 1000;
                int year = (julianDate - day + 2000000) / 1000;
                var date1 = new DateTime(year, 1, 1);
                return date1.AddDays(day - 1);

            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public List<Resolucion> ConvertirResolucion(DataTable dt)
        {
            List<Resolucion> response = new List<Resolucion>();

            response.AddRange(
                dt.AsEnumerable().Select(dr => new Resolucion()
                {
                    ConsecutivoInicial = Columna<int>(dr, "consecutivoInicio"),
                    ConsecutivoFinal = Columna<int>(dr, "consecutivoFinal"),
                    ConsecutivoActual = Columna<int>(dr, "consecutivoActual"),
                    DescripcionResolucion = Columna<string>(dr, "descripcionRes"),
                    FechaFinalResolucion = Columna<DateTime>(dr, "fechafinal"),
                    FechaInicioResolucion = Columna<DateTime>(dr, "fechaInicio"),
                    Autorizacion = Columna<string>(dr, "Autorizacion"),
                })
            );
            return response;
        }

        public List<FacturaFechaReporte> ConvertirFacturaFechaReporte(DataTable dt2)
        {
            List<FacturaFechaReporte> response = new List<FacturaFechaReporte>();

            response.AddRange(
                dt2.AsEnumerable().Select(dr => new FacturaFechaReporte()
                {
                    FechaReporte = dr.Field<DateTime?>("FechaReporte"),
                    IdVentaLocal = dr.Field<int>("IdVentaLocal")
                })
            );
            return response;
        }

        public List<FacturaCanastilla> ConvertirFacturaCanastilla(DataTable dt)
        {
            List<FacturaCanastilla> response = new List<FacturaCanastilla>();

            response.AddRange(
                dt.AsEnumerable().Select(dr => {
                    var fc = new FacturaCanastilla();

                    fc.FacturasCanastillaId = Columna<int>(dr, "FacturasCanastillaId");
                    fc.consecutivo = Columna<int>(dr, "consecutivo");
                    fc.fecha = Columna<DateTime>(dr, "fecha");
                    fc.impresa = Columna<int>(dr, "impresa");
                    fc.estado = Columna<string>(dr, "estado");
                    fc.codigoFormaPago = new FormaPagoSiges() { Id = Columna<int>(dr, "codigoFormaPago") };
                    fc.numeroTransaccion = dt.Columns.Contains("numeroTransaccion") && !dr.IsNull("numeroTransaccion")
                        ? Columna<string>(dr, "numeroTransaccion")
                        : null;
                    fc.descuento = Convert.ToSingle(Columna<double>(dr, "descuento"));
                    fc.subtotal = Convert.ToSingle(Columna<double>(dr, "subtotal"));
                    fc.total = Convert.ToSingle(Columna<double>(dr, "total"));
                    fc.iva = Convert.ToSingle(Columna<double>(dr, "iva"));
                    fc.resolucion = ConvertirResolucion(dt).FirstOrDefault();
                    fc.enviada = Columna<int>(dr, "enviada");
                    fc.terceroId = new Tercero();

                    fc.terceroId.COD_CLI = Columna<string>(dr, "COD_CLI");
                    fc.terceroId.Direccion = Columna<string>(dr, "direccion");
                    fc.terceroId.Nombre = Columna<string>(dr, "Nombre");
                    fc.terceroId.Telefono = Columna<string>(dr, "Telefono");
                    fc.terceroId.identificacion = Columna<string>(dr, "identificacion");

                    fc.terceroId.Correo = Columna<string>(dr, "correo");
                    fc.terceroId.terceroId = Columna<int>(dr, "terceroId");
                    fc.terceroId.tipoIdentificacion = Columna<int?>(dr, "tipoIdentificacion");
                    fc.terceroId.tipoIdentificacionS = Columna<string>(dr, "descripcion");
                    fc.TurnoGuid = dt.Columns.Contains("turnoguid") && !dr.IsNull("turnoguid")
                        ? Columna<string>(dr, "turnoguid")
                        : null;
                    fc.Placa = dt.Columns.Contains("placa") && !dr.IsNull("placa")
                        ? Columna<string>(dr, "placa")
                        : (dt.Columns.Contains("Placa") && !dr.IsNull("Placa") ? Columna<string>(dr, "Placa") : null);


                    return fc;
                })
            );
            return response;
        }

        public List<CanastillaFactura> ConvertirFacturaCanastillaDEtalle(DataTable dt)
        {
            List<CanastillaFactura> response = new List<CanastillaFactura>();

            response.AddRange(
                dt.AsEnumerable().Select(dr => new CanastillaFactura()
                {
                    cantidad = Convert.ToSingle(Columna<double>(dr, "cantidad")),
                    iva = Convert.ToSingle(Columna<double>(dr, "iva")),
                    precio = Convert.ToSingle(Columna<double>(dr, "precio")),
                    subtotal = Convert.ToSingle(Columna<double>(dr, "subtotal")),
                    total = Convert.ToSingle(Columna<double>(dr, "total")),
                    Canastilla = new Canastilla()
                    {
                        guid = Columna<Guid>(dr, "guid"),
                        CanastillaId = Columna<int>(dr, "CanastillaId"),
                        descripcion = Columna<string>(dr, "descripcion"),
                    }
                })
            );
            return response;
        }

        public IEnumerable<Puntos> ConvertirPuntos(DataTable dt)
        {
            List<Puntos> response = new List<Puntos>();

            response.AddRange(
                dt.AsEnumerable().Select(dr => new Puntos((float)dr.Field<double>("ValorVenta"),
                     dr.Field<string>("Factura"),
                   dr.Field<string>("DocumentoFidelizado"),
                    ""))
            );
            return response;
        }

        public IEnumerable<Fidelizado> ConvertirFidelizado(DataTable dt)
        {
            List<Fidelizado> response = new List<Fidelizado>();

            response.AddRange(
                dt.AsEnumerable().Select(dr => new Fidelizado()
                {
                    Puntos = (float)dr.Field<double>("puntos"),
                    Documento = dr.Field<string>("documento")
                }
            ));
            return response;
        }
    }
}
