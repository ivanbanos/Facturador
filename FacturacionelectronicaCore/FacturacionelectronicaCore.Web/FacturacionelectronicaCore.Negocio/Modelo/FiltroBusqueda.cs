using System;

namespace FacturacionelectronicaCore.Negocio.Modelo
{
    public class FiltroBusqueda
    {
        public DateTime? FechaInicial { get; set; }
        public DateTime? FechaFinal { get; set; }
        public string Identificacion { get; set; }
        public string NombreTercero { get; set; }
        public Guid Estacion { get; set; }
    }
}
