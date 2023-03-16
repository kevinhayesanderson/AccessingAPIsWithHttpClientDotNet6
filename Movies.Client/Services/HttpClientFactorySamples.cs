using Movies.Client.Helpers;
using Movies.Client.Models;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Movies.Client.Services;

public class HttpClientFactorySamples : IIntegrationService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly JsonSerializerOptionsWrapper _jsonSerializerOptionsWrapper;
    private readonly MoviesApiClient _moviesAPIClient;

    public HttpClientFactorySamples(IHttpClientFactory httpClientFactory, JsonSerializerOptionsWrapper jsonSerializerOptionsWrapper, MoviesApiClient moviesAPIClient)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _jsonSerializerOptionsWrapper = jsonSerializerOptionsWrapper ?? throw new ArgumentNullException(nameof(jsonSerializerOptionsWrapper));
        _moviesAPIClient = moviesAPIClient ?? throw new ArgumentNullException(nameof(moviesAPIClient));
    }

    public async Task RunAsync()
    {
        //// await TestDisposeHttpClientAsync();
        //// await TestReuseHttpClientAsync();
        //// await GetFilmsAsync();
        //// await GetMoviesWithTypedHttpClientAsync();
        await GetMoviesViaMoviesApiClientAsync();
    }

    private async Task GetMoviesViaMoviesApiClientAsync()
    {
        _ = await _moviesAPIClient.GetMoviesAsync();
    }

    //public async Task GetMoviesWithTypedHttpClientAsync()
    //{
    //    var request = new HttpRequestMessage(HttpMethod.Get, "api/movies");
    //    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

    //    var response = await _moviesAPIClient.Client.SendAsync(request);
    //    response.EnsureSuccessStatusCode();

    //    var content = await response.Content.ReadAsStringAsync();

    //    var movies = JsonSerializer.Deserialize<IEnumerable<Movie>>(content, _jsonSerializerOptionsWrapper.Options);
    //}

    public async Task GetFilmsAsync()
    {
        HttpClient httpClient = _httpClientFactory.CreateClient("MoviesAPIClient");

        HttpRequestMessage request = new(HttpMethod.Get, "api/films");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        HttpResponseMessage response = await httpClient.SendAsync(request);
        _ = response.EnsureSuccessStatusCode();

        string content = await response.Content.ReadAsStringAsync();
        _ = JsonSerializer.Deserialize<IEnumerable<Movie>>(content, _jsonSerializerOptionsWrapper.Options);
    }

    public async Task TestDisposeHttpClientAsync()
    {
        for (int i = 0; i < 10; i++)
        {
            using HttpClient httpClient = new();
            HttpRequestMessage request = new(HttpMethod.Get, "http://www.google.com");

            HttpResponseMessage response = await httpClient.SendAsync(request);
            _ = response.EnsureSuccessStatusCode();

            string content = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Request completed with statuis code: {response.StatusCode}");
        }
    }

    public async Task TestReuseHttpClientAsync()
    {
        HttpClient httpClient = new();

        for (int i = 0; i < 10; i++)
        {
            HttpRequestMessage request = new(HttpMethod.Get, "http://www.google.com");

            HttpResponseMessage response = await httpClient.SendAsync(request);
            _ = response.EnsureSuccessStatusCode();
            _ = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Request completed with statuis code: {response.StatusCode}");
        }
    }
}