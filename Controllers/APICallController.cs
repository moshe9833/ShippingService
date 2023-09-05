using CourierRates.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.DotNet.MSIdentity.Shared;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Policy;
using System.Text;
using System.Text.Json.Nodes;
using static System.Net.Mime.MediaTypeNames;

namespace CourierRates.Controllers
{
    public class APICallController : Controller
    {
        public async Task<IActionResult> Index()
        {
            string _shipperPostalCode = HttpContext.Request.Form["shipperPostalCode"];
            string _recipientPostalCode = HttpContext.Request.Form["recipientPostalCode"];
            // TEST URL: https://localhost:7210/Apicall/index

            var data = await testFinal("740561073", _shipperPostalCode, _recipientPostalCode);
            var rateReply = data.output.rateReplyDetails;
            var gridResult = new List<RateCompareList>();
            int index = 0;
            foreach (var item in rateReply)
            {
                index = index + 1;

                gridResult.Add(
           new RateCompareList { ID = index, Carrier = "FedEx", pricingCode = item.serviceType, Rate = Convert.ToDecimal(item.ratedShipmentDetails[0].totalNetFedExCharge), Service = item.serviceName }
           );
                //ADD THE RATE LIST OF UPS


            }



            return View(gridResult);
        }

        private async Task<Application> testFinal(string accountNumber, string _shipperPostalCode, string _recipientPostalCode)
        {
            var SanboxLink = "https://apis-sandbox.fedex.com/rate/v1/rates/quotes";
            var ProLink = "https://apis.fedex.com/rate/v1/rates/quotes";
            var client = new RestClient(SanboxLink);
            var request = new RestRequest(Method.Post.ToString());
            var token = GetToken();


            var data = await GetrateResponseAsync(token, accountNumber, _shipperPostalCode, _recipientPostalCode);
            return data;


        }
        public async Task<ActionResult> GetAPIData()
        {


            var client = new HttpClient();

            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            var BaseUrl = "https://apis-sandbox.fedex.com/oauth/token";

            var responseData = string.Empty;
            HttpClient httpClient = new HttpClient(new HttpClientHandler { UseCookies = true });

            HttpContent content = new FormUrlEncodedContent(
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
                response = await httpClient.PostAsync(BaseUrl, content);
                responseData = response.Content.ReadAsStringAsync().Result;
                response.EnsureSuccessStatusCode();
            }).Wait();
            var BaseUrl1 = "https://apis-sandbox.fedex.com/rate/v1/rates/quotes";
            // Define the JSON data
            var data = new Dictionary<string, string>()
            {
                { "accountNumber[value]", "740561073" },
                { "requestedShipment[shipper][address][postalCode]", "65247" },
                { "requestedShipment[shipper][address][countryCode]", "US" },
                { "requestedShipment[recipient][address][postalCode]", "75063" },
                { "requestedShipment[recipient][address][countryCode]", "US" },
                { "requestedShipment[pickupType]", "DROPOFF_AT_FEDEX_LOCATION" },
                { "requestedShipment[rateRequestType][0]", "ACCOUNT" },
                { "requestedShipment[rateRequestType][1]", "LIST" },
                { "requestedShipment[requestedPackageLineItems][0][weight][units]", "LB" },
                { "requestedShipment[requestedPackageLineItems][0][weight][value]", "10" }
            };

            var content4 = new FormUrlEncodedContent(data);

            // Convert JSON data to key-value pairs


            System.Threading.Tasks.Task.Run(async () =>
            {
                response = await httpClient.PostAsync(BaseUrl1, content4);
                responseData = response.Content.ReadAsStringAsync().Result;
                response.EnsureSuccessStatusCode();
            }).Wait();

