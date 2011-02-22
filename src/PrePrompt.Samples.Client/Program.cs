using System;
using System.Net;
using System.Net.Http;
using System.Xml.Linq;
using Microsoft.Net.Http;
using Microsoft.Xml.Linq;
using PrePrompt.Samples.Client.Twitter;
using PrePrompt.Samples.Common;

namespace PrePrompt.Samples.Client
{
    public class Program
    {
        private const string TWITTER = "http://api.twitter.com/";
        public static void Main()
        {
            var client = new HttpClient(TWITTER)
            {
                Channel = new HttpMessageInspector(new WebRequestChannel())
            };

            var request = new HttpRequestMessage(HttpMethod.Get, "1/users/show/{0}.xml".FormatWith("duarte_nunes"));
            request.With().Accept("application/xml");

            HttpResponseMessage response = client.Send(request);
            response.EnsureSuccessStatusCode();

            Console.WriteLine(response.Content.ReadAsString());

            XElement element = response.Content.ReadAsXElement();
            Console.WriteLine(element.Element("description").Value);

            var c = new JsonDataContractRestClient<TwitterUser>(TWITTER);
            Tuple<HttpStatusCode, TwitterUser> result = c.ExecuteGet("1/users/show/{0}.json".FormatWith("duarte_nunes"));
            Console.WriteLine(result.Item1);
            Console.WriteLine(result.Item2.Description);
            Console.WriteLine(result.Item2.ProfileImageUrl);
        }
    }
}