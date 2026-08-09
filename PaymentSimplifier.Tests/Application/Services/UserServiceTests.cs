using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using PaymentSimplifier.Application.Services;
using PaymentSimplifier.Domain.Users;
using PaymentSimplifier.Dtos;
using PaymentSimplifier.Infrastructure.Users;
using Xunit;

namespace PaymentSimplifier.Tests.Application.Services
{
    public class UserServiceTests
    {
        [Fact]
        public async Task DepositInUserAccountAsync_ShouldThrow_WhenUserIsNotFound()
        {
            var userRepository = new Mock<IUserRepository>();
            userRepository.Setup(repository => repository.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((User?)null);
            var service = new UserService(userRepository.Object);

            Func<Task> act = async () => await service.DepositInUserAccountAsync(Guid.NewGuid(), 10m);

            await act.Should().ThrowAsync<ArgumentException>().WithMessage("User not found");
            userRepository.Verify(repository => repository.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task DepositInUserAccountAsync_ShouldThrow_WhenAmountIsInvalid()
        {
            var user = CreateUser();
            var userRepository = new Mock<IUserRepository>();
            userRepository.Setup(repository => repository.GetByIdAsync(user.Id)).ReturnsAsync(user);
            var service = new UserService(userRepository.Object);

            Func<Task> act = async () => await service.DepositInUserAccountAsync(user.Id, 0m);

            await act.Should().ThrowAsync<ArgumentException>().WithMessage("Invalid deposit amount");
            user.Balance.Should().Be(0m);
            userRepository.Verify(repository => repository.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task DepositInUserAccountAsync_ShouldAddBalanceAndReturnResponse_WhenRequestIsValid()
        {
            var user = CreateUser();
            var userRepository = new Mock<IUserRepository>();
            userRepository.Setup(repository => repository.GetByIdAsync(user.Id)).ReturnsAsync(user);
            var service = new UserService(userRepository.Object);

            var response = await service.DepositInUserAccountAsync(user.Id, 150m);

            response.Should().BeEquivalentTo(new UserDepositResponse
            {
                Name = user.Name,
                Document = user.Document,
                UserType = user.UserType,
                Balance = 150m
            });
            user.Balance.Should().Be(150m);
            userRepository.Verify(repository => repository.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateUserAsync_ShouldThrow_WhenDocumentAlreadyExists()
        {
            var userRepository = new Mock<IUserRepository>();
            userRepository.Setup(repository => repository.ExistsByDocumentAsync("52998224725")).ReturnsAsync(true);
            var service = new UserService(userRepository.Object);
            var request = CreateRequest();

            Func<Task> act = async () => await service.CreateUserAsync(request);

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Document already registered");
            userRepository.Verify(repository => repository.ExistsByEmailAsync(It.IsAny<string>()), Times.Never);
            userRepository.Verify(repository => repository.AddAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task CreateUserAsync_ShouldThrow_WhenEmailAlreadyExists()
        {
            var userRepository = new Mock<IUserRepository>();
            userRepository.Setup(repository => repository.ExistsByDocumentAsync("52998224725")).ReturnsAsync(false);
            userRepository.Setup(repository => repository.ExistsByEmailAsync("user@email.com")).ReturnsAsync(true);
            var service = new UserService(userRepository.Object);
            var request = CreateRequest();

            Func<Task> act = async () => await service.CreateUserAsync(request);

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Email already registered");
            userRepository.Verify(repository => repository.AddAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task CreateUserAsync_ShouldNormalizeValuesAndReturnResponse_WhenRequestIsValid()
        {
            var userRepository = new Mock<IUserRepository>();
            User? createdUser = null;

            userRepository.Setup(repository => repository.ExistsByDocumentAsync("52998224725")).ReturnsAsync(false);
            userRepository.Setup(repository => repository.ExistsByEmailAsync("user@email.com")).ReturnsAsync(false);
            userRepository.Setup(repository => repository.AddAsync(It.IsAny<User>()))
                .Callback<User>(user => createdUser = user)
                .Returns(Task.CompletedTask);

            var service = new UserService(userRepository.Object);
            var request = CreateRequest();

            var response = await service.CreateUserAsync(request);

            createdUser.Should().NotBeNull();
            createdUser!.Name.Should().Be("User Name");
            createdUser.Document.Should().Be("52998224725");
            createdUser.Email.Should().Be("user@email.com");
            createdUser.Password.Should().Be("password");
            createdUser.UserType.Should().Be(UserType.Commom);
            createdUser.Balance.Should().Be(0m);

            response.Should().BeEquivalentTo(new CreateUserResponse
            {
                Id = createdUser.Id,
                Name = "User Name",
                Document = "52998224725",
                Email = "user@email.com",
                UserType = UserType.Commom,
                Balance = 0m
            });

            userRepository.Verify(repository => repository.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateUserAsync_ShouldWrapDbUpdateException()
        {
            var userRepository = new Mock<IUserRepository>();
            userRepository.Setup(repository => repository.ExistsByDocumentAsync("52998224725")).ReturnsAsync(false);
            userRepository.Setup(repository => repository.ExistsByEmailAsync("user@email.com")).ReturnsAsync(false);
            userRepository.Setup(repository => repository.AddAsync(It.IsAny<User>())).Returns(Task.CompletedTask);
            userRepository.Setup(repository => repository.SaveChangesAsync()).ThrowsAsync(new DbUpdateException("duplicate"));
            var service = new UserService(userRepository.Object);

            Func<Task> act = async () => await service.CreateUserAsync(CreateRequest());

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("User could not be created because the document or email is already registered");
        }

        [Fact]
        public async Task CreateUserAsync_ShouldThrow_WhenEmailIsInvalid()
        {
            var userRepository = new Mock<IUserRepository>();
            var service = new UserService(userRepository.Object);
            var request = CreateRequest();
            request.Email = "invalid-email";

            Func<Task> act = async () => await service.CreateUserAsync(request);

            await act.Should().ThrowAsync<ArgumentException>().WithMessage("Invalid email");
            userRepository.Verify(repository => repository.AddAsync(It.IsAny<User>()), Times.Never);
        }

        private static User CreateUser()
        {
            return new User
            {
                Id = Guid.NewGuid(),
                Name = "User Name",
                Document = "52998224725",
                Email = "user@email.com",
                Password = "password",
                UserType = UserType.Commom
            };
        }

        private static CreateUserRequest CreateRequest()
        {
            return new CreateUserRequest
            {
                Name = " User Name ",
                Document = "529.982.247-25",
                Email = " USER@EMAIL.COM ",
                Password = " password ",
                UserType = UserType.Commom
            };
        }
    }
}
