using System;
using System.Collections.Generic;
using System.Text;

namespace FacturacionelectronicaCore.Negocio.Contabilidad.FacturacionElectronica
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class FacturaDataico
    {
        public ActionsDataico actions { get; set; }
        public InvoiceDataico invoice { get; set; }
    }

    public class ActionsDataico
    {
        public bool send_dian { get; set; }
        public bool send_email { get; set; }
        public string email { get; set; }
        public string pdf { get; set; }
        public List<AttachmentDataico> attachments { get; set; }
    }

    public class AttachmentDataico
    {
        public string name { get; set; }
        public string data { get; set; }
    }

    public class InvoiceDataico
    {
        public string currency_exchange_rate_date { get; set; }
        public string issue_date { get; set; }
        public HealthDataico health { get; set; }
        public string currency { get; set; }
        public string invoice_type_code { get; set; }
        public List<ChargeDataico> charges { get; set; }
        public string order_reference { get; set; }
        public List<ItemDataico> items { get; set; }
        public string payment_means_type { get; set; }
        public List<RetentionDataico> retentions { get; set; }
        public List<PrepaymentDataico> prepayments { get; set; }
        public string operation { get; set; }
        public string number { get; set; }
        public NumberingDataico numbering { get; set; }
        public string dataico_account_id { get; set; }
        public string payment_date { get; set; }
        public string env { get; set; }
        public double currency_exchange_rate { get; set; }
        public List<string> notes { get; set; }
        public CustomerDataico customer { get; set; }
        public string payment_means { get; set; }

    }

    public class HealthDataico
    {
        public string coverage { get; set; }
        public string policy_number { get; set; }
        public string contract_number { get; set; }
        public string period_end_date { get; set; }
        public string provider_code { get; set; }
        public string period_start_date { get; set; }
        public List<AssociatedUserDataico> associated_users { get; set; }
        public string payment_modality { get; set; }
        public PersonDataico person { get; set; }
        public string version { get; set; }
        public List<RecaudoDataico> recaudos { get; set; }
    }

    public class AssociatedUserDataico
    {
        public string coverage { get; set; }
        public string policy_number { get; set; }
        public string contract_number { get; set; }
        public RecaudoDataico recaudo { get; set; }
        public string provider_code { get; set; }
        public string user_type { get; set; }
        public string payment_modality { get; set; }
        public string mipres_delivery_number { get; set; }
        public PersonDataico person { get; set; }
        public string mipres_number { get; set; }
        public string authorization_number { get; set; }
    }

    public class RecaudoDataico
    {
        public string amount { get; set; }
        public string issue_date { get; set; }
        public string medical_fee_code { get; set; }
    }

    public class PersonDataico
    {
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string identification { get; set; }
        public string identification_type { get; set; }
        public string dian_identification_type { get; set; }
        public string identification_origin_country { get; set; }
    }

    public class ChargeDataico
    {
        public double base_amount { get; set; }
        public string reason { get; set; }
        public bool discount { get; set; }
    }

    public class ItemDataico
    {
        public string sku { get; set; }
        public string mandante_identification { get; set; }
        public List<TaxDataico> taxes { get; set; }
        public string measuring_unit { get; set; }
        public double quantity { get; set; }
        public List<RetentionDataico> retentions { get; set; }
        public string mandante_identification_type { get; set; }
    public double? original_price { get; set; }
    public decimal? discount_rate { get; set; }
        public double price { get; set; }
        public string description { get; set; }
    }

    public class TaxDataico
    {
        public string tax_category { get; set; }
        public double tax_rate { get; set; }
        public double tax_amount { get; set; }
        public string tax_description { get; set; }
        public double tax_base { get; set; }
        public double base_amount { get; set; }
    }

    public class RetentionDetailDataico
    {
        public string tax_category { get; set; }
        public double tax_rate { get; set; }
        public double base_amount { get; set; }
        public double amount { get; set; }
    }

    public class RetentionDataico
    {
        public string tax_category { get; set; }
        public double tax_rate { get; set; }
    }

    public class PrepaymentDataico
    {
        public double amount { get; set; }
        public string description { get; set; }
        public string received_date { get; set; }
    }

    public class NumberingDataico
    {
        public string resolution_number { get; set; }
        public string prefix { get; set; }
        public bool flexible { get; set; }
    }

    public class CustomerDataico
    {
        public string department { get; set; }
        public string address_line { get; set; }
        public string party_type { get; set; }
        public string city { get; set; }
        public string tax_level_code { get; set; }
        public string id { get; set; }
        public string email { get; set; }
        public string country_code { get; set; }
        public string updated_at { get; set; }
        public string first_name { get; set; }
        public string phone { get; set; }
        public string party_identification_type { get; set; }
        public string company_name { get; set; }
        public string family_name { get; set; }
        public string regimen { get; set; }
        public string party_identification { get; set; }
    }


}
