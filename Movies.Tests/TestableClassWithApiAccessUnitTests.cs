using Movies.Client;
using Movies.Client.Handlers;
using Movies.Client.Helpers;

namespace Movies.Tests
{
    public class TestableClassWithApiAccessUnitTests
    {
        [Fact]
        public async Task GetMovie_On401Response_MustThrowUnauthorizedApiAccessException()
        {
            var httpClient = new HttpClient(new Return401UnauthorizedResponseHandler())
            {
                BaseAddress = new Uri(AppSettingsWrapper.BaseURL)
            };
            var testableClass = new TestableClassWithApiAccess(httpClient, new JsonSerializerOptionsWrapper());

            await Assert.ThrowsAsync<UnauthorizedApiAccessException>(() => testableClass.GetMovieAsync(CancellationToken.None));
        }
    }
}