using Microsoft.EntityFrameworkCore;
using PaymentSimplifier.Domain.Users;

namespace PaymentSimplifier.Infrastructure.Users
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _dbContext;

        public UserRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<User?> GetByIdAsync(Guid userId)
        {
            return await _dbContext.Users.FindAsync(userId);
        }

        public Task<List<User>> GetAllAsync()
        {
            return _dbContext.Users
                .OrderBy(user => user.Name)
                .ToListAsync();
        }

        public Task<bool> ExistsByDocumentAsync(string document)
        {
            return _dbContext.Users.AnyAsync(user => user.Document == document);
        }

        public Task<bool> ExistsByEmailAsync(string email)
        {
            return _dbContext.Users.AnyAsync(user => user.Email == email);
        }

        public async Task AddAsync(User user)
        {
            await _dbContext.Users.AddAsync(user);
        }

        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}
