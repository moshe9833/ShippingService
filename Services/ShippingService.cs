using Newtonsoft.Json;
using System.Text;
using CourierRates.Controllers;
using Microsoft.AspNetCore.Mvc;
using CourierRates.Models;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json.Linq;
using RestSharp;
using static CourierRates.Models.GetShip;
using static CourierRates.Models.GetStamp;
using static CourierRates.Models.Shipping_Response.UPS_Response;
using static CourierRates.Models.Shipping_Response;
using System.Net.Http.Headers;
using System.Net;
using Microsoft.DotNet.MSIdentity.Shared;
using static CourierRates.Models.GetTrack;
using static CourierRates.Models.Shippment;

namespace CourierRates.Services
{
    public class ShippingService
    {

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

        public async Task<List<RateCompareList>> Get_All_RateList(string code, string shipperPostalCode, string recipientPostalCode, bool IsFormPost = true)
        {

            if (code == null)
            {
                var result = Stamps_Login_Redirect(code);
                //return this.Redirect(Request.Scheme + "://" + Request.Host.Value + "/APICall/Stamps_Login_Redirect?code=" + code);

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
           new RateCompareList
           {
               ID = index,
               Carrier = "FedEx",
               pricingCode = item.serviceType,
               Rate = Convert.ToDecimal(item.ratedShipmentDetails[0].totalNetFedExCharge),
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
           new RateCompareList
           {
               ID = index,
               Carrier = "Stamps",
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
            return gridResult;
        }
        public async Task<List<Root1>> Get_STMPS_Label(string code ,ShippingRequest shippingRequest)
        {
            List<Root1> labels = new List<Root1>();
            string acess_tokenFedex = GetToken();
            Root1 label3 = await CreateShipmentWithFedEx(acess_tokenFedex, shippingRequest);
            labels.Add(label3);
            #region STAMPS
            string acess_token = await Get_Access_Token_For_Stamsp(code);
            Root1 label1 = await Get_STMPS_LabelAPI(acess_token, shippingRequest);
            #endregion
            labels.Add(label1);

            string access_token = await GetToken_UPS();
            Root1 label2 = await Create_Shipment_with_UPS(access_token, shippingRequest);
            labels.Add(label2);


            return labels;
        }
        public async Task<Root1> Get_STMPS_LabelAPI(string tokenAccess, ShippingRequest shippingRequest)
        {
            var uuid = new Guid();
            Root1 DeserilizeData = new Root1();
            var DateForShipment = DateTime.Now.ToShortDateString();
            try
            {
                var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Post, "https://api.testing.stampsendicia.com/sera/v1/labels");
                request.Headers.Add("Accept", "application/json");
                request.Headers.Add("Idempotency-Key", Convert.ToString(uuid));
                //request.Headers.Add("", "");
                request.Headers.Add("Authorization", "Bearer " + tokenAccess);
                var content = new StringContent("{\n  \"from_address\": {\n    \"company_name\": \"Test Sender\",\n    \"name\": \"Test Sender\",\n    \"address_line1\": \"123 Main Street\",\n    \"city\": \"Anytown\",\n    \"state_province\": \"CA\",\n    \"postal_code\": \"65247\",\n    \"country_code\": \"US\"\n  },\n  \"return_address\": {\n    \"company_name\": \"Test Return Address\",\n    \"name\": \"Test Return Address\",\n    \"address_line1\": \"456 Elm Street\",\n    \"city\": \"Anytown\",\n    \"state_province\": \"CA\",\n    \"postal_code\": \"12345\",\n    \"country_code\": \"US\"\n  },\n  \"to_address\": {\n    \"company_name\": \"Recipient Company\",\n    \"name\": \"Recipient Name\",\n    \"address_line1\": \"456 Recipient Rd\",\n    \"address_line2\": \"Apt 789\",\n    \"address_line3\": \"Recipient Line 3\",\n    \"city\": \"Recipient City\",\n    \"state_province\": \"NY\",\n    \"postal_code\": \"75063\",\n    \"country_code\": \"US\",\n    \"phone\": \"987-654-3210\",\n    \"email\": \"recipient@email.com\"\n  },\n  \"service_type\": \"usps_first_class_mail\",\n  \"package\": {\n    \"packaging_type\": \"package\",\n    \"weight\": 1.0,\n    \"weight_unit\": \"ounce\",\n    \"length\": 6.0,\n    \"width\": 3.0,\n    \"height\": 2.0,\n    \"dimension_unit\": \"inch\"\n  },\n  \"ship_date\": \"" + DateForShipment + "\",\n  \"is_return_label\": false,\n  \"advanced_options\": {\n    \"non_machinable\": false,\n    \"saturday_delivery\": false,\n    \"delivered_duty_paid\": false,\n    \"hold_for_pickup\": false,\n    \"certified_mail\": false,\n    \"return_receipt\": false,\n    \"return_receipt_electronic\": false,\n    \"collect_on_delivery\": {\n      \"amount\": 0.00,\n      \"currency\": \"usd\"\n    },\n    \"registered_mail\": {\n      \"amount\": 0.00,\n      \"currency\": \"usd\"\n    },\n    \"sunday_delivery\": false,\n    \"holiday_delivery\": false,\n    \"restricted_delivery\": false,\n    \"notice_of_non_delivery\": false,\n    \"special_handling\": {\n      \"special_contents_type\": \"\",\n      \"fragile\": false\n    },\n    \"no_label\": {\n      \"is_drop_off\": false,\n      \"is_prepackaged\": false\n    },\n    \"is_pay_on_use\": false,\n    \"return_options\": {\n      \"outbound_label_id\": \"\"\n    }\n  },\n  \"label_options\": {\n    \"label_size\": \"4x6\",\n    \"label_format\": \"pdf\",\n    \"label_logo_image_id\": 0,\n    \"label_output_type\": \"url\"\n  },\n  \"references\": {\n    \"printed_message1\": \"\",\n    \"printed_message2\": \"\",\n    \"printed_message3\": \"\",\n    \"cost_code_id\": 0,\n    \"reference1\": \"\",\n    \"reference2\": \"\",\n    \"reference3\": \"\",\n    \"reference4\": \"\"\n  }\n}", null, "application/json");
                request.Content = content;
                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var responseret = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == System.Net.HttpStatusCode.Created || response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    DeserilizeData = new Root1();
                    DeserilizeData = JsonConvert.DeserializeObject<Root1>(responseret);
                }
            }
            catch (Exception ex)
            {

            }

            return DeserilizeData;
        }
        public async Task<dynamic> Get_Shipment_Label(string Courier_type, string PostalcodeShiper, string Reciptcodeshiper)
        {

            return 1;
        }


        public async Task<Root1> CreateShipmentWithFedEx(string AccessToken, ShippingRequest shippingRequest)
        {
            Root_fedex Shipment_response = new Root_fedex();
            Root1 label = new Root1();
            var uuid = new Guid();

            try
            {
                var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Post, "https://apis-sandbox.fedex.com/ship/v1/shipments");
                request.Headers.Add("Accept", "application/json");
                request.Headers.Add("x-customer-transaction-id", Convert.ToString(uuid));
                //request.Headers.Add("content-type", "application/json");
                //request.Headers.Add("x-locale", "en_US");
                request.Headers.Add("authorization", "Bearer " + AccessToken);
                var content = new StringContent("{\r\n  \"labelResponseOptions\": \"URL_ONLY\",\r\n  \"requestedShipment\": {\r\n    \"shipper\": {\r\n      \"contact\": {\r\n        \"personName\": \"SHIPPER NAME\",\r\n        \"phoneNumber\": 1234567890,\r\n        \"companyName\": \"Shipper Company Name\"\r\n      },\r\n      \"address\": {\r\n        \"streetLines\": [\r\n          \"SHIPPER STREET LINE 1\"\r\n        ],\r\n        \"city\": \"HARRISON\",\r\n        \"stateOrProvinceCode\": \"AR\",\r\n        \"postalCode\": 7260,\r\n        \"countryCode\": \"US\"\r\n      }\r\n    },\r\n    \"recipients\": [\r\n      {\r\n        \"contact\": {\r\n          \"personName\": \"RECIPIENT NAME\",\r\n          \"phoneNumber\": 1234567890,\r\n          \"companyName\": \"Recipient Company Name\"\r\n        },\r\n        \"address\": {\r\n          \"streetLines\": [\r\n            \"RECIPIENT STREET LINE 1\",\r\n            \"RECIPIENT STREET LINE 2\"\r\n          ],\r\n          \"city\": \"Collierville\",\r\n          \"stateOrProvinceCode\": \"TN\",\r\n          \"postalCode\": 38017,\r\n          \"countryCode\": \"US\"\r\n        }\r\n      }\r\n    ],\r\n    \"shipDatestamp\": \"2020-12-30\",\r\n    \"serviceType\": \"STANDARD_OVERNIGHT\",\r\n    \"packagingType\": \"FEDEX_SMALL_BOX\",\r\n    \"pickupType\": \"USE_SCHEDULED_PICKUP\",\r\n    \"blockInsightVisibility\": false,\r\n    \"shippingChargesPayment\": {\r\n      \"paymentType\": \"SENDER\"\r\n    },\r\n    \"shipmentSpecialServices\": {\r\n      \"specialServiceTypes\": [\r\n        \"FEDEX_ONE_RATE\"\r\n      ]\r\n    },\r\n    \"labelSpecification\": {\r\n      \"imageType\": \"PDF\",\r\n      \"labelStockType\": \"PAPER_85X11_TOP_HALF_LABEL\"\r\n    },\r\n    \"requestedPackageLineItems\": [\r\n      {}\r\n    ]\r\n  },\r\n  \"accountNumber\": {\r\n    \"value\": \"740561073\"\r\n  }\r\n}", null, "application/json");
                request.Content = content;
                var response = await client.SendAsync(request);


                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        var myJsonResponse = await response.Content.ReadAsStringAsync();
                        Shipment_response = JsonConvert.DeserializeObject<Root_fedex>(myJsonResponse);
                    }
                    catch (Exception)
                    {
                        Console.WriteLine(await response.Content.ReadAsStringAsync());
                    }

                }
            }
            catch (Exception ex)
            {

            }