            return View();
        }
        //this is New function used for Get Combined all Rate List into 1
        public async Task<ActionResult> Get_All_RateList(string code, string shipperPostalCode, string recipientPostalCode, bool IsFormPost= true )
        {
            if (IsFormPost)
            {
                //STORE POSTAL CODE INTO SESSION
                HttpContext.Session.SetString("Shipper_Postal_Code", shipperPostalCode);
                HttpContext.Session.SetString("Recipient_Postal_Code", recipientPostalCode);
            }

            if (code == null)
            {
                return this.Redirect(Request.Scheme + "://" + Request.Host.Value + "/APICall/Stamps_Login_Redirect?code=" + code);

            }

            //FED EX RATE- START
            string fedEx_account_number = "740561073";
            var data = await testFinal(fedEx_account_number, shipperPostalCode, recipientPostalCode);
            var rateReply = data.output.rateReplyDetails;
            var gridResult = new List<RateCompareList>();
            int index = 0;
            foreach (var item in rateReply)
            {
                index = index + 1;
                gridResult.Add(
           new RateCompareList { ID = index, Carrier = "FedEx",
                                pricingCode = item.serviceType, Rate = Convert.ToDecimal(item.ratedShipmentDetails[0].totalNetFedExCharge), 
                                Service = item.serviceName,
                                Packaging_type = item.packagingType
           }
           );
                //ADD THE RATE LIST OF UPS
            }
            #region STAMPS
            string acess_token = await Get_Access_Token_For_Stamsp(code);
            var stamps_rate = await Get_Stamps_RateList_v2(acess_token, shipperPostalCode, recipientPostalCode);
            #endregion
            foreach (var item in stamps_rate)
            {
                index = index + 1;
                gridResult.Add(
           new RateCompareList { ID = index, Carrier = "Stamps", 
                                      pricingCode = item.service_type, 
                                      Rate = Convert.ToDecimal(item.shipment_cost.total_amount), 
                                      Service = item.service_type,
                                      Packaging_type = item.packaging_type
           }
           );
            }
            #region UPS
            var UPS_Rate_REsponse = await UPS_GenerateToken();
            #endregion
           index = index + 1;
                gridResult.Add(
                new RateCompareList
               {
               ID = index,
               Carrier = "UPS",
               pricingCode = UPS_Rate_REsponse.RateResponse.RatedShipment.Service.Code,
               Rate = Convert.ToDecimal(UPS_Rate_REsponse.RateResponse.RatedShipment.TotalCharges.MonetaryValue),
               Service = UPS_Rate_REsponse.RateResponse.RatedShipment.Service.Description,
               Packaging_type = "N/A"
            }
            );

            gridResult = gridResult.OrderBy(x => x.Rate).ToList();
            return View("index",gridResult);
        }
        public void mytest()
        {
            //System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
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
            var tokent = "";
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                //tokent = JsonConvert.DeserializeObject<dynamic>(responseData);
            }

            var responseData1 = string.Empty;
            HttpClient httpClient1 = new HttpClient(new HttpClientHandler { UseCookies = true });

            httpClient1.Timeout = TimeSpan.FromMinutes(30);
            httpClient1.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/x-www-form-urlencoded");
            httpClient1.DefaultRequestHeaders.ExpectContinue = false;

            HttpResponseMessage response1 = new HttpResponseMessage();

            var BaseUrl1 = "https://apis-sandbox.fedex.com/rate/v1/rates/quotes";
            // Define the JSON data
            var data = new Dictionary<string, string>()
            {
                { "accountNumber[value]", "740561073" },
                { "requestedShipment[shipper][address][postalCode]", "65247" },
                { "requestedShipment[shipper][address][countryCode]", "US" },
                { "requestedShipment[recipient][address][postalCode]", "75063" },
                { "requestedShipment[recipient][address][countryCode]", "US" },
                { "requestedShipment[pickupType]", "DROPOFF_AT_FEDEX_LOCATION" },
                { "requestedShipment[rateRequestType][0]", "ACCOUNT" },
                { "requestedShipment[rateRequestType][1]", "LIST" },
                { "requestedShipment[requestedPackageLineItems][0][weight][units]", "LB" },
                { "requestedShipment[requestedPackageLineItems][0][weight][value]", "10" }
            };
            var content4 = new FormUrlEncodedContent(data);


