using PaymentSimplifier.Domain.Transactions;

namespace PaymentSimplifier.Infrastructure.Transactions
{
    public interface ITransferRepository
    {
        Task AddAsync(Transaction transaction);

        Task SaveChangesAsync();
    }
}
