using Movies.Client.Helpers;
using Movies.Client.Models;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Xml.Serialization;

namespace Movies.Client.Services;

public class CRUDSamples : IIntegrationService {
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly JsonSerializerOptionsWrapper _jsonSerializerOptionsWrapper;

    public CRUDSamples(IHttpClientFactory httpClientFactory, JsonSerializerOptionsWrapper jsonSerializerOptionsWrapper) {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _jsonSerializerOptionsWrapper = jsonSerializerOptionsWrapper ?? throw new ArgumentNullException(nameof(jsonSerializerOptionsWrapper));
    }

    public async Task RunAsync() {
        ////await GetResourceAsync();
        ////await GetResourceThroughHttpRequestMessageAsync();
        ////await CreateResourceAsync();
        ////await UpdateResourceAsync();
        await DeleteResourceAsync();
    }

    public async Task GetResourceAsync() {
        HttpClient httpClient = _httpClientFactory.CreateClient("MoviesAPIClient");

        httpClient.DefaultRequestHeaders.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml", 0.9));

        HttpResponseMessage response = await httpClient.GetAsync("api/movies");
        _ = response.EnsureSuccessStatusCode();

        string content = await response.Content.ReadAsStringAsync();

        if (response.Content.Headers.ContentType?.MediaType == "application/json") {
            _ = JsonSerializer.Deserialize<List<Movie>>(content, _jsonSerializerOptionsWrapper.Options);
        }
        else if (response.Content.Headers.ContentType?.MediaType == "application/xml") {
            XmlSerializer serializer = new(typeof(List<Movie>));
            _ = serializer.Deserialize(new StringReader(content)) as List<Movie>;
        }
    }

    public async Task GetResourceThroughHttpRequestMessageAsync() {
        HttpClient httpClient = _httpClientFactory.CreateClient("MoviesAPIClient");

        HttpRequestMessage request = new(HttpMethod.Get, "api/movies");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        HttpResponseMessage response = await httpClient.SendAsync(request);
        _ = response.EnsureSuccessStatusCode();

        string content = await response.Content.ReadAsStringAsync();
        _ = JsonSerializer.Deserialize<IEnumerable<Movie>>(content, _jsonSerializerOptionsWrapper.Options);
    }

    public async Task CreateResourceAsync() {
        HttpClient httpClient = _httpClientFactory.CreateClient("MoviesAPIClient");

        MovieForCreation movieToCreate = new() {
            Title = "Reservoir Dogs",
            Description = "Six criminals, hired to steal diamonds, do not know each other's true identity. While attempting the heist, the police ambushes them, leading them to believe that one of them is an undercover officer.",
            DirectorId = Guid.Parse("d28888e9-2ba9-473a-a40f-e38cb54f9b35"), // is a foreign key
            ReleaseDate = new DateTimeOffset(new DateTime(1992, 9, 2)),
            Genre = "Crime, Drama"
        };

        string serializedMovieToCreate = JsonSerializer.Serialize(movieToCreate, _jsonSerializerOptionsWrapper.Options);

        HttpRequestMessage request = new(HttpMethod.Post, "api/movies");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json")); // request type in general
        request.Content = new StringContent(serializedMovieToCreate);
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json"); // content type

        HttpResponseMessage response = await httpClient.SendAsync(request);
        _ = response.EnsureSuccessStatusCode();

        string content = await response.Content.ReadAsStringAsync();
        _ = JsonSerializer.Deserialize<Movie>(content, _jsonSerializerOptionsWrapper.Options);
    }

    public async Task UpdateResourceAsync() {
        HttpClient httpClient = _httpClientFactory.CreateClient("MoviesAPIClient");

        MovieForUpdate movieToUpdate = new() {
            Title = "Pulp Fiction",
            Description = "The movie with Zed.",
            DirectorId = Guid.Parse("d28888e9-2ba9-473a-a40f-e38cb54f9b35"), // is a foreign key
            ReleaseDate = new DateTimeOffset(new DateTime(1992, 9, 2)),
            Genre = "Crime, Drama"
        };

        string serializedMovieToUpdate = JsonSerializer.Serialize(movieToUpdate, _jsonSerializerOptionsWrapper.Options);

        HttpRequestMessage request = new(HttpMethod.Put, "api/movies/5b1c2b4d-48c7-402a-80c3-cc796ad49c6b");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json")); // request type in general
        request.Content = new StringContent(serializedMovieToUpdate);
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        HttpResponseMessage response = await httpClient.SendAsync(request);
        _ = response.EnsureSuccessStatusCode();

        string content = await response.Content.ReadAsStringAsync();
        _ = JsonSerializer.Deserialize<Movie>(content, _jsonSerializerOptionsWrapper.Options);
    }

    public async Task DeleteResourceAsync() {
        HttpClient httpClient = _httpClientFactory.CreateClient("MoviesAPIClient");

        HttpRequestMessage request = new(HttpMethod.Delete, "api/movies/5b1c2b4d-48c7-402a-80c3-cc796ad49c6b");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json")); // request type in general, needed becoz some api returns error json instead of false incase of exceptions

        HttpResponseMessage response = await httpClient.SendAsync(request);
        _ = response.EnsureSuccessStatusCode();
        _ = await response.Content.ReadAsStringAsync();
    }
}