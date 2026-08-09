using FluentAssertions;
using PaymentSimplifier.Application.Services;
using PaymentSimplifier.Tests.Common;
using System.Net;
using Xunit;

namespace PaymentSimplifier.Tests.Application.Services
{
    public class TransferAuthorizationServiceTests
    {
        [Fact]
        public async Task IsTransferAuthorizedAsync_ShouldReturnTrue_WhenRequestIsAuthorized()
        {
            var handler = new HttpClientTestHelper.CapturingHttpMessageHandler(HttpClientTestHelper.CreateJsonResponse(HttpStatusCode.OK, "{\"status\":\"success\",\"data\":{\"authorization\":true}}"));
            var service = CreateService(new HttpClient(handler));

            var result = await service.IsTransferAuthorizedAsync();

            result.Should().BeTrue();
            handler.Request.Should().NotBeNull();
            handler.Request!.Method.Should().Be(HttpMethod.Get);
            handler.Request.RequestUri!.ToString().Should().Be("https://util.devi.tools/api/v2/authorize");
        }

        [Fact]
        public async Task IsTransferAuthorizedAsync_ShouldReturnFalse_WhenRequestIsDenied()
        {
            var service = CreateService(new HttpClient(new HttpClientTestHelper.StubHttpMessageHandler(HttpClientTestHelper.CreateJsonResponse(HttpStatusCode.OK, "{\"status\":\"success\",\"data\":{\"authorization\":false}}"))));

            var result = await service.IsTransferAuthorizedAsync();

            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsTransferAuthorizedAsync_ShouldReturnFalse_WhenStatusCodeIsNotSuccessful()
        {
            var service = CreateService(new HttpClient(new HttpClientTestHelper.StubHttpMessageHandler(HttpClientTestHelper.CreateJsonResponse(HttpStatusCode.BadRequest, "{}"))));

            var result = await service.IsTransferAuthorizedAsync();

            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsTransferAuthorizedAsync_ShouldReturnFalse_WhenPayloadIsInvalid()
        {
            var service = CreateService(new HttpClient(new HttpClientTestHelper.StubHttpMessageHandler(HttpClientTestHelper.CreateJsonResponse(HttpStatusCode.OK, "{\"status\":\"success\",\"data\":null}"))));

            var result = await service.IsTransferAuthorizedAsync();

            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsTransferAuthorizedAsync_ShouldThrow_WhenRequestThrowsException()
        {
            var service = CreateService(new HttpClient(new HttpClientTestHelper.ThrowingHttpMessageHandler()));

            Func<Task> act = async () => await service.IsTransferAuthorizedAsync();

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Failed to check authorization for transaction");
        }

        private static TransferAuthorizationService CreateService(HttpClient httpClient)
        {
            return new TransferAuthorizationService(new HttpClientTestHelper.StubHttpClientFactory(httpClient));
        }
    }
}
