namespace PaymentSimplifier.Dtos
{
    public class TransferRequest
    {
        public required Guid PayerId { get; set; }

        public required Guid PayeeId { get; set; }

        public required decimal Value { get; set; }
    }
}
