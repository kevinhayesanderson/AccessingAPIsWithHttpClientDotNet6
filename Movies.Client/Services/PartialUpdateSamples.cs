using Microsoft.AspNetCore.JsonPatch;
using Movies.Client.Models;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace Movies.Client.Services;

public class PartialUpdateSamples : IIntegrationService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public PartialUpdateSamples(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    }

    public async Task RunAsync()
    {
        //// await PatchResourceAsync();
        await PatchResourceShortcutAsync();
    }

    public async Task PatchResourceAsync()
    {
        HttpClient httpClient = _httpClientFactory.CreateClient("MoviesAPIClient");

        JsonPatchDocument<MovieForUpdate> patchDoc = new();
        _ = patchDoc.Replace(m => m.Title, "Updated title");
        _ = patchDoc.Remove(m => m.Description);

        string serializedChangeSet = JsonConvert.SerializeObject(patchDoc);
        HttpRequestMessage request = new(HttpMethod.Patch, "api/movies/5b1c2b4d-48c7-402a-80c3-cc796ad49c6b");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        request.Content = new StringContent(serializedChangeSet);
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json-patch+json");

        HttpResponseMessage response = await httpClient.SendAsync(request);
        _ = response.EnsureSuccessStatusCode();

        string content = await response.Content.ReadAsStringAsync();
        Movie? updatedMovie = JsonConvert.DeserializeObject<Movie>(content);
        if(updatedMovie != null)
        {
            Console.WriteLine(updatedMovie);
        }
    }

    public async Task PatchResourceShortcutAsync()
    {
        HttpClient httpClient = _httpClientFactory.CreateClient("MoviesAPIClient");

        JsonPatchDocument<MovieForUpdate> patchDoc = new();
        _ = patchDoc.Replace(m => m.Title, "Updated title");
        _ = patchDoc.Remove(m => m.Description);

        HttpResponseMessage response = await httpClient.PatchAsync("api/movies/5b1c2b4d-48c7-402a-80c3-cc796ad49c6b",
            new StringContent(JsonConvert.SerializeObject(patchDoc), Encoding.UTF8, "application/json-patch+json"));
        _ = response.EnsureSuccessStatusCode();

        string content = await response.Content.ReadAsStringAsync();
        Movie? updatedMovie = JsonConvert.DeserializeObject<Movie>(content);
        if (updatedMovie != null)
        {
            Console.WriteLine(updatedMovie);
        }
    }
}