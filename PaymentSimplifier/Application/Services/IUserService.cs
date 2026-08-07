using PaymentSimplifier.Dtos;

namespace PaymentSimplifier.Application.Services
{
    public interface IUserService
    {
        Task<UserDepositResponse> DepositInUserAccountAsync(Guid userId, decimal amount);
    }
}
