namespace CourierRates.Models
{
    public class GetStamp
    {
        public class AccountBalance
        {
            public double amount_available { get; set; }
            public double max_balance_amount_allowed { get; set; }
            public string currency { get; set; }
        }

        public class CostDetail1
        {
            public string fee_code { get; set; }
            public string fee_type { get; set; }
            public double amount { get; set; }
        }

        public class Label
        {
            public string href { get; set; }
        }

        public class Root1
        {
            public string label_id { get; set; }
            public string tracking_number { get; set; }
            public string carrier { get; set; }
            public string service_type { get; set; }
            public string packaging_type { get; set; }
            public string estimated_delivery_days { get; set; }
            public DateTime estimated_delivery_date { get; set; }
            public bool is_guaranteed_service { get; set; }
            public string trackable { get; set; }
            public bool is_return_label { get; set; }
            public bool is_gap { get; set; }
            public bool is_smartsaver { get; set; }
            public bool is_etoe { get; set; }
            public ShipmentCost1 shipment_cost { get; set; }
            public AccountBalance account_balance { get; set; }
            public List<Label> labels { get; set; }
            public object forms { get; set; }
            public object branding_id { get; set; }
            public object notification_setting_id { get; set; }
        }

        public class ShipmentCost1
        {
            public double total_amount { get; set; }
            public string currency { get; set; }
            public List<CostDetail1> cost_details { get; set; }
        }
    }
}
