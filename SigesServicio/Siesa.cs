using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SigesServicio
{
    public class Siesa
    {
        public string UrlSiesa { get; set; }
        public string IdCompania { get; set; }
        public string Idsistema { get; set; }
        public string IdDocumento { get; set; }
        public string KeySiesa { get; set; }
        public string Tokensiesa { get; set; }
        public string CentroOperacionesCaja { get; set; }
        public string DocumentoFactura { get; set; }
        public string CentroCostoCaja { get; set; }
        public string MovimientoCaja { get; set; }
        public string UnidadNegocioCaja { get; set; }
        public string IdFe { get; set; }
        public string Caja { get; set; }
        public string ConsecutivoAutoRegulado { get; set; }
        public string CentroOperacionesDocumento { get; set; }
        public string CentroOperaciones { get; set; }
        public string CentroCosto { get; set; }
        public string Movimiento { get; set; }
        public string UnidadNegocio { get; set; }
        public string Sucursal { get; set; }

        // Propiedades para pagos no efectivo
        public string? CentroOperacionesContableOtros { get; set; }
        public string? MovimientoContableOtros { get; set; }
        public string? UnidadNegocioContableOtros { get; set; }
        public string? CentroCostoContableOtros { get; set; }
        public string? CentroOperacionesOtros { get; set; }
        public string? MovimientoOtros { get; set; }
        public string? UnidadNegocioOtros { get; set; }
        public string? CentroCostoOtros { get; set; }
        public string? IdFeOtros { get; set; }
        public string? IdDOcumentoCliente { get; set; }
        public string? FechaMinimaEnvioSiesa { get; set; }
        public string? FechaMaximaEnvioSiesa { get; set; }
        public string CentroOperacionesContableDescuento { get; set; }
        public string AuxiliarDescuento { get; set; }
        public string MovimientoContableDescuento { get; set; }
        public string UnidadNegocioDescuento { get; set; }
        public string CentroCostoDescuento { get; set; }
        public string IdFeDescuento { get; set; }
    }
}
