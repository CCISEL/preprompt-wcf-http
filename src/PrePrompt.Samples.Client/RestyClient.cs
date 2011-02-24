using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Net.Http;
using PrePrompt.Samples.Common;

namespace PrePrompt.Samples.Client
{
    public class RestyClient<T>
    {
        private readonly string _serviceUrl;
        private readonly IEnumerable<IContentFormatter> _formatters;
        private readonly string[] _mediaTypes;
        private readonly Action<HttpClient> _configClient;

        public RestyClient(string serviceUrl, IEnumerable<IContentFormatter> formatters, Action<HttpClient> configClient = null)
        {
            if (serviceUrl.IsNullOrEmpty())
            {
                throw new ArgumentNullException("serviceUrl");
            }

            if (formatters.Any() == false)
            {
                throw new ArgumentOutOfRangeException("formatters");
            }

            _serviceUrl = serviceUrl;
            _formatters = formatters;
            _mediaTypes = _formatters.SelectMany(formatter => formatter.SupportedMediaTypes).Select(m => m.MediaType).ToArray();
            _configClient = configClient;
        }

        public Task<Tuple<HttpStatusCode, T>> ExecuteGetAsync(string requestUri, 
                                                              CancellationToken token = default(CancellationToken))
        {
            return SendAsync(createRequestMessage(requestUri, HttpMethod.Get), token);
        }

        public Tuple<HttpStatusCode, T> ExecuteGet(string requestUri, 
                                                   CancellationToken token = default(CancellationToken))
        {
            return ExecuteGetAsync(requestUri, token).Result;
        }

        public Task<Tuple<HttpStatusCode, T>> ExecutePutAsync(string requestUri, T entity,
                                                              CancellationToken token = default(CancellationToken))
        {
            return SendAsync(createRequestMessage(requestUri, HttpMethod.Put, entity),
                             token);
        }

        public Tuple<HttpStatusCode, T> ExecutePut(string requestUri, T entity, 
                                                   CancellationToken token = default(CancellationToken))
        {
            return ExecutePutAsync(requestUri, entity, token).Result;
        }

        public Task<Tuple<HttpStatusCode, T>> ExecutePostAsync(string requestUri, T entity,
                                                               CancellationToken token = default(CancellationToken))
        {
            return SendAsync(createRequestMessage(requestUri, HttpMethod.Post, entity),
                           token);
        }

        public Tuple<HttpStatusCode, T> ExecutePost(string requestUri, T entity, 
                                                    CancellationToken token = default(CancellationToken))
        {
            return ExecutePostAsync(requestUri, entity, token).Result;
        }

        public Task<Tuple<HttpStatusCode, T>> ExecuteDeleteAsync(string requestUri, 
                                                                 CancellationToken token = default(CancellationToken))
        {
            return SendAsync(createRequestMessage(requestUri, HttpMethod.Delete), token);
        }

        public Tuple<HttpStatusCode, T> ExecuteDelete(string requestUri, 
                                                      CancellationToken token = default(CancellationToken))
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
                                       ? readEntity(response)
                                       : default(T);
                    return Tuple.Create(response.StatusCode, representation);
                }
            });
        }

        public Tuple<HttpStatusCode, T> Send(HttpRequestMessage message, CancellationToken token)
        {
            return SendAsync(message, token).Result;
        }

        private T readEntity(HttpResponseMessage response)
        {
            var content = response.Content;
            var formatter = _formatters.First(f => f.SupportedMediaTypes.Any(m => m.Equals(content.Headers.ContentType)));
            return (T)formatter.ReadFromStream(content.ContentReadStream);
        }

        private HttpRequestMessage createRequestMessage(string requestUri, HttpMethod method, Object entity = null)
        {
            return new HttpRequestMessage(method, requestUri).Configure(req =>
            {
                req.Accept(_mediaTypes);
                if (entity == null)
                {
                    return;
                }
                var ms = new MemoryStream();
                var formatter = _formatters.First();
                formatter.WriteToStream(entity, ms);
                req.Content(() =>
                {
                    var content = new StreamContent(ms);
                    content.Headers.ContentType = formatter.SupportedMediaTypes.First();
                    return content;
                });
            });
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