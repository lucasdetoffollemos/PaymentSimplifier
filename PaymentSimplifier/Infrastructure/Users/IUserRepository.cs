using PaymentSimplifier.Domain.Users;

namespace PaymentSimplifier.Infrastructure.Users
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid userId);

        Task<bool> ExistsByDocumentAsync(string document);

        Task<bool> ExistsByEmailAsync(string email);

        Task AddAsync(User user);

        Task SaveChangesAsync();
    }
}
