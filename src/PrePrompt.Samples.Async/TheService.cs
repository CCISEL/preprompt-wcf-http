using System.Net;
using System.Net.Http;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Threading.Tasks;

namespace PrePrompt.Samples.Async
{
    [ServiceContract]
    public class TheService
    {
        [WebGet(UriTemplate = "test/")]
        public async Task<string> Get(HttpResponseMessage resp)
        {
            await TaskEx.Delay(10000);
            resp.StatusCode = HttpStatusCode.OK;
            return "done";
        }
    }
}
