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
                entity.Property(e => e.Balance).HasColumnType("decimal(18,2)").HasDefaultValue(0);
            });



            modelBuilder.Entity<User>().HasData(
               new User
               {
                   Id = Guid.Parse("019fd90d-e427-74cd-aaf7-a6464f779375"),
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
                    Id = Guid.Parse("019fd90d-97c1-720c-812b-f502f65f600d"),
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
