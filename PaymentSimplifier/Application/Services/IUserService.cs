using PaymentSimplifier.Dtos;

namespace PaymentSimplifier.Application.Services
{
    public interface IUserService
    {
        Task<CreateUserResponse> CreateUserAsync(CreateUserRequest request);

        Task<UserDepositResponse> DepositInUserAccountAsync(Guid userId, decimal amount);
    }
}
