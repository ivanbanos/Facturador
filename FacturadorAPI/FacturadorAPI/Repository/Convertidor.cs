using Dominio.Entidades;
using FactoradorEstacionesModelo.Fidelizacion;
using FacturadorAPI.Models;
using FacturadorAPI.Models.Externos;
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
                    Id = dr.Field<int>("Id"),
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
                    Id = dr.Field<int>("Id"),
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
    }
}
