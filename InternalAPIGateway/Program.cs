using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;




var strEnv = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

var builder = WebApplication.CreateBuilder(args);

// Configure CORS
builder.Services
    .AddCors(options =>
    {
        options.AddPolicy("CorsPolicy", policy =>
        {
            policy
                .WithOrigins(builder.Configuration.GetSection("CORS:Origins").Get<string[]>() ?? [])
                .AllowAnyMethod()
                .AllowCredentials()
                .AllowAnyHeader()
                .SetIsOriginAllowedToAllowWildcardSubdomains();
        });
    });

// Configure health checks
builder.Services
    .AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

// Add Ocelot
builder.Services.AddOcelot(GetOcelotConfiguration(strEnv));

builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.AddSeq(builder.Configuration.GetSection("Seq"));
});
//builder.Host.UseSerilog((context, services, loggerConfiguration) =>
//{
//    // Configure here Serilog instance...
//    loggerConfiguration
//        .MinimumLevel.Information()
//        .Enrich.WithProperty("ApplicationContext", "Ocelot.APIGateway")
//        .Enrich.FromLogContext()
//        .WriteTo.Console()
//        .ReadFrom.Configuration(context.Configuration);
//});


// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();




var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Map health check endpoints
app.MapHealthChecks("/gtw/hc", new HealthCheckOptions()
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
});

app.MapHealthChecks("/gtw/liveness", new HealthCheckOptions
{
    Predicate = r => r.Name.Contains("self")
});

app.Map("/gtw/hi", async (context) =>
{
    await context.Response.WriteAsync("Hello World!");
});

app.UseCors("CorsPolicy");

app.Logger.LogInformation("Starting up {Environment}", strEnv);

await app.UseOcelot();
await app.RunAsync();

//app.Run();


/// <summary>
/// Retrieves the Ocelot configuration based on the specified environment.
/// </summary>
/// <param name="strEnv">The environment name.</param>
/// <returns>The Ocelot configuration.</returns>
IConfiguration GetOcelotConfiguration(string strEnv)
{
    var builder = new ConfigurationBuilder();
    var configFile = "ocelot.json";
    try
    {
        switch (strEnv)
        {
            case "LocalDev":
                // If the environment is LocalDev, set the config file to "ocelot.LocalDev.json"
                // If the file doesn't exist, fallback to "ocelot.json"

                configFile = "ocelot.LocalDev.json";
                configFile = File.Exists(configFile) ? configFile :  "ocelot.json";
                builder
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile(configFile, optional: false, reloadOnChange: true)
                    .AddEnvironmentVariables();
                break;
            case "Development":
                // If the environment is Development, set the config file to "ocelot.Development.json"
                // If the file doesn't exist, fallback to "ocelot.json"

                configFile = "ocelot.Development.json";
                configFile = File.Exists(configFile) ? configFile : "ocelot.json";
                builder
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile(configFile, optional: false, reloadOnChange: true)
                    .AddEnvironmentVariables();
                break;
            case "Staging":
                // If the environment is Staging, set the config file to "ocelot.Staging.json"
                // If the file doesn't exist, fallback to "ocelot.json"

                configFile = "ocelot.Staging.json";
                configFile = File.Exists(configFile) ? configFile : "ocelot.json";
                builder
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile(configFile, optional: false, reloadOnChange: true)
                    .AddEnvironmentVariables();
                break;
            default:
                // If the environment is not specified or doesn't match any of the cases above,
                // set the config file to "ocelot.Production.json"
                // If the file doesn't exist, fallback to "ocelot.json"

                configFile = "ocelot.Production.json";
                configFile = File.Exists(configFile) ? configFile : "ocelot.json";
                builder
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile(configFile, optional: false, reloadOnChange: true)
                    .AddEnvironmentVariables();
                break;
        }
        return builder.Build();
    }
    catch (FileNotFoundException)
    {
        // If the config file specified in the switch case or fallback is not found,
        // fallback to "ocelot.json" and continue without throwing an exception
        // If the file is not found, use the default configuration.
        builder
       .SetBasePath(Directory.GetCurrentDirectory())
       .AddJsonFile("ocelot.json", optional: false, reloadOnChange: true)
       .AddEnvironmentVariables();

        return builder.Build();
    }
}