using System;
using System.Collections.Generic;
using System.Text;

namespace FacturacionelectronicaCore.Negocio.Contabilidad.FacturacionElectronica
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class AddressResposne
    {
        public string address { get; set; }
        public string department { get; set; }
        public string city { get; set; }
    }


    public class Client
    {
        public string id { get; set; }
        public string name { get; set; }
        public string identification { get; set; }
        public string phonePrimary { get; set; }
        public object phoneSecondary { get; set; }
        public object fax { get; set; }
        public string mobile { get; set; }
        public string email { get; set; }
        public AddressResposne address { get; set; }
        public string kindOfPerson { get; set; }
        public string regime { get; set; }
        public IdentificationObject identificationObject { get; set; }
    }

    public class NumberTemplate
    {
        public string id { get; set; }
        public string prefix { get; set; }
        public string number { get; set; }
        public string text { get; set; }
        public string fullNumber { get; set; }
        public string formattedNumber { get; set; }
    }

    public class Warehouse
    {
        public string id { get; set; }
        public string name { get; set; }
    }

    public class PriceList
    {
        public string id { get; set; }
        public string name { get; set; }
    }

    public class StampResponse
    {
        public string legalStatus { get; set; }
        public string cufe { get; set; }
        public string date { get; set; }
        public string barCodeContent { get; set; }
    }

    public class DiscountObject
    {
        public double discount { get; set; }
        public object nature { get; set; }
    }

    public class ItemResponse
    {
        public string name { get; set; }
        public string description { get; set; }
        public int price { get; set; }
        public double discount { get; set; }
        public object reference { get; set; }
        public double quantity { get; set; }
        public int id { get; set; }
        public DiscountObject discountObject { get; set; }
        public object productKey { get; set; }
        public string unit { get; set; }
        public List<object> tax { get; set; }
        public double total { get; set; }
    }

    public class ResponseInvoice
    {
        public int id { get; set; }
        public string date { get; set; }
        public string dueDate { get; set; }
        public string datetime { get; set; }
        public object observations { get; set; }
        public object anotation { get; set; }
        public string termsConditions { get; set; }
        public string status { get; set; }
        public Client client { get; set; }
        public NumberTemplate numberTemplate { get; set; }
        public double total { get; set; }
        public int totalPaid { get; set; }
        public double balance { get; set; }
        public int decimalPrecision { get; set; }
        public Warehouse warehouse { get; set; }
        public string type { get; set; }
        public string operationType { get; set; }
        public string paymentForm { get; set; }
        public string paymentMethod { get; set; }
        public object seller { get; set; }
        public PriceList priceList { get; set; }
        public StampResponse stamp { get; set; }
        public List<ItemResponse> items { get; set; }
        public object costCenter { get; set; }
    }


}
