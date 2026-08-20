using Microsoft.EntityFrameworkCore;
using PaymentSimplifier.Dtos;
using PaymentSimplifier.Domain.Users;
using PaymentSimplifier.Infrastructure.Users;
using System.Net.Mail;

namespace PaymentSimplifier.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<CreateUserResponse> CreateUserAsync(CreateUserRequest request)
        {
            ValidateCreateUserRequest(request);

            var normalizedName = request.Name.Trim();
            var normalizedDocument = NormalizeDocument(request.Document);
            var normalizedEmail = NormalizeEmail(request.Email);
            var normalizedPassword = request.Password.Trim();

            ValidateDocumentByUserType(normalizedDocument, request.UserType);

            if (await _userRepository.ExistsByDocumentAsync(normalizedDocument))
                throw new InvalidOperationException("Document already registered");

            if (await _userRepository.ExistsByEmailAsync(normalizedEmail))
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
                await _userRepository.AddAsync(user);
                await _userRepository.SaveChangesAsync();
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

        public async Task<List<UserResponse>> GetUsersAsync()
        {
            var users = await _userRepository.GetAllAsync();

            return users.Select(user => new UserResponse
            {
                Id = user.Id,
                Name = user.Name,
                Document = user.Document,
                Email = user.Email,
                UserType = user.UserType,
                Balance = user.Balance
            }).ToList();
        }

        public async Task<UserDepositResponse> DepositInUserAccountAsync(Guid userId, DepositUserRequest request)
        {
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
                throw new ArgumentException("User not found");

            if (request.Amount <= 0)
                throw new ArgumentException("Invalid deposit amount");

            if (string.IsNullOrWhiteSpace(request.Password) || user.Password != request.Password.Trim())
                throw new ArgumentException("Invalid password");

            user.AddBalance(request.Amount);

            await _userRepository.SaveChangesAsync();

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
            return cpf.Length == 11;
        }

        private static bool IsValidCnpj(string cnpj)
        {
            return cnpj.Length == 14;
        }
    }
}
