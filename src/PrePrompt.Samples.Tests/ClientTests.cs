using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Net.Http;
using Moq;
using NUnit.Framework;
using PrePrompt.Samples.Client;

namespace PrePrompt.Samples.Tests
{
    [TestFixture]
    public class ClientTests
    {
        private static readonly Uri _url = new Uri("http://www.domain.com/");
        private static readonly MediaTypeHeaderValue _mediaType = new MediaTypeHeaderValue("application/something");

        public class TestChannel : HttpMessageChannel
        {
            protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return SendAsync(request, cancellationToken).Result;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var tcs = new TaskCompletionSource<HttpResponseMessage>();
                tcs.TrySetResult(MySend(request));
                return tcs.Task;
            }

            public virtual HttpResponseMessage MySend(HttpRequestMessage request)
            {
                var response = new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new ByteArrayContent(new byte[0])
                };
                response.Content.Headers.ContentType = _mediaType;
                return response;
            }
        }

        public static RestyClient<object> GetClient(TestChannel channel)
        {
            var formatter = new Mock<IContentFormatter>();
            formatter.Setup(f => f.SupportedMediaTypes).Returns(() => new[] { _mediaType });
            formatter.Setup(f => f.ReadFromStream(It.IsAny<Stream>())).Returns(() => new object());
            return new RestyClient<object>(_url.ToString(), new[] { formatter.Object }, client => client.Channel = channel);
        }

        [Test]
        public void ShouldIssueGetRequest()
        {
            const string relative = "get/";
            var channel = new Mock<TestChannel> { CallBase = true };
            Tuple<HttpStatusCode, object> result = GetClient(channel.Object).ExecuteGet(relative);
            
            Assert.IsNotNull(result.Item2);
            channel.Verify(c => c.MySend(It.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get
                                                                       && req.RequestUri == new Uri(_url, relative))),
                                Times.Once());
        }

        [Test]
        public void ShouldIssuePutRequest()
        {
            const string relative = "put/";
            var channel = new Mock<TestChannel> { CallBase = true };
            Tuple<HttpStatusCode, object> result = GetClient(channel.Object).ExecutePut(relative, new object());

            Assert.IsNotNull(result.Item2);
            channel.Verify(c => c.MySend(It.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Put
                                                                       && req.RequestUri == new Uri(_url, relative)
                                                                       && req.Content.Headers.ContentType == _mediaType)),
                                Times.Once());
        }

        [Test]
        public void ShouldReturnADefaultValueWhenTheResponseHasNoContent()
        {
            const string relative = "put/";
            var channel = new Mock<TestChannel> { CallBase = true };
            channel.Setup(c => c.MySend(It.IsAny<HttpRequestMessage>())).Returns(() =>
            {
                var response = new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NoContent,
                    Content = new ByteArrayContent(new byte[0])
                };
                response.Content.Headers.ContentType = _mediaType;
                return response;
            });
            Tuple<HttpStatusCode, object> result = GetClient(channel.Object).ExecutePut(relative, new object());

            Assert.IsNull(result.Item2);
        }
    }
}