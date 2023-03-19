using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Movies.Client.Helpers;
using Movies.Client.Services;

namespace Movies.Client
{
    public class Program
    {
        protected Program() { }

        private static async Task Main(string[] args)
        {
            using IHost host = Host.CreateDefaultBuilder(args)
                    .ConfigureServices((_, services) =>
                    {
                        // register services for DI
                        services.AddLogging(configure => configure.AddDebug().AddConsole());

                        services.AddSingleton<JsonSerializerOptionsWrapper>();

                        services.AddHttpClient("MoviesAPIClient", configureClient =>
                        {
                            configureClient.BaseAddress = new Uri(AppSettingsWrapper.BaseURL);
                            configureClient.Timeout = new TimeSpan(0, 0, 30);
                        }).ConfigurePrimaryHttpMessageHandler(() =>
                        {
                            SocketsHttpHandler handler = new()
                            {
                                AutomaticDecompression = System.Net.DecompressionMethods.GZip
                            };
                            return handler;
                        });

                        services.AddHttpClient<MoviesApiClient>();

                        // For the cancellation samples
                        //// services.AddScoped<IIntegrationService, CancellationSamples>();

                        // For the compression samples
                        //// 
                        services.AddScoped<IIntegrationService, CompressionSamples>();

                        // For the CRUD samples
                        //// services.AddScoped<IIntegrationService, CRUDSamples>();

                        // For the custom message handler samples
                        //// services.AddScoped<IIntegrationService, CustomMessageHandlersSamples>();

                        // For the faults and errors samples
                        //// services.AddScoped<IIntegrationService, FaultsAndErrorsSamples>();

                        // For the HttpClientFactory samples
                        //// services.AddScoped<IIntegrationService, HttpClientFactorySamples>();

                        // For the local streams samples
                        //// services.AddScoped<IIntegrationService, LocalStreamsSamples>();

                        // For the partial update samples
                        //// services.AddScoped<IIntegrationService, PartialUpdateSamples>();

                        // For the remote streaming samples
                        //// services.AddScoped<IIntegrationService, RemoteStreamingSamples>();
                    }).Build();

            // For demo purposes: overall catch-all to log any exception that might
            // happen to the console & wait for key input afterwards so we can easily
            // inspect the issue.
            try
            {
                ILogger<Program> logger = host.Services.GetRequiredService<ILogger<Program>>();
                logger.LogInformation("Host created.");

                // Run the IntegrationService containing all samples and
                // await this call to ensure the application doesn't
                // prematurely exit.
                await host.Services.GetRequiredService<IIntegrationService>().RunAsync();
            }
            catch (Exception generalException)
            {
                // log the exception
                ILogger<Program> logger = host.Services.GetRequiredService<ILogger<Program>>();
                logger.LogError(generalException,
                    "An exception happened while running the integration service.");
            }

            _ = Console.ReadKey();

            await host.RunAsync();
        }
    }
}