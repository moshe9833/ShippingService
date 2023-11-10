namespace CourierRates.Models
{


    public class GetShip
    {




        public class SurCharges
        {
            public string type { get; set; }
            public string description { get; set; }
            public string amount { get; set; }

        }
        public class TotalBillingWeight
        {
            public string units { get; set; }
            public string value { get; set; }

        }
        public class ShipmentRateDetail
        {
            public string rateZone { get; set; }
            public string dimDivisor { get; set; }
            public string fuelSurchargePercent { get; set; }
            public string totalSurcharges { get; set; }
            public string totalFreightDiscount { get; set; }
            public IList<SurCharges> surCharges { get; set; }
            public string pricingCode { get; set; }
            public TotalBillingWeight totalBillingWeight { get; set; }
            public string currency { get; set; }
            public string rateScale { get; set; }

        }
        public class BillingWeight
        {
            public string units { get; set; }
            public string value { get; set; }
            // public UnitOfMeasurement UnitOfMeasurement { get; set; }
            public string Weight { get; set; }

        }
        public class Surcharges
        {
            public string type { get; set; }
            public string description { get; set; }
            public string amount { get; set; }

        }
        public class PackageRateDetail
        {
            public string rateType { get; set; }
            public string ratedWeightMethod { get; set; }
            public string baseCharge { get; set; }
            public string netFreight { get; set; }
            public string totalSurcharges { get; set; }
            public string netFedExCharge { get; set; }
            public string totalTaxes { get; set; }
            public string netCharge { get; set; }
            public string totalRebates { get; set; }
            public BillingWeight billingWeight { get; set; }
            public string totalFreightDiscounts { get; set; }
            public IList<Surcharges> surcharges { get; set; }
            public string currency { get; set; }

        }
        public class RatedPackages
        {
            public string groupNumber { get; set; }
            public string effectiveNetDiscount { get; set; }
            public PackageRateDetail packageRateDetail { get; set; }

        }
        public class RatedShipmentDetails
        {
            public string rateType { get; set; }
            public string ratedWeightMethod { get; set; }
            public string totalDiscounts { get; set; }
            public string totalBaseCharge { get; set; }
            public string totalNetCharge { get; set; }
            public string totalNetFedExCharge { get; set; }
            public ShipmentRateDetail shipmentRateDetail { get; set; }
            public IList<RatedPackages> ratedPackages { get; set; }
            public string currency { get; set; }

        }
        public class OperationalDetail
        {
            public string ineligibleForMoneyBackGuarantee { get; set; }
            public string astraDescription { get; set; }
            public string airportId { get; set; }
            public string serviceCode { get; set; }

        }
        public class Names
        {
            public string type { get; set; }
            public string encoding { get; set; }
            public string value { get; set; }

        }
        public class ServiceDescription
        {
            public string serviceId { get; set; }
            public string serviceType { get; set; }
            public string code { get; set; }
            public IList<Names> names { get; set; }
            public string serviceCategory { get; set; }
            public string description { get; set; }
            public string astraDescription { get; set; }

        }
        public class RateReplyDetails
        {
            public string serviceType { get; set; }
            public string serviceName { get; set; }
            public string packagingType { get; set; }
            public IList<RatedShipmentDetails> ratedShipmentDetails { get; set; }
            public OperationalDetail operationalDetail { get; set; }
            public string signatureOptionType { get; set; }
            public ServiceDescription serviceDescription { get; set; }

        }
        public class Output
        {
            public IList<RateReplyDetails> rateReplyDetails { get; set; }
            public string quoteDate { get; set; }
            public string encoded { get; set; }

        }
        public class Application
        {
            public string transactionId { get; set; }
            public Output output { get; set; }

        }
        public class ups_auth_response
        {
            public string result { get; set; }
            public string type { get; set; }
            public string LassoRedirectURL { get; set; }
        }
        public class Alert
        {
            public string Code { get; set; }
            public string Description { get; set; }
        }
        public class BaseServiceCharge
        {
            public string CurrencyCode { get; set; }
            public string MonetaryValue { get; set; }
        }
        public class ItemizedCharges
        {
            public string Code { get; set; }
            public string CurrencyCode { get; set; }
            public string MonetaryValue { get; set; }
        }
        public class RatedPackage
        {
            //public TransportationCharges TransportationCharges { get; set; }
            public BaseServiceCharge BaseServiceCharge { get; set; }
            //public ServiceOptionsCharges ServiceOptionsCharges { get; set; }
            public ItemizedCharges ItemizedCharges { get; set; }
            // public TotalCharges TotalCharges { get; set; }
            public string Weight { get; set; }
            public BillingWeight BillingWeight { get; set; }
            // public SimpleRate SimpleRate { get; set; }
        }
        public class RatedShipment
        {
            // public Service Service { get; set; }
            public RatedShipmentAlert RatedShipmentAlert { get; set; }
            public BillingWeight BillingWeight { get; set; }
            // public TransportationCharges TransportationCharges { get; set; }
            public BaseServiceCharge BaseServiceCharge { get; set; }
            // public ServiceOptionsCharges ServiceOptionsCharges { get; set; }
            // public TotalCharges TotalCharges { get; set; }
            public RatedPackage RatedPackage { get; set; }
        }
        public class RatedShipmentAlert
        {
            public string Code { get; set; }
            public string Description { get; set; }
        }

        //UPS RATE RESPOSE MODEL-START
        public class Rootobject
        {
            public Rateresponse RateResponse { get; set; }
        }
        public class Rateresponse
        {
            public Response Response { get; set; }
            public Ratedshipment RatedShipment { get; set; }
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
            public string CustomerContext { get; set; }
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

        public class CostDetail
        {
            public string fee_code { get; set; }
            public string fee_type { get; set; }
            public double amount { get; set; }
        }
        public class StampsRateResponse
        {
            public Root Root { get; set; }
        }
        public class Root
        {
            public string carrier { get; set; }
            public string service_type { get; set; }
            public string packaging_type { get; set; }
            public string estimated_delivery_days { get; set; }
            public object estimated_delivery_date { get; set; }
            public bool is_guaranteed_service { get; set; }
            public string trackable { get; set; }
            public bool is_return_label { get; set; }
            public bool is_customs_required { get; set; }
            public ShipmentCost shipment_cost { get; set; }
        }
        public class ShipmentCost
        {
            public double total_amount { get; set; }
            public string currency { get; set; }
            public List<CostDetail> cost_details { get; set; }
        }

    }
}
