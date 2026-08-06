using Microsoft.EntityFrameworkCore;
using PaymentSimplifier.Domain.Users;

namespace PaymentSimplifier.Infrastructure
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Cpf).IsRequired().HasMaxLength(11);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Password).IsRequired().HasMaxLength(200);
                entity.Property(e => e.UserType).IsRequired();
            });


            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = Guid.NewGuid(),
                    Name = "Lojista1",
                    Cpf = "12345678",
                    Email = "lojista@gmail.com",
                    Password = "lojista1",
                    UserType = UserType.Merchant
                }
            );


            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = Guid.NewGuid(),
                    Name = "User1",
                    Cpf = "12345679",
                    Email = "user@gmail.com",
                    Password = "user1",
                    UserType = UserType.Commom
                }
            );

            
        }
    }
}
