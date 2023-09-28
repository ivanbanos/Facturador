using System;
using System.Collections.Generic;
using System.Text;

namespace FacturadorAPI.Models.Externos
{
    public class ResolucionExterno
    {
        public ResolucionExterno()
        {
        }
        public ResolucionExterno(Models.Resolucion resolucion)
        {
            Descripcion = resolucion.DescripcionResolucion;
            FechaInicial = resolucion.FechaInicioResolucion;
            FechaFinal = resolucion.FechaFinalResolucion;
            ConsecutivoInicial = resolucion.ConsecutivoInicial;
            ConsecutivoFinal = resolucion.ConsecutivoFinal;
            ConsecutivoActual = resolucion.ConsecutivoActual;
            Autorizacion = resolucion.Autorizacion;
            ConsecutivoActual = resolucion.ConsecutivoActual;
            Tipo = resolucion.Tipo;
        }
        public Guid guid { get; set; }
        public int ConsecutivoInicial { get; set; }
        public int ConsecutivoFinal { get; set; }
        public DateTime FechaInicial { get; set; }
        public DateTime FechaFinal { get; set; }
        public Guid IdEstado { get; set; }
        public int ConsecutivoActual { get; set; }
        public DateTime Fecha { get; set; }
        public Guid IdEstacion { get; set; }
        public string Autorizacion { get; set; }
        public bool Habilitada { get; set; }
        public string Descripcion { get; set; }
        public int Tipo { get; set; }

    }
}
