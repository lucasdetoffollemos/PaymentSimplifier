using System.Text;
using System.Text.Json;

namespace PaymentSimplifier.Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ILogger<NotificationService> _logger;

        private readonly IHttpClientFactory _httpClientFactory;

        public NotificationService(ILogger<NotificationService> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<bool> SendNotificationToPayeeAsync(Guid payeeId, decimal value)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient();

                var response = await httpClient.PostAsync("https://util.devi.tools/api/v1/notify", new StringContent(JsonSerializer.Serialize(new { message = $"Payment received successfully for amount {value}" }), Encoding.UTF8, "application/json"));

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Failed to send notification to payee {payeeId}. Status code: {response.StatusCode}");
                    return false;
                }

                _logger.LogInformation($"Notification sent to payee {payeeId} for amount {value}");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception occurred while sending notification to payee {payeeId}: {ex.Message}");
                return false;
            }
        }
    }
}
