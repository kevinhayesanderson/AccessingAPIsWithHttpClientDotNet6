using Movies.Client.Helpers;
using Movies.Client.Models;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Movies.Client
{
    public class TestableClassWithApiAccess
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptionsWrapper _jsonSerializerOptionsWrapper;

        public TestableClassWithApiAccess(HttpClient httpClient, JsonSerializerOptionsWrapper jsonSerializerOptionsWrapper)
        {
            _httpClient = httpClient;
            _jsonSerializerOptionsWrapper = jsonSerializerOptionsWrapper;
        }

        public async Task<Movie?> GetMovieAsync(CancellationToken cancellationToken)
        {
            HttpRequestMessage request = new(HttpMethod.Get, $"api/movies/145ad288-921f-4fd2-806b-409f7d33d8bc");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));

            using HttpResponseMessage response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            switch (response.StatusCode)
            {
                case HttpStatusCode.NotFound:
                    Console.WriteLine("The requested movie cannot be found");
                    return null;

                case HttpStatusCode.Unauthorized:
                    throw new UnauthorizedApiAccessException();
                default:
                    break;
            }

            _ = response.EnsureSuccessStatusCode();

            Stream stream = await response.Content.ReadAsStreamAsync();
            return await JsonSerializer.DeserializeAsync<Movie>(stream, _jsonSerializerOptionsWrapper.Options);
        }
    }
}