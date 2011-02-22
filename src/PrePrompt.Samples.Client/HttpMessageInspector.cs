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
            Console.WriteLine("Sending message to {0}", request.RequestUri);
            return request;
        }

        protected override HttpResponseMessage ProcessResponse(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            Console.WriteLine("Received message from {0} with status {1}", response.RequestMessage.RequestUri, response.StatusCode);
            return response;
        }
    }
}