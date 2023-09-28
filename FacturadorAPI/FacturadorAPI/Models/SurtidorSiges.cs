using System;
using System.Collections.Generic;
using System.Text;

namespace FacturadorAPI.Models
{
    public class SurtidorSiges
    {
        public bool Cerrando { get; set; }

        public string Puerto { get; set; }
        public string Descripcion { get; set; }
        public int Id { get; set; }
        public int Numero { get; set; }
        public List<CaraSiges> caras { get; set; }
        public string PuertoIButton { get; set; }
        
        public int? idTurno  {get;set;}
        public SurtidorSiges()
        {
            ventaPar = 0;
            ventaImpar = 0;
            action = 6;
            esperando = false;
            respondio = false;
        }
        public int lapso { get; set; } = 0;
        public int ventaPar { get; set; }
        public int ventaImpar { get; set; }
        public int action { get; set; }
        public bool esperando { get; set; }
        public List<MangueraSiges> mangueras { get; set; }
        public bool respondio { get; set; }
        public string IButtonPar { get; set; }
        public string IButtonImpar { get; set; }
        public TurnoSiges turno { get; set; }
    }
}
