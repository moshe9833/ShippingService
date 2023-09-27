﻿using CourierRates.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using static CourierRates.Controllers.APICallController;

namespace CourierRates.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public async Task<IActionResult> Index(string? code)
        {
            if(code == null)
            {
                HttpContext.Session.Remove("Shipper_Postal_Code");
                HttpContext.Session.Remove("Recipient_Postal_Code");
                HttpContext.Session.Remove("IsForlabel");
            }
            //UPS token
            //var accessToken = code;
            #region FED-EX

            #endregion

            #region UPS
            //var UPS_Rate_REsponse = await UPS_GenerateToken();
            #endregion

            #region STAMPS
            //string acess_token = await Get_Access_Token_For_Stamsp(code);
            //await Get_Stamps_RateList(acess_token);
            #endregion

            var IsForLable = HttpContext.Session.GetString("IsForlabel");
            if (code != null && IsForLable == "1")
            {
                return this.Redirect(Request.Scheme + "://" + Request.Host.Value + "/APICall/Stamps_Login_RedirectLabel?code=" + code);

            }
            else if(code != null)
            {
                return this.Redirect(Request.Scheme + "://" + Request.Host.Value + "/APICall/Stamps_Login_Redirect?code=" + code);
            }
            //await upsAsync();
            return View();
        }
       
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

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
        /// <summary>
        /// THIS FUNCTION GETS THE UPS RATING RESPONSE
        /// </summary>
        /// <param name="access_token"></param>
        /// ACCESS TOKEN IS GENERATED BY UPS_GenerateToken METHOD
        /// <returns>RETURN RATING RESPONSE OBJECT</returns>
        public async Task<Rootobject> UPS_Rating_v2(string access_token)
        {
            string Acc_number = "C5J577";
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://wwwcie.ups.com/api/rating/v1801/Rate");
            request.Headers.Add("Authorization", "Bearer " + access_token);
            request.Headers.Add("transId", "Tran123");
            request.Headers.Add("transactionSrc", "testing");
            var content = new StringContent("{\r\n    \"RateRequest\": {\r\n      \"Request\": {\r\n        \"TransactionReference\": {\r\n          \"CustomerContext\": \"CustomerContext\",\r\n          \"TransactionIdentifier\": \"TransactionIdentifier\"\r\n        }\r\n      },\r\n      \"Shipment\": {\r\n        \"Shipper\": {\r\n          \"Name\": \"ShipperName\",\r\n          \"ShipperNumber\": \"C5J577\",\r\n          \"Address\": {\r\n            \"AddressLine\": [\r\n              \"ShipperAddressLine\",\r\n              \"ShipperAddressLine\",\r\n              \"ShipperAddressLine\"\r\n            ],\r\n            \"City\": \"TIMONIUM\",\r\n            \"StateProvinceCode\": \"MD\",\r\n            \"PostalCode\": \"21093\",\r\n            \"CountryCode\": \"US\"\r\n          }\r\n        },\r\n        \"ShipTo\": {\r\n          \"Name\": \"ShipToName\",\r\n          \"Address\": {\r\n            \"AddressLine\": [\r\n              \"ShipToAddressLine\",\r\n              \"ShipToAddressLine\",\r\n              \"ShipToAddressLine\"\r\n            ],\r\n            \"City\": \"Alpharetta\",\r\n            \"StateProvinceCode\": \"GA\",\r\n            \"PostalCode\": \"30005\",\r\n            \"CountryCode\": \"US\"\r\n          }\r\n        },\r\n        \"ShipFrom\": {\r\n          \"Name\": \"ShipFromName\",\r\n          \"Address\": {\r\n            \"AddressLine\": [\r\n              \"ShipFromAddressLine\",\r\n              \"ShipFromAddressLine\",\r\n              \"ShipFromAddressLine\"\r\n            ],\r\n            \"City\": \"TIMONIUM\",\r\n            \"StateProvinceCode\": \"MD\",\r\n            \"PostalCode\": \"21093\",\r\n            \"CountryCode\": \"US\"\r\n          }\r\n        },\r\n        \"PaymentDetails\": {\r\n          \"ShipmentCharge\": {\r\n            \"Type\": \"01\",\r\n            \"BillShipper\": {\r\n              \"AccountNumber\": \"C5J577\"\r\n            }\r\n          }\r\n        },\r\n        \"Service\": {\r\n          \"Code\": \"03\",\r\n          \"Description\": \"Ground\"\r\n        },\r\n        \"NumOfPieces\": \"1\",\r\n        \"Package\": {\r\n          \"SimpleRate\": {\r\n            \"Description\": \"SimpleRateDescription\",\r\n            \"Code\": \"XS\"\r\n          },\r\n          \"PackagingType\": {\r\n            \"Code\": \"02\",\r\n            \"Description\": \"Packaging\"\r\n          },\r\n          \"Dimensions\": {\r\n            \"UnitOfMeasurement\": {\r\n              \"Code\": \"IN\",\r\n              \"Description\": \"Inches\"\r\n            },\r\n            \"Length\": \"5\",\r\n            \"Width\": \"5\",\r\n            \"Height\": \"5\"\r\n          },\r\n          \"PackageWeight\": {\r\n            \"UnitOfMeasurement\": {\r\n              \"Code\": \"LBS\",\r\n              \"Description\": \"Pounds\"\r\n            },\r\n            \"Weight\": \"1\"\r\n          }\r\n        }\r\n      }\r\n    }\r\n  }", null, "application/json");
            request.Content = content;
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var Rate_response = response.Content.ReadAsStringAsync().Result;
            var UPs_response = JsonConvert.DeserializeObject<Rootobject>(Rate_response);

            return UPs_response;
        }
        public async Task upsAsync()
        {
            //APICallController.UPS_GenerateToken();
            // In production code, don't destroy the HttpClient through using, but better use IHttpClientFactory factory or at least reuse an existing HttpClient instance
            // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests
            // https://www.aspnetmonsters.com/2016/08/2016-08-27-httpclientwrong/
            using (var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(new HttpMethod("POST"), "https://wwwcie.ups.com/api/rating/v1/Rate?additionalinfo=string"))
                {
                    request.Headers.TryAddWithoutValidation("Authorization", "Bearer ajVBRzVSR2YtVTJGc2RHVmtYMTlpSUYrSU1ORHRKeFRHelY4TWxHZW9sQm81WFl3aFliU2Q5SGlIeXg2bEdMbk9xK08vRmRkNjc3ZHhuWnpUaW01VzNMamIzZ0J6bUE9PQ==");
                    request.Headers.TryAddWithoutValidation("transId", "string");
                    request.Headers.TryAddWithoutValidation("transactionSrc", "testing");

                    request.Content = new StringContent("{\n    \"RateRequest\": {\n      \"Request\": {\n        \"TransactionReference\": {\n          \"CustomerContext\": \"CustomerContext\",\n          \"TransactionIdentifier\": \"TransactionIdentifier\"\n        }\n      },\n      \"Shipment\": {\n        \"Shipper\": {\n          \"Name\": \"ShipperName\",\n          \"Address\": {\n            \"AddressLine\": [\n              \"ShipperAddressLine\",\n              \"ShipperAddressLine\",\n              \"ShipperAddressLine\"\n            ],\n            \"City\": \"TIMONIUM\",\n            \"StateProvinceCode\": \"MD\",\n            \"PostalCode\": \"21093\",\n            \"CountryCode\": \"US\"\n          }\n        },\n        \"ShipTo\": {\n          \"Name\": \"ShipToName\",\n          \"Address\": {\n            \"AddressLine\": [\n              \"ShipToAddressLine\",\n              \"ShipToAddressLine\",\n              \"ShipToAddressLine\"\n            ],\n            \"City\": \"Alpharetta\",\n            \"StateProvinceCode\": \"GA\",\n            \"PostalCode\": \"30005\",\n            \"CountryCode\": \"US\"\n          }\n        },\n        \"ShipFrom\": {\n          \"Name\": \"ShipFromName\",\n          \"Address\": {\n            \"AddressLine\": [\n              \"ShipFromAddressLine\",\n              \"ShipFromAddressLine\",\n              \"ShipFromAddressLine\"\n            ],\n            \"City\": \"TIMONIUM\",\n            \"StateProvinceCode\": \"MD\",\n            \"PostalCode\": \"21093\",\n            \"CountryCode\": \"US\"\n          }\n        },\n        \"Service\": {\n          \"Code\": \"03\",\n          \"Description\": \"Ground\"\n        },\n        \"NumOfPieces\": \"1\",\n        \"Package\": {\n          \"PackagingType\": {\n            \"Code\": \"02\",\n            \"Description\": \"Packaging\"\n          },\n          \"Dimensions\": {\n            \"UnitOfMeasurement\": {\n              \"Code\": \"IN\",\n              \"Description\": \"Inches\"\n            },\n            \"Length\": \"5\",\n            \"Width\": \"5\",\n            \"Height\": \"5\"\n          },\n          \"PackageWeight\": {\n            \"UnitOfMeasurement\": {\n              \"Code\": \"LBS\",\n              \"Description\": \"Pounds\"\n            },\n            \"Weight\": \"1\"\n          }\n        }\n      }\n    }\n  }");
                    request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

                    var response = await httpClient.SendAsync(request);
                }
            }
        }
        public IActionResult ShipmentDetails()
        {
            RateRequest rateRequest = new RateRequest();
            return View(rateRequest);
        }
        [HttpPost]
        public IActionResult ShipmentDetails(RateRequest model)
        {
            if (ModelState.IsValid)
            {

            }
            return View(model);
        }
        public async Task<string> Get_Access_Token_For_Stamsp(string access_Code)
        {
            string client_id = "5rcNPLgvzkqSYFJYeSTMr029O6fPfZFe";
            string client_Secret_id = "N2wD7_prJjEFwOgghxAr6q68hn722ShC9XzrscXGqGH7KWYgfesg8xcQ72TSXjq3";
            string redirect_uri = "https://localhost:7210/";
            var responseData = string.Empty;

            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://signin.testing.stampsendicia.com/oauth/token");
            request.Headers.Add("Accept", "application/json");

            var content = new StringContent("{\n  \"grant_type\": \"authorization_code\",\n  \"client_id\": \"5rcNPLgvzkqSYFJYeSTMr029O6fPfZFe\",\n  \"client_secret\": \"N2wD7_prJjEFwOgghxAr6q68hn722ShC9XzrscXGqGH7KWYgfesg8xcQ72TSXjq3\",\n  \"code\": "+"\""+ access_Code + "\"" +",\n  \"redirect_uri\": \"https://localhost:7210/\"\n}", null, "application/json");
            request.Content = content;
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
        public async Task<int> Get_Stamps_RateList(string Access_token)
        {
            
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.testing.stampsendicia.com/sera/v1/rates");
            request.Headers.Add("carriers", "usps");
            request.Headers.Add("Authorization", "Bearer "+ Access_token);
            var content = new StringContent("{\r\n  \"from_address\": {\r\n    \"company_name\": \"Vancouver Sun\",\r\n    \"name\": \"Sun Times\",\r\n    \"address_line1\": \"400 – 2985 Virtual Wa, Vancouver, British Columbia, Canada\",\r\n    \"address_line2\": \"\",\r\n    \"address_line3\": \"\",\r\n    \"city\": \"Vancouver\",\r\n    \"state_province\": \"British Columbia\",\r\n    \"postal_code\": \"V5M 4x7\",\r\n    \"country_code\": \"CA\",\r\n    \"phone\": \"604-605-7381\",\r\n    \"email\": \"nitinyadav@gmail.com\"\r\n  },\r\n  \"to_address\": {\r\n    \"company_name\": \"Trucking Company\",\r\n    \"name\": \"Trucking Company ltd\",\r\n    \"address_line1\": \"400-800 Burrard St Vancouver BC V6Z 2J8\",\r\n    \"address_line2\": \"\",\r\n    \"address_line3\": \"\",\r\n    \"city\": \"Vancouver\",\r\n    \"state_province\": \"British Columbia\",\r\n    \"postal_code\": \"V6Z 2J8\",\r\n    \"country_code\": \"Ca\",\r\n    \"phone\": \"604-666-0834\",\r\n    \"email\": \"nitinyadav@gmail.com\"\r\n  },\r\n  \"service_type\": \"globalpost_plus\",\r\n  \"package\": {\r\n    \"packaging_type\": \"large_envelope\",\r\n    \"weight\": 1,\r\n    \"weight_unit\": \"ounce\",\r\n    \"length\": 5,\r\n    \"width\": 5,\r\n    \"height\": 5,\r\n    \"dimension_unit\": \"inch\"\r\n  },\r\n  \"delivery_confirmation_type\": \"tracking\",\r\n  \"insurance\": {\r\n    \"insurance_provider\": \"\",\r\n    \"insured_value\": {\r\n      \"amount\": 20,\r\n      \"currency\": \"usd\"\r\n    }\r\n  },\r\n  \"customs\": {\r\n    \"contents_type\": \"gift\",\r\n    \"contents_description\": \"none\",\r\n    \"non_delivery_option\": \"return_to_sender\",\r\n    \"sender_info\": {\r\n      \"license_number\": \"\",\r\n      \"certificate_number\": \"\",\r\n      \"invoice_number\": \"\",\r\n      \"internal_transaction_number\": \"\",\r\n      \"passport_number\": \"\",\r\n      \"passport_issue_date\": \"2022-10-21\",\r\n      \"passport_expiration_date\": \"2027-12-30\"\r\n    },\r\n    \"recipient_info\": {\r\n      \"tax_id\": \"string\"\r\n    },\r\n    \"customs_items\": [\r\n      {\r\n        \"item_description\": \"string\",\r\n        \"quantity\": 10,\r\n        \"unit_value\": {\r\n          \"amount\": 20,\r\n          \"currency\": \"usd\"\r\n        },\r\n        \"item_weight\": 10,\r\n        \"weight_unit\": \"ounce\",\r\n        \"harmonized_tariff_code\": \"string\",\r\n        \"country_of_origin\": \"CA\",\r\n        \"sku\": \"string\"\r\n      }\r\n    ]\r\n  },\r\n  \"ship_date\": \"2023-08-2\",\r\n  \"is_return_label\": false,\r\n  \"advanced_options\": {\r\n    \"non_machinable\": false,\r\n    \"saturday_delivery\": true,\r\n    \"delivered_duty_paid\": false,\r\n    \"hold_for_pickup\": false,\r\n    \"certified_mail\": false,\r\n    \"return_receipt\": false,\r\n    \"return_receipt_electronic\": false,\r\n    \"collect_on_delivery\": {\r\n      \"amount\": 10,\r\n      \"currency\": \"usd\"\r\n    },\r\n    \"registered_mail\": {\r\n      \"amount\": 20,\r\n      \"currency\": \"usd\"\r\n    },\r\n    \"sunday_delivery\": false,\r\n    \"holiday_delivery\": false,\r\n    \"restricted_delivery\": false,\r\n    \"notice_of_non_delivery\": false,\r\n    \r\n    \"no_label\": {\r\n      \"is_drop_off\": false,\r\n      \"is_prepackaged\": false\r\n    },\r\n    \"is_pay_on_use\": false\r\n  }\r\n}", null, "application/json");
            request.Content = content;
            var response = await client.SendAsync(request);
            //Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
            //response.EnsureSuccessStatusCode();
            Console.WriteLine(await response.Content.ReadAsStringAsync());

            return 0;
        }
    }
}