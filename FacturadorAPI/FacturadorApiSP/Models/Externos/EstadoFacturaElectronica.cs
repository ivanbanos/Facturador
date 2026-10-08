namespace FacturadorAPI.Models.Externos
{
    /// <summary>
    /// Respuesta de la web (SIGES) indicando si la venta ya tiene factura electronica emitida.
    /// </summary>
    public class EstadoFacturaElectronica
    {
        public bool TieneFacturaElectronica { get; set; }
        public string? Numero { get; set; }
        public string? Identificacion { get; set; }
        public string? NombreTercero { get; set; }
    }
}
