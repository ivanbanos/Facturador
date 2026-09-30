using EnviadorInformacionService.Models;
using FactoradorEstacionesModelo.Objetos;
using FacturacionelectronicaCore.Negocio.Modelo;
using FacturacionelectronicaCore.Repositorio.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using Factura = FactoradorEstacionesModelo.Objetos.Factura;
using Resolucion = FactoradorEstacionesModelo.Objetos.Resolucion;
using SigesCombustible = FactoradorEstacionesModelo.Siges.Combustible;
using SigesManguera = FactoradorEstacionesModelo.Siges.MangueraSiges;
using SigesTurno = FactoradorEstacionesModelo.Siges.TurnoSiges;
using SigesTurnoSurtidor = FactoradorEstacionesModelo.Siges.TurnoSurtidor;
using Tercero = FactoradorEstacionesModelo.Objetos.Tercero;

namespace FactoradorEstacionesModelo.Convertidor
{
    public class Convertidor
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        public IEnumerable<T> Convertir<T>(DataTable dt)
        {
            throw new NotImplementedException();
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
                    COD_MAN = dr.Field<short?>("COD_MAN"),
                    COD_TANQ = dr.Field<short?>("COD_TANQ"),
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

            venta.CONSECUTIVO = Columna<int>(dr, "CONSECUTIVO");
            venta.COD_CLI = Columna(dr, "COD_CLI", string.Empty);
            venta.PLACA = Columna(dr, "PLACA", string.Empty);
            venta.KILOMETRAJE = Columna<decimal?>(dr, "KIL_ACT", 0);
            venta.CANTIDAD = Columna<decimal>(dr, "CANTIDAD");
            venta.PRECIO_UNI = Columna<decimal>(dr, "PRECIO_UNI");
            venta.IVA = Columna<int>(dr, "IVA");
            venta.SUBTOTAL = Columna<decimal>(dr, "SUBTOTAL");
            venta.TOTAL = Columna<decimal>(dr, "TOTAL");
            venta.VALORNETO = Columna<decimal>(dr, "VALORNETO");
            venta.NOMBRE = Columna(dr, "NOMBRE", string.Empty);
            venta.TIPO_NIT = Columna(dr, "TIPO_NIT", string.Empty);
            venta.NIT = Columna(dr, "NIT", string.Empty);
            venta.DIR_OFICINA = Columna(dr, "DIR_OFICINA", string.Empty);
            venta.TEL_OFICINA = Columna(dr, "TEL_OFICINA", string.Empty);
            venta.IMP_NOM = Columna(dr, "IMP_NOM", string.Empty);
            venta.COD_EMP = Columna(dr, "COD_EMP", string.Empty);

            venta.COD_CAR = Columna<short>(dr, "COD_CAR");
            venta.COD_SUR = Columna<short>(dr, "COD_SUR");
            venta.COD_INT = Columna(dr, "COD_INT", string.Empty);
            // Se conserva la regla previa: sin KIL_ACT la forma de pago queda en 0.
            venta.COD_FOR_PAG = Columna<decimal?>(dr, "KIL_ACT") == null ? 0 : Columna<int>(dr, "COD_FOR_PAG");
            venta.FECH_ULT_ACTU = Columna<DateTime?>(dr, "FECH_ULT_ACTU");
            venta.FECH_PRMA = Columna<DateTime?>(dr, "MANTENIMIENTO");

            venta.Combustible = Columna(dr, "DESCRIPCION", string.Empty);
            venta.Descuento = Columna<decimal>(dr, "DESCUENTO");
            venta.EMPLEADO = Columna(dr, "VENDEDOR", string.Empty);
            venta.CEDULA = Columna(dr, "CEDULA", string.Empty);
            venta.FECHA_REAL = Columna<DateTime?>(dr, "FechaReporte");

            var suma = Convert.ToInt64(venta.PRECIO_UNI * venta.CANTIDAD);
            var sumaTotal = Convert.ToInt64((venta.PRECIO_UNI * venta.CANTIDAD) - venta.Descuento);
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

        private static DateTime? JulianToDateTime(int julianDate)
        {
            try
            {
                int day = julianDate % 1000;
                int year = (julianDate - day + 2000000) / 1000;
                var date1 = new DateTime(year, 1, 1);
                return date1.AddDays(day - 1);

            }
            catch (Exception ex) {
                return null;
            }
        }
        public IEnumerable<Factura> ConvertirFactura(DataTable dt)
        {
            List<Factura> response = new List<Factura>();

            int fila = 0;
            foreach (var dr in dt.AsEnumerable())
            {
                fila++;
                try
                {
                    response.Add(new Factura()
                    {
                        facturaPOSId = dr.Field<int>("facturaPOSId"),
                        ventaId = dr.Field<int>("ventaId"),
                        Consecutivo = dr.Field<int>("CONSECUTIVO"),
                        DescripcionResolucion = dr.Field<string>("descripcionRes"),
                        Autorizacion = dr.Field<string>("autorizacion"),
                        Placa = dr.Field<string>("Placa"),
                        Kilometraje = dr.Field<string>("Kilometraje"),
                        fecha = dr.Field<DateTime>("fecha"),
                        Final = dr.Field<int>("consecutivoFinal"),
                        Inicio = dr.Field<int>("consecutivoInicio"),
                        FechaFinalResolucion = dr.Field<DateTime>("fechafinal"),
                        FechaInicioResolucion = dr.Field<DateTime>("fechaInicio"),
                        habilitada = dr.Field<bool>("habilitada"),
                        impresa = dr.Field<int>("impresa"),
                        Estado = dr.Field<string>("estado"),
                        codigoFormaPago = dr.Field<int>("codigoFormaPago"),
                        codigoFormaPago2 = dr.Table.Columns.Contains("codigoFormaPago2") && !dr.IsNull("codigoFormaPago2") ? (int?)Convert.ToInt32(dr["codigoFormaPago2"]) : null,
                        total1 = dr.Table.Columns.Contains("total1") && !dr.IsNull("total1") ? (decimal?)Convert.ToDecimal(dr["total1"]) : null,
                        total2 = dr.Table.Columns.Contains("total2") && !dr.IsNull("total2") ? (decimal?)Convert.ToDecimal(dr["total2"]) : null,
                        numeroTransaccion = dr.Table.Columns.Contains("numeroTransaccion") && !dr.IsNull("numeroTransaccion")
                            ? dr.Field<string>("numeroTransaccion")
                            : null,

                        Tercero = new Objetos.Tercero() {
                            COD_CLI = dr.IsNull("COD_CLI") ? "" : dr.Field<string>("COD_CLI"),
                            Direccion = dr.Field<string>("direccion"),
                            Nombre = dr.Field<string>("Nombre"),
                            Apellidos = dr.Table.Columns.Contains("apellidos") ? (dr.IsNull("apellidos") ? null : dr.Field<string>("apellidos")) : null,
                            Telefono = dr.Field<string>("Telefono"),
                            identificacion = dr.Field<string>("identificacion"),

                            Correo = dr.Field<string>("correo"),
                            terceroId = dr.Field<int>("terceroId"),
                            tipoIdentificacion = dr.Field<int?>("tipoIdentificacion"),
                            tipoIdentificacionS = dr.Field<string>("descripcion"),
                            EnviadoSiesa = dr.Table.Columns.Contains("enviadoSiesa") ? (dr.IsNull("enviadoSiesa") ? (bool?)null : dr.Field<bool?>("enviadoSiesa")) : (bool?)null,
                        },
                    });
                }
                catch (Exception ex)
                {
                    var camposNulos = dt.Columns.Cast<DataColumn>()
                        .Where(c => dr.IsNull(c))
                        .Select(c => c.ColumnName)
                        .ToList();
                    var ventaIdRef = dr.Table.Columns.Contains("ventaId") && !dr.IsNull("ventaId") ? dr["ventaId"].ToString() : "desconocido";
                    var facturaPOSIdRef = dr.Table.Columns.Contains("facturaPOSId") && !dr.IsNull("facturaPOSId") ? dr["facturaPOSId"].ToString() : "desconocido";

                    Logger.Error(
                        $"Se omite la fila {fila} de getFacturaSinEnviarSiesa (ventaId={ventaIdRef}, facturaPOSId={facturaPOSIdRef}) por error de conversión. " +
                        $"Campos con valor DBNull en esta fila: {(camposNulos.Any() ? string.Join(", ", camposNulos) : "ninguno detectado")}. " +
                        $"Error original: {ex.Message}");
                    // No se relanza: se omite esta factura para no bloquear el procesamiento de las demás.
                    continue;
                }
            }
            return response;
        }

        public IEnumerable<Objetos.Tercero> ConvertirTercero(DataTable dt)
        {
            List<Tercero> response = new List<Tercero>();

            response.AddRange(
                dt.AsEnumerable().Select(dr => new Tercero()
                {
                    COD_CLI = dr.IsNull("COD_CLI") ? "" : dr.Field<string>("COD_CLI"),
                    Direccion = dr.Field<string>("direccion"),
                    Nombre = dr.Field<string>("Nombre"),
                    Apellidos = dr.Table.Columns.Contains("apellidos") ? (dr.IsNull("apellidos") ? null : dr.Field<string>("apellidos")) : null,
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
                    CodigoDian = dr.Field<short?>("CodigoDian"),
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

        public IEnumerable<SigesTurno> ConvertirTurnoSiges(DataTable dt)
        {
            List<SigesTurno> response = new List<SigesTurno>();
            foreach (var dr in dt.AsEnumerable())
            {
                response.Add(
                new SigesTurno()
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

        public List<SigesTurnoSurtidor> ConvertirTurnoSurtidoresSiges(DataTable dt)
        {
            List<SigesTurnoSurtidor> response = new List<SigesTurnoSurtidor>();

            response.AddRange(
                dt.AsEnumerable().Select(dr => new SigesTurnoSurtidor()
                {
                    Apertura = dr.Field<double>("Apertura"),
                    Cierre = dr.Field<double?>("Cierre"),
                    Combustible = new SigesCombustible()
                    {
                        Id = dr.Field<int>("IdCombustible"),
                        Descripcion = dr.Field<string>("combustible"),
                        Precio = dr.Field<double>("precio")
                    },
                    Manguera = new SigesManguera()
                    {
                        Id = dr.Field<int>("Id"),
                        Descripcion = dr.Field<string>("descripcion"),
                        Ubicacion = dr.Field<string>("ubicacion")
                    },
                })
            );

            return response;
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

        internal List<Resolucion> ConvertirResolucion(DataTable dt)
        {
            List<Resolucion> response = new List<Resolucion>();

            response.AddRange(
                dt.AsEnumerable().Select(ConvertirResolucion)
            );
            return response;
        }

        private static Resolucion ConvertirResolucion(DataRow dr)
        {
            return new Resolucion()
            {
                ConsecutivoInicial = Columna<int>(dr, "consecutivoInicio"),
                ConsecutivoFinal = Columna<int>(dr, "consecutivoFinal"),
                ConsecutivoActual = Columna<int>(dr, "consecutivoActual"),
                DescripcionResolucion = Columna<string>(dr, "descripcionRes"),
                FechaFinalResolucion = Columna<DateTime>(dr, "fechafinal"),
                FechaInicioResolucion = Columna<DateTime>(dr, "fechaInicio"),
                Autorizacion = Columna<string>(dr, "Autorizacion"),
            };
        }

        /// <summary>
        /// Lee una columna de forma tolerante: si el SP no la retorna, viene en NULL
        /// o con un tipo numérico distinto (ej. decimal vs float), devuelve el valor convertido
        /// o <paramref name="valorPorDefecto"/> en lugar de lanzar excepción.
        /// </summary>
        private static T Columna<T>(DataRow dr, string columna, T valorPorDefecto = default)
        {
            if (!dr.Table.Columns.Contains(columna) || dr.IsNull(columna))
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

        /// <summary>
        /// Registra en el log las columnas esperadas que el SP no retornó, para detectar SP desactualizados.
        /// </summary>
        private static void AdvertirColumnasFaltantes(DataTable dt, string origen, params string[] columnasEsperadas)
        {
            var faltantes = columnasEsperadas.Where(c => !dt.Columns.Contains(c)).ToList();
            if (faltantes.Any())
            {
                Logger.Warn($"{origen}: el resultado del SP no contiene las columnas [{string.Join(", ", faltantes)}]; se usarán valores por defecto. Revise si el SP está desactualizado.");
            }
        }

        internal List<FacturaFechaReporte> ConvertirFacturaFechaReporte(DataTable dt2)
        {
            List<FacturaFechaReporte> response = new List<FacturaFechaReporte>();

            response.AddRange(
                dt2.AsEnumerable().Select(dr => new FacturaFechaReporte()
                {
                    FechaReporte = dr.Field<DateTime>("FechaReporte"),
                    IdVentaLocal = dr.Field<int>("IdVentaLocal")
                })
            );
            return response;
        }

        internal List<FacturaCanastilla> ConvertirFacturaCanastilla(DataTable dt)
        {
            List<FacturaCanastilla> response = new List<FacturaCanastilla>();
            if (dt == null || dt.Rows.Count == 0)
            {
                return response;
            }

            // FacturasCanastillaId identifica la factura (se usa para marcarla como enviada): sin ella no se puede procesar.
            if (!dt.Columns.Contains("FacturasCanastillaId"))
            {
                throw new InvalidOperationException("ConvertirFacturaCanastilla: el SP no retornó la columna obligatoria 'FacturasCanastillaId'.");
            }

            AdvertirColumnasFaltantes(dt, nameof(ConvertirFacturaCanastilla),
                "consecutivo", "fecha", "impresa", "estado", "codigoFormaPago", "descuento", "subtotal", "total", "iva",
                "enviada", "COD_CLI", "direccion", "Nombre", "Telefono", "identificacion", "correo", "terceroId",
                "tipoIdentificacion", "descripcion", "Vendedor", "isla");

            // Todas las filas comparten la resolución de la primera fila (mismo comportamiento que antes).
            var filaResolucion = dt.Rows[0];

            response.AddRange(dt.AsEnumerable().Select(dr => new FacturaCanastilla()
            {
                FacturasCanastillaId = Columna<int>(dr, "FacturasCanastillaId"),
                consecutivo = Columna<int>(dr, "consecutivo"),
                fecha = Columna<DateTime>(dr, "fecha"),
                impresa = Columna<int>(dr, "impresa"),
                estado = Columna<string>(dr, "estado"),
                codigoFormaPago = new FormasPagos() { Id = Columna<int>(dr, "codigoFormaPago") },
                codigoFormaPago2 = Columna<int?>(dr, "codigoFormaPago2"),
                numeroTransaccion = Columna<string>(dr, "numeroTransaccion"),
                total1 = Columna<decimal?>(dr, "total1"),
                total2 = Columna<decimal?>(dr, "total2"),
                descuento = Columna<float>(dr, "descuento"),
                subtotal = Columna<float>(dr, "subtotal"),
                total = Columna<float>(dr, "total"),
                iva = Columna<float>(dr, "iva"),
                resolucion = ConvertirResolucion(filaResolucion),
                enviada = Columna<bool>(dr, "enviada"),
                terceroId = new Objetos.Tercero()
                {
                    COD_CLI = Columna(dr, "COD_CLI", string.Empty),
                    Direccion = Columna<string>(dr, "direccion"),
                    Nombre = Columna<string>(dr, "Nombre"),
                    Apellidos = Columna<string>(dr, "apellidos"),
                    Telefono = Columna<string>(dr, "Telefono"),
                    identificacion = Columna<string>(dr, "identificacion"),
                    Correo = Columna<string>(dr, "correo"),
                    terceroId = Columna<int>(dr, "terceroId"),
                    tipoIdentificacion = Columna<int?>(dr, "tipoIdentificacion"),
                    tipoIdentificacionS = Columna<string>(dr, "descripcion"),
                },
                Empleado = Columna<string>(dr, "Vendedor"),
                Isla = Columna<string>(dr, "isla"),
                TurnoGuid = Columna<string>(dr, "turnoguid"),
                // Columns.Contains no distingue mayúsculas, así que cubre "placa" y "Placa".
                Placa = Columna(dr, "placa", string.Empty),
            }));
            return response;
        }

        internal List<CanastillaFactura> ConvertirFacturaCanastillaDEtalle(DataTable dt)
        {
            List<CanastillaFactura> response = new List<CanastillaFactura>();
            if (dt == null || dt.Rows.Count == 0)
            {
                return response;
            }

            AdvertirColumnasFaltantes(dt, nameof(ConvertirFacturaCanastillaDEtalle),
                "cantidad", "iva", "precio", "subtotal", "total", "guid", "CanastillaId", "descripcion");

            response.AddRange(
                dt.AsEnumerable().Select(dr => new CanastillaFactura()
                {
                    cantidad = Columna<float>(dr, "cantidad"),
                    iva = Columna<float>(dr, "iva"),
                    precio = Columna<float>(dr, "precio"),
                    subtotal = Columna<float>(dr, "subtotal"),
                    total = Columna<float>(dr, "total"),
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

        internal Turno ConvertirTurno(DataSet ds)
        {
            if (ds == null || ds.Tables.Count == 0 || !ds.Tables[0].AsEnumerable().Any())
            {
                return null;
            }

            var drTurno = ds.Tables[0].Rows[0];
            var fechaApertura = drTurno.Table.Columns.Contains("FechaApertura")
                ? drTurno.Field<DateTime>("FechaApertura")
                : drTurno.Field<DateTime>("FECHA");
            var fechaCierre = drTurno.Table.Columns.Contains("FechaCierre")
                ? drTurno.Field<DateTime?>("FechaCierre")
                : null;

            int fechaAperturaJuliana;
            if (drTurno.Table.Columns.Contains("FECHA") && !drTurno.IsNull("FECHA"))
            {
                var fechaRaw = drTurno["FECHA"];
                if (fechaRaw is int)
                {
                    fechaAperturaJuliana = (int)fechaRaw;
                }
                else if (fechaRaw is long)
                {
                    fechaAperturaJuliana = Convert.ToInt32((long)fechaRaw);
                }
                else if (fechaRaw is short)
                {
                    fechaAperturaJuliana = Convert.ToInt32((short)fechaRaw);
                }
                else if (fechaRaw is DateTime)
                {
                    var fecha = (DateTime)fechaRaw;
                    fechaAperturaJuliana = (fecha.Year * 1000) + fecha.DayOfYear;
                }
                else
                {
                    fechaAperturaJuliana = (fechaApertura.Year * 1000) + fechaApertura.DayOfYear;
                }
            }
            else
            {
                fechaAperturaJuliana = (fechaApertura.Year * 1000) + fechaApertura.DayOfYear;
            }

            var turno = new FacturacionelectronicaCore.Negocio.Modelo.Turno {
                Empleado = drTurno.Field<string>("empleado"),
                FechaApertura = fechaApertura,
                FechaAperturaJuliana = fechaAperturaJuliana,
                FechaCierre = fechaCierre,
                IdEstado = drTurno.Field<int>("IdEstado"),
                Isla = drTurno.Field<string>("Isla"),
                Numero = Convert.ToInt16(drTurno["Numero"]),
                turnoSurtidores = new List<TurnoSurtidor>()
            };
            if (ds.Tables.Count > 1)
            {
                var dtTurnoLec = ds.Tables[1];
                if (dtTurnoLec.AsEnumerable().Any())
                {
                    turno.turnoSurtidores.AddRange(
                    dtTurnoLec.AsEnumerable().Select(dr => new TurnoSurtidor()
                    {
                        Apertura = Convert.ToDouble(dr.Field<decimal>("Apertura")),
                        Cierre = Convert.ToDouble(dr.Field<decimal>("Cierre")),
                        Combustible = dr.Field<string>("Combustible"),
                        Manguera = dr.Field<short>("Manguera").ToString(),
                        precioCombustible = Convert.ToSingle(dr.Field<decimal>("precioCombustible")),
                        Surtidor = dr.Field<short>("Surtidor").ToString(),
                    })
                );
                }
            }

            if (ds.Tables.Count > 2)
            {
                var dtBolsa = ds.Tables[2];
                if (dtBolsa.AsEnumerable().Any())
                {
                    turno.Bolsas.AddRange(
                    dtBolsa.AsEnumerable().Select(drBolsa => new Bolsa()
                    {

                        Fecha = drBolsa.Field<DateTime>("Fecha"),
                        Consecutivo = Convert.ToInt32(drBolsa["Consecutivo"]),
                        NumeroTurno = Convert.ToInt32(drBolsa["NumeroTurno"]),
                        Isla = Convert.ToString(drBolsa["Isla"]),
                        Empleado = drBolsa.Field<string>("Empleado"),
                        Moneda = Convert.ToDouble(drBolsa.Field<decimal>("Moneda")),
                        Billete = Convert.ToDouble(drBolsa.Field<decimal>("Billete")),
                    })
                );
                }
            }

            return turno;
        }

        public IEnumerable<ObjetoImprimir> ConvertirObjetoImprimir(DataTable ds)
        {
            return ds.AsEnumerable().Select(dr => new ObjetoImprimir()
            {
                Id = Convert.ToInt32(dr["Id"]),
                fecha = dr.Field<DateTime>("fecha"),
                Isla = Convert.ToInt32(dr["Isla"]),
                impreso = Convert.ToInt32(dr["impreso"]),
                Numero = Convert.ToInt32(dr["Numero"]),
                Objeto = dr.Field<string>("Objeto")
            });
        }

        public Bolsa ConvertirBolsa(DataTable dt)
        {
            var response = new Bolsa();
            if(dt.Rows.Count > 0)
            {
                var drBolsa = dt.Rows[0];
                response.Fecha = drBolsa.Field<DateTime>("Fecha");
                response.Consecutivo = Convert.ToInt32(drBolsa["Consecutivo"]);
                response.NumeroTurno = Convert.ToInt32(drBolsa["NumeroTurno"]);
                response.Isla = Convert.ToString(drBolsa["Isla"]);
                response.Empleado = drBolsa.Field<string>("Empleado");
                response.Moneda = Convert.ToDouble(drBolsa.Field<decimal>("Moneda"));
                response.Billete = Convert.ToDouble(drBolsa.Field<decimal>("Billete"));
            }
            return response;
        }

        public CuposRequest ConvertirInfoCupos(DataSet ds)
        {
            var request = new CuposRequest();
            var autos = ds.Tables[0];
            if (autos.AsEnumerable().Any())
            {
                request.cuposAutomotores = autos.AsEnumerable().Select(dr => new CupoAutomotor()
                {
                    Placa = dr.Field<string>("PLACA"),
                    Cliente = dr.Field<string>("NOMBRE"),
                    COD_CLI = dr.Field<string>("COD_CLI"),
                    Nit = dr.Field<string>("NIT"),
                    CupoAsignado = Convert.ToDouble(dr.Field<decimal>("CUPO_ASIGNADO")),
                    CupoDisponible = Convert.ToDouble(dr.Field<decimal>("CUPO_DISPONIBLE")),
                });
            }
            else
            {
                request.cuposAutomotores = new List<CupoAutomotor>();
            }

            var clientes = ds.Tables[1];
            if (clientes.AsEnumerable().Any())
            {
                request.cuposClientes = clientes.AsEnumerable().Select(dr => new CupoCliente()
                {
                    Cliente = dr.Field<string>("NOMBRE"),
                    COD_CLI = dr.Field<string>("COD_CLI"),
                    Nit = dr.Field<string>("NIT"),
                    CupoAsignado = Convert.ToDouble(dr.Field<decimal>("CUPO_ASIGNADO")),
                    CupoDisponible = Convert.ToDouble(dr.Field<decimal>("CUPO_DISPONIBLE")),
                });
            }
            else
            {
                request.cuposClientes = new List<CupoCliente>();
            }

            return request;
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
