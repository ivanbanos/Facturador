using System;
using System.Collections.Generic;
using System.Text;

namespace FacturacionelectronicaCore.Repositorio.Entities
{
    public class Canastilla
    {
        public int CanastillaId { get; set; }
        public Guid guid { get; set; }
        public string descripcion { get; set; }
        public string unidad { get; set; }
        public float precio { get; set; }

        public bool deleted { get; set; }
        public int iva { get; set; }
        public string? campoextra { get; set; }
        public Guid? estacion { get; set; }
    }
}
