using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace FacturacionelectronicaCore.Negocio.Contabilidad.FacturacionElectronica
{

    public class Name
    {
        public string firstName { get; set; }
        public string secondName { get; set; }
        public string lastName { get; set; }
    }

    public class Identification
    {
        public string type { get; set; }
        public string number { get; set; }
        public int dv { get; internal set; }
    }

    public class Settings
    {
        public bool sendElectronicDocuments { get; set; }
    }

    public class Address
    {
        public string description { get; set; }
        public string department { get; set; }
        public string city { get; set; }
        public string country { get; set; }
        public string zipCode { get; set; }
    }

    public class Contacts
    {
        public bool ignoreRepeated { get; set; }

        public int id {  get; set; }
        public string name { get; set; }
        public Name nameObject { get; set; }
        public Identification identificationObject { get; set; }
        public string email { get; set; }
        public string phonePrimary { get; set; }
        public string kindOfPerson { get; set; }
        public string regime { get; set; }
        public string mobile { get; set; }
        public string seller { get; set; }
        public string comments { get; set; }
        public string phoneSecondary { get; set; }
        public Settings settings { get; set; }
        public string observations { get; set; }
        public List<string> type { get; set; }
        public Address address { get; set; }
    }


}
