using PaymentSimplifier.Domain.Users;

namespace PaymentSimplifier.Dtos
{
    public class CreateUserRequest
    {
        public required string Name { get; set; }

        public required string Document { get; set; }

        public required string Email { get; set; }

        public required string Password { get; set; }

        public required UserType UserType { get; set; }
    }
}
