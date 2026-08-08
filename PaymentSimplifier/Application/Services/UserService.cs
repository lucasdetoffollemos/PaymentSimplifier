using PaymentSimplifier.Dtos;
using PaymentSimplifier.Domain.Users;
using PaymentSimplifier.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;

namespace PaymentSimplifier.Application.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _dbContext;

        public UserService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<CreateUserResponse> CreateUserAsync(CreateUserRequest request)
        {
            ValidateCreateUserRequest(request);

            var normalizedName = request.Name.Trim();
            var normalizedDocument = NormalizeDocument(request.Document);
            var normalizedEmail = NormalizeEmail(request.Email);
            var normalizedPassword = request.Password.Trim();

            ValidateDocumentByUserType(normalizedDocument, request.UserType);

            if (await _dbContext.Users.AnyAsync(user => user.Document == normalizedDocument))
                throw new InvalidOperationException("Document already registered");

            if (await _dbContext.Users.AnyAsync(user => user.Email == normalizedEmail))
                throw new InvalidOperationException("Email already registered");

            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = normalizedName,
                Document = normalizedDocument,
                Email = normalizedEmail,
                Password = normalizedPassword,
                UserType = request.UserType
            };

            try
            {
                await _dbContext.Users.AddAsync(user);
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("User could not be created because the document or email is already registered", ex);
            }

            return new CreateUserResponse
            {
                Id = user.Id,
                Name = user.Name,
                Document = user.Document,
                Email = user.Email,
                UserType = user.UserType,
                Balance = user.Balance
            };
        }

        public async Task<UserDepositResponse> DepositInUserAccountAsync(Guid userId, decimal amount)
        {
            var user = await _dbContext.Users.FindAsync(userId);

            if (user == null)
                throw new ArgumentException("User not found");

            if(amount <= 0)
                throw new ArgumentException("Invalid deposit amount");

            user.AddBalance(amount);

            await _dbContext.SaveChangesAsync();

            return new UserDepositResponse
            {
                Name = user.Name,
                Document = user.Document,
                UserType = user.UserType,
                Balance = user.Balance
            };             
        }

        private static void ValidateCreateUserRequest(CreateUserRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("Name is required");

            if (string.IsNullOrWhiteSpace(request.Document))
                throw new ArgumentException("Document is required");

            if (string.IsNullOrWhiteSpace(request.Email))
                throw new ArgumentException("Email is required");

            if (string.IsNullOrWhiteSpace(request.Password))
                throw new ArgumentException("Password is required");

            if (!Enum.IsDefined(request.UserType))
                throw new ArgumentException("Invalid user type");

            try
            {
                _ = new MailAddress(request.Email.Trim());
            }
            catch (FormatException ex)
            {
                throw new ArgumentException("Invalid email", ex);
            }
        }

        private static string NormalizeDocument(string document)
        {
            return new string(document.Where(char.IsDigit).ToArray());
        }

        private static string NormalizeEmail(string email)
        {
            return email.Trim().ToLowerInvariant();
        }

        private static void ValidateDocumentByUserType(string document, UserType userType)
        {
            if (userType == UserType.Commom)
            {
                if (!IsValidCpf(document))
                    throw new ArgumentException("Invalid CPF for common user");

                return;
            }

            if (userType == UserType.Merchant)
            {
                if (!IsValidCnpj(document))
                    throw new ArgumentException("Invalid CNPJ for merchant user");

                return;
            }

            throw new ArgumentException("Invalid user type");
        }

        private static bool IsValidCpf(string cpf)
        {
            if (cpf.Length != 11 || cpf.Distinct().Count() == 1)
                return false;

            var numbers = cpf.Select(c => c - '0').ToArray();

            var firstDigit = CalculateCheckDigit(numbers, 9, 10);
            var secondDigit = CalculateCheckDigit(numbers, 10, 11);

            return numbers[9] == firstDigit && numbers[10] == secondDigit;
        }

        private static bool IsValidCnpj(string cnpj)
        {
            if (cnpj.Length != 14 || cnpj.Distinct().Count() == 1)
                return false;

            var numbers = cnpj.Select(c => c - '0').ToArray();
            var firstWeights = new[] { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            var secondWeights = new[] { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

            var firstDigit = CalculateWeightedCheckDigit(numbers, firstWeights);
            var secondDigit = CalculateWeightedCheckDigit(numbers, secondWeights);

            return numbers[12] == firstDigit && numbers[13] == secondDigit;
        }

        private static int CalculateCheckDigit(int[] numbers, int length, int weightStart)
        {
            var sum = 0;

            for (var index = 0; index < length; index++)
            {
                sum += numbers[index] * (weightStart - index);
            }

            var remainder = sum % 11;
            return remainder < 2 ? 0 : 11 - remainder;
        }

        private static int CalculateWeightedCheckDigit(int[] numbers, int[] weights)
        {
            var sum = 0;

            for (var index = 0; index < weights.Length; index++)
            {
                sum += numbers[index] * weights[index];
            }

            var remainder = sum % 11;
            return remainder < 2 ? 0 : 11 - remainder;
        }
    }
}
