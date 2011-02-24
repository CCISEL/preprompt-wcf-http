using System;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Xml.Linq;
using Microsoft.Net.Http;
using Microsoft.Xml.Linq;
using PrePrompt.Samples.Client.Authentication;
using PrePrompt.Samples.Client.Twitter;
using PrePrompt.Samples.Common;

namespace PrePrompt.Samples.Client
{
    public class Program
    {
        public static void Main()
        {
            //twitterSample();
            //basicAuthenticationSample();
            TwitterTunneling.Start();
        }

        private static void twitterSample()
        {
            var client = new HttpClient(Urls.Twitter)
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

            var formatter = new JsonDataContractFormatter(new DataContractJsonSerializer(typeof(TwitterUser)));
            var c = new RestyClient<TwitterUser>(Urls.Twitter, new[] { formatter });
            Tuple<HttpStatusCode, TwitterUser> result = c.ExecuteGet("1/users/show/{0}.json".FormatWith("duarte_nunes"));
            Console.WriteLine(result.Item1);
            Console.WriteLine(result.Item2.Description);
            Console.WriteLine(result.Item2.ProfileImageUrl);
        }

        private static void basicAuthenticationSample()
        {
            var client = new HttpClient(Urls.Code)
            {
                Channel = new BasicAuthenticationChannel(new WebRequestChannel(), 
                                                         _ => Tuple.Create("test", "changeit"), 
                                                         new WebRequestChannel())
            };

            var response = client.Get("preprompt/wcf/banzai.txt");
            response.EnsureSuccessStatusCode();
            Console.WriteLine(response);
            Console.WriteLine(response.Content.ReadAsString());
        }
    }
}