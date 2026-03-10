using System;
using System.Collections.Generic;
using System.Text;

namespace FactoradorEstacionesModelo.Objetos
{
    public class Tercero
    {
        public string tipoIdentificacionS;
        public Tercero()
        { }
            public Tercero(FacturacionelectronicaCore.Negocio.Modelo.Tercero x)
        {
            this.terceroId = x.IdLocal;
            this.Nombre = x.Nombre;
                this.Apellidos = x.Apellidos;
            this.Telefono = x.Telefono;
            this.Direccion = x.Direccion;
            this.identificacion = x.Identificacion;
            this.Correo = x.Correo;
            this.tipoIdentificacionS = x.DescripcionTipoIdentificacion;
        }

        public int terceroId { get; set; }
        public string COD_CLI { get; set; }
        public string Nombre { get; set; }
        public string Apellidos { get; set; }
        public string Telefono { get; set; }
        public string Direccion { get; set; }
        public string identificacion { get; set; }
        public string Correo { get; set; }
        public int? tipoIdentificacion { get; set; }
        public bool? EnviadoSiesa { get; internal set; }
    }
}
