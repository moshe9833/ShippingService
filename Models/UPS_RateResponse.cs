namespace CourierRates.Models
{
    public class UPS_RateResponse
    {
        public class Rateresponse
        {
            public Response Response { get; set; }
            public Ratedshipment RatedShipment { get; set; }
        }
        public class Alert
        {
            public string Code { get; set; }
            public string Description { get; set; }
        }
        public class Response
        {
            public Responsestatus ResponseStatus { get; set; }
            public Alert[] Alert { get; set; }
            public Transactionreference TransactionReference { get; set; }
        }

        public class Responsestatus
        {
            public string Code { get; set; }
            public string Description { get; set; }
        }
        
        public class Transactionreference
        {
            public string CustomerContext{ get; set; }
            public string TransactionIdentifier { get; set; }
        }
        public class Alert1
        {
            public string Code { get; set; }
            public string Description { get; set; }
        }

        public class Ratedshipment
        { 
            public Service Service { get; set; }
            public Ratedshipmentalert RatedShipmentAlert { get; set; }
            public Billingweight BillingWeight { get; set; }
            public Transportationcharges TransportationCharges { get; set; }
            public Baseservicecharge BaseServiceCharge { get; set; }
            public Serviceoptionscharges ServiceOptionsCharges { get; set; }
            public Totalcharges TotalCharges { get; set; }
            public Ratedpackage RatedPackage { get; set; }
        }
    
        public class Service
        {
            public string Code { get; set; }
            public string Description { get; set; }
        }
    
        public class Ratedshipmentalert
        {
            public string Code { get; set; }
            public string Description { get; set; }
        }

        public class Billingweight
        {
            public Unitofmeasurement UnitOfMeasurement { get; set; }
            public string Weight { get; set; }
        }

        public class Unitofmeasurement
        {
            public string Code { get; set; }
            public string Description { get; set; }
        }

        public class Transportationcharges
        {
            public string CurrencyCode { get; set; }
            public string MonetaryValue { get; set; }
        }

        public class Baseservicecharge
        {
            public string CurrencyCode { get; set; }
            public string MonetaryValue { get; set; }
        }

        public class Serviceoptionscharges
        {
            public string CurrencyCode { get; set; }
            public string MonetaryValue { get; set; }
        }

        public class Totalcharges
        {
            public string CurrencyCode { get; set; }
            public string MonetaryValue { get; set; }
        }

        public class Ratedpackage
        {
            public Transportationcharges1 TransportationCharges { get; set; }
            public Baseservicecharge1 BaseServiceCharge { get; set; }
            public Serviceoptionscharges1 ServiceOptionsCharges { get; set; }
            public Itemizedcharges ItemizedCharges { get; set; }
            public Totalcharges1 TotalCharges { get; set; }
            public string Weight { get; set; }
            public Billingweight1 BillingWeight { get; set; }
            public Simplerate SimpleRate { get; set; }
        }

        public class Transportationcharges1
        {
            public string CurrencyCode { get; set; }
            public string MonetaryValue { get; set; }
        }

        public class Baseservicecharge1
        {
            public string CurrencyCode { get; set; }
            public string MonetaryValue { get; set; }
        }

        public class Serviceoptionscharges1
        {
            public string CurrencyCode { get; set; }
            public string MonetaryValue { get; set; }
        }

        public class Itemizedcharges
        {
            public string Code { get; set; }
            public string CurrencyCode { get; set; }
            public string MonetaryValue { get; set; }
        }

        public class Totalcharges1
        {
            public string CurrencyCode { get; set; }
            public string MonetaryValue { get; set; }
        }

        public class Billingweight1
        {
            public Unitofmeasurement1 UnitOfMeasurement { get; set; }
            public string Weight { get; set; }
        }

        public class Unitofmeasurement1
        {
            public string Code { get; set; }
            public string Description { get; set; }
        }

        public class Simplerate
        {
            public string Code { get; set; }
        }
        //UPS RATE RESPOSE MODEL-END
    }
}
