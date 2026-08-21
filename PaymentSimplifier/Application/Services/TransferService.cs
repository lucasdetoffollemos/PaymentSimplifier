using PaymentSimplifier.Domain.Transactions;
using PaymentSimplifier.Domain.Users;
using PaymentSimplifier.Infrastructure.Transactions;
using PaymentSimplifier.Infrastructure.Users;

namespace PaymentSimplifier.Application.Services
{
    public class TransferService : ITransferService
    {
        private readonly IUserRepository _userRepository;

        private readonly ITransferRepository _transferRepository;

        private readonly ILogger<TransferService> _logger;

        private readonly INotificationService _notificationService;

        private readonly ITransferAuthorizationService _transferAuthorizationService;

        public TransferService(IUserRepository userRepository, ITransferRepository transferRepository, ILogger<TransferService> logger, INotificationService notificationService, ITransferAuthorizationService transferAuthorizationService)
        {
            _userRepository = userRepository;
            _transferRepository = transferRepository;
            _logger = logger;
            _notificationService = notificationService;
            _transferAuthorizationService = transferAuthorizationService;
        }

        public async Task<(bool canTransfer, bool canNotify)> TransferAsync(Guid payerId, Guid payeeId, decimal value, string password)
        {
            if (value <= 0)
            {
                throw new ArgumentException("Transfer value must be greater than zero");
            }

            //validate id payerid and payee id are not the same

            if (payerId == payeeId)
            {
                throw new ArgumentException("Payer and payee cannot be the same user");
            }

            //validate if payer and payee exist in the database

            var payer = await _userRepository.GetByIdAsync(payerId);
            var payee = await _userRepository.GetByIdAsync(payeeId);

            if (payer == null)
            {
                throw new ArgumentException("Payer not found");
            }

            if (payee == null)
            {
                throw new ArgumentException("Payee not found");
            }

            if (string.IsNullOrWhiteSpace(password) || payer.Password != password.Trim())
            {
                throw new ArgumentException("Invalid password");
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

           /* if (!await _transferAuthorizationService.IsTransferAuthorizedAsync())
            {
                return (false, false);
            }*/

            //proceed with the transaction
            await CreateTransaction(payerId, payeeId, value);

            payer.DiscountBalance(value);
            payee.AddBalance(value);

            await _transferRepository.SaveChangesAsync();

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
                await _transferRepository.AddAsync(transaction);
            }
            catch(Exception ex)
            {
                _logger.LogError($"Failed to create transaction from payer {payerId} to payee {payeeId} for amount {value}. Exception: {ex.Message}");
                throw new InvalidOperationException("Failed to create transaction", ex);
            }

        }
    }
}
