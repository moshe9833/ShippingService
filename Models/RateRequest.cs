using Microsoft.AspNetCore.Mvc.Rendering;

namespace CourierRates.Models
{
    
    public class RateRequest
    {
        public enum Packaging_Type
        {
            Envelope=1,
            Package = 2,
            Own_Packaging =3
        }
        public RateRequest()
        {
            this.from_address = new FromAddress();
            this.to_address = new ToAddress();
            this.package = new Package();
            this.insurance = new Insurance();
            this.customs = new Customs();
            this.advanced_options = new AdvancedOptions();

            this.Country_List = new List<SelectListItem> {
        new SelectListItem{ Text = "---SELECT---", Value = "0"},
        new SelectListItem{ Text = "Canada", Value = "CA"},
        new SelectListItem{ Text = "India", Value = "IN"}
        };
            this.StateList = new List<SelectListItem> {
                new SelectListItem { Text = "---SELECT---", Value = "0" },
                new SelectListItem { Text = "Punjab", Value = "1" },
                new SelectListItem { Text = "State", Value = "2" }
            };
        }
        public FromAddress from_address { get; set; }
        public ToAddress to_address { get; set; }
        public string service_type { get; set; }
        public Package package { get; set; }
        public string delivery_confirmation_type { get; set; }
        public Insurance insurance { get; set; }
        public Customs customs { get; set; }
        public string ship_date { get; set; }
        public string is_return_label { get; set; }
        public AdvancedOptions advanced_options { get; set; }
        public List<SelectListItem> Country_List { get; set; }
        public List<SelectListItem> StateList { get; set; }
    }
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class AdvancedOptions
    {
        public string non_machinable { get; set; }
        public string saturday_delivery { get; set; }
        public string delivered_duty_paid { get; set; }
        public string hold_for_pickup { get; set; }
        public string certified_mail { get; set; }
        public string return_receipt { get; set; }
        public string return_receipt_electronic { get; set; }
        public CollectOnDelivery collect_on_delivery { get; set; }
        public RegisteredMail registered_mail { get; set; }
        public string sunday_delivery { get; set; }
        public string holiday_delivery { get; set; }
        public string restricted_delivery { get; set; }
        public string notice_of_non_delivery { get; set; }
        public SpecialHandling special_handling { get; set; }
        public NoLabel no_label { get; set; }
        public string is_pay_on_use { get; set; }
        public ReturnOptions return_options { get; set; }
    }
    
    public class CollectOnDelivery
    {
        public string amount { get; set; }
        public string currency { get; set; }
    }
    
    public class Customs
    {
        public string contents_type { get; set; }
        public string contents_description { get; set; }
        public string non_delivery_option { get; set; }
        public SenderInfo sender_info { get; set; }
        public RecipientInfo recipient_info { get; set; }
        public List<CustomsItem> customs_items { get; set; }
    }

    public class CustomsItem
    {
        public string item_description { get; set; }
        public string quantity { get; set; }
        public UnitValue unit_value { get; set; }
        public string item_weight { get; set; }
        public string weight_unit { get; set; }
        public string harmonized_tariff_code { get; set; }
        public string country_of_origin { get; set; }
        public string sku { get; set; }
    }

    public class FromAddress
    {
        public FromAddress()
        {

        }
        public string company_name { get; set; }
        public string name { get; set; }
        public string address_line1 { get; set; }
        public string address_line2 { get; set; }
        public string address_line3 { get; set; }
        public string city { get; set; }
        public string state_province { get; set; }
        public string postal_code { get; set; }
        public string country_code { get; set; }
        public string phone { get; set; }
        public string email { get; set; }
       
    }

    public class Insurance
    {
        public string insurance_provider { get; set; }
        public InsuredValue insured_value { get; set; }
    }

    public class InsuredValue
    {
        public string amount { get; set; }
        public string currency { get; set; }
    }

    public class NoLabel
    {
        public string is_drop_off { get; set; }
        public string is_prepackaged { get; set; }
    }

    public class Package
    {
        public string packaging_type { get; set; }
        public string weight { get; set; }
        public string weight_unit { get; set; }
        public string length { get; set; }
        public string width { get; set; }
        public string height { get; set; }
        public string dimension_unit { get; set; }
    }

    public class RecipientInfo
    {
        public string tax_id { get; set; }
    }

    public class RegisteredMail
    {
        public string amount { get; set; }
        public string currency { get; set; }
    }

    public class ReturnOptions
    {
        public string outbound_label_id { get; set; }
    }

    public class Root
    {
        
    }

    public class SenderInfo
    {
        public string license_number { get; set; }
        public string certificate_number { get; set; }
        public string invoice_number { get; set; }
        public string internal_transaction_number { get; set; }
        public string passport_number { get; set; }
        public string passport_issue_date { get; set; }
        public string passport_expiration_date { get; set; }
    }

    public class SpecialHandling
    {
        public string special_contents_type { get; set; }
        public string fragile { get; set; }
    }

    public class ToAddress
    {
        public string company_name { get; set; }
        public string name { get; set; }
        public string address_line1 { get; set; }
        public string address_line2 { get; set; }
        public string address_line3 { get; set; }
        public string city { get; set; }
        public string state_province { get; set; }
        public string postal_code { get; set; }
        public string country_code { get; set; }
        public string phone { get; set; }
        public string email { get; set; }
    }

    public class UnitValue
    {
        public string amount { get; set; }
        public string currency { get; set; }
    }


}
