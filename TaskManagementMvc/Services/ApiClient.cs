using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using TaskManagementMvc.Exceptions;
using TaskManagementMvc.Services.Interfaces;

namespace TaskManagementMvc.Services;

public class ApiClient : IApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ApiClient(
        HttpClient httpClient,
        IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<T?> GetAsync<T>(string endpoint)
    {
        var response = await _httpClient.GetAsync(endpoint);

        await HandleResponseAsync(response);

        return await response.Content.ReadFromJsonAsync<T>();
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(
        string endpoint,
        TRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync(
            endpoint,
            request);

        await HandleResponseAsync(response);

        return await response.Content.ReadFromJsonAsync<TResponse>();
    }

    public async Task PutAsync<TRequest>(
        string endpoint,
        TRequest request)
    {
        var response = await _httpClient.PutAsJsonAsync(
            endpoint,
            request);

        await HandleResponseAsync(response);
    }

    public async Task PatchAsync<TRequest>(
        string endpoint,
        TRequest request)
    {
        var response = await _httpClient.PatchAsJsonAsync(
            endpoint,
            request);

        await HandleResponseAsync(response);
    }

    public async Task DeleteAsync(string endpoint)
    {
        var response = await _httpClient.DeleteAsync(endpoint);

        await HandleResponseAsync(response);
    }

    private async Task HandleResponseAsync(
        HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            _httpContextAccessor.HttpContext?
                .Session
                .Clear();

            throw new SessionExpiredException();
        }

        var message = await ReadErrorMessageAsync(response);

        throw new ApiException(
            (int)response.StatusCode,
            message);
    }

    private static async Task<string> ReadErrorMessageAsync(
        HttpResponseMessage response)
    {
        try
        {
            var json = await response.Content
                .ReadAsStringAsync();

            using var document =
                JsonDocument.Parse(json);

            if (document.RootElement.TryGetProperty(
                "message",
                out var message))
            {
                return message.GetString()
                    ?? "API request failed.";
            }
        }
        catch
        {
            // Ignore JSON parsing failure.
        }

        return $"API request failed with status {(int)response.StatusCode}.";
    }
}