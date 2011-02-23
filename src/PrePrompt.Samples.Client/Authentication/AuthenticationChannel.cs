using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;

namespace PrePrompt.Samples.Client.Authentication
{
    public abstract class AuthenticationChannel : MessageProcessingChannel
    {
        protected AuthenticationChannel(HttpMessageChannel inner, string scheme)
            : base(inner)
        {
            Scheme = scheme;
        }

        public string Scheme { get; private set; }

        protected override HttpRequestMessage ProcessRequest(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return request.Headers.Authorization == null ? TryAuthenticate(request) : request;
        }

        protected override HttpResponseMessage ProcessResponse(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            if (response.StatusCode != HttpStatusCode.Unauthorized)
            {
                return response;
            }
            
            return response.Headers.WwwAuthenticate
                       .Select(auth => TryAuthenticate(auth, response, response.RequestMessage))
                       .FirstOrDefault(r => r.StatusCode != HttpStatusCode.Unauthorized) ?? response;
        }

        protected abstract HttpResponseMessage TryAuthenticate(AuthenticationHeaderValue authHeaderValue, 
                                                               HttpResponseMessage response,
                                                               HttpRequestMessage issuedRequest);

        protected abstract HttpRequestMessage TryAuthenticate(HttpRequestMessage request);
    }
}