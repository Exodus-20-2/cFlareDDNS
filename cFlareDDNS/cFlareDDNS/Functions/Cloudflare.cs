using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace cFlareDDNS.Functions
{
    internal class Cloudflare
    {
        private WebHeaderCollection cflareHeaders = new WebHeaderCollection
        {
            { "X-Auth-Email", string.Empty },
            { "Authorization", $"Bearer 0" },
            { "Content-Type", "application/json" }
        };
        private Dictionary<string, dynamic> cflareDnsBody = new Dictionary<string, dynamic>
        {
            { "type", "A" },
            { "name", string.Empty },
            { "content", string.Empty },
            { "ttl", string.Empty },
            { "proxied", true }
        };

        private string domainName { get; set; }
        private string domainID { get; set; }
        private string accountEmail { get; set; }
        private string accountKey { get; set; }
        private List<string> subdomains { get; set; }

        /// <summary>
        /// Cloudflare wrapper with upto-date functions that allow easy communication between your cloudflare account
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="accountEmail"></param>
        /// <param name="subdomains"></param>
        /// <param name="accountKey"></param>
        public Cloudflare(string domain, string accountEmail, List<string> subdomains, string accountKey)
        {
            cflareHeaders["X-Auth-Email"] = accountEmail;
            cflareHeaders["Authorization"] = $"Bearer {accountKey}";

            this.domainName = domain;
            this.accountEmail = accountEmail;
            this.accountKey = accountKey;
            this.subdomains = subdomains;
        }

        /// <summary>
        /// Validation function to check if parsed account key is valid. Return true/false
        /// </summary>
        /// <returns></returns>
        public bool isKeyValid()
        {
            // Create client object and set appropriate parameters
            HttpWebRequest client = HttpWebRequest.CreateHttp("https://api.cloudflare.com/client/v4/user/tokens/verify");
            client.Method = "GET";
            client.Headers = cflareHeaders;
            try
            {
                // Send response and convert to json object
                dynamic response = JsonConvert.DeserializeObject(new StreamReader(client.GetResponse().GetResponseStream()).ReadToEnd());

                // Check if response contains errors, if it does key is invalid so return false
                if (response["errors"].Count > 0) { return false; }

                // If there are no errors, key is valid so return true
                return true;
            }
            catch
            {
                // if theres a try error it means the key is invalid
                return false;
            }
        }

        /// <summary>
        /// Returns a json object of all information about specified record name under the domain
        /// </summary>
        /// <returns></returns>
        public dynamic GetDnsRecords(string recordName, string domainID)
        {
            HttpWebRequest client = HttpWebRequest.CreateHttp($"https://api.cloudflare.com/client/v4/zones/{domainID}/dns_records?name={recordName}");
            client.Method = "GET";
            client.Headers = cflareHeaders;
            dynamic jsonData = JsonConvert.DeserializeObject(new StreamReader(client.GetResponse().GetResponseStream()).ReadToEnd());
            try
            {
                return jsonData["result"][0];
            }
            catch
            {
                return $"{jsonData["errors"]}";
            }
        }

        /// <summary>
        /// Returns the unique domain ID of given domain name
        /// </summary>
        /// <returns></returns>
        public string GetDomainID()
        {
            HttpWebRequest client = HttpWebRequest.CreateHttp($"https://api.cloudflare.com/client/v4/zones?name={this.domainName}");
            client.Method = "GET";
            client.Headers = cflareHeaders;
            dynamic jsonData = JsonConvert.DeserializeObject(new StreamReader(client.GetResponse().GetResponseStream()).ReadToEnd());
            try
            {
                this.domainID = $"{jsonData["result"][0]["id"]}";
                return $"{jsonData["result"][0]["id"]}";
            }
            catch
            {
                return $"{jsonData["errors"]}";
            }
        }

        /// <summary>
        /// Changes the DNS "A" record to point your domain to your new IP
        /// </summary>
        /// <param name="recordID"></param>
        /// <param name="zoneID"></param>
        /// <param name="recordData"></param>
        public void ChangeDns(string recordID, string zoneID, Dictionary<string, dynamic> recordData)
        {
            cflareDnsBody["name"] = recordData["recordName"];
            cflareDnsBody["content"] = Data.Globals.globalIpAddress;
            cflareDnsBody["ttl"] = recordData["recordTTL"];
            cflareDnsBody["proxied"] = true;

            HttpWebRequest client = HttpWebRequest.CreateHttp($"https://api.cloudflare.com/client/v4/zones/{zoneID}/dns_records/{recordID}");
            client.Method = "PUT";
            client.Headers = cflareHeaders;
            client.ContentType = "application/json";

            try
            {
                byte[] requestBody = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(cflareDnsBody));

                using (Stream requestStream = client.GetRequestStream())
                {
                    requestStream.Write(requestBody, 0, requestBody.Length);
                    requestStream.Close();
                }

                dynamic jsonData = JsonConvert.DeserializeObject(new StreamReader(client.GetResponse().GetResponseStream()).ReadToEnd());
                if (jsonData["errors"].Count > 0)
                {
                    Console.WriteLine(jsonData["errors"]);
                }
                else
                {
                    Console.WriteLine($"DNS Settings For '{recordData["recordName"]}' Have Been Updated");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            
        }


    }
}
