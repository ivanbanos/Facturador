using System;
using System.Collections.Generic;
using System.Text;

namespace ManejadorSurtidor.SICOM
{
    public class Sicom
    {
        public string Url { get; set; }
        public string Usuario { get; set; }
        public string Contrasena { get; set; }
        public string PuertoBoton { get; set; }
        public string APIKey { get; set; }
        public bool ValidarOnline { get; set; }
    }
}