            httpClient1.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokent);

            // Make the request to the rate API

            System.Threading.Tasks.Task.Run(async () =>
            {
                response1 = await httpClient1.PostAsync(BaseUrl1, content4);
                responseData1 = response1.Content.ReadAsStringAsync().Result;
                response1.EnsureSuccessStatusCode();
            }).Wait();

        }
        public string GetJson()
        {
            string jsonData = @"
           {
            ""accountNumber"": {
                ""value"": ""740561073""
            },
            ""requestedShipment"": {
                ""shipper"": {
                    ""address"": {
                        ""postalCode"": 65247,
                        ""countryCode"": ""US""
                    }
                },
                ""recipient"": {
                    ""address"": {
                        ""postalCode"": 75063,
                        ""countryCode"": ""US""
                    }
                },
                ""pickupType"": ""DROPOFF_AT_FEDEX_LOCATION"",
                ""rateRequestType"": [
                    ""ACCOUNT"",
                    ""LIST""
                ],
                ""requestedPackageLineItems"": [
                    {
                        ""weight"": {
                            ""units"": ""LB"",
                            ""value"": 10
                        }
                    }
                ]
            }
        }";
            return jsonData;
        }

        // Api to generate token to access other apis
        public string GetToken()
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


            return token;
        }
        /// <summary>
        /// THIS METHOD GENERATE AUTH TOKEN OB BEHALF OF AUTH-CODE [GENERATED BY UPS LOGIN SCREEN AND GETTING IN UPS REDIRECT URL]
        /// </summary>
        /// <param name="Auth_code"></param>
        /// Auth_code- GENERATED BY UPS LOGIN SCREEN AND GETTING IN UPS REDIRECT URL
        public async void Get_Auth_Token_by_Auth_Code(string Auth_code)
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://wwwcie.ups.com/security/v1/oauth/token");
            request.Headers.Add("Authorization", "Basic R0tWbldUOTFqaUpWc1ZCQWl6R0owcWlzNnpKcVhvRVk3NFJG=");

            var collection = new List<KeyValuePair<string, string>>();
            collection.Add(new("grant_type", "authorization_code"));
            collection.Add(new("code", Auth_code));
            var content = new FormUrlEncodedContent(collection);
            request.Content = content;
            var response = await client.SendAsync(request);
            //response.EnsureSuccessStatusCode();
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine(await response.Content.ReadAsStringAsync());
            }
            Console.WriteLine(await response.Content.ReadAsStringAsync());

        }

        // Api method to get rate quotes on the basis of recipient postal code
        public async Task<Application> GetrateResponseAsync(string Token, String accountnumber, string _shipperPostalCode, string _recipientPostalCode)
        {

            var tokenBeareer = "Bearer " + Token;
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://apis-sandbox.fedex.com/rate/v1/rates/quotes");
            request.Headers.Add("Authorization", tokenBeareer);

            var content = new StringContent("{\r\n  \"accountNumber\": {\r\n    \"value\": \"" + accountnumber + "\"\r\n  },\r\n  \"requestedShipment\": {\r\n    \"shipper\": {\r\n      \"address\": {\r\n        \"postalCode\": " + _shipperPostalCode + ",\r\n        \"countryCode\": \"US\"\r\n      }\r\n    },\r\n    \"recipient\": {\r\n      \"address\": {\r\n        \"postalCode\": " + _recipientPostalCode + ",\r\n        \"countryCode\": \"US\"\r\n      }\r\n    },\r\n    \"pickupType\": \"DROPOFF_AT_FEDEX_LOCATION\",\r\n    \"rateRequestType\": [\r\n      \"ACCOUNT\",\r\n      \"LIST\"\r\n    ],\r\n    \"requestedPackageLineItems\": [\r\n      {\r\n        \"weight\": {\r\n          \"units\": \"LB\",\r\n          \"value\": 10\r\n        }\r\n      }\r\n    ]\r\n  }\r\n}", null, "application/json");
            request.Content = content;
            var response = await client.SendAsync(request);
            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch (Exception e)
            {

                throw;
            }

            var resp = (await response.Content.ReadAsStringAsync());//20230608 make it as model and take

            //start 20230608
            // Getting content of response
            var documentResponse = resp;//.Content.ReadAsStringAsync();

            // mapped the response with DTO
            var model = JsonConvert.DeserializeObject<Application>(documentResponse.ToString());
            return model;
            // end 20230608
            // Console.WriteLine(await response.Content.ReadAsStringAsync());
            // return response;

        }

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
        //final 20230608

        // This api is used to get rate quotes for the couriers
        private async void test3()
        {
            var client = new HttpClient();

            string clientid = "l7b18f08d7cad24317a0ecb044d0426b74";
            string clientsecret = "28115ee5ea654f40a738d4989e4df940";

            var url = "https://apis-sandbox.fedex.com/oauth/token";

            var parametersAuth = new Dictionary<string, string>();
            parametersAuth.Add("grant_type", "client_credentials");
            parametersAuth.Add("client_id", clientid);
            parametersAuth.Add("client_secret", clientsecret);

            var reqAuth = new HttpRequestMessage(HttpMethod.Post, url) { Content = new FormUrlEncodedContent(parametersAuth) };
            var resAuth = await client.SendAsync(reqAuth);
            var authDetailsJson = await resAuth.Content.ReadAsStringAsync(); // here I am getting token successfully.

            if (resAuth.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var authDetails = JsonConvert.DeserializeObject<dynamic>(authDetailsJson);

                JObject objRateContent = JObject.FromObject(new
                {
                    AccountNumber = JObject.FromObject(new
                    {
                        Value = "740561073"
                    }),
                    RequestedShipment = JObject.FromObject(new
                    {
                        Shipper = JObject.FromObject(new
                        {
                            Address = JObject.FromObject(new
                            {
                                PostalCode = "65247",
                                CountryCode = "US"
                            })
                        }),
                        Recipient = JObject.FromObject(new
                        {
                            Address = JObject.FromObject(new
                            {
                                postalCode = 75063,
                                countryCode = "US"
                            })
                        }),
                        PickupType = "DROPOFF_AT_FEDEX_LOCATION",
                        RequestedPackageLineItem = JObject.FromObject(new
                        {
                            Weight = JObject.FromObject(new
                            {
                                units = "LB",
                                value = 10
                            })
                        })
                    })
                });

                var input = JsonConvert.SerializeObject(objRateContent);

                var urlquote = "https://apis-sandbox.fedex.com/rate/v1/rates/quotes";

                var request = new HttpRequestMessage(HttpMethod.Post, urlquote);

                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                request.Headers.Add("X-locale", "en_US");
                request.Headers.Add("Authorization", "Bearer " + authDetails.access_token);

                request.Content = new StringContent(input, Encoding.UTF8, "application/json");

                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                var response = await client.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();
            }
        }
        /// <summary>
        /// THIS FUNCTION USED FOR REDIRECTION TO UPS LOGIN PAGE.
        /// REQUIRED PARAM: CLIENT ID 
        /// </summary>
        /// <returns>URL FOR REDIRECT TO THE UPS LOGIN PAGE</returns>
        public async Task<ActionResult> Authenticate_UPS()
        {
            string client_id = "AhgxQ1FBEOCNloVM81cuxmoOfdtvYca8oQ03JC4ok743DIvd";
            string redirect_url = "https://f25c-182-73-149-18.ngrok-free.app/Apicall/UPSIndex";
            using (var httpClient = new HttpClient())
            {
                string urr = "https://www.ups.com/lasso/signin?client_id=" + client_id + "&redirect_uri=" + redirect_url + "&response_type=code&scope=read&type=ups_com_api";
                //THE BELOW CODE WILL VALIDATE THE CLIENT FIRST AND THEN CALL THE API FOR GET THE RETURN URL [UPS SIGN IN]
                using (var request = new HttpRequestMessage(new HttpMethod("GET"), "https://wwwcie.ups.com/security/v1/oauth/validate-client?client_id=" + client_id + "&redirect_uri=" + redirect_url + ""))
                {
                    httpClient.Timeout = TimeSpan.FromMinutes(30);
                    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/x-www-form-urlencoded");
                    httpClient.DefaultRequestHeaders.ExpectContinue = false;
                    var response = await httpClient.SendAsync(request);
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        // SUCCESS
                        var Auth_response = response.Content.ReadAsStringAsync().Result;
                        var UPs_response = JsonConvert.DeserializeObject<ups_auth_response>(Auth_response.ToString());

                        string url = UPs_response.LassoRedirectURL;
                        var param = new Dictionary<string, string>() { { "client_id", client_id } };
                        param.Add("redirect_uri", redirect_url);
                        param.Add("response_type", "code");
                        param.Add("scope", "read");
                        param.Add("type", UPs_response.type);
                        var newUrl = new Uri(QueryHelpers.AddQueryString(url, param));
                        return Redirect(newUrl.ToString());
                    }
                }
                return Redirect("redirect_url");//GIVE ERROR PAGE URL HERE- IN-CASE NO LOGIN URL IS RETURN BY API
            }
        }
        /// <summary>
        /// THIS METHOD CALLED FOR GENERATE AUTH TOKEN ON BEHALF OF CLIENT ID AND SECRET KEY
        /// REQUIRED PARAMS: CLIENT ID AND SECRET KEY
        /// </summary>
        /// <returns>AUTH TOKEN, REFRES TOKEN AND ETC</returns>
        public async Task<Rootobject> UPS_GenerateToken()
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
                return await UPS_Rating_v2(access_token);
            }
        }
        public async Task<ActionResult> UPSIndex(string code)
        {
            var UPS_RateResponse = await UPS_GenerateToken();
            //Get_Auth_Token_by_Auth_Code(code);/ TODO
            ViewBag.jsonResponse = UPS_RateResponse;
            return View("UPS_Rating", UPS_RateResponse);

        }

        public async Task<ActionResult> GeTAllRateList(string code, string _shipperPostalCode, string _recipientPostalCode)
        {
            var fed_ex_acc_No= "740561073";
            var SanboxLink = "https://apis-sandbox.fedex.com/rate/v1/rates/quotes";
            var ProLink = "https://apis.fedex.com/rate/v1/rates/quotes";
            var client = new RestClient(SanboxLink);
            var request = new RestRequest(Method.Post.ToString());
            var token = GetToken();


            var data = await GetrateResponseAsync(token, fed_ex_acc_No, _shipperPostalCode, _recipientPostalCode);
            // code used to get UPS rates start here     //
            var UPS_RateResponse = await UPS_GenerateToken();

            ViewBag.jsonResponse = UPS_RateResponse;
            // code used to get UPS rates end here     //

            // code used to get stamps rates start here     //
            string stamps_login_URL = String.Empty;
            if (code == null || code == "undefined")
            {
                stamps_login_URL = "https://signin.testing.stampsendicia.com/authorize?client_id=5rcNPLgvzkqSYFJYeSTMr029O6fPfZFe&response_type=code&redirect_uri=https://localhost:7210/";
            }
            else
            {
                string Access_Token = await Get_Access_Token_For_Stamsp(code);
                var RateList = await Get_Stamps_RateList(Access_Token);
                return View(RateList);
            }
            // return Redirect(stamps_login_URL);
            // code used to get stamps rates end here     //

            return View("UPS_Rating", UPS_RateResponse);
        }
        /// <summary>
        /// THIS FUNCTION GETS THE UPS RATING RESPONSE
        /// </summary>
        /// <param name="access_token"></param>
        /// ACCESS TOKEN IS GENERATED BY UPS_GenerateToken METHOD
        /// <returns>RETURN RATING RESPONSE OBJECT</returns>
        public async Task<Rootobject> UPS_Rating_v2(string access_token)
        {
            string Acc_number = "C5J577";
            var UPs_response = new Rootobject();
            string? Rate_response;
            //TODO- USE THE BELOW CODES
            var _shipperPostalCode = HttpContext.Session.GetString("Shipper_Postal_Code");
            var Receipient_postal_code = HttpContext.Session.GetString("Recipient_Postal_Code");
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://wwwcie.ups.com/api/rating/v1801/Rate");
            request.Headers.Add("Authorization", "Bearer " + access_token);
            request.Headers.Add("transId", "Tran123");
            request.Headers.Add("transactionSrc", "testing");
            var content = new StringContent("{\r\n    \"RateRequest\": {\r\n      \"Request\": {\r\n        \"RequestOption\":\"Ship\"\r\n    },  \"Shipment\": {\r\n        \"Shipper\": {\r\n          \"Name\": \"ShipperName\",\r\n          \"ShipperNumber\": \"C5J577\",\r\n          \"Address\": {\r\n            \"AddressLine\": [\r\n              \"ShipperAddressLine\",\r\n              \"ShipperAddressLine\",\r\n              \"ShipperAddressLine\"\r\n            ],\r\n            \"City\": \"TIMONIUM\",\r\n            \"StateProvinceCode\": \"MD\",\r\n            \"PostalCode\": " + _shipperPostalCode + ",\r\n            \"CountryCode\": \"US\"\r\n          }\r\n        },\r\n        \"ShipTo\": {\r\n          \"Name\": \"ShipToName\",\r\n          \"Address\": {\r\n            \"AddressLine\": [\r\n              \"ShipToAddressLine\",\r\n              \"ShipToAddressLine\",\r\n              \"ShipToAddressLine\"\r\n            ],\r\n            \"City\": \"Alpharetta\",\r\n            \"StateProvinceCode\": \"\",\r\n            \"PostalCode\": \"30005\",\r\n            \"CountryCode\": \"US\"\r\n          }\r\n        },\r\n        \"ShipFrom\": {\r\n          \"Name\": \"ShipFromName\",\r\n          \"Address\": {\r\n            \"AddressLine\": [\r\n              \"ShipFromAddressLine\",\r\n              \"ShipFromAddressLine\",\r\n              \"ShipFromAddressLine\"\r\n            ],\r\n            \"City\": \"TIMONIUM\",\r\n            \"StateProvinceCode\": \"MD\",\r\n            \"PostalCode\": \"21093\",\r\n            \"CountryCode\": \"US\"\r\n          }\r\n        },\r\n        \"PaymentDetails\": {\r\n          \"ShipmentCharge\": {\r\n            \"Type\": \"01\",\r\n            \"BillShipper\": {\r\n              \"AccountNumber\": \"C5J577\"\r\n            }\r\n          }\r\n        },\r\n        \"Service\": {\r\n          \"Code\": \"03\",\r\n          \"Description\": \"Ground\"\r\n        },\r\n        \"NumOfPieces\": \"1\",\r\n        \"Package\": {\r\n          \"SimpleRate\": {\r\n            \"Description\": \"SimpleRateDescription\",\r\n            \"Code\": \"XS\"\r\n          },\r\n          \"PackagingType\": {\r\n            \"Code\": \"02\",\r\n            \"Description\": \"Packaging\"\r\n          },\r\n          \"Dimensions\": {\r\n            \"UnitOfMeasurement\": {\r\n              \"Code\": \"IN\",\r\n              \"Description\": \"Inches\"\r\n            },\r\n            \"Length\": \"5\",\r\n            \"Width\": \"5\",\r\n            \"Height\": \"5\"\r\n          },\r\n          \"PackageWeight\": {\r\n            \"UnitOfMeasurement\": {\r\n              \"Code\": \"LBS\",\r\n              \"Description\": \"Pounds\"\r\n            },\r\n            \"Weight\": \"1\"\r\n          }\r\n        }\r\n      }\r\n    }\r\n  }", null, "application/json");
            request.Content = content;
            var response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                Rate_response = response.Content.ReadAsStringAsync().Result;
                UPs_response = JsonConvert.DeserializeObject<Rootobject>(Rate_response);
            }
            return UPs_response;
        }

        //
        /// <summary>
        /// THIS FUNCTION GETS authorize api  response of https://signin.testing.stampsendicia.com . We are sending three parameters to this get api.
        /// </summary>
        /// <param name="access_token"></param>
        /// <returns>RETURN RATING RESPONSE OBJECT</returns>
        public async Task<ActionResult> Stamps_Login_Redirect(string? code)
        {
            string stamps_login_URL = String.Empty;
            // In this function we need set code setting so that it will run in first click. Righ now the thing is happening that first time it hit the
            // the api and get code. In second attempt it hit rate list api when it has the code. So we need to get and save the code before hitting this api-
            if (code == null || code == "undefined")
            {
                stamps_login_URL = "https://signin.testing.stampsendicia.com/authorize?client_id=5rcNPLgvzkqSYFJYeSTMr029O6fPfZFe&response_type=code&redirect_uri=https://localhost:7210/";
            }
            else
            {
                var Sender_postal_code = HttpContext.Session.GetString("Shipper_Postal_Code");
                var Receipient_postal_code = HttpContext.Session.GetString("Recipient_Postal_Code");
                return await Get_All_RateList(code, Sender_postal_code, Receipient_postal_code, false);
            }
            return Redirect(stamps_login_URL);
        }
        /// < summary>
        /// This function get the Auth Token for Client
        /// </summary>
        /// <param name="access_Code"></param>
        /// Access Code is Getting from the redirect URL after Logged-in Successfully.
        public async Task<string> Get_Access_Token_For_Stamsp(string access_Code)
        {
            string client_id = "5rcNPLgvzkqSYFJYeSTMr029O6fPfZFe";
            string client_Secret_id = "N2wD7_prJjEFwOgghxAr6q68hn722ShC9XzrscXGqGH7KWYgfesg8xcQ72TSXjq3";
            string redirect_uri = "https://localhost:7210/";
            var responseData = string.Empty;

            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://signin.testing.stampsendicia.com/oauth/token");
            request.Headers.Add("Accept", "application/json");

            var content = new StringContent("{\n  \"grant_type\": \"authorization_code\",\n  \"client_id\": \"5rcNPLgvzkqSYFJYeSTMr029O6fPfZFe\",\n  \"client_secret\": \"N2wD7_prJjEFwOgghxAr6q68hn722ShC9XzrscXGqGH7KWYgfesg8xcQ72TSXjq3\",\n  \"code\": " + "\"" + access_Code + "\"" + ",\n  \"redirect_uri\": \"https://localhost:7210/\"\n}", null, "application/json");
            request.Content = content;
            var X = content.ToString();
            //request.Content = content_by_JSON;
            var response = await client.SendAsync(request);
            //response.EnsureSuccessStatusCode();
            Console.WriteLine(await response.Content.ReadAsStringAsync());
            responseData = response.Content.ReadAsStringAsync().Result;
            var authDetails = JsonConvert.DeserializeObject<dynamic>(responseData);
            var token = authDetails.access_token;

            //response.EnsureSuccessStatusCode();
            Console.WriteLine(await response.Content.ReadAsStringAsync());



            return token;

        }
        //updated on-19 AUG- 2023
        public async Task<List<Root>> Get_Stamps_RateList_v2(string Access_token, string? _shipperPostalCode, string? _recipientPostalCode)
        {
            //ship date muust be withing 7 days of today
            var ship_date = DateTime.UtcNow.AddDays(6);

            string DateString = ship_date.ToString();
            DateTime Date = DateTime.Parse(DateString);
            string SHIP_DATE_FORMATTED = Date.ToString("yyyy-MM-dd");

            List<Root> myDeserializedClass = new List<Root>();

            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.testing.stampsendicia.com/sera/v1/rates");
            request.Headers.Add("Authorization", "Bearer "+ Access_token);
            var content = new StringContent("\r\n{\"from_address\": {\r\n    \"company_name\": \"\",\r\n    \"name\": \"\",\r\n    \"address_line1\": \"\",\r\n    \"address_line2\": \"\",\r\n    \"address_line3\": \"\",\r\n    \"city\": \"\",\r\n    \"state_province\": \"\",\r\n    \"postal_code\":  " + _shipperPostalCode + ",\r\n    \"country_code\": \"st\",\r\n    \"phone\": \"\",\r\n    \"email\": \"\"\r\n  },\r\n  \"to_address\": {\r\n    \"company_name\": \"\",\r\n    \"name\": \"\",\r\n    \"address_line1\": \"\",\r\n    \"address_line2\": \"\",\r\n    \"address_line3\": \"\",\r\n    \"city\": \"\",\r\n    \"state_province\": \"\",\r\n    \"postal_code\":  " + _recipientPostalCode + ",\r\n    \"country_code\": \"st\",\r\n    \"phone\": \"\",\r\n    \"email\": \"\"\r\n  },\r\n  \"ship_date\":  \" "+SHIP_DATE_FORMATTED+"\"\r\n  }", null, "application/json");
            request.Content = content;
            var response = await client.SendAsync(request);
            //response.EnsureSuccessStatusCode();
            if (response.IsSuccessStatusCode)
            {
                var myJsonResponse = response.Content.ReadAsStringAsync().Result;

                myDeserializedClass = JsonConvert.DeserializeObject<List<Root>>(myJsonResponse);
            }
            return myDeserializedClass;
        }

        public async Task<List<Root>> Get_Stamps_RateList(string Access_token)
        {

            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.testing.stampsendicia.com/sera/v1/rates");
            request.Headers.Add("carriers", "usps");
            request.Headers.Add("Authorization", "Bearer " + Access_token);
            var content = new StringContent("{\r\n  \"from_address\": {\r\n    \"company_name\": \"\",\r\n    \"name\": \"\",\r\n    \"address_line1\": \"\",\r\n    \"address_line2\": \"\",\r\n    \"address_line3\": \"\",\r\n    \"city\": \"\",\r\n    \"state_province\": \"\",\r\n    \"postal_code\": \"65247\",\r\n    \"country_code\": \"CA\",\r\n    \"phone\": \"\",\r\n    \"email\": \"\"\r\n  },\r\n  \"to_address\": {\r\n    \"company_name\": \"\",\r\n    \"name\": \"\",\r\n    \"address_line1\": \"\",\r\n    \"address_line2\": \"\",\r\n    \"address_line3\": \"\",\r\n    \"city\": \"\",\r\n    \"state_province\": \"British Columbia\",\r\n    \"postal_code\": \"75063\",\r\n    \"country_code\": \"Ca\",\r\n    \"phone\": \"\",\r\n    \"email\": \"\"\r\n  },\r\n  \"service_type\": \"\",\r\n  \"package\": {\r\n    \"packaging_type\": \"\",\r\n    \"weight\": ,\r\n    \"weight_unit\": \"ounce\",\r\n    \"length\": 5,\r\n    \"width\": 5,\r\n    \"height\": 5,\r\n    \"dimension_unit\": \"inch\"\r\n  },\r\n  \"delivery_confirmation_type\": \"tracking\",\r\n  \"insurance\": {\r\n    \"insurance_provider\": \"\",\r\n    \"insured_value\": {\r\n      \"amount\": 20,\r\n      \"currency\": \"usd\"\r\n    }\r\n  },\r\n  \"customs\": {\r\n    \"contents_type\": \"gift\",\r\n    \"contents_description\": \"none\",\r\n    \"non_delivery_option\": \"return_to_sender\",\r\n    \"sender_info\": {\r\n      \"license_number\": \"\",\r\n      \"certificate_number\": \"\",\r\n      \"invoice_number\": \"\",\r\n      \"internal_transaction_number\": \"\",\r\n      \"passport_number\": \"\",\r\n      \"passport_issue_date\": \"2022-10-21\",\r\n      \"passport_expiration_date\": \"2027-12-30\"\r\n    },\r\n    \"recipient_info\": {\r\n      \"tax_id\": \"string\"\r\n    },\r\n    \"customs_items\": [\r\n      {\r\n        \"item_description\": \"string\",\r\n        \"quantity\": 10,\r\n        \"unit_value\": {\r\n          \"amount\": 20,\r\n          \"currency\": \"usd\"\r\n        },\r\n        \"item_weight\": 10,\r\n        \"weight_unit\": \"ounce\",\r\n        \"harmonized_tariff_code\": \"string\",\r\n        \"country_of_origin\": \"CA\",\r\n        \"sku\": \"string\"\r\n      }\r\n    ]\r\n  },\r\n  \"ship_date\": \"2023-09-7\",\r\n  \"is_return_label\": false,\r\n  \"advanced_options\": {\r\n    \"non_machinable\": false,\r\n    \"saturday_delivery\": true,\r\n    \"delivered_duty_paid\": false,\r\n    \"hold_for_pickup\": false,\r\n    \"certified_mail\": false,\r\n    \"return_receipt\": false,\r\n    \"return_receipt_electronic\": false,\r\n    \"collect_on_delivery\": {\r\n      \"amount\": 10,\r\n      \"currency\": \"usd\"\r\n    },\r\n    \"registered_mail\": {\r\n      \"amount\": 20,\r\n      \"currency\": \"usd\"\r\n    },\r\n    \"sunday_delivery\": false,\r\n    \"holiday_delivery\": false,\r\n    \"restricted_delivery\": false,\r\n    \"notice_of_non_delivery\": false,\r\n    \r\n    \"no_label\": {\r\n      \"is_drop_off\": false,\r\n      \"is_prepackaged\": false\r\n    },\r\n    \"is_pay_on_use\": false\r\n  }\r\n}", null, "application/json");
            request.Content = content;
            var response = await client.SendAsync(request);
            var myJsonResponse = response.Content.ReadAsStringAsync().Result;

            List<Root> myDeserializedClass = JsonConvert.DeserializeObject<List<Root>>(myJsonResponse);
            //response.EnsureSuccessStatusCode();
            Console.WriteLine(await response.Content.ReadAsStringAsync());

            return myDeserializedClass;
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
        //UPS RATE RESPOSE MODEL-END
        //public class AllowCrossSiteJsonAttribute : ActionFilterAttribute
        //{
        //    public override void OnActionExecuting(ActionExecutingContext filterContext)
        //    {
        //        filterContext.Request.HttpContext.Response.AddHeader("Access-Control-Allow-Origin", "*");

        //        base.OnActionExecuting(filterContext);
        //    }
        //}

        // STAMPS RATING RESPONSE CLASSES: START
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


        // STAMPS RATING RESPONSE CLASSES: END
        //Stamps Rate Response- start
        // Root myDeserializedClass = JsonConvert.DeserializeObject<List<Root>>(myJsonResponse);
        //public class CostDetail_v2
        //{
        //    public string fee_code { get; set; }
        //    public string fee_type { get; set; }
        //    public double amount { get; set; }
        //}

        //public class Root_V2
        //{
        //    public string carrier { get; set; }
        //    public string service_type { get; set; }
        //    public string packaging_type { get; set; }
        //    public string estimated_delivery_days { get; set; }
        //    public object estimated_delivery_date { get; set; }
        //    public bool is_guaranteed_service { get; set; }
        //    public string trackable { get; set; }
        //    public bool is_return_label { get; set; }
        //    public bool is_customs_required { get; set; }
        //    public ShipmentCost_v2 shipment_cost_v2 { get; set; }
        //}

        //public class ShipmentCost_v2
        //{
        //    public double total_amount { get; set; }
        //    public string currency { get; set; }
        //    public List<CostDetail_v2> CostDetail_v2 { get; set; }
        //}



    }
}
