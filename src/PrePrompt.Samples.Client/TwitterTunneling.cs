using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Web;
using System.Threading.Tasks;
using Microsoft.ServiceModel.Description;
using Microsoft.ServiceModel.Http;
using PrePrompt.Samples.Client.Twitter;
using PrePrompt.Samples.Common;

namespace PrePrompt.Samples.Client
{
    public class TwitterTunneling
    {
        public static void Start()
        {
            var config = new HttpHostConfiguration();
            config.SetProcessorProvider(new AsyncProcessorProvider());
            using (var host = new WebHttpServiceHost(typeof(TheService), config, new Uri("Http://localhost:8080/twitter/")))
            {
                host.Open();
                Console.WriteLine("host is opened at {0}, press any key to continue", host.BaseAddresses[0]);
                Console.ReadLine();
            }
        }
    }

    [ServiceContract]
    public class TheService
    {
        [WebGet(UriTemplate = "{user}")]
        public Task GetTwitterUserMessage(HttpResponseMessage resp, string user)
        {
            var tcs = new TaskCompletionSource<object>();

            var formatter = new JsonDataContractFormatter(new DataContractJsonSerializer(typeof(TwitterUser)));
            new RestyClient<TwitterUser>(Urls.Twitter, new[] { formatter })
                .ExecuteGetAsync("1/users/show/{0}.json".FormatWith(user))
                .ContinueWith(t1 =>
                {
                    if (faulted(() => t1.Result.Item1, resp, tcs))
                    {
                        return;
                    }

                    new HttpClient().GetAsync(t1.Result.Item2.ProfileImageUrl)
                        .ContinueWith(t2 =>
                        {
                            if (faulted(() => t2.Result.StatusCode, resp, tcs))
                            {
                                return;
                            }

                            resp.Content = new StreamContent(t2.Result.Content.ContentReadStream);
                            resp.StatusCode = t2.Result.StatusCode;
                            tcs.SetResult(null);
                        });
                });
            return tcs.Task;
        }

        private static bool faulted(Func<HttpStatusCode> statusCodeFunc, HttpResponseMessage resp,
                                    TaskCompletionSource<object> tcs)
        {
            HttpStatusCode statusCode = HttpStatusCode.InternalServerError;

            try
            {
                statusCode = statusCodeFunc();
                if (statusCode == HttpStatusCode.OK)
                {
                    return false;
                }
            }
            catch
            { }

            resp.StatusCode = statusCode;
            resp.Content = new StringContent("Fail");
            tcs.SetResult(null);
            return true;
        }
    }

    public class AsyncProcessorProvider : IProcessorProvider
    {
        public void RegisterRequestProcessorsForOperation(HttpOperationDescription operation, IList<Processor> processors,
                                                          MediaTypeProcessorMode mode)
        { }

        public void RegisterResponseProcessorsForOperation(HttpOperationDescription operation, IList<Processor> processors,
                                                           MediaTypeProcessorMode mode)
        {
            operation.EnableAsync();
        }
    }
}