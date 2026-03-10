using EnviadorInformacionService.Models;
using FactoradorEstacionesModelo.Objetos;
using FacturacionelectronicaCore.Negocio.Modelo;
using FacturacionelectronicaCore.Repositorio.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Factura = FactoradorEstacionesModelo.Objetos.Factura;
using Resolucion = FactoradorEstacionesModelo.Objetos.Resolucion;
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

            venta.CONSECUTIVO = dr.IsNull("CONSECUTIVO") ? 0 : dr.Field<int>("CONSECUTIVO");
            venta.COD_CLI = dr.IsNull("COD_CLI") ? "" : dr.Field<string>("COD_CLI");
            venta.PLACA = dr.IsNull("PLACA") ? "" : dr.Field<string>("PLACA");
            venta.KILOMETRAJE = dr.IsNull("KIL_ACT") ? 0 : dr.Field<decimal?>("KIL_ACT");
            venta.CANTIDAD = dr.IsNull("CANTIDAD") ? 0 : dr.Field<decimal>("CANTIDAD");
            venta.PRECIO_UNI = dr.IsNull("PRECIO_UNI") ? 0 : dr.Field<decimal>("PRECIO_UNI");
            venta.IVA = dr.IsNull("IVA") ? 0 : dr.Field<int>("IVA");
            venta.SUBTOTAL = dr.IsNull("SUBTOTAL") ? 0 : dr.Field<decimal>("SUBTOTAL");
            venta.TOTAL = dr.IsNull("TOTAL") ? 0 : dr.Field<decimal>("TOTAL");
            venta.VALORNETO = dr.IsNull("VALORNETO") ? 0 : dr.Field<decimal>("VALORNETO");
            venta.NOMBRE = dr.IsNull("NOMBRE") ? "" : dr.Field<string>("NOMBRE");
            venta.TIPO_NIT = dr.IsNull("TIPO_NIT") ? "" : dr.Field<string>("TIPO_NIT");
            venta.NIT = dr.IsNull("NIT") ? "" : dr.Field<string>("NIT");
            venta.DIR_OFICINA = dr.IsNull("DIR_OFICINA") ? "" : dr.Field<string>("DIR_OFICINA");
            venta.TEL_OFICINA = dr.IsNull("TEL_OFICINA") ? "" : dr.Field<string>("TEL_OFICINA");
            venta.IMP_NOM = dr.IsNull("IMP_NOM") ? "" : dr.Field<string>("IMP_NOM");
            venta.COD_EMP = dr.IsNull("COD_EMP") ? "" : dr.Field<string>("COD_EMP");

            var type = dr["COD_CAR"].GetType().Name;
            var typesur = dr["COD_SUR"].GetType().Name;
            venta.COD_CAR = dr.Table.Columns.Contains("COD_CAR") ? (dr.IsNull("COD_CAR") ? (short)0 : Convert.ToInt16(dr["COD_CAR"])) : (short)0;
            venta.COD_SUR = dr.Table.Columns.Contains("COD_SUR") ? (dr.IsNull("COD_SUR") ? (short)0 : Convert.ToInt16(dr["COD_SUR"])) : (short)0;
            venta.COD_INT = dr.Table.Columns.Contains("COD_INT") ? (dr.IsNull("COD_INT") ? "" : dr.Field<string>("COD_INT")) : "";
            venta.COD_FOR_PAG = dr.IsNull("KIL_ACT") ? 0 : dr.Field<int>("COD_FOR_PAG");
            venta.FECH_ULT_ACTU = dr.IsNull("FECH_ULT_ACTU") ? null : dr.Field<DateTime?>("FECH_ULT_ACTU");
            venta.FECH_PRMA = dr.IsNull("MANTENIMIENTO") ? null : dr.Field<DateTime?>("MANTENIMIENTO");
            
            venta.Combustible = dr.IsNull("DESCRIPCION") ? "" : dr.Field<string>("DESCRIPCION");
            venta.Descuento = dr.IsNull("DESCUENTO") ? 0 : dr.Field<decimal>("DESCUENTO");
            venta.EMPLEADO = dr.IsNull("VENDEDOR") ? "" : dr.Field<string>("VENDEDOR");
            venta.CEDULA = dr.IsNull("CEDULA") ? "" : dr.Field<string>("CEDULA");
            venta.FECHA_REAL = dr.IsNull("FechaReporte") ? null : dr.Field<DateTime?>("FechaReporte");

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

            response.AddRange(
                dt.AsEnumerable().Select(dr => new Factura()
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
                })
            ) ;
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
                dt.AsEnumerable().Select(dr => new Resolucion()
                {
                    ConsecutivoInicial = dr.Field<int>("consecutivoInicio"),
                    ConsecutivoFinal = dr.Field<int>("consecutivoFinal"),
                    ConsecutivoActual = dr.Field<int>("consecutivoActual"),
                    DescripcionResolucion = dr.Field<string>("descripcionRes"),
                    FechaFinalResolucion = dr.Field<DateTime>("fechafinal"),
                    FechaInicioResolucion = dr.Field<DateTime>("fechaInicio"),
                    Autorizacion = dr.Field<string>("Autorizacion"),
                })
            );
            return response;
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

            response.AddRange(
                dt.AsEnumerable().Select(dr => {
                    var fc = new FacturaCanastilla();

                    fc.FacturasCanastillaId = dr.Field<int>("FacturasCanastillaId");
                    fc.consecutivo = dr.Field<int>("consecutivo");
                    fc.fecha = dr.Field<DateTime>("fecha");
                    fc.impresa = dr.Field<int>("impresa");
                    fc.estado = dr.Field<string>("estado");
                    fc.codigoFormaPago = new FormasPagos() { Id = dr.Field<int>("codigoFormaPago") };
                    fc.codigoFormaPago2 = dr.Table.Columns.Contains("codigoFormaPago2") && !dr.IsNull("codigoFormaPago2")
                        ? (int?)Convert.ToInt32(dr["codigoFormaPago2"])
                        : null;
                    fc.total1 = dr.Table.Columns.Contains("total1") && !dr.IsNull("total1")
                        ? (decimal?)Convert.ToDecimal(dr["total1"])
                        : null;
                    fc.total2 = dr.Table.Columns.Contains("total2") && !dr.IsNull("total2")
                        ? (decimal?)Convert.ToDecimal(dr["total2"])
                        : null;
                    fc.descuento = Convert.ToSingle(dr.Field<double>("descuento"));
                    fc.subtotal = Convert.ToSingle(dr.Field<double>("subtotal"));
                    fc.total = Convert.ToSingle(dr.Field<double>("total"));
                    fc.iva = Convert.ToSingle(dr.Field<double>("iva"));
                    fc.resolucion = ConvertirResolucion(dt).FirstOrDefault();
                    fc.enviada = dr.Field<bool>("enviada");
                    fc.terceroId = new Objetos.Tercero();

                    fc.terceroId.COD_CLI = dr.IsNull("COD_CLI") ? "" : dr.Field<string>("COD_CLI");
                    fc.terceroId.Direccion = dr.Field<string>("direccion");
                    fc.terceroId.Nombre = dr.Field<string>("Nombre");
                    fc.terceroId.Apellidos = dr.Table.Columns.Contains("apellidos") ? (dr.IsNull("apellidos") ? null : dr.Field<string>("apellidos")) : null;
                    fc.terceroId.Telefono = dr.Field<string>("Telefono");
                    fc.terceroId.identificacion = dr.Field<string>("identificacion");

                    fc.terceroId.Correo = dr.Field<string>("correo");
                    fc.terceroId.terceroId = dr.Field<int>("terceroId");
                    fc.terceroId.tipoIdentificacion = dr.Field<int?>("tipoIdentificacion");
                    fc.terceroId.tipoIdentificacionS = dr.Field<string>("descripcion");
                    fc.Empleado = dr.Field<string>("Vendedor");
                    fc.Isla = dr.Field<string>("isla");
                    fc.Placa = dr.IsNull("placa") ? string.Empty : dr.Field<string>("placa");


                    return fc;
                })
            );
            return response;
        }

        internal List<CanastillaFactura> ConvertirFacturaCanastillaDEtalle(DataTable dt)
        {
            List<CanastillaFactura> response = new List<CanastillaFactura>();

            response.AddRange(
                dt.AsEnumerable().Select(dr => new CanastillaFactura()
                {
                    cantidad  = Convert.ToSingle(dr.Field<double>("cantidad")),
                    iva  = Convert.ToSingle(dr.Field<double>("iva")),
                    precio = Convert.ToSingle(dr.Field<double>("precio")),
                    subtotal = Convert.ToSingle(dr.Field<double>("subtotal")),
                    total = Convert.ToSingle(dr.Field<double>("total")),
                    Canastilla = new Canastilla()
                    {
                        guid = dr.Field<Guid>("guid"),
                        CanastillaId = dr.Field<int>("CanastillaId"),
                        descripcion = dr.Field<string>("descripcion"),
                    }
                })
            );
            return response;
        }

        internal Turno ConvertirTurno(DataSet ds)
        {
            if (ds.Tables[0].AsEnumerable().Any()) { 
            var drTurno = ds.Tables[0].Rows[0];
            var turno = new FacturacionelectronicaCore.Negocio.Modelo.Turno {
                Empleado = drTurno.Field<string>("empleado"),
                FechaApertura = drTurno.Field<DateTime>("FechaApertura"),
                FechaAperturaJuliana = drTurno.Field<int>("FECHA"),
                FechaCierre = drTurno.Field<DateTime>("FechaCierre"),
                IdEstado = drTurno.Field<int>("IdEstado"),
                Isla = drTurno.Field<string>("Isla"),
                Numero = drTurno.Field<short>("Numero"),
                turnoSurtidores = new List<TurnoSurtidor>()
            };
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

                return turno;
            }
            return null;
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
