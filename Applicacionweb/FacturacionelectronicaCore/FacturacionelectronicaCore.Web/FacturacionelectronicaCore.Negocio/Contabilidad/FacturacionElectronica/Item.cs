using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace FacturacionelectronicaCore.Negocio.Contabilidad.FacturacionElectronica
{
    public class Category
    {
        public string id { get; set; }
        public string name { get; set; }
    }

    public class Price
    {
        [JsonConverter(typeof(SafeStringConverter))]
        public string idPriceList { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        [JsonConverter(typeof(SafeStringConverter))]
        public string price { get; set; }
    }

    // Custom JsonConverter to safely handle GUID/ID conversions
    public class SafeStringConverter : JsonConverter<string>
    {
        public override void WriteJson(JsonWriter writer, string value, JsonSerializer serializer)
        {
            writer.WriteValue(value);
        }

        public override string ReadJson(JsonReader reader, Type objectType, string existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.Value == null)
                return null;

            return reader.Value.ToString();
        }
    }

    public class Inventory
    {
        public string unit { get; set; }
    }

    public class Item
    {
        public string id { get; set; }
        public Category category { get; set; }
        public bool hasNoIvaDays { get; set; }
        public string name { get; set; }
        public object description { get; set; }
        public object reference { get; set; }
        public string status { get; set; }
        public List<Price> price { get; set; }
        public Inventory inventory { get; set; }
        public List<object> tax { get; set; }
        public List<object> customFields { get; set; }
        public object productKey { get; set; }
        public string type { get; set; }
        public string itemType { get; set; }
    }
}
