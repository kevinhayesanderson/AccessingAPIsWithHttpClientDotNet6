using Movies.Client.Helpers;
using Movies.Client.Models;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Movies.Client.Services;

public class CancellationSamples : IIntegrationService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly JsonSerializerOptionsWrapper _jsonSerializerOptionsWrapper;
    private readonly CancellationTokenSource _cts = new();

    public CancellationSamples(IHttpClientFactory httpClientFactory, JsonSerializerOptionsWrapper jsonSerializerOptionsWrapper)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _jsonSerializerOptionsWrapper = jsonSerializerOptionsWrapper ?? throw new ArgumentNullException(nameof(jsonSerializerOptionsWrapper));
    }

    public async Task RunAsync()
    {
        _cts.CancelAfter(200);
        await GetTrailerAndCancelAsync(_cts.Token);
        await GetTrailerAndHandleTimeoutAsync();
    }

    private async Task GetTrailerAndHandleTimeoutAsync()
    {
        HttpClient httpClient = _httpClientFactory.CreateClient("MoviesAPIClient");

        HttpRequestMessage request = new(HttpMethod.Get, $"api/movies/d8663e5e-7494-4f81-8739-6e0de1bea7ee/trailers/{Guid.NewGuid()}");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));

        try
        {
            using HttpResponseMessage response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            Stream stream = await response.Content.ReadAsStreamAsync();
            _ = response.EnsureSuccessStatusCode();

            Trailer? poster = await JsonSerializer.DeserializeAsync<Trailer>(stream, _jsonSerializerOptionsWrapper.Options);
            Console.WriteLine(poster);
        }
        catch (OperationCanceledException operationCanceledExpection)
        {
            Console.WriteLine(operationCanceledExpection.Message);
        }
    }

    private async Task GetTrailerAndCancelAsync(CancellationToken cancellationToken)
    {
        HttpClient httpClient = _httpClientFactory.CreateClient("MoviesAPIClient");

        HttpRequestMessage request = new(HttpMethod.Get, $"api/movies/d8663e5e-7494-4f81-8739-6e0de1bea7ee/trailers/{Guid.NewGuid()}");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));

        try
        {
            using HttpResponseMessage response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            Stream stream = await response.Content.ReadAsStreamAsync();
            _ = response.EnsureSuccessStatusCode();

            Trailer? poster = await JsonSerializer.DeserializeAsync<Trailer>(stream, _jsonSerializerOptionsWrapper.Options);
            Console.WriteLine(poster);
        }
        catch (OperationCanceledException operationCanceledExpection)
        {
            Console.WriteLine(operationCanceledExpection.Message);
        }
    }
}