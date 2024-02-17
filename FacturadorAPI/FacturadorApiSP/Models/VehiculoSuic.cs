using System;
using System.Collections.Generic;
using System.Text;

namespace FacturadorAPI.Models
{
    public class VehiculoSuic
    {
        public string idrom{get;set;}
        public DateTime fechaInicio { get;set;}
        public DateTime fechaFin { get;set;}
        public string placa { get;set;}
        public string vin { get;set;}
        public string servicio { get;set;}
        public string capacidad { get;set;}
        public int estado { get;set;}
        public string motivo { get;set; }
        public string motivoTexto { get; set; }
        public int surtidor { get; set; }
    }
}
