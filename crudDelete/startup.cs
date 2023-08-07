using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FunctionApp10;
using System;
[assembly: FunctionsStartup(typeof(Startup))]
namespace FunctionApp10
{
    public class Startup : FunctionsStartup
    {
        private static readonly IConfigurationRoot Configuration = new ConfigurationBuilder()
            .SetBasePath(Environment.CurrentDirectory)
            .AddJsonFile("appsettings.json", true)
            .AddEnvironmentVariables()
            .Build();
        [Obsolete]
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddSingleton(s =>
            {
                var connectionString = Configuration["CosmosDBConnectionStringSetting"];
                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new InvalidOperationException(
                        "Please specify a valid CosmosDBConnection in the appSettings.json file or your Azure Functions Settings.");
                }
                return new CosmosClientBuilder(connectionString)
                    .Build();
            });
        }
    }
}