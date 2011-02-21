
using System.Net;
using System.Net.Http;
using System.ServiceModel;
using System.ServiceModel.Web;
using PrePrompt.Samples.Common;

namespace PrePrompt.Samples.Linking
{
    [ServiceContract]
    public class TheService
    {
        [WebGet(UriTemplate = "test/")]
        public string Get(HttpResponseMessage resp, WebLinkCollection linkCollection)
        {
            linkCollection.Links.Add(new WebLinkTarget(null, "http://someuri", "next"));
            resp.StatusCode = HttpStatusCode.OK;
            return "done";
        }
    }
}