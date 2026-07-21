using System;
using System.Collections.Generic;
using System.Text;

namespace FactoradorEstacionesModelo.Fidelizacion
{
    public class Puntos
    {
        public Puntos(float valorVenta, string factura, string documentoFidelizado, string nitCentroVenta, string centroVenta = null)
        {
            ValorVenta = valorVenta;
            Factura = factura;
            DocumentoFidelizado = documentoFidelizado;
            NitCentroVenta = nitCentroVenta;
            CentroVenta = centroVenta;
        }

        public float ValorVenta { get; set; }
        public string Factura { get; set; }
        public string DocumentoFidelizado { get; set; }
        public string NitCentroVenta { get; set; }
        public string CentroVenta { get; set; }
    }

}
