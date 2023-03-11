using Movies.Client.Models;
using System.Text.Json;

namespace Movies.Client.Services;

public class CRUDSamples : IIntegrationService {
    private readonly IHttpClientFactory _httpClientFactory;

    public CRUDSamples(IHttpClientFactory httpClientFactory) {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    }

    public async Task RunAsync() {
        await GetResourceAsync();
    }

    public async Task GetResourceAsync() {
        HttpClient httpClient = _httpClientFactory.CreateClient();
        httpClient.BaseAddress = new Uri("http://localhost:5001");
        httpClient.Timeout = new TimeSpan(0, 0, 30);

        HttpResponseMessage response = await httpClient.GetAsync("api/movies");
        _ = response.EnsureSuccessStatusCode();
        string content = await response.Content.ReadAsStringAsync();
        var movies = JsonSerializer.Deserialize<IEnumerable<Movie>>(content, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    }
}