using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using PaymentSimplifier.Application.Services;
using System.Net;
using System.Text;
using Xunit;

namespace PaymentSimplifier.Tests.Application.Services
{
    public class NotificationServiceTests
    {
        [Fact]
        public async Task SendNotificationToPayeeAsync_ShouldReturnTrue_WhenRequestSucceeds()
        {
            var handler = new CapturingHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK));
            var service = CreateService(new HttpClient(handler));
            var payeeId = Guid.NewGuid();

            var result = await service.SendNotificationToPayeeAsync(payeeId, 10m);

            result.Should().BeTrue();
            handler.Request.Should().NotBeNull();
            handler.Request!.Method.Should().Be(HttpMethod.Post);
            handler.Request.RequestUri!.ToString().Should().Be("https://util.devi.tools/api/v1/notify");
            handler.Request.Content.Should().NotBeNull();
            (await handler.Request.Content!.ReadAsStringAsync()).Should().Be("{\"message\":\"Payment received successfully for amount 10\"}");
        }

        [Fact]
        public async Task SendNotificationToPayeeAsync_ShouldReturnFalse_WhenStatusCodeIsNotSuccessful()
        {
            var service = CreateService(new HttpClient(new StubHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.BadRequest))));

            var result = await service.SendNotificationToPayeeAsync(Guid.NewGuid(), 10m);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task SendNotificationToPayeeAsync_ShouldReturnFalse_WhenRequestThrowsException()
        {
            var service = CreateService(new HttpClient(new ThrowingHttpMessageHandler()));

            var result = await service.SendNotificationToPayeeAsync(Guid.NewGuid(), 10m);

            result.Should().BeFalse();
        }

        private static NotificationService CreateService(HttpClient httpClient)
        {
            var logger = new Mock<ILogger<NotificationService>>();

            return new NotificationService(logger.Object, new StubHttpClientFactory(httpClient));
        }

        private sealed class CapturingHttpMessageHandler : HttpMessageHandler
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

        private sealed class StubHttpMessageHandler : HttpMessageHandler
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

        private sealed class ThrowingHttpMessageHandler : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                throw new HttpRequestException("network error");
            }
        }

        private sealed class StubHttpClientFactory : IHttpClientFactory
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
