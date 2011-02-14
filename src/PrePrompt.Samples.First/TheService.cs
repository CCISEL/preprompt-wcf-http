using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Principal;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace PrePrompt.Samples.First
{
    [ServiceContract]
    class TheService
    {
        [WebGet(UriTemplate="hello/")]
        public void Get(HttpRequestMessage req, HttpResponseMessage resp)
        {
            resp.StatusCode = HttpStatusCode.OK;
            resp.Content = new StringContent("Hello to you too");
        }
    }
}
