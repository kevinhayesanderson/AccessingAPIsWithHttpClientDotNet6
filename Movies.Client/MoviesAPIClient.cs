using Movies.Client.Helpers;
using Movies.Client.Models;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Movies.Client
{
    public class MoviesApiClient
    {
        private readonly JsonSerializerOptionsWrapper _jsonSerializerOptionsWrapper;

        private readonly HttpClient _client;

        public MoviesApiClient(HttpClient client, JsonSerializerOptionsWrapper jsonSerializerOptionsWrapper)
        {
            _client = client;
            _jsonSerializerOptionsWrapper = jsonSerializerOptionsWrapper ??
                throw new ArgumentNullException(nameof(jsonSerializerOptionsWrapper));
            _client.BaseAddress = new Uri(AppSettingsWrapper.BaseURL);
            _client.Timeout = new TimeSpan(0, 0, 30);
        }

        public async Task<IEnumerable<Movie>?> GetMoviesAsync()
        {
            HttpRequestMessage request = new(HttpMethod.Get, "api/movies");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await _client.SendAsync(request);
            _ = response.EnsureSuccessStatusCode();

            string content = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<IEnumerable<Movie>>(content, _jsonSerializerOptionsWrapper.Options);
        }
    }
}