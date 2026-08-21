using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using PaymentSimplifier.Application.Services;
using PaymentSimplifier.Domain.Transactions;
using PaymentSimplifier.Domain.Users;
using PaymentSimplifier.Infrastructure.Transactions;
using PaymentSimplifier.Infrastructure.Users;
using Xunit;

namespace PaymentSimplifier.Tests.Application.Services
{
    public class TransferServiceTests
    {
        [Fact]
        public async Task TransferAsync_ShouldThrow_WhenValueIsInvalid()
        {
            var service = CreateService();

            Func<Task> act = async () => await service.TransferAsync(Guid.NewGuid(), Guid.NewGuid(), 0m, "password");

            await act.Should().ThrowAsync<ArgumentException>().WithMessage("Transfer value must be greater than zero");
        }

        [Fact]
        public async Task TransferAsync_ShouldThrow_WhenPayerAndPayeeAreTheSame()
        {
            var service = CreateService();
            var userId = Guid.NewGuid();

            Func<Task> act = async () => await service.TransferAsync(userId, userId, 10m, "password");

            await act.Should().ThrowAsync<ArgumentException>().WithMessage("Payer and payee cannot be the same user");
        }

        [Fact]
        public async Task TransferAsync_ShouldThrow_WhenPayerIsNotFound()
        {
            var userRepository = new Mock<IUserRepository>();
            userRepository.Setup(repository => repository.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((User?)null);
            var transferRepository = new Mock<ITransferRepository>();
            var notificationService = new Mock<INotificationService>();
            var authorizationService = new Mock<ITransferAuthorizationService>();
            var service = CreateService(userRepository, transferRepository, notificationService, authorizationService);

            Func<Task> act = async () => await service.TransferAsync(Guid.NewGuid(), Guid.NewGuid(), 10m, "password");

            await act.Should().ThrowAsync<ArgumentException>().WithMessage("Payer not found");
            userRepository.Verify(repository => repository.GetByIdAsync(It.IsAny<Guid>()), Times.Exactly(2));
            transferRepository.Verify(repository => repository.AddAsync(It.IsAny<Transaction>()), Times.Never);
            notificationService.Verify(service => service.SendNotificationToPayeeAsync(It.IsAny<Guid>(), It.IsAny<decimal>()), Times.Never);
        }

        [Fact]
        public async Task TransferAsync_ShouldThrow_WhenPayeeIsNotFound()
        {
            var payer = CreateUser(balance: 100m);
            var userRepository = new Mock<IUserRepository>();
            userRepository.SetupSequence(repository => repository.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(payer)
                .ReturnsAsync((User?)null);

            var transferRepository = new Mock<ITransferRepository>();
            var notificationService = new Mock<INotificationService>();
            var authorizationService = new Mock<ITransferAuthorizationService>();
            var service = CreateService(userRepository, transferRepository, notificationService, authorizationService);

            Func<Task> act = async () => await service.TransferAsync(payer.Id, Guid.NewGuid(), 10m, "password");

            await act.Should().ThrowAsync<ArgumentException>().WithMessage("Payee not found");
            transferRepository.Verify(repository => repository.AddAsync(It.IsAny<Transaction>()), Times.Never);
            notificationService.Verify(service => service.SendNotificationToPayeeAsync(It.IsAny<Guid>(), It.IsAny<decimal>()), Times.Never);
        }

        [Fact]
        public async Task TransferAsync_ShouldThrow_WhenPasswordIsInvalid()
        {
            var payer = CreateUser(balance: 100m);
            var payee = CreateUser();
            var userRepository = CreateUserRepository(payer, payee);
            var transferRepository = new Mock<ITransferRepository>();
            var notificationService = new Mock<INotificationService>();
            var authorizationService = new Mock<ITransferAuthorizationService>();
            var service = CreateService(userRepository, transferRepository, notificationService, authorizationService);

            Func<Task> act = async () => await service.TransferAsync(payer.Id, payee.Id, 10m, "wrong-password");

            await act.Should().ThrowAsync<ArgumentException>().WithMessage("Invalid password");
            transferRepository.Verify(repository => repository.AddAsync(It.IsAny<Transaction>()), Times.Never);
            notificationService.Verify(service => service.SendNotificationToPayeeAsync(It.IsAny<Guid>(), It.IsAny<decimal>()), Times.Never);
        }

        [Fact]
        public async Task TransferAsync_ShouldThrow_WhenPayerIsNotCommon()
        {
            var payer = CreateUser(userType: UserType.Merchant, balance: 100m);
            var payee = CreateUser();
            var userRepository = CreateUserRepository(payer, payee);
            var transferRepository = new Mock<ITransferRepository>();
            var notificationService = new Mock<INotificationService>();
            var authorizationService = new Mock<ITransferAuthorizationService>();
            var service = CreateService(userRepository, transferRepository, notificationService, authorizationService);

            Func<Task> act = async () => await service.TransferAsync(payer.Id, payee.Id, 10m, "password");

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Only common users can transfer money");
            transferRepository.Verify(repository => repository.AddAsync(It.IsAny<Transaction>()), Times.Never);
        }

        [Fact]
        public async Task TransferAsync_ShouldThrow_WhenBalanceIsInsufficient()
        {
            var payer = CreateUser(balance: 5m);
            var payee = CreateUser();
            var userRepository = CreateUserRepository(payer, payee);
            var transferRepository = new Mock<ITransferRepository>();
            var notificationService = new Mock<INotificationService>();
            var authorizationService = new Mock<ITransferAuthorizationService>();
            var service = CreateService(userRepository, transferRepository, notificationService, authorizationService);

            Func<Task> act = async () => await service.TransferAsync(payer.Id, payee.Id, 10m, "password");

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Insufficient balance");
            transferRepository.Verify(repository => repository.AddAsync(It.IsAny<Transaction>()), Times.Never);
        }

        [Fact]
        public async Task TransferAsync_ShouldReturnFalseTuple_WhenAuthorizationIsDenied()
        {
            var payer = CreateUser(balance: 100m);
            var payee = CreateUser();
            var userRepository = CreateUserRepository(payer, payee);
            var transferRepository = new Mock<ITransferRepository>();
            var notificationService = new Mock<INotificationService>();
            var authorizationService = new Mock<ITransferAuthorizationService>();
            authorizationService.Setup(service => service.IsTransferAuthorizedAsync()).ReturnsAsync(false);
            var service = CreateService(userRepository, transferRepository, notificationService, authorizationService);

            var response = await service.TransferAsync(payer.Id, payee.Id, 10m, "password");

            response.Should().Be((false, false));
            payer.Balance.Should().Be(100m);
            payee.Balance.Should().Be(0m);
            transferRepository.Verify(repository => repository.AddAsync(It.IsAny<Transaction>()), Times.Never);
            transferRepository.Verify(repository => repository.SaveChangesAsync(), Times.Never);
            notificationService.Verify(service => service.SendNotificationToPayeeAsync(It.IsAny<Guid>(), It.IsAny<decimal>()), Times.Never);
        }

        [Fact]
        public async Task TransferAsync_ShouldThrow_WhenAuthorizationRequestFails()
        {
            var payer = CreateUser(balance: 100m);
            var payee = CreateUser();
            var userRepository = CreateUserRepository(payer, payee);
            var transferRepository = new Mock<ITransferRepository>();
            var notificationService = new Mock<INotificationService>();
            var authorizationService = new Mock<ITransferAuthorizationService>();
            authorizationService.Setup(service => service.IsTransferAuthorizedAsync())
                .ThrowsAsync(new InvalidOperationException("Failed to check authorization for transaction"));
            var service = CreateService(userRepository, transferRepository, notificationService, authorizationService);

            Func<Task> act = async () => await service.TransferAsync(payer.Id, payee.Id, 10m, "password");

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Failed to check authorization for transaction");
            transferRepository.Verify(repository => repository.AddAsync(It.IsAny<Transaction>()), Times.Never);
            transferRepository.Verify(repository => repository.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task TransferAsync_ShouldReturnCanNotifyFalse_WhenNotificationFails()
        {
            var payer = CreateUser(balance: 100m);
            var payee = CreateUser();
            var userRepository = CreateUserRepository(payer, payee);
            var transferRepository = new Mock<ITransferRepository>();
            Transaction? createdTransaction = null;

            transferRepository.Setup(repository => repository.AddAsync(It.IsAny<Transaction>()))
                .Callback<Transaction>(transaction => createdTransaction = transaction)
                .Returns(Task.CompletedTask);

            var notificationService = new Mock<INotificationService>();
            notificationService.Setup(service => service.SendNotificationToPayeeAsync(payee.Id, 10m)).ReturnsAsync(false);
            var authorizationService = new Mock<ITransferAuthorizationService>();
            authorizationService.Setup(service => service.IsTransferAuthorizedAsync()).ReturnsAsync(true);

            var service = CreateService(userRepository, transferRepository, notificationService, authorizationService);

            var response = await service.TransferAsync(payer.Id, payee.Id, 10m, "password");

            response.Should().Be((true, false));
            payer.Balance.Should().Be(90m);
            payee.Balance.Should().Be(10m);
            createdTransaction.Should().NotBeNull();
            createdTransaction!.PayerId.Should().Be(payer.Id);
            createdTransaction.PayeeId.Should().Be(payee.Id);
            createdTransaction.Value.Should().Be(10m);
            transferRepository.Verify(repository => repository.AddAsync(It.IsAny<Transaction>()), Times.Once);
            transferRepository.Verify(repository => repository.SaveChangesAsync(), Times.Once);
            notificationService.Verify(service => service.SendNotificationToPayeeAsync(payee.Id, 10m), Times.Once);
        }

        [Fact]
        public async Task TransferAsync_ShouldCreateTransactionAndNotify_WhenRequestIsValid()
        {
            var payer = CreateUser(balance: 100m);
            var payee = CreateUser();
            var userRepository = CreateUserRepository(payer, payee);
            var transferRepository = new Mock<ITransferRepository>();
            Transaction? createdTransaction = null;

            transferRepository.Setup(repository => repository.AddAsync(It.IsAny<Transaction>()))
                .Callback<Transaction>(transaction => createdTransaction = transaction)
                .Returns(Task.CompletedTask);

            var notificationService = new Mock<INotificationService>();
            notificationService.Setup(service => service.SendNotificationToPayeeAsync(payee.Id, 10m)).ReturnsAsync(true);
            var authorizationService = new Mock<ITransferAuthorizationService>();
            authorizationService.Setup(service => service.IsTransferAuthorizedAsync()).ReturnsAsync(true);

            var service = CreateService(userRepository, transferRepository, notificationService, authorizationService);

            var response = await service.TransferAsync(payer.Id, payee.Id, 10m, "password");

            response.Should().Be((true, true));
            payer.Balance.Should().Be(90m);
            payee.Balance.Should().Be(10m);
            createdTransaction.Should().NotBeNull();
            createdTransaction!.PayerId.Should().Be(payer.Id);
            createdTransaction.PayeeId.Should().Be(payee.Id);
            createdTransaction.Value.Should().Be(10m);
            createdTransaction.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            transferRepository.Verify(repository => repository.AddAsync(It.IsAny<Transaction>()), Times.Once);
            transferRepository.Verify(repository => repository.SaveChangesAsync(), Times.Once);
            notificationService.Verify(service => service.SendNotificationToPayeeAsync(payee.Id, 10m), Times.Once);
        }

        private static TransferService CreateService(
            Mock<IUserRepository>? userRepository = null,
            Mock<ITransferRepository>? transferRepository = null,
            Mock<INotificationService>? notificationService = null,
            Mock<ITransferAuthorizationService>? authorizationService = null)
        {
            var logger = new Mock<ILogger<TransferService>>();

            return new TransferService(
                (userRepository ?? new Mock<IUserRepository>()).Object,
                (transferRepository ?? new Mock<ITransferRepository>()).Object,
                logger.Object,
                (notificationService ?? new Mock<INotificationService>()).Object,
                (authorizationService ?? CreateAuthorizationServiceMock()).Object);
        }

        private static Mock<IUserRepository> CreateUserRepository(User payer, User payee)
        {
            var userRepository = new Mock<IUserRepository>();
            userRepository.Setup(repository => repository.GetByIdAsync(payer.Id)).ReturnsAsync(payer);
            userRepository.Setup(repository => repository.GetByIdAsync(payee.Id)).ReturnsAsync(payee);
            return userRepository;
        }

        private static User CreateUser(UserType userType = UserType.Commom, decimal balance = 0m)
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = "User Name",
                Document = "52998224725",
                Email = $"{Guid.NewGuid():N}@email.com",
                Password = "password",
                UserType = userType
            };

            if (balance > 0)
            {
                user.AddBalance(balance);
            }

            return user;
        }

        private static Mock<ITransferAuthorizationService> CreateAuthorizationServiceMock()
        {
            var authorizationService = new Mock<ITransferAuthorizationService>();
            authorizationService.Setup(service => service.IsTransferAuthorizedAsync()).ReturnsAsync(true);
            return authorizationService;
        }
    }
}
