using PaymentSimplifier.Dtos;

namespace PaymentSimplifier.Application.Services
{
    public interface IUserService
    {
        Task<CreateUserResponse> CreateUserAsync(CreateUserRequest request);

        Task<List<UserResponse>> GetUsersAsync();

        Task<UserDepositResponse> DepositInUserAccountAsync(Guid userId, DepositUserRequest request);
    }
}
