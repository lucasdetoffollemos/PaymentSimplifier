using PaymentSimplifier.Domain.Transactions;
using PaymentSimplifier.Domain.Users;
using PaymentSimplifier.Dtos;
using PaymentSimplifier.Infrastructure;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

namespace PaymentSimplifier.Application.Services
{
    public class TransferService : ITransferService
    {
        private readonly AppDbContext _appDbContext;

        private readonly ILogger<TransferService> _logger;

        private readonly INotificationService _notificationService;

        private readonly IHttpClientFactory _httpClientFactory;
        public TransferService(AppDbContext appDbContext, ILogger<TransferService> logger, INotificationService notificationService, IHttpClientFactory httpClientFactory)
        {
            _appDbContext = appDbContext;
            _logger = logger;
            _notificationService = notificationService;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<(bool canTransfer, bool canNotify)> TransferAsync(Guid payerId, Guid payeeId, decimal value)
        {
            //validate id payerid and payee id are not the same

            if (payerId == payeeId)
            {
                throw new ArgumentException("Payer and payee cannot be the same user");
            }

            //validate if payer and payee exist in the database

            var payer = await _appDbContext.Users.FindAsync(payerId);
            var payee = await _appDbContext.Users.FindAsync(payeeId);

            if (payer == null)
            {
                throw new ArgumentException("Payer not found");
            }

            if (payee == null)
            {
                throw new ArgumentException("Payee not found");
            }

            //only users with userType "common" can transfer money

            if (payer.UserType != UserType.Commom)
            {
                throw new InvalidOperationException("Only common users can transfer money");
            }

            //validate if payer has enough balance to transfer the value

            if (payer.Balance < value)
            {
                throw new InvalidOperationException("Insufficient balance");
            }

            //before confim transaction, check this url https://util.devi.tools/api/v2/authorize, if the response is false, return false and do not proceed with the transaction

            if (!await CheckAuthorizeTransaction())
            {
                return (false, false);
            }

            //proceed with the transaction
            await CreateTransaction(payerId, payeeId, value);

            payer.DiscountBalance(value);
            payee.AddBalance(value);

            await _appDbContext.SaveChangesAsync();

            if (!await _notificationService.SendNotificationToPayeeAsync(payeeId, value))
            {
                return (true, false);
            }

            return (true, true);
        }

        private async Task CreateTransaction(Guid payerId, Guid payeeId, decimal value)
        {
            var transaction = new Transaction
            {
                Id = Guid.NewGuid(),
                PayerId = payerId,
                PayeeId = payeeId,
                Value = value,
                CreatedAt = DateTime.UtcNow
            };

            try
            {
                await _appDbContext.Transactions.AddAsync(transaction);
            }
            catch(Exception ex)
            {
                _logger.LogError($"Failed to create transaction from payer {payerId} to payee {payeeId} for amount {value}. Exception: {ex.Message}");
                throw new InvalidOperationException("Failed to create transaction", ex);
            }

        }

        private async Task<bool> CheckAuthorizeTransaction()
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var response = await httpClient.GetAsync("https://util.devi.tools/api/v2/authorize");
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var resultParsed = JsonSerializer.Deserialize<AuthorizeResponse>(responseContent);
                    if (resultParsed == null || resultParsed.Data == null)
                        return false;
                    return resultParsed.Data.Authorization;
                }
                return false;

            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to check authorization for transaction", ex);
            }
        }
    }
}
