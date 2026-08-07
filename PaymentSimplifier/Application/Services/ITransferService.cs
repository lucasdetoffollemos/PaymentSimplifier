namespace PaymentSimplifier.Application.Services
{
    public interface ITransferService
    {
        Task<bool> TransferAsync(Guid payerId, Guid payeeId, decimal value);

    }
}
