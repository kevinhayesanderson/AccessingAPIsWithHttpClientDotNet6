using Movies.Client.Helpers;
using Movies.Client.Models;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Movies.Client.Services;

public class FaultsAndErrorsSamples : IIntegrationService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly JsonSerializerOptionsWrapper _jsonSerializerOptionsWrapper;

    public FaultsAndErrorsSamples(IHttpClientFactory httpClientFactory, JsonSerializerOptionsWrapper jsonSerializerOptionsWrapper)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _jsonSerializerOptionsWrapper = jsonSerializerOptionsWrapper ?? throw new ArgumentNullException(nameof(jsonSerializerOptionsWrapper));
    }

    public async Task RunAsync()
    {
        //// await GetMovieAndDealWithInvalidResponsesAsync(CancellationToken.None);
        await PostMovieAndHandleErrorsAsync(CancellationToken.None);
    }

    public async Task PostMovieAndHandleErrorsAsync(CancellationToken cancellationToken)
    {
        HttpClient httpClient = _httpClientFactory.CreateClient("MoviesAPIClient");

        MovieForUpdate movieForCreation = new()
        {
            //Title = "Pulp Fiction",
            //Description = "The movie with Zed.",
            //DirectorId = Guid.Parse("d28888e9-2ba9-473a-a40f-e38cb54f9b35"), // is a foreign key
            //ReleaseDate = new DateTimeOffset(new DateTime(1992, 9, 2)),
            //Genre = "Crime, Drama"
        };

        string serializedMovieForCreation = JsonSerializer.Serialize(movieForCreation, _jsonSerializerOptionsWrapper.Options);

        using HttpRequestMessage request = new(HttpMethod.Post, "api/movies");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
        request.Content = new StringContent(serializedMovieForCreation);
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        using HttpResponseMessage response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            switch (response.StatusCode)
            {
                case HttpStatusCode.BadRequest:
                    Stream errorStream = await response.Content.ReadAsStreamAsync();
                    ExtendedProblemDetailsWithErrors? errorAsProblemDetails = await JsonSerializer.DeserializeAsync<ExtendedProblemDetailsWithErrors>(errorStream, _jsonSerializerOptionsWrapper.Options);
                    Dictionary<string, string[]>? errors = errorAsProblemDetails?.Errors;
                    if (errors != null && errors.Any())
                    {
                        foreach (KeyValuePair<string, string[]> error in errors)
                        {
                            Console.WriteLine($"Error: {error.Key}-{error.Value}");
                        }
                    }
                    return;
                case HttpStatusCode.Unauthorized:
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

    public async Task GetMovieAndDealWithInvalidResponsesAsync(CancellationToken cancellationToken)
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
                    case HttpStatusCode.Unauthorized:
                        Console.WriteLine("Unauthorized access to requested movie");
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