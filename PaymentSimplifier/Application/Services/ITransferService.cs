namespace PaymentSimplifier.Application.Services
{
    public interface ITransferService
    {
        Task<(bool canTransfer, bool canNotify)> TransferAsync(Guid payerId, Guid payeeId, decimal value, string password);

    }
}