            label.label_id = Shipment_response.transactionId;
            label.tracking_number = Shipment_response.output.transactionShipments.FirstOrDefault().masterTrackingNumber;
            label.shipment_cost = new ShipmentCost1();
            label.shipment_cost.currency = "usd"; //using static type later we will change
            label.shipment_cost.total_amount = Shipment_response.output.transactionShipments.FirstOrDefault().pieceResponses.FirstOrDefault().baseRateAmount; ;
            return label;
        }
        public async Task<GetTrack.RootObject> GetUPSStatus(TrackingRequest trackingRequest)
        {
            // Construct the API URL
            string apiUrl = "https://wwwcie.ups.com/api/track/v1/details/1Z12345E8791315509?locale=en_US&returnSignature=false&inquiryNumber=1Z12345E8791315509";
            string access_token = await GetToken_UPS();
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {access_token}");
                client.DefaultRequestHeaders.Add("transId", "794971969821");
                client.DefaultRequestHeaders.Add("transactionSrc", "UPS");
               
                HttpResponseMessage response = await client.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var result = response.Content.ReadAsStringAsync().Result;
                    GetTrack.RootObject responseObject = JsonConvert.DeserializeObject<GetTrack.RootObject>(result);


                    return responseObject;
                }
                else
                {
                    // Handle the error (e.g., log or throw exception)
                    throw new Exception($"UPS API request failed with status code {response.StatusCode}");
                }
            }
        }
        public async Task<string> GetFEdEXStatus()
        {
            string acess_tokenFedex = GetToken();

            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://apis-sandbox.fedex.com/track/v1/");

                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {acess_tokenFedex}");
             
                client.DefaultRequestHeaders.Add("x-customer-transaction-id", Guid.NewGuid().ToString());

                var requestBody = new
                {
                    includeDetailedScans = true,
                    trackingInfo = new[]
                    {
                        new
                        {
                            shipDateBegin = "2020-03-29",
                            shipDateEnd = "2020-04-01",
                            trackingNumberInfo = new
                            {
                                trackingNumber = "794971969821",
                                carrierCode = "FDXE",
                                trackingNumberUniqueId = "794971969821"
                            }
                        }
                    }
                };
                var jsonRequestBody = JsonConvert.SerializeObject(requestBody);

                var content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync("trackingnumbers", content);

                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync();
                    return result;
                }
                else
                {
                    throw new Exception($"UPS API request failed with status code {response.StatusCode}");
                }
            }
        }
        public async Task<Root1> Create_Shipment_with_UPS(string AccessToken, ShippingRequest shippingRequest)
        {
            Root_ups shipment_Response = new Root_ups();
            Root1 label = new Root1();
            try
            {
                var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Post, "https://wwwcie.ups.com/api/shipments/v1/ship?additionaladdressvalidation=string");
                request.Headers.Add("Authorization", "Bearer " + AccessToken);
                request.Headers.Add("transId", "string");
                request.Headers.Add("transactionSrc", "testing");
                var content = new StringContent("{\n    \"ShipmentRequest\": {\n        \"Request\": {\n            \"SubVersion\": \"1801\",\n            \"RequestOption\": \"nonvalidate\",\n            \"TransactionReference\": {\n                \"CustomerContext\": \"\"\n            }\n        },\n        \"Shipment\": {\n            \"Description\": \"Ship WS test\",\n            \"Shipper\": {\n                \"Name\": \"ShipperName\",\n                \"AttentionName\": \"ShipperZs Attn Name\",\n                \"TaxIdentificationNumber\": \"123456\",\n                \"Phone\": {\n                    \"Number\": \"1115554758\",\n                    \"Extension\": \" \"\n                },\n                \"ShipperNumber\": \"C5J577\",\n                \"FaxNumber\": \"8002222222\",\n                \"Address\": {\n                    \"AddressLine\": [\n                        \"2311 York Rd\"\n                    ],\n                    \"City\": \"Timonium\",\n                    \"StateProvinceCode\": \"MD\",\n                    \"PostalCode\": \"21093\",\n                    \"CountryCode\": \"US\"\n                }\n            },\n            \"ShipTo\": {\n                \"Name\": \"Happy Dog Pet Supply\",\n                \"AttentionName\": \"1160b_74\",\n                \"Phone\": {\n                    \"Number\": \"9225377171\"\n                },\n                \"Address\": {\n                    \"AddressLine\": [\n                        \"123 Main St\"\n                    ],\n                    \"City\": \"timonium\",\n                    \"StateProvinceCode\": \"MD\",\n                    \"PostalCode\": \"21030\",\n                    \"CountryCode\": \"US\"\n                },\n                \"Residential\": \" \"\n            },\n            \"ShipFrom\": {\n                \"Name\": \"T and T Designs\",\n                \"AttentionName\": \"1160b_74\",\n                \"Phone\": {\n                    \"Number\": \"1234567890\"\n                },\n                \"FaxNumber\": \"1234567890\",\n                \"Address\": {\n                    \"AddressLine\": [\n                        \"2311 York Rd\"\n                    ],\n                    \"City\": \"Alpharetta\",\n                    \"StateProvinceCode\": \"GA\",\n                    \"PostalCode\": \"30005\",\n                    \"CountryCode\": \"US\"\n                }\n            },\n            \"PaymentInformation\": {\n                \"ShipmentCharge\": {\n                    \"Type\": \"01\",\n                    \"BillShipper\": {\n                        \"AccountNumber\": \"C5J577\"\n                    }\n                }\n            },\n            \"Service\": {\n                \"Code\": \"03\",\n                \"Description\": \"Express\"\n            },\n            \"Package\": {\n                \"Description\": \" \",\n                \"Packaging\": {\n                    \"Code\": \"02\",\n                    \"Description\": \"Nails\"\n                },\n                \"Dimensions\": {\n                    \"UnitOfMeasurement\": {\n                        \"Code\": \"IN\",\n                        \"Description\": \"Inches\"\n                    },\n                    \"Length\": \"10\",\n                    \"Width\": \"30\",\n                    \"Height\": \"45\"\n                },\n                \"PackageWeight\": {\n                    \"UnitOfMeasurement\": {\n                        \"Code\": \"LBS\",\n                        \"Description\": \"Pounds\"\n                    },\n                    \"Weight\": \"5\"\n                }\n            }\n        },\n        \"LabelSpecification\": {\n            \"LabelImageFormat\": {\n                \"Code\": \"GIF\",\n                \"Description\": \"GIF\"\n            },\n            \"HTTPUserAgent\": \"Mozilla/4.5\"\n        }\n    }\n}", null, "application/json");
                request.Content = content;
                var response = await client.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        var json_response = await response.Content.ReadAsStringAsync();
                        shipment_Response = JsonConvert.DeserializeObject<Root_ups>(json_response);
                    }
                    catch (Exception)
                    {
                        Console.WriteLine(await response.Content.ReadAsStringAsync());
                    }

                }
            }
            catch (Exception EX)
            {

            }

            label.label_id = shipment_Response.ShipmentResponse.ShipmentResults.ShipmentIdentificationNumber;
            label.tracking_number = shipment_Response.ShipmentResponse.ShipmentResults.PackageResults.TrackingNumber;
            label.shipment_cost = new ShipmentCost1();
            label.shipment_cost.currency = shipment_Response.ShipmentResponse.ShipmentResults.ShipmentCharges.TotalCharges.CurrencyCode;
            label.shipment_cost.total_amount = Convert.ToDouble(shipment_Response.ShipmentResponse.ShipmentResults.ShipmentCharges.TotalCharges.MonetaryValue);
            return label;
        }
        public void mytest()
        {
            var BaseUrl = "https://apis-sandbox.fedex.com/oauth/token";

            var responseData = string.Empty;
            HttpClient httpClient = new HttpClient(new HttpClientHandler { UseCookies = true });

            HttpContent content3 = new FormUrlEncodedContent(
            new List<KeyValuePair<string, string>> {
            new KeyValuePair<string, string>("grant_type","client_credentials"),
            new KeyValuePair<string, string>("client_id", "l7b18f08d7cad24317a0ecb044d0426b74"),
            new KeyValuePair<string, string>("client_secret", "28115ee5ea654f40a738d4989e4df940")});

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

            var documentResponse = resp;//.Content.ReadAsStringAsync();

            var model = JsonConvert.DeserializeObject<Application>(documentResponse.ToString());
            return model;     

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
        public async Task<string> GetToken_UPS()
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
                return access_token;
            }
        }


        public async Task<Rootobject> UPS_Rating_v2(string access_token)
        {
            string Acc_number = "C5J577";
            var UPs_response = new Rootobject();
            string? Rate_response;
            //TODO- USE THE BELOW CODES
            var _shipperPostalCode = "65247";
            var Receipient_postal_code = "75063";
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

        public async Task<string> Stamps_Login_Redirect(string? code)
        {
            string stamps_login_URL = String.Empty;
            if (code == null || code == "undefined")
            {
                stamps_login_URL = "https://signin.testing.stampsendicia.com/authorize?client_id=5rcNPLgvzkqSYFJYeSTMr029O6fPfZFe&response_type=code&redirect_uri=https://localhost:7210/";
            }

            return stamps_login_URL;

        }
        public async Task<string> Stamps_Login_RedirectLabel(string? code)
        {

            string stamps_login_URL = String.Empty;
            if (code == null || code == "undefined")
            {
                stamps_login_URL = "https://signin.testing.stampsendicia.com/authorize?client_id=5rcNPLgvzkqSYFJYeSTMr029O6fPfZFe&response_type=code&redirect_uri=https://localhost:7210/";
            }

            return stamps_login_URL;
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
            request.Headers.Add("Authorization", "Bearer " + Access_token);
            var content = new StringContent("\r\n{\"from_address\": {\r\n    \"company_name\": \"\",\r\n    \"name\": \"\",\r\n    \"address_line1\": \"\",\r\n    \"address_line2\": \"\",\r\n    \"address_line3\": \"\",\r\n    \"city\": \"\",\r\n    \"state_province\": \"\",\r\n    \"postal_code\":  " + _shipperPostalCode + ",\r\n    \"country_code\": \"st\",\r\n    \"phone\": \"\",\r\n    \"email\": \"\"\r\n  },\r\n  \"to_address\": {\r\n    \"company_name\": \"\",\r\n    \"name\": \"\",\r\n    \"address_line1\": \"\",\r\n    \"address_line2\": \"\",\r\n    \"address_line3\": \"\",\r\n    \"city\": \"\",\r\n    \"state_province\": \"\",\r\n    \"postal_code\":  " + _recipientPostalCode + ",\r\n    \"country_code\": \"st\",\r\n    \"phone\": \"\",\r\n    \"email\": \"\"\r\n  },\r\n  \"ship_date\":  \" " + SHIP_DATE_FORMATTED + "\"\r\n  }", null, "application/json");
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


    }

}
