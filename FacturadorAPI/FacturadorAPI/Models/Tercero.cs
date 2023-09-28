using System;
using System.Collections.Generic;
using System.Text;

namespace FacturadorAPI.Models
{
    public class Tercero
    {
        public Tercero()
        { }
        public Tercero(Externos.TerceroExterno x)
        {
            terceroId = x.IdLocal;
            Nombre = x.Nombre;
            Telefono = x.Telefono;
            Direccion = x.Direccion;
            identificacion = x.Identificacion;
            Correo = x.Correo;
            tipoIdentificacionS = x.DescripcionTipoIdentificacion;
        }

        public string tipoIdentificacionS;

        public int terceroId { get; set; }
        public string COD_CLI { get; set; }
        public string Nombre { get; set; }
        public string Telefono { get; set; }
        public string Direccion { get; set; }
        public string identificacion { get; set; }
        public string Correo { get; set; }
        public int? tipoIdentificacion { get; set; }
    }
}
