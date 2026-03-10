using EnviadorInformacionService.Models.Externos;
using System.Configuration;

namespace ReporteFacturas
{
    public class InfoEstacion
    {
        public InfoEstacion()
        {
        }
        public InfoEstacion(Estacion estacion)
        {
            vecesPermitidasImpresion = int.Parse(ConfigurationManager.AppSettings["vecesPermitidasImpresion"].ToString());
            CaracteresPorPagina = int.Parse(ConfigurationManager.AppSettings["CaracteresPorPagina"].ToString());
            Direccion = estacion.Direccion;
            Linea1 = estacion.linea1;
            Linea2 = estacion.linea2;
            Linea3 = estacion.linea3;
            Linea4 = estacion.linea4;
            NIT = estacion.Nit;
            Nombre = estacion.Nombre;
            Razon = estacion.Razon;
            Telefono = estacion.Telefono;
        }
        public int vecesPermitidasImpresion { get; set; }
        public string Razon { get; set; }
        public string NIT { get; set; }
        public string Nombre { get; set; }
        public string Direccion { get; set; }
        public string Telefono { get; set; }
        public int CaracteresPorPagina { get; set; }
        public string Linea1 { get; set; }
        public string Linea2 { get; set; }
        public string Linea3 { get; set; }
        public string Linea4 { get; set; }
        public bool ImpresionPDA { get; set; }
        public bool Rifa { get; set; }
        public decimal MontoMinimoRifa { get; set; } = 10000m;
    }
}
