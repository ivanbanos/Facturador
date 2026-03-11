using System;
using System.Collections.Generic;
using System.Text;

namespace FacturacionelectronicaCore.Negocio.Contabilidad.FacturacionElectronica
{
    public class Seller
    {
        public string id { get; set; }
        public string name { get; set; }
        public string identification { get; set; }
        public string observations { get; set; }
    }

    public class Term
    {
        public int id { get; set; }
        public string name { get; set; }
        public int days { get; set; }
    }

    public class AddressResponse
    {
        public string address { get; set; }
        public string department { get; set; }
        public string city { get; set; }
        public string country { get; set; }
        public string zipCode { get; set; }
    }

    public class InternalContact
    {
        public int id { get; set; }
        public string name { get; set; }
        public string lastName { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public string mobile { get; set; }
        public string sendNotifications { get; set; }
    }

    public class TerceroResponse
    {
        public int id { get; set; }
        public string name { get; set; }
        public string identification { get; set; }
        public string email { get; set; }
        public string phonePrimary { get; set; }
        public string phoneSecondary { get; set; }
        public string fax { get; set; }
        public string mobile { get; set; }
        public string observations { get; set; }
        public List<string> type { get; set; }
        public Seller seller { get; set; }
        public Term term { get; set; }
        public AddressResponse address { get; set; }
        public List<InternalContact> internalContacts { get; set; }
        public object priceList { get; set; }
    }

}
