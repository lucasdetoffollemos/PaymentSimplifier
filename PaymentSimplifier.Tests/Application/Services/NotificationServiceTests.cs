using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using PaymentSimplifier.Application.Services;
using PaymentSimplifier.Tests.Common;
using System.Net;
using Xunit;

namespace PaymentSimplifier.Tests.Application.Services
{
    public class NotificationServiceTests
    {
        [Fact]
        public async Task SendNotificationToPayeeAsync_ShouldReturnTrue_WhenRequestSucceeds()
        {
            var handler = new HttpClientTestHelper.CapturingHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK));
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
            var service = CreateService(new HttpClient(new HttpClientTestHelper.StubHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.BadRequest))));

            var result = await service.SendNotificationToPayeeAsync(Guid.NewGuid(), 10m);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task SendNotificationToPayeeAsync_ShouldReturnFalse_WhenRequestThrowsException()
        {
            var service = CreateService(new HttpClient(new HttpClientTestHelper.ThrowingHttpMessageHandler()));

            var result = await service.SendNotificationToPayeeAsync(Guid.NewGuid(), 10m);

            result.Should().BeFalse();
        }

        private static NotificationService CreateService(HttpClient httpClient)
        {
            var logger = new Mock<ILogger<NotificationService>>();

            return new NotificationService(logger.Object, new HttpClientTestHelper.StubHttpClientFactory(httpClient));
        }
    }
}
