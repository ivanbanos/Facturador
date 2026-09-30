using System;
using System.Collections.Generic;
using System.Text;

public class SiigoTokenRequest
{
    public string username { get; set; }
    public string access_key { get; set; }
}

public class SiigoTokenResponse
{
    public string access_token { get; set; }
    public int expires_in { get; set; }
    public string token_type { get; set; }
    public string scope { get; set; }
}


// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
public class AdditionalFieldsSiigoRequest
{
}

public class AddressSiigoRequest
{
    public string address { get; set; }
    public CitySiigoRequest city { get; set; }
    public string postal_code { get; set; }
}

public class CitySiigoRequest
{
    public string country_code { get; set; }
    public string country_name { get; set; }
    public string state_code { get; set; }
    public string state_name { get; set; }
    public string city_code { get; set; }
    public string city_name { get; set; }
}

public class ContactSiigoRequest
{
    public string first_name { get; set; }
    public string last_name { get; set; }
    public string email { get; set; }
    public PhoneSiigoRequest phone { get; set; }
}

public class CurrencySiigoRequest
{
    public string code { get; set; }
    public double exchange_rate { get; set; }
}

public class CustomerSiigoRequest
{
    public string person_type { get; set; }
    public string id_type { get; set; }
    public string identification { get; set; }
    public int branch_office { get; set; }
    public List<string> name { get; set; }
    public AddressSiigoRequest address { get; set; }
    public List<PhoneSiigoRequest> phones { get; set; }
    public List<ContactSiigoRequest> contacts { get; set; }
}

public class DocumentSiigoRequest
{
    public int id { get; set; }
}

public class GlobaldiscountSiigoRequest
{
    public int id { get; set; }
    public int percentage { get; set; }
    public int value { get; set; }
}

public class ItemSiigoRequest
{
    public string code { get; set; }
    public string description { get; set; }
    public double quantity { get; set; }
    public double price { get; set; }
    public int discount { get; set; }
    public List<TaxisSiigoRequest> taxes { get; set; }
    public TransportSiigoRequest transport { get; set; }
}

public class MailSiigoRequest
{
    public bool send { get; set; }
}

public class PaymentSiigoRequest
{
    public int id { get; set; }
    public double value { get; set; }
    public string due_date { get; set; }
}

public class PhoneSiigoRequest
{
    public string indicative { get; set; }
    public string number { get; set; }
    public string extension { get; set; }
}

public class Phone2SiigoRequest
{
    public string indicative { get; set; }
    public string number { get; set; }
    public string extension { get; set; }
}

public class FacturaSiigo
{
    public DocumentSiigoRequest document { get; set; }
    public string date { get; set; }
    public CustomerSiigoRequest customer { get; set; }
    public int? cost_center { get; set; }
    public CurrencySiigoRequest currency { get; set; }
    public int seller { get; set; }
    public StampSiigoRequest stamp { get; set; }
    public MailSiigoRequest mail { get; set; }
    public string observations { get; set; }
    public List<ItemSiigoRequest> items { get; set; }
    public List<PaymentSiigoRequest> payments { get; set; }
    public List<GlobaldiscountSiigoRequest> globaldiscounts { get; set; }
    public AdditionalFieldsSiigoRequest additional_fields { get; set; }
}

public class StampSiigoRequest
{
    public bool send { get; set; }
}

public class TaxisSiigoRequest
{
    public int id { get; set; }
}

public class TransportSiigoRequest
{
    public int file_number { get; set; }
    public string shipment_number { get; set; }
    public int transported_quantity { get; set; }
    public string measurement_unit { get; set; }
    public int freight_value { get; set; }
    public string purchase_order { get; set; }
    public string service_type { get; set; }
}


// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
public class AdditionalFieldsSiigoResponse
{
}

public class CurrencySiigoResponse
{
    public string code { get; set; }
    public double exchange_rate { get; set; }
}

public class CustomerSiigoResponse
{
    public string id { get; set; }
    public string identification { get; set; }
    public string branch_office { get; set; }
}

public class DiscountSiigoResponse
{
    public string percentage { get; set; }
    public string value { get; set; }
}

public class DocumentSiigoResponse
{
    public string id { get; set; }
}

public class GlobaldiscountSiigoResponse
{
    public string id { get; set; }
    public string name { get; set; }
    public string percentage { get; set; }
    public string value { get; set; }
}

public class ItemSiigoResponse
{
    public string id { get; set; }
    public string code { get; set; }
    public string description { get; set; }
    public string quantity { get; set; }
    public double price { get; set; }
    public DiscountSiigoResponse discount { get; set; }
    public List<TaxisSiigoResponse> taxes { get; set; }
    public double total { get; set; }
}

public class MailSiigoResponse
{
    public string status { get; set; }
    public string observations { get; set; }
}

public class MetadataSiigoResponse
{
    public DateTime created { get; set; }
    public string last_updated { get; set; }
}

public class PaymentSiigoResponse
{
    public int id { get; set; }
    public string name { get; set; }
    public double value { get; set; }
    public string due_date { get; set; }
}

public class FacturaSiigoResponse
{
    public string id { get; set; }
    public DocumentSiigoResponse document { get; set; }

    public string prefix { get; set; }
    public string number { get; set; }
    public string name { get; set; }
    public string date { get; set; }
    public CustomerSiigoResponse customer { get; set; }
    public string cost_center { get; set; }
    public CurrencySiigoResponse currency { get; set; }
    public double total { get; set; }
    public string balance { get; set; }
    public string seller { get; set; }
    public StampSiigoResponse stamp { get; set; }
    public MailSiigoResponse mail { get; set; }
    public string observations { get; set; }
    public List<ItemSiigoResponse> items { get; set; }
    public List<PaymentSiigoResponse> payments { get; set; }
    public List<GlobaldiscountSiigoResponse> globaldiscounts { get; set; }
    public AdditionalFieldsSiigoResponse additional_fields { get; set; }
    public MetadataSiigoResponse metadata { get; set; }
}

public class StampSiigoResponse
{
    public string status { get; set; }
    public string cufe { get; set; }
    public string observations { get; set; }
    public string errors { get; set; }
}

public class TaxisSiigoResponse { 
    public int id { get; set; }
    public string name { get; set; }
    public string type { get; set; }
    public int percentage { get; set; }
    public double value { get; set; }
}

