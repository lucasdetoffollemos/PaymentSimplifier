using PaymentSimplifier.Domain.Users;

namespace PaymentSimplifier.Dtos
{
    public class UserDepositResponse
    {
        public required string Name { get; set; }

        public required string Cpf { get; set; }

        public required UserType UserType { get; set; }

        public decimal Balance { get; set; } = 0;
    }
}
