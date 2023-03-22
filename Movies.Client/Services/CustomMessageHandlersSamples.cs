using Movies.Client.Helpers;
using Movies.Client.Models;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Movies.Client.Services;

public class CustomMessageHandlersSamples : IIntegrationService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly JsonSerializerOptionsWrapper _jsonSerializerOptionsWrapper;

    public CustomMessageHandlersSamples(IHttpClientFactory httpClientFactory, JsonSerializerOptionsWrapper jsonSerializerOptionsWrapper)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _jsonSerializerOptionsWrapper = jsonSerializerOptionsWrapper ?? throw new ArgumentNullException(nameof(jsonSerializerOptionsWrapper));
    }

    public async Task RunAsync()
    {
        await GetmovieWithCustomRetryHandlerAsync(CancellationToken.None);
    }

    private async Task GetmovieWithCustomRetryHandlerAsync(CancellationToken cancellationToken)
    {
        HttpClient httpClient = _httpClientFactory.CreateClient("MoviesAPIClient");

        HttpRequestMessage request = new(HttpMethod.Get, $"api/movies/145ad288-921f-4fd2-806b-409f7d33d8bc");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));

        try
        {
            using HttpResponseMessage response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                switch (response.StatusCode)
                {
                    case HttpStatusCode.NotFound:
                        Console.WriteLine("The requested movie cannot be found");
                        return;

                    default:
                        break;
                }
            }

            _ = response.EnsureSuccessStatusCode();

            Stream stream = await response.Content.ReadAsStreamAsync();

            Movie? movie = await JsonSerializer.DeserializeAsync<Movie>(stream, _jsonSerializerOptionsWrapper.Options);
            Console.WriteLine(movie);
        }
        catch (OperationCanceledException operationCanceledExpection)
        {
            Console.WriteLine(operationCanceledExpection.Message);
        }
    }
}