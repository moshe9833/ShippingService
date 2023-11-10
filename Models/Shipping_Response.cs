namespace CourierRates.Models
{
    public class Shipping_Response
    {

 
        public class Alert
        {
            public string code { get; set; }
            public string message { get; set; }
            public string alertType { get; set; }
            public List<object> parameterList { get; set; }
        }

        public class Barcodes
        {
            public List<BinaryBarcode> binaryBarcodes { get; set; }
            public List<StringBarcode> stringBarcodes { get; set; }
        }

        public class BinaryBarcode
        {
            public string type { get; set; }
            public string value { get; set; }
        }

        public class CompletedPackageDetail
        {
            public int sequenceNumber { get; set; }
            public List<TrackingId> trackingIds { get; set; }
            public int groupNumber { get; set; }
            public PackageRating packageRating { get; set; }
            public string signatureOption { get; set; }
            public OperationalDetail operationalDetail { get; set; }
        }

        public class CompletedShipmentDetail
        {
            public bool usDomestic { get; set; }
            public string carrierCode { get; set; }
            public MasterTrackingId masterTrackingId { get; set; }
            public ServiceDescription serviceDescription { get; set; }
            public string packagingDescription { get; set; }
            public OperationalDetail operationalDetail { get; set; }
            public ShipmentRating shipmentRating { get; set; }
            public List<CompletedPackageDetail> completedPackageDetails { get; set; }
        }

        public class MasterTrackingId
        {
            public string trackingIdType { get; set; }
            public string formId { get; set; }
            public string trackingNumber { get; set; }
        }

        public class Name
        {
            public string type { get; set; }
            public string encoding { get; set; }
            public string value { get; set; }
        }

        public class OperationalDetail
        {
            public string ursaPrefixCode { get; set; }
            public string ursaSuffixCode { get; set; }
            public string originLocationId { get; set; }
            public int originLocationNumber { get; set; }
            public string originServiceArea { get; set; }
            public string destinationLocationId { get; set; }
            public int destinationLocationNumber { get; set; }
            public string destinationServiceArea { get; set; }
            public string destinationLocationStateOrProvinceCode { get; set; }
            public string deliveryDate { get; set; }
            public string deliveryDay { get; set; }
            public string commitDate { get; set; }
            public string commitDay { get; set; }
            public bool ineligibleForMoneyBackGuarantee { get; set; }
            public string astraPlannedServiceLevel { get; set; }
            public string astraDescription { get; set; }
            public string postalCode { get; set; }
            public string stateOrProvinceCode { get; set; }
            public string countryCode { get; set; }
            public string airportId { get; set; }
            public string serviceCode { get; set; }
            public string packagingCode { get; set; }
            public string publishedDeliveryTime { get; set; }
            public string scac { get; set; }
            public Barcodes barcodes { get; set; }
            public string astraHandlingText { get; set; }
            public List<OperationalInstruction> operationalInstructions { get; set; }
        }

        public class OperationalInstruction
        {
            public int number { get; set; }
            public string content { get; set; }
        }

        public class Output
        {
            public List<TransactionShipment> transactionShipments { get; set; }
        }

        public class PackageDocument
        {
            public string url { get; set; }
            public string contentType { get; set; }
            public int copiesToPrint { get; set; }
            public string docType { get; set; }
        }

        public class PackageRateDetail
        {
            public string rateType { get; set; }
            public string ratedWeightMethod { get; set; }
            public string minimumChargeType { get; set; }
            public double baseCharge { get; set; }
            public double totalFreightDiscounts { get; set; }
            public double netFreight { get; set; }
            public double totalSurcharges { get; set; }
            public double netFedExCharge { get; set; }
            public double totalTaxes { get; set; }
            public double netCharge { get; set; }
            public double totalRebates { get; set; }
            public List<object> surcharges { get; set; }
            public string currency { get; set; }
        }

        public class PackageRating
        {
            public string actualRateType { get; set; }
            public double effectiveNetDiscount { get; set; }
            public List<PackageRateDetail> packageRateDetails { get; set; }
        }

        public class PieceResponse
        {
            public string masterTrackingNumber { get; set; }
            public string deliveryDatestamp { get; set; }
            public string trackingNumber { get; set; }
            public double additionalChargesDiscount { get; set; }
            public double netRateAmount { get; set; }
            public double netChargeAmount { get; set; }
            public double netDiscountAmount { get; set; }
            public List<PackageDocument> packageDocuments { get; set; }
            public string currency { get; set; }
            public List<object> customerReferences { get; set; }
            public double codcollectionAmount { get; set; }
            public double baseRateAmount { get; set; }
        }

        public class Root_fedex
        {
            public string transactionId { get; set; }
            public string customerTransactionId { get; set; }
            public Output output { get; set; }
        }

        public class ServiceDescription
        {
            public string serviceId { get; set; }
            public string serviceType { get; set; }
            public string code { get; set; }
            public List<Name> names { get; set; }
            public List<string> operatingOrgCodes { get; set; }
            public string serviceCategory { get; set; }
            public string description { get; set; }
            public string astraDescription { get; set; }
        }

        public class ShipmentAdvisoryDetails
        {
        }

        public class ShipmentRateDetail
        {
            public string rateType { get; set; }
            public string rateScale { get; set; }
            public string rateZone { get; set; }
            public string pricingCode { get; set; }
            public string ratedWeightMethod { get; set; }
            public int dimDivisor { get; set; }
            public double fuelSurchargePercent { get; set; }
            public double totalBaseCharge { get; set; }
            public double totalFreightDiscounts { get; set; }
            public double totalNetFreight { get; set; }
            public double totalSurcharges { get; set; }
            public double totalNetFedExCharge { get; set; }
            public double totalTaxes { get; set; }
            public double totalNetCharge { get; set; }
            public double totalRebates { get; set; }
            public double totalDutiesAndTaxes { get; set; }
            public double totalAncillaryFeesAndTaxes { get; set; }
            public double totalDutiesTaxesAndFees { get; set; }
            public double totalNetChargeWithDutiesAndTaxes { get; set; }
            public List<object> surcharges { get; set; }
            public List<object> freightDiscounts { get; set; }
            public List<object> taxes { get; set; }
            public string currency { get; set; }
        }

        public class ShipmentRating
        {
            public string actualRateType { get; set; }
            public List<ShipmentRateDetail> shipmentRateDetails { get; set; }
        }

        public class StringBarcode
        {
            public string type { get; set; }
            public string value { get; set; }
        }

        public class TrackingId
        {
            public string trackingIdType { get; set; }
            public string formId { get; set; }
            public string trackingNumber { get; set; }
        }

        public class TransactionShipment
        {
            public List<Alert> alerts { get; set; }
            public string masterTrackingNumber { get; set; }
            public string serviceType { get; set; }
            public string shipDatestamp { get; set; }
            public string serviceName { get; set; }
            public List<PieceResponse> pieceResponses { get; set; }
            public ShipmentAdvisoryDetails shipmentAdvisoryDetails { get; set; }
            public CompletedShipmentDetail completedShipmentDetail { get; set; }
            public string serviceCategory { get; set; }
        }
        #region UPS RESPONSE
        public class UPS_Response
        {
            // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
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

            public class BillingWeight
            {
                public UnitOfMeasurement UnitOfMeasurement { get; set; }
                public string Weight { get; set; }
            }

            public class ImageFormat
            {
                public string Code { get; set; }
                public string Description { get; set; }
            }

            public class ItemizedCharge
            {
                public string Code { get; set; }
                public string CurrencyCode { get; set; }
                public string MonetaryValue { get; set; }
            }

            public class PackageResults
            {
                public string TrackingNumber { get; set; }
                public BaseServiceCharge BaseServiceCharge { get; set; }
                public ServiceOptionsCharges ServiceOptionsCharges { get; set; }
                public ShippingLabel ShippingLabel { get; set; }
                public List<ItemizedCharge> ItemizedCharges { get; set; }
            }

            public class Response
            {
                public ResponseStatus ResponseStatus { get; set; }
                public Alert Alert { get; set; }
                public TransactionReference TransactionReference { get; set; }
            }

            public class ResponseStatus
            {
                public string Code { get; set; }
                public string Description { get; set; }
            }

            public class Root_ups
            {
                public ShipmentResponse ShipmentResponse { get; set; }
            }

            public class ServiceOptionsCharges
            {
                public string CurrencyCode { get; set; }
                public string MonetaryValue { get; set; }
            }

            public class ShipmentCharges
            {
                public TransportationCharges TransportationCharges { get; set; }
                public ServiceOptionsCharges ServiceOptionsCharges { get; set; }
                public TotalCharges TotalCharges { get; set; }
            }

            public class ShipmentResponse
            {
                public Response Response { get; set; }
                public ShipmentResults ShipmentResults { get; set; }
            }

            public class ShipmentResults
            {
                public ShipmentCharges ShipmentCharges { get; set; }
                public BillingWeight BillingWeight { get; set; }
                public string ShipmentIdentificationNumber { get; set; }
                public PackageResults PackageResults { get; set; }
            }

            public class ShippingLabel
            {
                public ImageFormat ImageFormat { get; set; }
                public string GraphicImage { get; set; }
                public string HTMLImage { get; set; }
            }

            public class TotalCharges
            {
                public string CurrencyCode { get; set; }
                public string MonetaryValue { get; set; }
            }

            public class TransactionReference
            {
                public string CustomerContext { get; set; }
                public string TransactionIdentifier { get; set; }
            }

            public class TransportationCharges
            {
                public string CurrencyCode { get; set; }
                public string MonetaryValue { get; set; }
            }

            public class UnitOfMeasurement
            {
                public string Code { get; set; }
                public string Description { get; set; }
            }
            public class AccessTokenResponse
            {
                public string access_token { get; set; }
                public string token_type { get; set; }
                public int expires_in { get; set; }
                public string scope { get; set; }
            }

        }
        
        #endregion
    }

}
