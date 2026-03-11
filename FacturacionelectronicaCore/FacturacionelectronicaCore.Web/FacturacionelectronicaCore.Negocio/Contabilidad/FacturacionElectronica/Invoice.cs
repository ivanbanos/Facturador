using System;
using System.Collections.Generic;
using System.Text;

namespace FacturacionelectronicaCore.Negocio.Contabilidad.FacturacionElectronica
{
    public class Stamp
    {
        public bool generateStamp { get; set; }
    }

    public class Tax
    {
        public int id { get; set; }
    }

    public class ItemInvoice
    {
        public string id { get; set; }
        public int price { get; set; }
        public string quantity { get; set; }
        public string description { get; set; }
        public string discount { get; set; }
        public List<Tax> tax { get; set; }
    }

    public class Invoice
    {
        public string numberDeliveryOrder { get; set; }

        public string id {  get; set; }
        public string date { get; set; }
        public string dueDate { get; set; }
        public string client { get; set; }
        public Stamp stamp { get; set; }
        public string paymentForm { get; set; }
        public string paymentMethod { get; set; }
        public List<ItemInvoice> items { get; set; }
    }
}
