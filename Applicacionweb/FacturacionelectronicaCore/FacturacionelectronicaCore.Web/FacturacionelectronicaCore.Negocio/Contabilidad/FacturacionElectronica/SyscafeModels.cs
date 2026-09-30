using System.Collections.Generic;

namespace FacturacionelectronicaCore.Negocio.Contabilidad.FacturacionElectronica
{
    public class SyscafeDocument
    {
        public string tipo { get; set; }
        public string numero { get; set; }
        public string noext { get; set; }
        public string fecha { get; set; }
        public string fechaven { get; set; }
        public string fpago { get; set; }
        public string nit { get; set; }
        public string detalle { get; set; }
        public string obs { get; set; }
        public SyscafeCliente cliente { get; set; }
        public List<SyscafeItem> items { get; set; }
        public List<SyscafeFpago> fpagos { get; set; }
    }

    public class SyscafeItem
    {
        public string referencia { get; set; }
        public string servicio { get; set; }
        public decimal cant { get; set; }
        public decimal precio { get; set; }
        public decimal vrunit { get; set; }
        public decimal vrtotal { get; set; }
        public decimal piva { get; set; }
        public decimal vriva { get; set; }
        public decimal vrico { get; set; }
    }

    public class SyscafeCliente
    {
        public string nit { get; set; }
        public string dv { get; set; }
        public string claseid { get; set; }
        public string nom1 { get; set; }
        public string nom2 { get; set; }
        public string ape1 { get; set; }
        public string ape2 { get; set; }
        public string nombrec { get; set; }
        public string dir { get; set; }
        public string tel { get; set; }
        public string email { get; set; }
    }

    public class SyscafeFpago
    {
        public string fpago { get; set; }
        public decimal vrfpago { get; set; }
        public string ctafpago { get; set; }
    }

    public class SyscafeOptions
    {
        public string Url { get; set; }
        public string Token { get; set; }
        public string TipoComprobante { get; set; } = "FV1";
        public bool Habilitado { get; set; }
    }
}
