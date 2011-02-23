using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Net.Http;
using PrePrompt.Samples.Common;

namespace PrePrompt.Samples.Client.Authentication
{
    public class BasicAuthenticationChannel : AuthenticationChannel
    {
        private static readonly ConcurrentDictionary<Uri, string> _cache = new ConcurrentDictionary<Uri, string>();

        private readonly Func<string, Tuple<string, string>> _getCredentials;
        private readonly HttpMessageChannel _channel;

        public BasicAuthenticationChannel(HttpMessageChannel inner, Func<string, Tuple<string, string>> getCredentials, HttpMessageChannel channel)
            : base(inner, "Basic")
        {
            _getCredentials = getCredentials;
            _channel = channel;
        }

        protected override HttpResponseMessage TryAuthenticate(AuthenticationHeaderValue authHeaderValue,
                                                               HttpResponseMessage response,
                                                               HttpRequestMessage issuedRequest)
        {
            var credentials = _getCredentials(authHeaderValue.Parameter);
            if (credentials == null)
            {
                return response;
            }

            var request = issuedRequest.Clone();
            var auth = "{0}:{1}".FormatWith(credentials.Item1, credentials.Item2).ToBase64();
            request.With().Authorization(new AuthenticationHeaderValue(Scheme, auth));

            var newResponse = new HttpClient { Channel = _channel }.Send(request);

            if (newResponse.IsSuccessStatusCode)
            {
                _cache.TryAdd(request.RequestUri, auth);
                return newResponse;
            }

            return response;
        }

        protected override HttpRequestMessage TryAuthenticate(HttpRequestMessage request)
        {
            string auth;
            if (_cache.TryGetValue(request.RequestUri, out auth))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue(Scheme, auth);
            }
            return request;
        }
    }
}