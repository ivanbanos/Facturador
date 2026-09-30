using System;

namespace FacturacionelectronicaCore.Negocio.Modelo
{
    public class CreacionResolucion
    {
        public int ConsecutivoInicial { get; set; }
        public int ConsecutivoFinal { get; set; }
        public DateTime FechaInicial { get; set; }
        public DateTime FechaFinal { get; set; }
        public int ConsecutivoActual { get; set; }
        public Guid IdEstacion { get; set; }
        public string Autorizacion { get; set; }
        public bool Habilitada { get; set; }
        public string Descripcion { get; set; }
        public int tipo { get; set; }
    }
}
