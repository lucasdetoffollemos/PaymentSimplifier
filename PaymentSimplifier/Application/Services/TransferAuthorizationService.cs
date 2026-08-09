using PaymentSimplifier.Dtos;
using System.Text.Json;

namespace PaymentSimplifier.Application.Services
{
    public class TransferAuthorizationService : ITransferAuthorizationService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public TransferAuthorizationService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<bool> IsTransferAuthorizedAsync()
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var response = await httpClient.GetAsync("https://util.devi.tools/api/v2/authorize");

                if (!response.IsSuccessStatusCode)
                {
                    return false;
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var resultParsed = JsonSerializer.Deserialize<AuthorizeResponse>(responseContent);

                if (resultParsed == null || resultParsed.Data == null)
                {
                    return false;
                }

                return resultParsed.Data.Authorization;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to check authorization for transaction", ex);
            }
        }
    }
}
