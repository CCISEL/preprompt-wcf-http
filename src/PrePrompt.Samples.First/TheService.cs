using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.ServiceModel;
using System.ServiceModel.Syndication;
using System.ServiceModel.Web;
using System.Text;
using System.Web;
using System.Xml;
using System.Xml.Linq;

using Microsoft.Xml.Linq;
using PrePrompt.Samples.Common;



namespace PrePrompt.Samples.First
{
    [ServiceContract]
    public class TheService
    {
        // Simple operation:
        // Note:
        //   - The request and response as parameters (no statics)
        //   - The explicit response definion (both content and status code)
        [WebGet(UriTemplate="time/")]
        public void GetTime(HttpRequestMessage req, HttpResponseMessage resp)
        {
            resp.StatusCode = HttpStatusCode.OK;
            resp.Content = new StringContent(DateTime.Now.ToLongTimeString());
        }


        // Parameters as URL segments (also works for query string values)
        // Note:
        //    - The explicit response definion (both content and status code - 200, 404 - )
        [WebGet(UriTemplate = "time/{zone}")]
        public void GetTimeForZone(HttpRequestMessage req, HttpResponseMessage resp, string zone)
        {
            try
            {
                var zoneInfo = TimeZoneInfo.FindSystemTimeZoneById(zone);
                resp.StatusCode = HttpStatusCode.OK;
                resp.Content = new StringContent(TimeZoneInfo.ConvertTime(DateTime.Now, zoneInfo).ToLongTimeString());
            }catch(TimeZoneNotFoundException e)
            {
                resp.StatusCode = HttpStatusCode.NotFound;
                resp.Content = new StringContent(e.Message);
            }
        }

        
        [WebGet(UriTemplate = "zones.txt")]
        public void GetZonesInText(HttpRequestMessage req, HttpResponseMessage resp)
        {
            var sb = new StringBuilder();
            foreach (var tz in TimeZoneInfo.GetSystemTimeZones())
            {
                sb.AppendLine(string.Format("{0}: {1}", tz.Id, tz.StandardName));
            }
            resp.StatusCode = HttpStatusCode.OK;
            resp.Content = new StringContent(sb.ToString());
        }

        // Changing the content type (application/xml)
        [WebGet(UriTemplate = "zones.xml")]
        public void GetZonesInXml(HttpRequestMessage req, HttpResponseMessage resp)
        {
            var sb = new StringBuilder();
            var xe = new XElement("zones",
                         TimeZoneInfo.GetSystemTimeZones().Select(
                             tz =>
                             new XElement("zone", new XAttribute("id", tz.Id), new XAttribute("name", tz.StandardName))));
            resp.StatusCode = HttpStatusCode.OK;
            resp.Content = xe.ToContent();
            resp.Content.Headers.ContentType = new MediaTypeHeaderValue("application/xml");
        }


        private static Uri GetUriForZone(string zone)
        {
            return new Uri(string.Format("http://localhost:8080/first/time/{0}", HttpUtility.UrlPathEncode(zone)));
        }

        // Changing the content type (application/atom+xml)
        [WebGet(UriTemplate = "zones.atom")]
        public void GetZonesInAtom(HttpRequestMessage req, HttpResponseMessage resp)
        {
            var sb = new StringBuilder();
            var sf = new SyndicationFeed("Time zones", "List of time zones", null,
                                         TimeZoneInfo.GetSystemTimeZones().Select(tz =>
                                                                                  new SyndicationItem(tz.Id,
                                                                                                      tz.StandardName,
                                                                                                      GetUriForZone(tz.Id))));
            var ms = new MemoryStream();
            var xw = XmlWriter.Create(ms);
            new Atom10FeedFormatter(sf).WriteTo(xw);
            xw.Flush();
            ms.Seek(0, SeekOrigin.Begin);
            resp.StatusCode = HttpStatusCode.OK;
            resp.Content = new StreamContent(ms);
            resp.Content.Headers.ContentType = new MediaTypeHeaderValue("application/atom+xml");
        }


        public class TimeZoneModel
        {
            public String Id {get; set;}
            public String Name { get; set;}
            public String Uri { get; set; }
            public TimeZoneModel(TimeZoneInfo tz)
            {
                Id = tz.Id;
                Name = tz.StandardName;
                Uri = GetUriForZone(tz.Id).ToString();
            }
            // XmlSerializer requires it
            public TimeZoneModel(){}
        }

        public class TimeZoneListModel
        {
            public TimeZoneModel[] Zones { get;  set; }
            public TimeZoneListModel(IEnumerable<TimeZoneInfo> tzs)
            {
                Zones = tzs.Select(tz => new TimeZoneModel(tz)).ToArray();
            }
            // XmlSerializer requires it
            public TimeZoneListModel(){}
        }


        // Using content negotiation
        [WebGet(UriTemplate = "zones/")]
        public TimeZoneListModel GetZones(HttpRequestMessage req, HttpResponseMessage resp)
        {
            return new TimeZoneListModel(TimeZoneInfo.GetSystemTimeZones());
        }

        // Using content negotiation
        [WebGet(UriTemplate = "time2/")]
        public string GetTime2(HttpRequestMessage req, HttpResponseMessage resp)
        {
            return DateTime.Now.ToLongTimeString();
        }


        // Using data annotation
        [WebGet(UriTemplate = "da/{str}")]
        public string GetUpperString(
            DataValidationResult isValid,
            [StringLength(4)]
            string str)
        {
            var res = string.Format("Valid: {0}, Value: {1}", isValid.Valid, str.ToUpper());
            Console.WriteLine(res);
            return res;
        }
    }
}
