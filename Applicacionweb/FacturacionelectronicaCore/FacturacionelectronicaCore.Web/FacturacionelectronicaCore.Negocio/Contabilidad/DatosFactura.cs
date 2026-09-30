using FacturacionelectronicaCore.Negocio.Contabilidad.FacturacionElectronica;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Configuration;
using System.Globalization;
using System.Linq;

namespace FacturacionelectronicaCore.Negocio.Contabilidad
{
    public class DatosFactura
    {

        public DatosFactura(FacturaSilog factura, string usuario, Alegra options, string estacion)
        {
            Consecutivo = factura.Consecutivo + "";
            Placa = factura.Placa ?? "";
            Kilometraje = factura.Kilometraje ?? "0.00";
            var transaccion = !string.IsNullOrWhiteSpace(factura.numeroTransaccion) ? factura.numeroTransaccion : "N/A";
            Detalles = $"Placa: {Placa}, Kilometraje : {Kilometraje}, Nro Transaccion : {transaccion}";
            Surtidor = factura.Surtidor;
            Cara = factura.Cara;
            Manguera = factura.Manguera;
            if (DateTime.Now.Date < factura.Fecha.Date)
            {
                FechaFacturacion = DateTime.Now.ToString("yyyy-MM-dd");
            }
            else
            {

                FechaFacturacion = factura.Fecha.ToString("yyyy-MM-dd");
            }
            Tercero = factura.Tercero.Identificacion;
            Usuario = usuario;

            Cantidad = factura.Cantidad.ToString("F3",
                  CultureInfo.InvariantCulture);
            Precio = factura.Precio.ToString("F3",
                  CultureInfo.InvariantCulture);
            Descuento = factura.Descuento.ToString("F3",
                  CultureInfo.InvariantCulture);
            Iva = "0";
            Total = (factura.SubTotal - factura.Descuento).ToString("F3",
                  CultureInfo.InvariantCulture);
            Subtotal = factura.SubTotal.ToString("F3",
                  CultureInfo.InvariantCulture);
            FechaProximoMantenimiento = factura.FechaProximoMantenimiento;
            Guid = factura.Guid;
            Prefijo = factura.Prefijo;
            Console.WriteLine();
            if (options.Cliente.ToLower() == "cotaxi")
            {
                if (factura.FormaDePago.ToLower().Contains("efectivo"))
                {
                    FormaPago = "1";
                    TipoTarjeta = "";
                    NroTransaccion = "";
                }
                else if (factura.FormaDePago.ToLower().Contains("dito"))
                {
                    FormaPago = "3";
                    TipoTarjeta = "";
                    NroTransaccion = "";
                }
                else if (factura.FormaDePago.ToLower().Contains("trans"))
                {
                    FormaPago = "7";
                    TipoTarjeta = "";
                    NroTransaccion = factura.numeroTransaccion ?? factura.Kilometraje ?? "NA";
                }
                else
                {
                    FormaPago = "2";
                    TipoTarjeta = GetByFormaPago(factura.FormaDePago.Trim().ToLower());
                    NroTransaccion = factura.numeroTransaccion ?? factura.Kilometraje ?? "NA";
                }

            }
            else
            {
                if (factura.FormaDePago.ToLower().Contains("efe"))
                {
                    FormaPago = "1";
                    TipoTarjeta = "";
                    NroTransaccion = "";
                }
                else if (factura.FormaDePago.ToLower().Contains("dat"))
                {
                    FormaPago = "2";
                    TipoTarjeta = GetByFormaPagoCootranshuila(factura.FormaDePago.Trim(), estacion);
                    NroTransaccion = factura.Kilometraje ?? "";
                }
                else
                {
                    FormaPago = "3";
                    TipoTarjeta = GetByFormaPagoCootranshuila(factura.FormaDePago.Trim(), estacion);
                    NroTransaccion = factura.Kilometraje ?? "";
                }
            }

            // Get station-specific fuel codes from Alegra configuration
            EstacionCombustibles combustibleConfig = null;
            if (options != null && options.Estaciones != null)
            {
                // Try to find the station configuration (case-insensitive comparison)
                var estacionKey = options.Estaciones.Keys.FirstOrDefault(k =>
                    k.Equals(estacion, StringComparison.OrdinalIgnoreCase));

                if (!string.IsNullOrEmpty(estacionKey))
                {
                    combustibleConfig = options.Estaciones[estacionKey];
                }
            }

            // Determine combustible based on factura.Combustible value using station-specific configuration
            var combustibleLower = factura.Combustible?.ToLower() ?? "";

            if (combustibleLower.Contains("corriente"))
            {
                Combustible = combustibleConfig?.Corriente ?? options?.Corriente ?? factura.Combustible;
            }
            else if (combustibleLower.Contains("acpm") ||
                     combustibleLower.Contains("bio") ||
                     combustibleLower.Contains("diesel") ||
                     combustibleLower.Contains("a.c.p.m"))
            {
                Combustible = combustibleConfig?.Acpm ?? options?.Acpm ?? factura.Combustible;
            }
            else if (combustibleLower.Contains("extra"))
            {
                Combustible = combustibleConfig?.Extra ?? options?.Extra ?? factura.Combustible;
            }
            else
            {
                Combustible = combustibleConfig?.Gas ?? options?.Gas ?? factura.Combustible;
            }
        }

