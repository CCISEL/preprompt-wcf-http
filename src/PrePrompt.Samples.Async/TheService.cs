using System.Net;
using System.Net.Http;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Threading;
using System.Threading.Tasks;

namespace PrePrompt.Samples.Async
{
    [ServiceContract]
    public class TheService
    {
        [WebGet(UriTemplate = "test/")]
        public Task<string> Get(HttpResponseMessage resp)
        {
            var tcs = new TaskCompletionSource<string>();
            new Timer(t =>
            {
                resp.StatusCode = HttpStatusCode.OK;
                tcs.SetResult("done");
                ((Timer) t).Dispose();
            }).Change(10000, Timeout.Infinite);
            return tcs.Task;
        }
    }
}
