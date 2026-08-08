using PaymentSimplifier.Domain.Users;

namespace PaymentSimplifier.Domain.Transactions
{
    public class Transaction
    {
        public Guid Id { get; set; }

        public Guid PayerId { get; set; }

        public User? Payer { get; set; }

        public Guid PayeeId { get; set; }

        public User? Payee { get; set; }

        public decimal Value { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
