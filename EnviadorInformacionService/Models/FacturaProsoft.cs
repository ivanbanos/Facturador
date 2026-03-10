using FactoradorEstacionesModelo.Objetos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnviadorInformacionService.Models
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class FacturaProsoft
    {

        public FacturaProsoft(Factura x, string forma)
        {
            nro = x.ventaId.ToString();
            fecha = x.fecha.ToString("dd/MM/yyyy");
            id_cliente = x.Tercero.identificacion;
            id_cliente = x.Tercero.Nombre;
            vendedor = x.Venta.EMPLEADO;
            forma_pago = getForma(forma);
            retencion = "000000000000";
            rete_iva = "000000000000";
            rete_ica = "000000000000";
            items = new List<Item>() { new Item() {
                grupo = "0EM",
                codigo = getCodigoCombustible(x.Venta.Combustible),
                clase = " ",
                cantidad = x.Venta.CANTIDAD.ToString(),
                vlr_unit = x.Venta.PRECIO_UNI.ToString(),
                vlr_tot = x.Venta.TOTAL.ToString(),
                tarifa_iva = "0",
                vlr_iva = x.Venta.IVA.ToString(),
                vlr_dscto = x.Venta.Descuento.ToString(),
            } };
        }

        private string getCodigoCombustible(string combustible)
        {
            switch (combustible)
            {
                case "MAX PRO DIESEL":
                    return "004";
                case "ACPM":
                    return "003";
                case "EXTRA G-PRIX":
                    return "002";
                default:
                    return "001";
            }
        }

        private string getForma(string forma)
        {
            switch (forma.ToLower())
            {
                case "credito":
                    return "006";
                default:
                    return "004";
            }
        }

        public string nro { get; set; }
        public string fecha { get; set; }
        public string id_cliente { get; set; }
        public string nombre_cliente { get; set; }
        public string vendedor { get; set; }
        public string forma_pago { get; set; }
        public string retencion { get; set; }
        public string rete_iva { get; set; }
        public string rete_ica { get; set; }
        public List<Item> items { get; set; }
    }

    public class Item
    {
        public string grupo { get; set; }
        public string codigo { get; set; }
        public string clase { get; set; }
        public string cantidad { get; set; }
        public string vlr_unit { get; set; }
        public string vlr_tot { get; set; }
        public string tarifa_iva { get; set; }
        public string vlr_iva { get; set; }
        public string vlr_dscto { get; set; }
    }

    

}
