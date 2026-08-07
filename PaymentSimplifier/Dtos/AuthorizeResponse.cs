using System.Text.Json.Serialization;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PaymentSimplifier.Dtos
{
    public class AuthorizeResponse
    {
        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("data")]
        public Data? Data { get; set; }
    }

    public class Data
    {
        [JsonPropertyName("authorization")]
        public bool Authorization { get; set; }
    }
}
