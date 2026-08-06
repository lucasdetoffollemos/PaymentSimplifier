namespace PaymentSimplifier.Domain.Users
{
    public class User
    {
        public Guid Id { get; set; }

        public required string Name { get; set; }

        public required string Cpf { get; set; }

        public required string Email { get; set; }

        public required string Password { get; set; }

        public required UserType UserType { get; set; }

    }
}
