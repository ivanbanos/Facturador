using FacturacionelectronicaCore.Negocio.Contabilidad.FacturacionElectronica;
using FacturacionelectronicaCore.Repositorio.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FacturacionelectronicaCore.Negocio.Extention
{
    public static class CommonExtensions
    {
        public static List<TerceroInput> ConvertToTerceros(this IEnumerable<TerceroResponse> terceros)
        {
            List<TerceroInput> result = new List<TerceroInput>();
            result.AddRange(terceros.Select(x => ConvertToTercero(x)));
            return result;
        }

        private static TerceroInput ConvertToTercero(TerceroResponse x)
        {
            return new TerceroInput()
            {
                Nombre = x.name,
                Celular = x.mobile,
                Correo = x.email,
                Telefono = x.phonePrimary,
                Telefono2 = x.phoneSecondary,
                Identificacion = x.identification,
                DescripcionTipoIdentificacion = "Nit",
                Direccion = x.address.address,
                Municipio = x.address.city,
                Pais = x.address.country,
                Departamento = x.address.department,
                CodigoPostal = x.address.zipCode,
                idFacturacion = x.id.ToString()
            };
        }
    }
}
