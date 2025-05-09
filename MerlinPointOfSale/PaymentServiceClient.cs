using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using MerlinPointOfSale.Models;
public class PaymentServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public PaymentServiceClient(string baseUrl)
    {
        _httpClient = new HttpClient();
        _baseUrl = baseUrl;
    }

    public async Task<string> CreateConnectionTokenAsync()
    {
        var response = await _httpClient.GetAsync($"{_baseUrl}api/StripeTerminal/CreateConnectionToken");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(content);
        return tokenResponse?.Token;
    }

    public async Task<PaymentIntentResponse> CreatePaymentIntentAsync(long amount, string currency)
    {
        var request = new { Amount = amount, Currency = currency };
        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync($"{_baseUrl}api/PaymentIntent/Create", content);
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        var paymentIntentResponse = JsonSerializer.Deserialize<PaymentIntentResponse>(responseContent);
        return paymentIntentResponse;
    }

    public async Task<PaymentIntentStatusResponse> GetPaymentIntentStatusAsync(string paymentIntentId)
    {
        var response = await _httpClient.GetAsync($"{_baseUrl}api/PaymentIntent/Status/{paymentIntentId}");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<PaymentIntentStatusResponse>(content);
    }
    public async Task<RefundResponse> CreateRefundAsync(string chargeId, long? amount = null)
    {
        var request = new { ChargeId = chargeId, Amount = amount };
        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync($"{_baseUrl}api/Refund/Refund", content);

        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            try
            {
                var error = JsonSerializer.Deserialize<ErrorResponse>(responseContent);
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    throw new Exception("ChargeID not found. Please verify the Merlin ID entered.");
                }
                throw new Exception($"Error processing refund: {error?.error ?? response.ReasonPhrase}");
            }
            catch (JsonException)
            {
                throw new Exception($"Error processing refund: Unexpected response format. {response.ReasonPhrase}");
            }
        }

        return JsonSerializer.Deserialize<RefundResponse>(responseContent);
    }

    private class ErrorResponse
    {
        public string error { get; set; }
    }


}