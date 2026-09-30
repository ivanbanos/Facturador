using System;
using System.Collections.Generic;
using System.Text;

namespace FacturacionelectronicaCore.Negocio.Contabilidad.FacturacionElectronica
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    

    public class RespuestaDataico
    {
        public string payment_means_type { get; set; }
        public string dian_status { get; set; }
        public string order_reference { get; set; }
        public string number { get; set; }
        public string issue_date { get; set; }
        public string xml_url { get; set; }
        public string payment_date { get; set; }
        public string customer_status { get; set; }
        public string pdf_url { get; set; }
        public string email_status { get; set; }
        public string cufe { get; set; }
        public string invoice_type_code { get; set; }
        public string payment_means { get; set; }
        public string uuid { get; set; }
        public List<ItemDataico> items { get; set; }
        public string qrcode { get; set; }
        public string xml { get; set; }
        public NumberingDataico numbering { get; set; }
        public List<object> retentions { get; set; }
    }



}
