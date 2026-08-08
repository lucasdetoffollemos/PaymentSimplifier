using Microsoft.EntityFrameworkCore;
using PaymentSimplifier.Domain.Transactions;
using PaymentSimplifier.Domain.Users;
using System.Reflection.Metadata;
using System.Transactions;
using Transaction = PaymentSimplifier.Domain.Transactions.Transaction;

namespace PaymentSimplifier.Infrastructure
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Document).IsRequired().HasMaxLength(14);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Password).IsRequired().HasMaxLength(200);
                entity.Property(e => e.UserType).IsRequired();
                entity.Property(e => e.Balance).HasColumnType("decimal(18,2)").HasDefaultValue(0);
                entity.HasIndex(e => e.Document).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
            });

            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.PayerId).IsRequired();
                entity.Property(e => e.PayeeId).IsRequired();
                entity.Property(e => e.Value).HasColumnType("decimal(18,2)").IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();
            });


            //doing FK´s payer and payee
            modelBuilder.Entity<Transaction>()
                .HasOne(e => e.Payer)
                .WithMany(e => e.TransactionsForPayer)
                .HasForeignKey(e => e.PayerId);

            modelBuilder.Entity<Transaction>()
                .HasOne(e => e.Payee)
                .WithMany(e => e.TransactionsForPayee)
                .HasForeignKey(e => e.PayeeId);

            modelBuilder.Entity<User>().HasData(
               new User
               {
                   Id = Guid.Parse("019fd90d-e427-74cd-aaf7-a6464f779375"),
                   Name = "Lojista1",
                   Document = "11222333000181",
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
                    Document = "52998224725",
                    Email = "user@gmail.com",
                    Password = "user1",
                    UserType = UserType.Commom
                }
            );
        }
    }
}
