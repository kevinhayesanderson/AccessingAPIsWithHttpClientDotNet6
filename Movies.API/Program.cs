using Microsoft.EntityFrameworkCore;
using Movies.API.DbContexts;
using Movies.API.Services;
using Newtonsoft.Json.Serialization;

internal class Program {
    private static async Task Main(string[] args) {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        _ = builder.Services.AddControllers(options => {
            // Return a 406 when an unsupported media type was requested
            options.ReturnHttpNotAcceptable = true;

            //options.OutputFormatters.Insert(0, new XmlSerializerOutputFormatter());
            //options.InputFormatters.Insert(0,new XmlSerializerInputFormatter(options));
        })
        // Override System.Text.Json with Json.NET
        .AddNewtonsoftJson(setupAction => {
            setupAction.SerializerSettings.ContractResolver =
               new CamelCasePropertyNamesContractResolver();
        })
        .AddXmlSerializerFormatters();

        // add support for (de)compressing requests/responses (eg gzip)
        _ = builder.Services.AddResponseCompression();
        _ = builder.Services.AddRequestDecompression();

        // register the DbContext on the container, getting the
        // connection string from appSettings
        _ = builder.Services.AddDbContext<MoviesDbContext>(o => o.UseSqlite(
            builder.Configuration["ConnectionStrings:MoviesDBConnectionString"]));

        _ = builder.Services.AddScoped<IMoviesRepository, MoviesRepository>();
        _ = builder.Services.AddScoped<IPostersRepository, PostersRepository>();
        _ = builder.Services.AddScoped<ITrailersRepository, TrailersRepository>();

        _ = builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        _ = builder.Services.AddEndpointsApiExplorer();
        _ = builder.Services.AddSwaggerGen(setupAction => {
            setupAction.SwaggerDoc("v1",
                new() { Title = "Movies API", Version = "v1" });
        }
        );

        WebApplication app = builder.Build();

        // For demo purposes, delete the database & migrate on startup so
        // we can start with a clean slate
        using (IServiceScope scope = app.Services.CreateScope()) {
            try {
                MoviesDbContext? context = scope.ServiceProvider.GetService<MoviesDbContext>();
                if (context != null) {
                    _ = await context.Database.EnsureDeletedAsync();
                    await context.Database.MigrateAsync();
                }
            }
            catch (Exception ex) {
                ILogger logger = scope.ServiceProvider.GetRequiredService<ILogger>();
                logger.LogError(ex, "An error occurred while migrating the database.");
            }
        }

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment()) {
            _ = app.UseSwagger();
            _ = app.UseSwaggerUI();
        }

        // use response compression (client should pass through
        // Accept-Encoding)
        _ = app.UseResponseCompression();

        // use request decompression (client should pass through
        // Content-Encoding)
        _ = app.UseRequestDecompression();

        _ = app.UseAuthorization();

        _ = app.MapControllers();

        app.Run();
    }
}