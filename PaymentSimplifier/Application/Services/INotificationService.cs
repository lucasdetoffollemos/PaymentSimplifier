namespace PaymentSimplifier.Application.Services
{
    public interface INotificationService
    {
        Task<bool> SendNotificationToPayeeAsync(Guid payeeId, decimal value);
    }
}
