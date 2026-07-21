namespace FacturadorAPI.Models
{
    public class FidelizarVentaRequestDto
    {
        public int FacturaPOSId { get; set; }
        public int TerceroId { get; set; }
        public int CodigoFormaPago { get; set; }
        public int? CodigoFormaPago2 { get; set; }
        public string Placa { get; set; } = "NP";
        public string Kilometraje { get; set; } = "NP";
        public string NumeroTransaccion { get; set; } = "NP";
        public double? Total1 { get; set; }
        public double? Total2 { get; set; }
    }
}
