namespace PaymentSimplifier.Dtos
{
    public class DepositUserRequest
    {
        public decimal Amount { get; set; }

        public required string Password { get; set; }
    }
}
