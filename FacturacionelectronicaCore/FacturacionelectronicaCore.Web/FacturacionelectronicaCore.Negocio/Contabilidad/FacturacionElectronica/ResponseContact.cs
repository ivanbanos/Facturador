using System;
using System.Collections.Generic;
using System.Text;

namespace FacturacionelectronicaCore.Negocio.Contabilidad.FacturacionElectronica
{
    public class IdentificationObject
    {
        public string type { get; set; }
        public string number { get; set; }
        public int dv { get; set; }
    }


    public class CategoryRule
    {
        public string id { get; set; }
        public string name { get; set; }
        public string key { get; set; }
    }

    public class AccountReceivable
    {
        public string id { get; set; }
        public string idParent { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string type { get; set; }
        public string nature { get; set; }
        public object code { get; set; }
        public bool readOnly { get; set; }
        public CategoryRule categoryRule { get; set; }
        public string blocked { get; set; }
    }

    public class DebtToPay
    {
        public string id { get; set; }
        public string idParent { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string type { get; set; }
        public string nature { get; set; }
        public object code { get; set; }
        public bool readOnly { get; set; }
        public CategoryRule categoryRule { get; set; }
        public string blocked { get; set; }
    }

    public class Accounting
    {
        public AccountReceivable accountReceivable { get; set; }
        public DebtToPay debtToPay { get; set; }
    }

    public class ResponseContact
    {
        public int id { get; set; }
        public string name { get; set; }
        public string identification { get; set; }
        public string email { get; set; }
        public string phonePrimary { get; set; }
        public object phoneSecondary { get; set; }
        public object fax { get; set; }
        public string mobile { get; set; }
        public object observations { get; set; }
        public string status { get; set; }
        public string kindOfPerson { get; set; }
        public string regime { get; set; }
        public IdentificationObject identificationObject { get; set; }
        public Address address { get; set; }
        public List<string> type { get; set; }
        public object seller { get; set; }
        public object term { get; set; }
        public object priceList { get; set; }
        public List<object> internalContacts { get; set; }
        public Settings settings { get; set; }
        public bool statementAttached { get; set; }
        public Accounting accounting { get; set; }
    }
}
