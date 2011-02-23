using System;
using System.Net.Http;
using System.Threading;

namespace PrePrompt.Samples.Client
{
    public class HttpMessageInspector : MessageProcessingChannel
    {
        public HttpMessageInspector(HttpMessageChannel innerChannel)
            : base(innerChannel)
        { }

        protected override HttpRequestMessage ProcessRequest(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Console.WriteLine("Sending request {0}", request);
            return request;
        }

        protected override HttpResponseMessage ProcessResponse(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            Console.WriteLine("Received response {0}", response);
            return response;
        }
    }
}