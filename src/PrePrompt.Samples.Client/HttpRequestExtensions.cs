using System;
using System.Net.Http;
using Microsoft.Net.Http;

namespace PrePrompt.Samples.Client
{
    public static class HttpRequestExtensions
    {
        public static HttpRequestMessage Configure(this HttpRequestMessage message, Action<HttpRequestBuilder> configurer)
        {
            configurer(new HttpRequestBuilder(message));
            return message;
        }
    }
}