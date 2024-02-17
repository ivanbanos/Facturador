

namespace FacturadorAPI.Models.Externos
{
    public class RequestCambiarFechasReporte
    {
        public IEnumerable<FacturaFechaReporte> facturas { get; set; }

        public Guid Estacion { get; set; }
    }
}