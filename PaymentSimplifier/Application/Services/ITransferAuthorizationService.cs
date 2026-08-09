namespace PaymentSimplifier.Application.Services
{
    public interface ITransferAuthorizationService
    {
        Task<bool> IsTransferAuthorizedAsync();
    }
}
