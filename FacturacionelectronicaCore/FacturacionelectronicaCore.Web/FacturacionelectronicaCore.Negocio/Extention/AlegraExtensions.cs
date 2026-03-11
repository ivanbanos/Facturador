
using FacturacionelectronicaCore.Repositorio;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace FacturacionelectronicaCore.Negocio.Contabilidad.FacturacionElectronica
{
    public static class AlegraExtensions
    {


        public static IServiceCollection AddFacturaElectronica(
        this IServiceCollection services,
        IConfiguration configuration)
        {
            switch (configuration["Alegra:Proveedor"])
            {
                case "ALEGRA":
                    services.AddScoped<IFacturacionElectronicaFacade, AlegraFacade>();
                    break;
                case "SILOG":
                    services.AddScoped<IFacturacionElectronicaFacade, FacturacionSilog>();
                    break;
                case "SILOG2":
                    services.AddScoped<IFacturacionElectronicaFacade, FacturacionSilog2>();
                    break;
                case "DATATICO":
                case "DATAICO":
                    services.AddScoped<IFacturacionElectronicaFacade, FacturacionDataico>();
                    break;
                case "TITAN":
                    services.AddScoped<IFacturacionElectronicaFacade, FacturacionTitan>();
                    break;
                case "SIIGO":
                    services.AddScoped<IFacturacionElectronicaFacade, FacturacionSiigo>();
                    break;
                case "FACTURA1":
                    services.AddScoped<IFacturacionElectronicaFacade, FacturacionFactura1>();
                    break;
            }

            services.Configure<Alegra>(options => configuration.GetSection("Alegra").Bind(options));
            return services;
        }


        public static Invoice ConvertirAInvoice(this Modelo.Factura factura, Item item)
        {
            return new Invoice()
            {
                client = factura.Tercero.idFacturacion,
                date = factura.Fecha.ToString("yyyy-MM-dd"),
                dueDate = factura.Fecha.ToString("yyyy-MM-dd"),
                paymentForm = GetPaymentForm(factura.FormaDePago),
                paymentMethod = GetPaymentMethod(factura.FormaDePago),
                stamp = new Stamp() { generateStamp = true },
                items = new System.Collections.Generic.List<ItemInvoice>() {
                 new ItemInvoice()
                 {
                     id = item.id,
                     description = factura.Combustible,
                     price = (int)factura.Precio,
                     quantity = factura.Cantidad.ToString("0.##"),
                     discount = ((factura.Descuento  / factura.SubTotal) * 100m).ToString("0.##").Replace(",",".")

                 }
                 }
            };
        }
        public static Invoice ConvertirAInvoice(this Modelo.OrdenDeDespacho orden, Item item, int client=0)
        {
            return new Invoice()
            {
                client = client!=0?client.ToString():orden.Tercero.idFacturacion,
                date = orden.Fecha.ToString("yyyy-MM-dd"),
                dueDate = orden.Fecha.ToString("yyyy-MM-dd"),
                paymentForm = GetPaymentForm(orden.FormaDePago),
                paymentMethod = GetPaymentMethod(orden.FormaDePago),
                numberDeliveryOrder=orden.IdVentaLocal.ToString(),
                 stamp = new Stamp() { generateStamp = true},
                 items = new System.Collections.Generic.List<ItemInvoice>() { 
                 new ItemInvoice()
                 {
                     id = item.id,
                     description = orden.Combustible,
                     price = (int)orden.Precio,
                     quantity = orden.Cantidad.ToString("0.##"),
                     discount =  ((orden.Descuento / orden.SubTotal) * 100m).ToString("0.##").Replace(",", ".")

                 }
                 }
            };
        }

        public static Invoice ConvertirAInvoice(this List<Modelo.OrdenDeDespacho> ordenes, Modelo.Tercero tercero, IEnumerable<Item> items)
        {
            var itemsToFactura = new System.Collections.Generic.List<ItemInvoice>();
            foreach (var orden in ordenes)
            {
                itemsToFactura.Add(new ItemInvoice()
                {
                    id = items.First(x=>x.name == orden.Combustible).id,
                    description = orden.Combustible,
                    price = (int)orden.Precio,
                    quantity = orden.Cantidad.ToString("0.##"),
                    discount = ((orden.Descuento / orden.SubTotal) * 100m).ToString("0.##").Replace(",", ".")

                });
                 
            }
            return new Invoice()
            {
                client = tercero.idFacturacion,
                date = DateTime.Now.ToString("yyyy-MM-dd"),
                dueDate = DateTime.Now.ToString("yyyy-MM-dd"),
                paymentForm = GetPaymentForm("Efectivo"),
                paymentMethod = GetPaymentMethod("Efectivo"),
                stamp = new Stamp() { generateStamp = true },
                items = itemsToFactura
            };
        }


        public static Invoice ConvertirAInvoice(this List<Modelo.Factura> facturas, Modelo.Tercero tercero, IEnumerable<Item> items)
        {
            var itemsToFactura = new System.Collections.Generic.List<ItemInvoice>();
            foreach (var factura in facturas)
            {
                itemsToFactura.Add(new ItemInvoice()
                {
                    id = items.First(x => x.name == factura.Combustible).id,
                    description = factura.Combustible,
                    price = (int)factura.Precio,
                    quantity = factura.Cantidad.ToString("0.##"),
                    discount = ((factura.Descuento / factura.SubTotal) * 100m).ToString("0.##").Replace(",", ".")

                });

            }
            return new Invoice()
            {
                client = tercero.idFacturacion,
                date = DateTime.Now.ToString("yyyy-MM-dd"),
                dueDate = DateTime.Now.ToString("yyyy-MM-dd"),
                paymentForm = GetPaymentForm("Efectivo"),
                paymentMethod = GetPaymentMethod("Efectivo"),
                stamp = new Stamp() { generateStamp = true },
                items = itemsToFactura
            };
        }

        public static Invoice ConvertirAInvoice(this Modelo.FacturaCanastilla factura, Modelo.Tercero tercero, IEnumerable<Item> items, int client = 0)
        {
            var itemsToFactura = new System.Collections.Generic.List<ItemInvoice>();
            
            foreach (var canastilla in factura.canastillas)
            {
                var item = items.FirstOrDefault(x => x.name == canastilla.Canastilla.descripcion);
                if (item != null)
                {
                    itemsToFactura.Add(new ItemInvoice()
                    {
                        id = item.id,
                        description = canastilla.Canastilla.descripcion,
                        price = (int)(canastilla.total / canastilla.cantidad), // precio unitario
                        quantity = canastilla.cantidad.ToString("0.##"),
                        discount = "0.00" // FacturaCanastilla doesn't have individual discounts per item
                    });
                }
            }

            return new Invoice()
            {
                client = client != 0 ? client.ToString() : tercero.idFacturacion,
                date = factura.fecha.ToString("yyyy-MM-dd"),
                dueDate = factura.fecha.ToString("yyyy-MM-dd"),
                paymentForm = GetPaymentForm(factura.codigoFormaPago?.Descripcion ?? "Efectivo"),
                paymentMethod = GetPaymentMethod(factura.codigoFormaPago?.Descripcion ?? "Efectivo"),
                stamp = new Stamp() { generateStamp = true },
                items = itemsToFactura
            };
        }

        private static string GetPaymentMethod(string formaDePago)
        {
            if (formaDePago == "Efectivo")
            {
                return "INSTRUMENT_NOT_DEFINED";
            }
            return "INSTRUMENT_NOT_DEFINED";
        }

        private static string GetPaymentForm(string formaDePago)
        {
            if(formaDePago == "Efectivo")
            {
                return "CASH";
            }
            else
            {
                return "CASH";//return "CREDIT";
            }
        }

        public static Contacts ConvertirAContact(this Modelo.Tercero tercero)
        {
            if (tercero.DescripcionTipoIdentificacion == "Nit")
            {

                return new Contacts()
                {
                    name = tercero.Nombre,
                    identificationObject = new Identification() { 
                        number = tercero.Identificacion, type = GetTipoIdentificacion(tercero.DescripcionTipoIdentificacion) ,
                    dv=9},
                    kindOfPerson = GetKindOfPErson(tercero.DescripcionTipoIdentificacion),
                    regime = GetRegime(tercero.ResponsabilidadTributaria),
                    ignoreRepeated=false
                };
            }
            else
            {
                var nombre = "";
                var apellido = "";
                var nombreCompleto = tercero.Nombre.Trim();
                if (string.IsNullOrEmpty(tercero.Apellidos) || tercero.Apellidos.Contains("no informado"))
                {
                    if (nombreCompleto.Split(' ').Count() > 1)
                    {
                        nombre = nombreCompleto.Substring(0, nombreCompleto.LastIndexOf(" "));
                        apellido = nombreCompleto.Split(' ').Last();
                    }
                    else
                    {
                        nombre = nombreCompleto;
                        apellido = "no informado";
                    }
                }
                else
                {
                    nombre = nombreCompleto;
                    apellido = tercero.Apellidos;
                }
                return new Contacts()
                {
                    nameObject = new Name() { firstName = nombre, lastName = apellido },
                    identificationObject = new Identification() { number = tercero.Identificacion, type = GetTipoIdentificacion(tercero.DescripcionTipoIdentificacion) },
                    kindOfPerson = GetKindOfPErson(tercero.DescripcionTipoIdentificacion),
                    regime = GetRegime(tercero.ResponsabilidadTributaria),
                    ignoreRepeated = false
                };
            }
        }

        private static string GetRegime(int responsabilidadTributaria)
        {
            switch (responsabilidadTributaria)
            {
                case 1:
                    return "COMMON_REGIME";
                case 2:
                    return "SIMPLIFIED_REGIME";
                case 3:
                    return "NOT_REPONSIBLE_FOR_CONSUMPTION";
                case 4:
                    return "SPECIAL_REGIME";
                case 5:
                    return "NATIONAL_CONSUMPTION_TAX";
                default:
                    return "COMMON_REGIME";
            }
        }

        private static string GetKindOfPErson(string descripcionTipoIdentificacion)
        {
            if (descripcionTipoIdentificacion == "Nit")
            {
                return "LEGAL_ENTITY";
            }
            else
            {
                return "PERSON_ENTITY";
            }
        
        }

        private static string GetTipoIdentificacion(string descripcionTipoIdentificacion)
        {
            if(descripcionTipoIdentificacion == "Nit")
            {
                return "NIT";
            }
            else
            {
                return "CC";
            }
        }
    }
}
