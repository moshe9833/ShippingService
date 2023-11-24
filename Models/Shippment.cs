namespace CourierRates.Models
{
    public class Shippment
    {
        public class ShippingRequest
        {
            public Address FromAddress { get; set; }
            public Address ToAddress { get; set; }
            public Package[] Packages { get; set; }
        }

        public class Address
        {
            public string AddressLine1 { get; set; }
            public string AddressLine2 { get; set; }
            public string City { get; set; }
            public string State { get; set; }
            public string Zip { get; set; }
            public string Country { get; set; }
        }

        public class Package
        {
            public decimal? Length { get; set; }
            public decimal? Width { get; set; }
            public decimal? Height { get; set; }
            public decimal? Weight { get; set; }
            public string? Carrier { get; set; }
            public string? ServiceCode { get; set; }
            public string? PackageCode { get; set; }
            public string? Email { get; set; }
            public string? PhoneNumber { get; set; }
        }
    }
    public class TrackingRequest
    {
        public string TrackingNumber { get; set; }
        public string Carrier { get; set; }
    }
}
