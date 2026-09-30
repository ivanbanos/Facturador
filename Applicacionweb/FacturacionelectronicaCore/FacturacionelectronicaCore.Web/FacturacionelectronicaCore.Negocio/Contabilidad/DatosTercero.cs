using FacturacionelectronicaCore.Negocio.Contabilidad.FacturacionElectronica;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace FacturacionelectronicaCore.Negocio.Contabilidad
{
    public class DatosTercero
    {

        public DatosTercero(FacturaSilog factura)
        {
            Identificacion = factura.Tercero.Identificacion;
            if(factura.Tercero.DescripcionTipoIdentificacion.ToLower().Contains("nit"))
            {
                TipoIdentificacion = 6+"";
            }
            else
            {
                TipoIdentificacion = 3 + "";
            }
            if (factura.Tercero.Identificacion.Contains("222222"))
            {
                Nombre = "menores";
                PrimerApellido = "Cuantías";
            }
            else
            {
                Regex regex = new Regex(@"[ ]{2,}", RegexOptions.None);
                var str = regex.Replace(factura.Tercero.Nombre, @" ");
                var nombres = str.Trim().Split(' ');
                Nombre = nombres[0];
                if (nombres.Length > 1)
                {

                    PrimerApellido = nombres[1];
                }
                else
                {
                    PrimerApellido = "";
                }
                if (nombres.Length > 2)
                {

                    SegundoApellido = nombres[2];
                }
                else
                {
                    SegundoApellido = "";
                }
            }
            if (SegundoApellido == null)
            {
                SegundoApellido = "";
            }
            Direccion = factura.Tercero.Direccion;
            Correo = string.IsNullOrWhiteSpace(factura.Tercero.Correo)|| !factura.Tercero.Correo.Contains("@") ? "correo@correo.com": factura.Tercero.Correo;
            Telefono = factura.Tercero.Telefono;
            
            if(Telefono.Length!=7 && Telefono.Length != 9)
            {
                Telefono = "0000000";
            }
        }

        public string PrimerApellido { get; set; }
		public string SegundoApellido { get; set; }
        public string Identificacion { get; set; }
        public string TipoIdentificacion { get; set; }
        public string Nombre { get; set; }
        public string Direccion { get; set; }
        public string Correo { get; set; }
        public string Telefono { get; set; }

    }
}
