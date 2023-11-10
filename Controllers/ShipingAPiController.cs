using CourierRates.Models;
using CourierRates.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Net;
using static CourierRates.Models.Shipping_Response.UPS_Response;
using System.Net.Http.Headers;
using RestSharp;
using System.Net.Sockets;
using static CourierRates.Models.Shipping_Response;
using NuGet.Common;
using Newtonsoft.Json.Linq;
using System.Diagnostics.Metrics;
using static CourierRates.Models.UPS_RateResponse;
using static CourierRates.Models.GetShip;
using static CourierRates.Models.GetStamp;

namespace CourierRates.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShipingAPiController : ControllerBase
    {
        private readonly ILogger<HomeController> _logger;

        private readonly ShippingService _shippingService;
        private readonly IHttpClientFactory _httpClientFactory;
      
      

        public ShipingAPiController(ILogger<HomeController> logger, IHttpClientFactory httpClientFactory, ShippingService shippingService)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _shippingService = shippingService;
          
        
        }

      
        [Route("GetUPSRate")]
        [HttpGet]
        public async Task<Rootobject> GetUPSRate()
        {
            var baseAddress = "https://wwwcie.ups.com";         // testing
            string client_id = "AhgxQ1FBEOCNloVM81cuxmoOfdtvYca8oQ03JC4ok743DIvd";
            string Secret_Key = "fwQPweeipmcZ3zGshWb4IXzFQBoVi7cTwmp4iO93JXsPJ4UOt4rATMGgaGhgmTO5";

            var accessID = $"{"" + client_id + ""}:{"" + Secret_Key + ""}";
            var base64AccessID = Convert.ToBase64String(Encoding.ASCII.GetBytes(accessID));

            using (var client = new HttpClient())
            {
                // Get Access Token
                var request = new HttpRequestMessage()
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri($"{baseAddress}/security/v1/oauth/token"),
                    Content = new FormUrlEncodedContent(new[]
                    {
            new KeyValuePair<string, string>("grant_type", "client_credentials")
        })
                };
                request.Headers.Add("Authorization", $"Basic {base64AccessID}");

                var response = await client.SendAsync(request);

                var jsonResult = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<dynamic>(jsonResult);

                var access_token = result?["access_token"]?.ToString();

                string Acc_number = "C5J577";

                var request1 = new HttpRequestMessage(HttpMethod.Post, "https://wwwcie.ups.com/api/rating/v1801/Rate");
                request1.Headers.Add("Authorization", "Bearer " + access_token);
                request1.Headers.Add("transId", "Tran123");
                request1.Headers.Add("transactionSrc", "testing");
                var content = new StringContent("{\r\n    \"RateRequest\": {\r\n      \"Request\": {\r\n        \"TransactionReference\": {\r\n          \"CustomerContext\": \"CustomerContext\",\r\n          \"TransactionIdentifier\": \"TransactionIdentifier\"\r\n        }\r\n      },\r\n      \"Shipment\": {\r\n        \"Shipper\": {\r\n          \"Name\": \"ShipperName\",\r\n          \"ShipperNumber\": \"C5J577\",\r\n          \"Address\": {\r\n            \"AddressLine\": [\r\n              \"ShipperAddressLine\",\r\n              \"ShipperAddressLine\",\r\n              \"ShipperAddressLine\"\r\n            ],\r\n            \"City\": \"TIMONIUM\",\r\n            \"StateProvinceCode\": \"MD\",\r\n            \"PostalCode\": \"21093\",\r\n            \"CountryCode\": \"US\"\r\n          }\r\n        },\r\n        \"ShipTo\": {\r\n          \"Name\": \"ShipToName\",\r\n          \"Address\": {\r\n            \"AddressLine\": [\r\n              \"ShipToAddressLine\",\r\n              \"ShipToAddressLine\",\r\n              \"ShipToAddressLine\"\r\n            ],\r\n            \"City\": \"Alpharetta\",\r\n            \"StateProvinceCode\": \"GA\",\r\n            \"PostalCode\": \"30005\",\r\n            \"CountryCode\": \"US\"\r\n          }\r\n        },\r\n        \"ShipFrom\": {\r\n          \"Name\": \"ShipFromName\",\r\n          \"Address\": {\r\n            \"AddressLine\": [\r\n              \"ShipFromAddressLine\",\r\n              \"ShipFromAddressLine\",\r\n              \"ShipFromAddressLine\"\r\n            ],\r\n            \"City\": \"TIMONIUM\",\r\n            \"StateProvinceCode\": \"MD\",\r\n            \"PostalCode\": \"21093\",\r\n            \"CountryCode\": \"US\"\r\n          }\r\n        },\r\n        \"PaymentDetails\": {\r\n          \"ShipmentCharge\": {\r\n            \"Type\": \"01\",\r\n            \"BillShipper\": {\r\n              \"AccountNumber\": \"C5J577\"\r\n            }\r\n          }\r\n        },\r\n        \"Service\": {\r\n          \"Code\": \"03\",\r\n          \"Description\": \"Ground\"\r\n        },\r\n        \"NumOfPieces\": \"1\",\r\n        \"Package\": {\r\n          \"SimpleRate\": {\r\n            \"Description\": \"SimpleRateDescription\",\r\n            \"Code\": \"XS\"\r\n          },\r\n          \"PackagingType\": {\r\n            \"Code\": \"02\",\r\n            \"Description\": \"Packaging\"\r\n          },\r\n          \"Dimensions\": {\r\n            \"UnitOfMeasurement\": {\r\n              \"Code\": \"IN\",\r\n              \"Description\": \"Inches\"\r\n            },\r\n            \"Length\": \"5\",\r\n            \"Width\": \"5\",\r\n            \"Height\": \"5\"\r\n          },\r\n          \"PackageWeight\": {\r\n            \"UnitOfMeasurement\": {\r\n              \"Code\": \"LBS\",\r\n              \"Description\": \"Pounds\"\r\n            },\r\n            \"Weight\": \"1\"\r\n          }\r\n        }\r\n      }\r\n    }\r\n  }", null, "application/json");
                request1.Content = content;
                var response1 = await client.SendAsync(request1);
                response.EnsureSuccessStatusCode();
                var Rate_response = response1.Content.ReadAsStringAsync().Result;
                var UPs_response = JsonConvert.DeserializeObject<Rootobject>(Rate_response);
                return UPs_response;
            }

        }

        [Route("GetFedExRate")]
        [HttpGet]
        public async Task<Application> GetFedExRate()
        {
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            var BaseUrl = "https://apis-sandbox.fedex.com/oauth/token";

            var responseData = string.Empty;
            HttpClient httpClient = new HttpClient(new HttpClientHandler { UseCookies = true });

            HttpContent content3 = new FormUrlEncodedContent(
            new List<KeyValuePair<string, string>> {
            new KeyValuePair<string, string>("grant_type","client_credentials"),
            new KeyValuePair<string, string>("client_id", "l7b18f08d7cad24317a0ecb044d0426b74"),
            new KeyValuePair<string, string>("client_secret", "28115ee5ea654f40a738d4989e4df940")});

            //httpClient.BaseAddress = new Uri(BaseUrl);
            httpClient.Timeout = TimeSpan.FromMinutes(30);
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/x-www-form-urlencoded");
            httpClient.DefaultRequestHeaders.ExpectContinue = false;

            HttpResponseMessage response = new HttpResponseMessage();
            System.Threading.Tasks.Task.Run(async () =>
            {
                response = await httpClient.PostAsync(BaseUrl, content3);
                responseData = response.Content.ReadAsStringAsync().Result;
                response.EnsureSuccessStatusCode();
            }).Wait();

            var authDetails = JsonConvert.DeserializeObject<dynamic>(responseData);
            var token = authDetails.access_token;





            var tokenBeareer = "Bearer " + token;
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://apis-sandbox.fedex.com/rate/v1/rates/quotes");
            request.Headers.Add("Authorization", tokenBeareer);

            var content = new StringContent("{\r\n  \"accountNumber\": {\r\n    \"value\": \"" + 740561073 + "\"\r\n  },\r\n  \"requestedShipment\": {\r\n    \"shipper\": {\r\n      \"address\": {\r\n        \"postalCode\": " + 65247 + ",\r\n        \"countryCode\": \"US\"\r\n      }\r\n    },\r\n    \"recipient\": {\r\n      \"address\": {\r\n        \"postalCode\": " + 75063 + ",\r\n        \"countryCode\": \"US\"\r\n      }\r\n    },\r\n    \"pickupType\": \"DROPOFF_AT_FEDEX_LOCATION\",\r\n    \"rateRequestType\": [\r\n      \"ACCOUNT\",\r\n      \"LIST\"\r\n    ],\r\n    \"requestedPackageLineItems\": [\r\n      {\r\n        \"weight\": {\r\n          \"units\": \"LB\",\r\n          \"value\": 10\r\n        }\r\n      }\r\n    ]\r\n  }\r\n}", null, "application/json");
            request.Content = content;
            var response1 = await client.SendAsync(request);
            try
            {
                response1.EnsureSuccessStatusCode();
            }
            catch (Exception e)
            {

                throw;
            }

            var resp = (await response1.Content.ReadAsStringAsync());//20230608 make it as model and take

            //start 20230608
            // Getting content of response
            var documentResponse = resp;//.Content.ReadAsStringAsync();

            // mapped the response with DTO
            var model = JsonConvert.DeserializeObject<Application>(documentResponse.ToString());
            return model;



        }

        [HttpGet("Get_All_RateList")]
        public async Task<IActionResult> Authorize(string? code, string shipperPostalCode, string recipientPostalCode, bool IsFormPost = true)
        {
            var result = await _shippingService.Get_All_RateList(code, shipperPostalCode, recipientPostalCode, IsFormPost);

            if (result != null)
            {
                return Ok(result); 
            }
            else
            {
                return BadRequest("Invalid input"); 
            }
        }

        [Route("GetShipping")]
        [HttpGet]
        public async Task<List<Root1>> GetShipping(string? code, string shipperPostalCode, string recipientPostalCode)
        {
            var result = await _shippingService.Get_STMPS_Label(code, shipperPostalCode , recipientPostalCode );


            return result;
            
        }
        //[Route("VoidShipping")]
        //[HttpGet]
        //public IActionResult VoidShipping()
        //{
        //    return Ok();
        //}
        [Route("TrackShipping")]
        [HttpGet]
        public async Task<IActionResult> TrackShipping()
        {
           var result = await _shippingService.GetUPSStatus();
            //var result = await _aPICallController.GetFEdEXStatus();

            return Ok(result);
        }
    }
}
