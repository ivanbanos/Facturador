using System.Collections.Generic;

namespace FacturacionelectronicaCore.Negocio.Modelo
{
    public class ReporteFiscal
    {
        public int TotalFacturasAnuladas { get; set; }
        public int TotalOrdenesAnuladas { get; set; }

        public int ConsecutivoFacturaInicial { get; set; }
        public int ConsecutivoDeFacturaFinal { get; set; }
        public int TotalDeVentas { get; set; }
        public int TotalDeOrdenes { get; set; }
        public IEnumerable<ConsolidadoCombustible> Consolidados { get; set; }
        public IEnumerable<ConsolidadoCombustible> ConsolidadosOrdenes { get; set; }
        public IEnumerable<ConsolidadoCliente> consolidadoClienteFacturas { get; set; }
        public IEnumerable<ConsolidadoCliente> consolidadoClienteOrdenes { get; set; }
        public int CantidadDeFacturasAnuladas { get; set; }
        public IEnumerable<ConsolidadoCombustible> ConsolidadoFacturasAnuladas { get; set; }
        public IEnumerable<ConsolidadoCombustible> ConsolidadoOrdenesAnuladas { get; set; }
        
        // Nuevos consolidados por forma de pago
        public IEnumerable<ConsolidadoFormaPago> ConsolidadoFormaPagoFacturas { get; set; }
        public IEnumerable<ConsolidadoFormaPago> ConsolidadoFormaPagoOrdenes { get; set; }
    }
}
