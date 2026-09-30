using System;

namespace FacturacionelectronicaCore.Repositorio.Entities
{
    public class CombustibleEstacion
    {
        public string Combustible { get; set; }
        public decimal Precio { get; set; }
        public bool EsGas { get; set; }
        public DateTime FechaActualizacion { get; set; }
    }
}
