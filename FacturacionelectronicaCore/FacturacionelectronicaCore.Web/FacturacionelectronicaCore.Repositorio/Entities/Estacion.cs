using System;

namespace FacturacionelectronicaCore.Repositorio.Entities
{
    public class Estacion
    {
        public Guid guid { get; set; }
        public string Direccion { get; set; }
        public string linea1 { get; set; }
        public string linea2 { get; set; }
        public string linea3 { get; set; }
        public string linea4 { get; set; }
        public string Nit { get; set; }
        public string Nombre { get; set; }
        public string Razon { get; set; }
        public string Telefono { get; set; }
    }
}
