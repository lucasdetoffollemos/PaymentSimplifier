using PaymentSimplifier.Domain.Users;
using PaymentSimplifier.Dtos;
using PaymentSimplifier.Infrastructure;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PaymentSimplifier.Application.Services
{
    public class TransferService : ITransferService
    {
        private readonly AppDbContext _appDbContext;
        public TransferService(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<bool> TransferAsync(Guid payerId, Guid payeeId, decimal value)
        {
            //validate id payerid and payee id are not the same

            if (payerId == payeeId)
            {
                throw new ArgumentException("Payer and payee cannot be the same user");
            }

            //validate if payer and payee exist in the database

            var payer = await _appDbContext.Users.FindAsync(payerId);
            var payee = await _appDbContext.Users.FindAsync(payeeId);

            if (payer == null)
            {
                throw new ArgumentException("Payer not found");
            }

            if (payee == null)
            {
                throw new ArgumentException("Payee not found");
            }

            //only users with userType "common" can transfer money

            if (payer.UserType != UserType.Commom)
            {
                throw new InvalidOperationException("Only common users can transfer money");
            }

            //validate if payer has enough balance to transfer the value

            if (payer.Balance < value)
            {
                throw new InvalidOperationException("Insufficient balance");
            }

            //before confim transaction, check this url https://util.devi.tools/api/v2/authorize, if the response is false, return false and do not proceed with the transaction

            if (!await CheckAuthorizeTransaction())
            {
                return false;
            }

            //proceed with the transaction

            payer.Balance -= value;
            payee.Balance += value;

            await _appDbContext.SaveChangesAsync();

            await SendNotificationToPayee(payeeId);

            return true; 
        }

        private async Task SendNotificationToPayee(Guid payeeId)
        {
            var httpClient = new HttpClient();
            HttpResponseMessage response;

            do
            {
                response = await httpClient.PostAsync("https://util.devi.tools/api/v1/notify", new StringContent(JsonSerializer.Serialize(new { payeeId }), Encoding.UTF8, "application/json"));
                await Task.Delay(500); // Wait for 500 milliseconds before retrying
            }
            while (!response.IsSuccessStatusCode);
        }

        private static async Task<bool> CheckAuthorizeTransaction()
        {
            var httpClient = new HttpClient();

            var response = await httpClient.GetAsync("https://util.devi.tools/api/v2/authorize");

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();

                var resultParsed = JsonSerializer.Deserialize<AuthorizeResponse>(responseContent);

                if (resultParsed == null || resultParsed.Data == null)
                    return false;

                return resultParsed.Data.Authorization;
            }

            return false;
        }
    }
}
