using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FacturacionelectronicaCore.Negocio.Contabilidad.FacturacionElectronica
{
    public class Factura1
    {
        public string usuario { get; set; }
        public string contrasena { get; set; }
        public string sucursal { get; set; }
        public string base64doc { get; set; }
    }
    public class Factura1TokenRequest
    {
        public string username { get; set; }
        public string password { get; set; }
    }
    public class Factura1TokenResponse
    {
        public string Token { get; set; }
    }
}