        private string GetByFormaPagoCootranshuila(string formaPago, string estacion)
        {
            // Handle null or empty payment form
            if (string.IsNullOrWhiteSpace(formaPago))
            {
                return "21";
            }

            // Clean the input - remove any numeric prefixes or unexpected characters
            var cleanFormaPago = System.Text.RegularExpressions.Regex.Replace(formaPago, @"^\d+", "").Trim();

            if (cleanFormaPago.ToLower().Contains("asum") || cleanFormaPago.ToLower().Contains("cali"))
            {
                return "21";
            }
            else
            {
                if (estacion.ToLower() == "BF5D6034-82FC-484D-A362-65EFB0E4C1C6".ToLower())
                {
                    //libague
                    return "22";
                }
                if (estacion.ToLower() == "F6A3C48F-ACC9-4E9A-9B3C-206A0E52C302".ToLower())
                {
                    //terminal
                    return "23";
                }
                if (estacion.ToLower() == "4E8E1941-D8E4-4732-A3C8-27A2DFEE9048".ToLower())
                {
                    //toma
                    return "9";
                }
                if (estacion.ToLower() == "9DC61E74-AEB4-4E3A-8121-3F20F391C4A3".ToLower())
                {
                    //principal
                    return "9";
                }
            }
            return "21";
        }

        private string GetByFormaPago(string formaPago)
        {
            // Handle null or empty payment form
            if (string.IsNullOrWhiteSpace(formaPago))
            {
                return "21";
            }

            // Clean the input - remove any numeric prefixes or unexpected characters
            var cleanFormaPago = System.Text.RegularExpressions.Regex.Replace(formaPago, @"^\d+", "").Trim().ToLower();
            if (cleanFormaPago.ToLower().Contains("consum") || cleanFormaPago.ToLower().Contains("cali"))
            {
                return "21";
            }
            switch (cleanFormaPago)
            {
                case "visa":
                    return "1";
                case "mastercard":
                    return "9";
                case "dinners":
                    return "2";
                case "amex":
                    return "14";
                case "puntos colombia":
                    return "15";
                case "sodexo quantum":
                    return "16";
                case "vales":
                    return "17";
                case "bonos sodexo":
                    return "18";
                case "prepago":
                    return "27";
                case "consumo interno":
                case "calibración":
                default:
                    return "21";
            }

        }

        public string Consecutivo { get; set; }
        public string Detalles { get; set; }
        public string Placa { get; set; }
        public string Kilometraje { get; set; }
        public string fechaUltimoMantenimiento { get; set; }
        public string Surtidor { get; set; }
        public string Cara { get; set; }
        public string Manguera { get; set; }
        public string FechaFacturacion { get; set; }
        public string Tercero { get; set; }
        public string FormaPago { get; set; }
        public string TipoTarjeta { get; set; } = "";
        public string NroTransaccion { get; set; } = "";
        public string Combustible { get; set; }
        public string Cantidad { get; set; }
        public string Precio { get; set; }
        public string Descuento { get; set; }
        public string Iva { get; set; }
        public string Total { get; set; }
        public DateTime? FechaProximoMantenimiento { get; set; }
        public string Subtotal { get; set; }
        public string Usuario { get; set; }
        public string Prefijo { get; set; }
        public Guid Guid { get; set; }

    }
}
