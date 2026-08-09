using PaymentSimplifier.Domain.Transactions;

namespace PaymentSimplifier.Infrastructure.Transactions
{
    public class TransferRepository : ITransferRepository
    {
        private readonly AppDbContext _dbContext;

        public TransferRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(Transaction transaction)
        {
            await _dbContext.Transactions.AddAsync(transaction);
        }

        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}
