using Movies.Client.Helpers;
using Movies.Client.Models;
using System.IO.Compression;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Movies.Client.Services;

public class CompressionSamples : IIntegrationService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly JsonSerializerOptionsWrapper _jsonSerializerOptionsWrapper;

    public CompressionSamples(IHttpClientFactory httpClientFactory, JsonSerializerOptionsWrapper jsonSerializerOptionsWrapper)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _jsonSerializerOptionsWrapper = jsonSerializerOptionsWrapper ?? throw new ArgumentNullException(nameof(jsonSerializerOptionsWrapper));
    }

    public async Task RunAsync()
    {
        //// await GetPosterWithGZipCompressionAsync();
        await SendAndReceivePosterWithGZipCompressionAsync();
    }

    private async Task SendAndReceivePosterWithGZipCompressionAsync()
    {
        HttpClient httpClient = _httpClientFactory.CreateClient("MoviesAPIClient");

        // generate a movie poster of 5MB
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
        request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));

        using MemoryStream compressedMemoryContentStream = new();
        using GZipStream gzipStream = new(compressedMemoryContentStream, CompressionMode.Compress);
        memoryContentStream.CopyTo(gzipStream);
        gzipStream.Flush();
        compressedMemoryContentStream.Position = 0;

        using StreamContent streamContent = new(compressedMemoryContentStream);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        streamContent.Headers.ContentEncoding.Add("gzip");

        request.Content = streamContent;

        HttpResponseMessage response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        _ = response.EnsureSuccessStatusCode();

        Stream stream = await response.Content.ReadAsStreamAsync();
        Poster? poster = await JsonSerializer.DeserializeAsync<Poster>(stream, _jsonSerializerOptionsWrapper.Options);

        // do something with the newly created poster
        Console.WriteLine(poster);
    }

    public async Task GetPosterWithGZipCompressionAsync()
    {
        HttpClient httpClient = _httpClientFactory.CreateClient("MoviesAPIClient");

        HttpRequestMessage request = new(HttpMethod.Get,
            $"api/movies/d8663e5e-7494-4f81-8739-6e0de1bea7ee/posters/{Guid.NewGuid()}");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));

        using HttpResponseMessage response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

        Stream stream = await response.Content.ReadAsStreamAsync();

        _ = response.EnsureSuccessStatusCode();

        Poster? poster = await JsonSerializer.DeserializeAsync<Poster>(stream, _jsonSerializerOptionsWrapper.Options);

        Console.WriteLine(poster);
    }
}