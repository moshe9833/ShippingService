namespace CourierRates.Models
{
    public class GetTrack
    {
        public class DeliveryDate
        {
            public string Type { get; set; }
            public string Date { get; set; }
        }

        public class DeliveryTime
        {
            public string Type { get; set; }
            public string EndTime { get; set; }
        }

        public class Location
        {
            public Address Address { get; set; }
            public string Slic { get; set; }
        }

        public class Address
        {
            public string City { get; set; }
            public string StateProvince { get; set; }
            public string CountryCode { get; set; }
            public string Country { get; set; }
        }

        public class Status
        {
            public string Type { get; set; }
            public string Description { get; set; }
            public string Code { get; set; }
            public string StatusCode { get; set; }
        }

        public class Activity
        {
            public Location Location { get; set; }
            public Status Status { get; set; }
            public string Date { get; set; }
            public string Time { get; set; }
        }

        public class Package
        {
            public string TrackingNumber { get; set; }
            public List<DeliveryDate> DeliveryDate { get; set; }
            public DeliveryTime DeliveryTime { get; set; }
            public List<Activity> Activity { get; set; }
            public int PackageCount { get; set; }
        }

        public class Shipment
        {
            public string InquiryNumber { get; set; }
            public List<Package> Package { get; set; }
        }

        public class TrackResponse
        {
            public List<Shipment> Shipment { get; set; }
        }

        public class RootObject
        {
            public TrackResponse TrackResponse { get; set; }
        }
    }


}
