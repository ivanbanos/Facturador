using System;
using System.Collections.Generic;
using System.Text;

namespace FacturadorEstacionesPOSWinForm
{
    public class InfoEstacion
    {
        public string UrlLocalService { get; set; }
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
        public string ip { get; set; }
        public string puerto { get; set; }
        public int vecesImprimir { get; set; }
        public string ArchivoMovimientoContable { get; set; }
        public bool ConvertirAFactura { get; set; }
        public bool CreaMovimientoContable { get; set; }
        public bool ConvertirAOrden { get; set; }
        public string EstacionFuente { get; set; }
        public bool ImpresionAutomatica { get; set; }
        public bool ImpresionFormaDePagoOrdenDespacho { get; set; }
        public bool GeneraFacturaElectronica { get; internal set; }
        public bool ImpresionPDA { get; internal set; }
    }
}
