using System;
using System.Collections.Generic;
using System.Text;

namespace FacturadorAPI.Models
{
    public class MangueraSiges
    {
        public int Id { get; set; }
        public string Descripcion { get; set; }
        public string Ubicacion { get; set; }
        public string Estado { get; set; }
        public double ultimaVenta { get; set; }
        public bool Totalizador { get; set; } = false;
        public bool esperando { get; set; }
        public bool Vendiendo { get; set; }
        public VehiculoSuic Vehiculo { get; set; }
        public double NuevoTotalizador { get; set; }
        public DateTime Date { get; set; }
        public double NuevaVenta { get; set; }
        public bool CambioVenta { get; set; }
        public int tiempoOcio { get; set; }
    }
}
