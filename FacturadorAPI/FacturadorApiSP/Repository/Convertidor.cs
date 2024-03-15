using Dominio.Entidades;
using FactoradorEstacionesModelo.Fidelizacion;
using FactoradorEstacionesModelo.Objetos;
using FacturadorAPI.Models;
using FacturadorAPI.Models.Externos;
using FacturadorApiSP.Models;
using System.Data;

namespace FacturadorAPI.Repository
{
    public static class Convertidor
    {
        public static IEnumerable<T> Convertir<T>(this DataTable dt)
        {
            throw new NotImplementedException();
        }

        public static IEnumerable<Tercero> ConvertirTercero(this DataTable dt)
        {
            List<Tercero> response = new List<Tercero>();

            response.AddRange(
                dt.AsEnumerable().Select(dr => new Tercero()
                {
                    COD_CLI = dr.Field<string>("COD_CLI"),
                    Direccion = dr.Field<string>("direccion"),
                    Nombre = dr.Field<string>("Nombre"),
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

        public static IEnumerable<TipoIdentificacion> ConvertirTipoIdentificacion(this DataTable dt)
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

        public static List<string> ConvertirIds(this DataTable dt)
        {
            List<string> response = new List<string>();
            return dt.AsEnumerable().Select(dr => dr.Field<string>("consecutivo")).ToList();
        }

        public static List<int> ConvertirIntIds(this DataTable dt1)
        {
            List<int> response = new List<int>();
            return dt1.AsEnumerable().Select(dr => dr.Field<int>("ventaId")).ToList();
        }

        public static List<SurtidorSiges> ConvertirSurtidoresSiges(this DataTable dt)
        {
            List<SurtidorSiges> response = new List<SurtidorSiges>();
            foreach (var dr in dt.AsEnumerable())
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
                response.Find(x => x.Numero == dr.Field<int>("Numero")).caras.Add(
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

        public static List<MangueraSiges> ConvertirManguerasSiges(this DataTable dt)
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

        public static List<CaraSiges> ConvertirCarasSiges(this DataTable dt)
        {
            List<CaraSiges> response = new List<CaraSiges>();
            foreach (var dr in dt.AsEnumerable())
            {
                response.Add(
                new CaraSiges()
                {
                    Id = dr.Field<short>("Id"),
                    Descripcion = dr.Field<string>("descripcion"),
                    Isla = dr.Field<string>("Isla"),
                    IdIsla = dr.Field<int>("IdIsla")
                });
            }
            return response;
        }

        public static IEnumerable<Canastilla> ConvertirCanastilla(this DataTable dt)
        {
            List<Canastilla> response = new List<Canastilla>();
            response.AddRange(
                  dt.AsEnumerable().Select(dr => new Canastilla()
                  {
                      CanastillaId = dr.Field<int>("CanastillaId"),
                      descripcion = dr.Field<string>("descripcion"),
                      precio = Convert.ToSingle(dr.Field<double>("precio")),
                      unidad = dr.Field<string>("unidad"),
                      deleted = dr.Field<bool>("deleted"),
                      guid = dr.Field<Guid>("guid"),
                      iva = dr.Field<int>("iva"),

                  })
              );
            return response;
        }

        public static List<FormaPagoSiges> ConvertirFormaPagoSiges(this DataTable dt)
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

        public static List<FacturaSiges> ConvertirFacturasSiges(this DataTable dt)
        {
            List<FacturaSiges> response = new List<FacturaSiges>();

            response.AddRange(
                dt.AsEnumerable().Select(dr => new FacturaSiges()
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
                    Combustible = dr.Field<string>("Combustible"),
                    Surtidor = dr.Field<string>("Surtidor"),
                    Cara = dr.Field<string>("Cara"),
                    Mangueras = dr.Field<string>("Manguera"),
                    Cantidad = dr.Field<double>("cantidad"),
                    Precio = dr.Field<double>("precio"),
                    Total = dr.Field<double>("total"),
                    Subtotal = dr.Field<double>("subtotal"),
                    Descuento = dr.Field<double>("descuento"),
                    Empleado = dr.Field<string>("Empleado"),
                    fechaProximoMantenimiento = dr.Field<DateTime?>("fechaProximoMantenimiento"),

                    Tercero = new Tercero()
                    {
                        COD_CLI = dr.Field<string>("COD_CLI"),
                        Direccion = dr.Field<string>("direccion"),
                        Nombre = dr.Field<string>("Nombre"),
                        Telefono = dr.Field<string>("Telefono"),
                        identificacion = dr.Field<string>("identificacion"),

                        Correo = dr.Field<string>("correo"),
                        terceroId = dr.Field<int>("terceroId"),
                        tipoIdentificacion = dr.Field<int?>("tipoIdentificacion"),
                        tipoIdentificacionS = dr.Field<string>("descripcion"),
                    },
                })
            );
            return response;
        }

        public static IEnumerable<TurnoSiges> ConvertirTurnoSiges(this DataTable dt)
        {
            List<TurnoSiges> response = new List<TurnoSiges>();
            foreach (var dr in dt.AsEnumerable())
            {
                response.Add(
                new TurnoSiges()
                {
                    Id = dr.Field<short>("Id"),
                    Empleado = dr.Field<string>("Nombre"),
                    Isla = dr.Field<string>("Isla"),
                    IdEstado = dr.Field<int>("IdEstado"),
                    FechaApertura = dr.Field<DateTime>("FechaApertura"),
                    FechaCierre = dr.Field<DateTime?>("FechaCierre")
                });
            }
            return response;
        }

        public static List<FacturaSiges> ConvertirVentasSiges(this DataTable dt)
        {
            List<FacturaSiges> response = new List<FacturaSiges>();

            response.AddRange(
                dt.AsEnumerable().Select(dr => new FacturaSiges()
                {
                    ventaId = dr.Field<int>("Id"),

                    Cantidad = dr.Field<double>("cantidad"),


                    IButton = dr.Field<string>("Ibutton"),
                })
            );
            return response;
        }

        public static List<TurnoSurtidor> ConvertirTurnoSurtidoresSiges(this DataTable dt)
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

        public static List<VehiculoSuic> ConvertirVehiculoSiges(this DataTable dt)
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
        public static List<Resolucion> ConvertirResolucion(this DataTable dt)
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

        public static List<FacturaFechaReporte> ConvertirFacturaFechaReporte(this DataTable dt2)
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

        public static IEnumerable<Puntos> ConvertirPuntos(this DataTable dt)
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

        public static IEnumerable<Fidelizado> ConvertirFidelizado(this DataTable dt)
        {
            List<Fidelizado> response = new List<Fidelizado>();

            response.AddRange(
                dt.AsEnumerable().Select(dr => new Fidelizado()
                {
                    Puntos = (float)dr.Field<double>("puntos"),
                    Documento = dr.Field<string>("documento"),
                }
            ));
            return response;
        }

        public static IEnumerable<IslaSiges> ConvertirIsla(this DataTable dt)
        {
            List<IslaSiges> response = new List<IslaSiges>();

            response.AddRange(
                dt.AsEnumerable().Select(dr => new IslaSiges()
                {
                    Id = dr.Field<short>("COD_ISL"),
                    Isla = dr.Field<string>("DESCRIPCION")
                })
            );
            return response;
        }

        public static IEnumerable<FacturaSiges> ConvertirVenta(this DataTable dt)
        {
                List<FacturaSiges> response = new List<FacturaSiges>();

                response.AddRange(
                    dt.AsEnumerable().Select(dr =>

                        CrearVenta(dr)
                    )
                );
                return response;
        }

        private static FacturaSiges CrearVenta(DataRow dr)
        {
            var venta = new FacturaSiges();

            venta.Consecutivo = dr.IsNull("CONSECUTIVO") ? 0 : dr.Field<int>("CONSECUTIVO");
            venta.Placa = dr.IsNull("PLACA") ? "" : dr.Field<string>("PLACA");
            venta.Kilometraje = dr.IsNull("KIL_ACT") ? "0" : dr.Field<string>("KIL_ACT");
            venta.Cantidad = dr.IsNull("CANTIDAD") ? 0 : dr.Field<double>("CANTIDAD");
            venta.Precio = dr.IsNull("PRECIO_UNI") ? 0 : dr.Field<double>("PRECIO_UNI");
            venta.Iva = dr.IsNull("IVA") ? 0 : dr.Field<int>("IVA");
            venta.Subtotal = dr.IsNull("SUBTOTAL") ? 0 : dr.Field<double>("SUBTOTAL");
            venta.Total = dr.IsNull("TOTAL") ? 0 : dr.Field<double>("TOTAL");

            venta.Tercero = new Tercero()
            {
                COD_CLI = dr.Field<string>("COD_CLI"),
                Direccion = dr.Field<string>("DIR_OFICINA"),
                Nombre = dr.Field<string>("NOMBRE"),
                Telefono = dr.Field<string>("TEL_OFICINA"),
                identificacion = dr.Field<string>("NIT"),
                tipoIdentificacion = dr.Field<int?>("tipoIdentificacion"),
                tipoIdentificacionS = dr.Field<string>("TIPO_NIT"),
            };
            var type = dr["COD_CAR"].GetType().Name;
            var typesur = dr["COD_SUR"].GetType().Name;
            venta.Cara = dr.Table.Columns.Contains("COD_CAR") ? (dr.IsNull("COD_CAR") ? "0" : Convert.ToInt16(dr["COD_CAR"]).ToString()) : "0";
            venta.Surtidor = dr.Table.Columns.Contains("COD_SUR") ? (dr.IsNull("COD_SUR") ? "0" : Convert.ToInt16(dr["COD_SUR"]).ToString()) : "0";
            venta.CodigoInterno = dr.Table.Columns.Contains("COD_INT") ? (dr.IsNull("COD_INT") ? "" : dr.Field<string>("COD_INT")) : "";
            venta.codigoFormaPago = dr.IsNull("KIL_ACT") ? 0 : dr.Field<int>("COD_FOR_PAG");
            venta.fechaUltimaActualizacion = dr.IsNull("FECH_ULT_ACTU") ? null : dr.Field<DateTime?>("FECH_ULT_ACTU");

            venta.Combustible = dr.IsNull("DESCRIPCION") ? "" : dr.Field<string>("DESCRIPCION");
            venta.Descuento = dr.IsNull("DESCUENTO") ? 0 : dr.Field<double>("DESCUENTO");
            venta.Empleado = dr.IsNull("VENDEDOR") ? "" : dr.Field<string>("VENDEDOR");

            var suma = Convert.ToInt32(venta.Precio * venta.Cantidad);
            var sumaTotal = Convert.ToInt32((venta.Precio * venta.Cantidad) - venta.Descuento);
            if (suma >= 1000000 && venta.Total < 1000000)
            {
                venta.Total = suma;
            }

            return venta;
        }

        public static IEnumerable<FormaPagoSiges> ConvertirFormasPagosSP(this DataTable dt)
        {
            List<FormaPagoSiges> response = new List<FormaPagoSiges>();

            response.AddRange(
                dt.AsEnumerable().Select(dr => new FormaPagoSiges()
                {
                    Id = dr.Field<short>("COD_FOR_PAG"),
                    Descripcion = dr.Field<string>("DESCRIPCION")
                })
            );
            return response;
        }

        public static List<CaraSiges> ConvertirCaraSP(this DataTable dt)
        {
            List<CaraSiges> response = new List<CaraSiges>();
            foreach (var dr in dt.AsEnumerable())
            {
                response.Add(
                new CaraSiges()
                {
                    Id = dr.Field<short>("COD_CAR"),
                    Descripcion = dr.Field<string>("DESCRIPCION"),
                    Isla = dr.Field<string>("Isla"),
                    IdIsla = dr.Field<short>("IdIsla")



                });
            }
            return response;
        }



        public static IEnumerable<Venta> ConvertirVentaSP(this DataTable dt)
        {
            List<Venta> response = new List<Venta>();

            response.AddRange(
                dt.AsEnumerable().Select(dr =>

                    CrearVentaSP(dr)
                )
            );
            return response;
        }

        private static Venta CrearVentaSP(DataRow dr)
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

            var type = dr["COD_CAR"].GetType().Name;
            var typesur = dr["COD_SUR"].GetType().Name;
            venta.COD_CAR = dr.Table.Columns.Contains("COD_CAR") ? (dr.IsNull("COD_CAR") ? (short)0 : Convert.ToInt16(dr["COD_CAR"])) : (short)0;
            venta.COD_SUR = dr.Table.Columns.Contains("COD_SUR") ? (dr.IsNull("COD_SUR") ? (short)0 : Convert.ToInt16(dr["COD_SUR"])) : (short)0;
            venta.COD_INT = dr.Table.Columns.Contains("COD_INT") ? (dr.IsNull("COD_INT") ? "" : dr.Field<string>("COD_INT")) : "";
            venta.COD_FOR_PAG = dr.IsNull("KIL_ACT") ? 0 : dr.Field<int>("COD_FOR_PAG");
            venta.FECH_ULT_ACTU = dr.IsNull("FECH_ULT_ACTU") ? null : dr.Field<DateTime?>("FECH_ULT_ACTU");

            venta.Combustible = dr.IsNull("DESCRIPCION") ? "" : dr.Field<string>("DESCRIPCION");
            venta.Descuento = dr.IsNull("DESCUENTO") ? 0 : dr.Field<decimal>("DESCUENTO");
            venta.EMPLEADO = dr.IsNull("VENDEDOR") ? "" : dr.Field<string>("VENDEDOR");

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


        public static IEnumerable<FacturaCanastilla> ConvertirFacturaCanastilla(this DataTable dt)
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
                    fc.descuento = Convert.ToSingle(dr.Field<double>("descuento"));
                    fc.subtotal = Convert.ToSingle(dr.Field<double>("subtotal"));
                    fc.total = Convert.ToSingle(dr.Field<double>("total"));
                    fc.iva = Convert.ToSingle(dr.Field<double>("iva"));
                    fc.resolucion = ConvertirResolucion(dt).FirstOrDefault();
                    fc.enviada = dr.Field<bool>("enviada");
                    fc.terceroId = new Tercero();

                    fc.terceroId.COD_CLI = dr.IsNull("COD_CLI") ? "" : dr.Field<string>("COD_CLI");
                    fc.terceroId.Direccion = dr.Field<string>("direccion");
                    fc.terceroId.Nombre = dr.Field<string>("Nombre");
                    fc.terceroId.Telefono = dr.Field<string>("Telefono");
                    fc.terceroId.identificacion = dr.Field<string>("identificacion");

                    fc.terceroId.Correo = dr.Field<string>("correo");
                    fc.terceroId.terceroId = dr.Field<int>("terceroId");
                    fc.terceroId.tipoIdentificacion = dr.Field<int?>("tipoIdentificacion");
                    fc.terceroId.tipoIdentificacionS = dr.Field<string>("descripcion");


                    return fc;
                })
            );
            return response;
        }

        public static IEnumerable<CanastillaFactura> ConvertirFacturaCanastillaDEtalle(this DataTable dt)
        {
            List<CanastillaFactura> response = new List<CanastillaFactura>();

            response.AddRange(
                dt.AsEnumerable().Select(dr => new CanastillaFactura()
                {
                    cantidad = Convert.ToSingle(dr.Field<double>("cantidad")),
                    iva = Convert.ToSingle(dr.Field<double>("iva")),
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

        public static IEnumerable<Manguera> ConvertirManguera(this DataTable dt)
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

        public static IEnumerable<Factura> ConvertirFactura(this DataTable dt)
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

                    Tercero = new Tercero()
                    {
                        COD_CLI = dr.Field<string>("COD_CLI"),
                        Direccion = dr.Field<string>("direccion"),
                        Nombre = dr.Field<string>("Nombre"),
                        Telefono = dr.Field<string>("Telefono"),
                        identificacion = dr.Field<string>("identificacion"),

                        Correo = dr.Field<string>("correo"),
                        terceroId = dr.Field<int>("terceroId"),
                        tipoIdentificacion = dr.Field<int?>("tipoIdentificacion"),
                        tipoIdentificacionS = dr.Field<string>("descripcion"),
                    },
                })
            );
            return response;
        }

        public static Bolsa ConvertirBolsa(this DataTable dt)
        {
            var response = new Bolsa();
            var drBolsa = dt.Rows[0];
            response.Fecha = drBolsa.Field<DateTime>("Fecha");
            response.Consecutivo = drBolsa.Field<int>("Consecutivo");
            response.NumeroTurno = drBolsa.Field<int>("NumeroTurno");
            response.Isla = drBolsa.Field<string>("Isla");
            response.Empleado = drBolsa.Field<string>("Empleado");
            response.Moneda = Convert.ToDouble(drBolsa.Field<decimal>("Moneda"));
            response.Billete = Convert.ToDouble(drBolsa.Field<decimal>("Billete"));
            return response;

        }

        public static TurnoSiges ConvertirTurno(this DataSet ds)
        {
            var response = new TurnoSiges();
            var dtTurno = ds.Tables[0];
            var drTurno = dtTurno.Rows[0];
           
            response.Id = drTurno.Field<short>("Id");
            response.Empleado = drTurno.Field<string>("NOMBRE");
            response.Isla = drTurno.Field<string>("Isla");
            response.IdEstado = drTurno.Field<int>("IdEstado");
            response.FechaApertura = drTurno.Field<DateTime>("FechaApertura");
            response.FechaCierre = drTurno.Field<DateTime>("FechaApertura");
            var dtLecturas = ds.Tables[1];
            response.turnoSurtidores = dtLecturas.AsEnumerable().Select(x => new TurnoSurtidor()
            {
                Apertura = Convert.ToDouble(drTurno.Field<decimal>("Apertura")),
                Cierre = Convert.ToDouble(drTurno.Field<decimal>("Apertura")),
                Combustible = new Combustible() { Descripcion = drTurno.Field<string>("Combustible"),
                Precio = Convert.ToDouble(drTurno.Field<decimal>("precioCombustible")),
                },
                Manguera = new MangueraSiges() { 
                    Descripcion = drTurno.Field<string>("Manguera"),
                },
            }).ToList();
            return response;
        }
        private static long getNumericValue(object dbValue)
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
    }
}
