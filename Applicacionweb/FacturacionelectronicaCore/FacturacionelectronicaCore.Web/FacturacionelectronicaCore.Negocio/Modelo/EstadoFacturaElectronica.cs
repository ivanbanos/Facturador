namespace FacturacionelectronicaCore.Negocio.Modelo
{
    /// <summary>
    /// Indica si una venta (orden de despacho) ya tiene factura electronica emitida y a nombre de quien.
    /// Lo usa el facturador local para bloquear el cambio de tercero una vez emitida la factura.
    /// </summary>
    public class EstadoFacturaElectronica
    {
        public bool TieneFacturaElectronica { get; set; }
        public string Numero { get; set; }
        public string Identificacion { get; set; }
        public string NombreTercero { get; set; }
    }
}
