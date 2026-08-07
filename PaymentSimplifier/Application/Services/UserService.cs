using PaymentSimplifier.Dtos;
using PaymentSimplifier.Infrastructure;

namespace PaymentSimplifier.Application.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _dbContext;

        public UserService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<UserDepositResponse> DepositInUserAccountAsync(Guid userId, decimal amount)
        {
            var user = await _dbContext.Users.FindAsync(userId);

            if (user == null)
                throw new ArgumentException("User not found");

            if(amount <= 0)
                throw new ArgumentException("Invalid deposit amount");

            user.Balance += amount;

            await _dbContext.SaveChangesAsync();

            return new UserDepositResponse
            {
                Name = user.Name,
                Cpf = user.Cpf,
                UserType = user.UserType,
                Balance = user.Balance
            };             
        }
    }
}
