namespace TaskManagementMvc.Services.Interfaces;

public interface IApiClient
{
    Task<T?> GetAsync<T>(string endpoint);
    Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest request);

    Task PutAsync<TRequest>(
       string endpoint,
       TRequest request);

    Task PatchAsync<TRequest>(
        string endpoint,
        TRequest request);

    Task DeleteAsync(string endpoint);
}