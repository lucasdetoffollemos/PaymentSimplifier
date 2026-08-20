using PaymentSimplifier.Domain.Users;

namespace PaymentSimplifier.Dtos
{
    public class UserResponse
    {
        public required Guid Id { get; set; }

        public required string Name { get; set; }

        public required string Document { get; set; }

        public required string Email { get; set; }

        public required UserType UserType { get; set; }

        public decimal Balance { get; set; }
    }
}
