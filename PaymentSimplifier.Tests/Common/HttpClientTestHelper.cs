using System.Net;
using System.Text;

namespace PaymentSimplifier.Tests.Common
{
    internal static class HttpClientTestHelper
    {
        public static HttpResponseMessage CreateJsonResponse(HttpStatusCode statusCode, string content)
        {
            return new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(content, Encoding.UTF8, "application/json")
            };
        }

        public sealed class CapturingHttpMessageHandler : HttpMessageHandler
        {
            private readonly HttpResponseMessage _response;

            public CapturingHttpMessageHandler(HttpResponseMessage response)
            {
                _response = response;
            }

            public HttpRequestMessage? Request { get; private set; }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                Request = request;
                return Task.FromResult(_response);
            }
        }

        public sealed class StubHttpMessageHandler : HttpMessageHandler
        {
            private readonly HttpResponseMessage _response;

            public StubHttpMessageHandler(HttpResponseMessage response)
            {
                _response = response;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return Task.FromResult(_response);
            }
        }

        public sealed class ThrowingHttpMessageHandler : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                throw new HttpRequestException("network error");
            }
        }

        public sealed class StubHttpClientFactory : IHttpClientFactory
        {
            private readonly HttpClient _httpClient;

            public StubHttpClientFactory(HttpClient httpClient)
            {
                _httpClient = httpClient;
            }

            public HttpClient CreateClient(string name)
            {
                return _httpClient;
            }
        }
    }
}
