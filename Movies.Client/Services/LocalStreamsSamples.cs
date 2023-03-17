using Movies.Client.Helpers;
using Movies.Client.Models;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Movies.Client.Services;

public class LocalStreamsSamples : IIntegrationService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly JsonSerializerOptionsWrapper _jsonSerializerOptionsWrapper;

    public LocalStreamsSamples(IHttpClientFactory httpClientFactory, JsonSerializerOptionsWrapper jsonSerializerOptionsWrapper)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _jsonSerializerOptionsWrapper = jsonSerializerOptionsWrapper ?? throw new ArgumentNullException(nameof(jsonSerializerOptionsWrapper));
    }

    public async Task RunAsync()
    {
        //// await GetPosterWithStreamAsync();
        //// await GetPosterWithStreamAndCompletionModeAsync();
        //// await GetPosterWithoutStreamAsync();
        //// await TestMethodAsync(GetPosterWithoutStreamAsync); //24 milliseconds
        //// await TestMethodAsync(GetPosterWithStreamAsync); //20 milliseconds
        //// await TestMethodAsync(GetPosterWithStreamAndCompletionModeAsync); //22 milliseconds
        //// await PostPosterWithStreamAsync();
        //// await PostAndReadPosterWithStreamAsync();
        await TestMethodAsync(PostPosterWithoutStreamAsync); //1123 milliseconds
        await TestMethodAsync(PostPosterWithStreamAsync); //1114 milliseconds
        await TestMethodAsync(PostAndReadPosterWithStreamAsync); //1090 milliseconds
    }

    public async Task GetPosterWithStreamAsync()
    {
        HttpClient httpClient = _httpClientFactory.CreateClient("MoviesAPIClient");

        HttpRequestMessage request = new(HttpMethod.Get,
            $"api/movies/d8663e5e-7494-4f81-8739-6e0de1bea7ee/posters/{Guid.NewGuid()}");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using HttpResponseMessage response = await httpClient.SendAsync(request);
        _ = response.EnsureSuccessStatusCode();

        Stream stream = await response.Content.ReadAsStreamAsync();
        Poster? poster = await JsonSerializer.DeserializeAsync<Poster>(stream, _jsonSerializerOptionsWrapper.Options);
    }

    public async Task GetPosterWithStreamAndCompletionModeAsync()
    {
        HttpClient httpClient = _httpClientFactory.CreateClient("MoviesAPIClient");

        HttpRequestMessage request = new(HttpMethod.Get,
            $"api/movies/d8663e5e-7494-4f81-8739-6e0de1bea7ee/posters/{Guid.NewGuid()}");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using HttpResponseMessage response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        _ = response.EnsureSuccessStatusCode();

        Stream stream = await response.Content.ReadAsStreamAsync();
        Poster? poster = await JsonSerializer.DeserializeAsync<Poster>(stream, _jsonSerializerOptionsWrapper.Options);
    }

    public async Task GetPosterWithoutStreamAsync()
    {
        HttpClient httpClient = _httpClientFactory.CreateClient("MoviesAPIClient");

        HttpRequestMessage request = new(HttpMethod.Get,
            $"api/movies/d8663e5e-7494-4f81-8739-6e0de1bea7ee/posters/{Guid.NewGuid()}");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        HttpResponseMessage response = await httpClient.SendAsync(request);
        _ = response.EnsureSuccessStatusCode();

        string content = await response.Content.ReadAsStringAsync();
        _ = JsonSerializer.Deserialize<Poster>(content, _jsonSerializerOptionsWrapper.Options);
    }

    public async Task PostPosterWithStreamAsync()
    {
        HttpClient httpClient = _httpClientFactory.CreateClient("MoviesAPIClient");

        //generate a movie poster of 5MB
        Random random = new();
        byte[] generatedBytes = new byte[5242880];
        random.NextBytes(generatedBytes);

        PosterForCreation posterForCreation = new()
        {
            Name = "A new poster for The Big Lebowski",
            Bytes = generatedBytes
        };

        using MemoryStream memoryContentStream = new();
        await JsonSerializer.SerializeAsync(memoryContentStream, posterForCreation);

        _ = memoryContentStream.Seek(0, SeekOrigin.Begin);

        using HttpRequestMessage request = new(HttpMethod.Post, "api/movies/d8663e5e-7494-4f81-8739-6e0de1bea7ee/posters");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using StreamContent streamContent = new(memoryContentStream);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        request.Content = streamContent;

        HttpResponseMessage response = await httpClient.SendAsync(request);
        _ = response.EnsureSuccessStatusCode();

        string content = await response.Content.ReadAsStringAsync();
        Poster? poster = JsonSerializer.Deserialize<Poster>(content, _jsonSerializerOptionsWrapper.Options);

        //do something with newly created poster
    }

    public async Task PostPosterWithoutStreamAsync()
    {
        HttpClient httpClient = _httpClientFactory.CreateClient("MoviesAPIClient");

        //generate a movie poster of 5MB
        Random random = new();
        byte[] generatedBytes = new byte[5242880];
        random.NextBytes(generatedBytes);

        PosterForCreation posterForCreation = new()
        {
            Name = "A new poster for The Big Lebowski",
            Bytes = generatedBytes
        };

        string serializedPosterToCreate = JsonSerializer.Serialize(posterForCreation, _jsonSerializerOptionsWrapper.Options);

        HttpRequestMessage request = new(HttpMethod.Post, "api/movies/d8663e5e-7494-4f81-8739-6e0de1bea7ee/posters");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        request.Content = new StringContent(serializedPosterToCreate);
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        HttpResponseMessage response = await httpClient.SendAsync(request);
        _ = response.EnsureSuccessStatusCode();

        string content = await response.Content.ReadAsStringAsync();
        _ = JsonSerializer.Deserialize<Poster>(content, _jsonSerializerOptionsWrapper.Options);

        //do something with newly created poster
    }

    public async Task PostAndReadPosterWithStreamAsync()
    {
        HttpClient httpClient = _httpClientFactory.CreateClient("MoviesAPIClient");

        //generate a movie poster of 5MB
        Random random = new();
        byte[] generatedBytes = new byte[5242880];
        random.NextBytes(generatedBytes);

        PosterForCreation posterForCreation = new()
        {
            Name = "A new poster for The Big Lebowski",
            Bytes = generatedBytes
        };

        using MemoryStream memoryContentStream = new();
        await JsonSerializer.SerializeAsync(memoryContentStream, posterForCreation);

        _ = memoryContentStream.Seek(0, SeekOrigin.Begin);

        using HttpRequestMessage request = new(HttpMethod.Post, "api/movies/d8663e5e-7494-4f81-8739-6e0de1bea7ee/posters");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using StreamContent streamContent = new(memoryContentStream);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        request.Content = streamContent;

        HttpResponseMessage response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        _ = response.EnsureSuccessStatusCode();

        Stream stream = await response.Content.ReadAsStreamAsync();
        Poster? poster = await JsonSerializer.DeserializeAsync<Poster>(stream, _jsonSerializerOptionsWrapper.Options);

        //do something with newly created poster
    }

    public async Task TestMethodAsync(Func<Task> functionToTest)
    {
        //warmup
        await functionToTest();

        //start stopwatch
        Stopwatch stopWatch = Stopwatch.StartNew();

        //run requests
        for (int i = 0; i < 200; i++)
        {
            await functionToTest();
        }

        //stop stopwatch
        stopWatch.Stop();

        Console.WriteLine($"Elapsed milliseconds without stream: {stopWatch.ElapsedMilliseconds}, averaging {stopWatch.ElapsedMilliseconds / 200} milliseconds/request");
    }
}