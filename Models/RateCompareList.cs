namespace CourierRates.Models
{
    public class RateCompareList
    {
        public string Carrier { get; set; }
        public string Service { get; set; }
        public string Packaging_type { get; set; }
        public Decimal Rate { get; set; }
        public string pricingCode { get; set; }
        public int ID { get; set; }
    }


}
