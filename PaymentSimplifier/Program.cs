
using Microsoft.EntityFrameworkCore;
using PaymentSimplifier.Application.Services;
using PaymentSimplifier.Infrastructure;
using PaymentSimplifier.Infrastructure.Transactions;
using PaymentSimplifier.Infrastructure.Users;

namespace PaymentSimplifier
{
    public class Program
    {
        private const string VueDevelopmentCorsPolicy = "VueDevelopmentCorsPolicy";

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddHttpClient();
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<ITransferRepository, TransferRepository>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<ITransferService, TransferService>();
            builder.Services.AddScoped<ITransferAuthorizationService, TransferAuthorizationService>();
            builder.Services.AddScoped<INotificationService, NotificationService>();    

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres"))
                .UseSnakeCaseNamingConvention());

            builder.Services.AddControllers();
            builder.Services.AddCors(options =>
            {
                options.AddPolicy(VueDevelopmentCorsPolicy, policy =>
                {
                    policy.WithOrigins("http://localhost:5173")
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                dbContext.Database.Migrate();
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }
            app.UseCors(VueDevelopmentCorsPolicy);

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
