using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace FacturacionelectronicaCore.Negocio.Contabilidad.FacturacionElectronica
{
    public class Alegra
    {

        public string Proveedor { get; set; }
        public bool UsaAlegra { get; set; }
        public string Token { get; set; }
        public string Correo { get; set; }
        public string Url { get; set; }
        public string DataicoAccountId { get; set; }
        public string Auth
        {
            get
            {
                var authToken = $"{Correo}:{Token}";
                var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(authToken);
                return System.Convert.ToBase64String(plainTextBytes);
            }
        }
        public string Cliente { get; set; }
        public bool ValidaTercero { get; set; }
        public string ResolutionNumber { get; set; }
        public string Prefix { get; set; }
        public int Current { get; set; }
        public bool MultiplicarPorDies { get; set; }
        public string AccessKey { get; set; }
        public int Seller { get; set; }
        public int Documento { get; set; }
        public string Corriente { get; set; }
        public string Acpm { get; set; }
        public string Extra { get; set; }
        public string Gas { get; set; }
        public bool EnvioDirecto { get; set; }
        public bool EnviaCreditos { get; set; }
        public bool EnviaMes { get; set; }
        public bool Desactivado { get; set; }
        public string City { get; set; }
        public string Department { get; set; }
        public bool ExcluirDireccion { get; set; }


        public string Contrasena { get; set; }
        public string Usuario { get; set; }
        public string Nit { get; set; }
        public int Debito { get; set; }
        public int Credito { get; set; }
        public int Efectivo { get; set; }

        // Nueva propiedad para configuración de combustibles por estación
        public Dictionary<string, EstacionCombustibles> Estaciones { get; set; }
        public bool AutenticaPorDefecto { get; set; }
        public object Desde { get; set; }
        public object Hasta { get; set; }
        public object DesdeFecha { get; set; }
        public object HastaFecha { get; set; }
        public bool EnviaTodo { get; set; }

        // Number of hours to add to incoming dates to align them with the server timezone.
        // Use a positive or negative integer depending on the required offset.
        // If not specified (null), a default of 0 will be used (no offset).
        public int? ServerTimeOffsetHours { get; set; }

        // Exact start date for Worker to begin processing orders from.
        // If not specified (null), defaults to 2 months ago.
        // Format: "yyyy-MM-dd" or "yyyy-MM-ddTHH:mm:ss"
        public DateTime? WorkerStartDate { get; set; }
        public int? ServerTimeOffsetHoursSearch { get; set; }
    }

    public class EstacionCombustibles
    {
        public string Corriente { get; set; }
        public string Acpm { get; set; }
        public string Extra { get; set; }
        public string Gas { get; set; }
    }
}
