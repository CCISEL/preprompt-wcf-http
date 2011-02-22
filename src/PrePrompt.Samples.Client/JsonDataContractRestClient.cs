using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Runtime.Serialization.Json;
using PrePrompt.Samples.Common;

namespace PrePrompt.Samples.Client
{
    public class JsonDataContractRestClient<T>
    {
        private readonly DataContractJsonSerializer _serializer;
        private readonly string _serviceUrl;
        private readonly Action<HttpClient> _configClient;

        public JsonDataContractRestClient(string serviceUrl, Action<HttpClient> configClient = null)
        {
            if (serviceUrl.IsNullOrEmpty())
            {
                throw new ArgumentNullException("serviceUrl");
            }

            _serializer = new DataContractJsonSerializer(typeof(T));
            _serviceUrl = serviceUrl;
            _configClient = configClient;
        }

        public Task<Tuple<HttpStatusCode, T>> ExecuteGetAsync(string requestUri, CancellationToken token = default(CancellationToken))
        {
            return SendAsync(createRequestMessage(requestUri, HttpMethod.Get), token);
        }

        public Tuple<HttpStatusCode, T> ExecuteGet(string requestUri, CancellationToken token = default(CancellationToken))
        {
            return ExecuteGetAsync(requestUri, token).Result;
        }

        public Task<Tuple<HttpStatusCode, T>> ExecutePutAsync(string requestUri, T entity,
                                                              CancellationToken token = default(CancellationToken))
        {
            return SendAsync(createRequestMessage(requestUri, HttpMethod.Put, entity.ToContentUsingDataContractJsonSerializer(_serializer)),
                             token);
        }

        public Tuple<HttpStatusCode, T> ExecutePut(string requestUri, T entity, CancellationToken token = default(CancellationToken))
        {
            return ExecutePutAsync(requestUri, entity, token).Result;
        }

        public Task<Tuple<HttpStatusCode, T>> ExecutePostAsync(string requestUri, T entity,
                                                               CancellationToken token = default(CancellationToken))
        {
            return SendAsync(createRequestMessage(requestUri, HttpMethod.Post, entity.ToContentUsingDataContractJsonSerializer(_serializer)),
                           token);
        }

        public Tuple<HttpStatusCode, T> ExecutePost(string requestUri, T entity, CancellationToken token = default(CancellationToken))
        {
            return ExecutePostAsync(requestUri, entity, token).Result;
        }

        public Task<Tuple<HttpStatusCode, T>> ExecuteDeleteAsync(string requestUri, CancellationToken token = default(CancellationToken))
        {
            return SendAsync(createRequestMessage(requestUri, HttpMethod.Delete), token);
        }

        public Tuple<HttpStatusCode, T> ExecuteDelete(string requestUri, CancellationToken token = default(CancellationToken))
        {
            return ExecuteDeleteAsync(requestUri, token).Result;
        }

        public Task<Tuple<HttpStatusCode, T>> SendAsync(HttpRequestMessage message, CancellationToken token)
        {
            var client = createHttpClient(token);
            return client.SendAsync(message, token).ContinueWith(t =>
            {
                client.Dispose();

                if (t.IsFaulted)
                {
                    throw t.Exception.GetBaseException();
                }

                if (t.IsCanceled)
                {
                    throw new OperationCanceledException(token);
                }

                using (HttpResponseMessage response = t.Result)
                {
                    var representation = response.IsSuccessStatusCode && response.StatusCode != HttpStatusCode.NoContent
                                       ? response.Content.ReadAsJsonDataContract<T>(_serializer) 
                                       : default(T);
                    return Tuple.Create(response.StatusCode, representation);
                }
            });
        }

        public Tuple<HttpStatusCode, T> Send(HttpRequestMessage message, CancellationToken token)
        {
            return SendAsync(message, token).Result;
        }

        private static HttpRequestMessage createRequestMessage(string requestUri, HttpMethod method, HttpContent content = null)
        {
            return new HttpRequestMessage(method, requestUri).Configure(req => req
                .Accept(new MediaTypeWithQualityHeaderValue("application/json", 1.0))
                .Content(() => content));
        }

        private HttpClient createHttpClient(CancellationToken token)
        {
            var client = new HttpClient(_serviceUrl);
            if (_configClient != null)
            {
                _configClient(client);
            }
            token.Register(client.CancelPendingRequests);
            return client;
        }
    }
}