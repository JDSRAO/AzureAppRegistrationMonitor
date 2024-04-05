using Azure.Identity;
using AzureAppRegistrationMonitor.Core.Managers;
using AzureAppRegistrationMonitor.Core.Models;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;

[assembly: FunctionsStartup(typeof(AzureAppRegistrationMonitor.Startup))]
namespace AzureAppRegistrationMonitor
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var configurationModel = new ConfigurationModel();
            builder.GetContext().Configuration.Bind(configurationModel);

            builder.Services.AddLogging(builder =>
            {
                // Only Application Insights is registered as a logger provider
                builder.AddApplicationInsights(
                    configureTelemetryConfiguration: (config) => config.ConnectionString = configurationModel.APPLICATIONINSIGHTS_CONNECTION_STRING,
                    configureApplicationInsightsLoggerOptions: (options) => { }
                );
            });

            builder.Services.AddSingleton<ConfigurationModel>(configurationModel);

            builder.Services.AddSingleton<GraphServiceClient>(x =>
            {
                // using Azure.Identity;
                var options = new ClientCertificateCredentialOptions
                {
                    AuthorityHost = AzureAuthorityHosts.AzurePublicCloud,
                };

                var config = x.GetService<ConfigurationModel>();

                var credentails = new ClientSecretCredential(config.TenantId, config.ClientId, config.ClientSecret, options);
                var scopes = new[] { "https://graph.microsoft.com/.default" };
                return new GraphServiceClient(credentails, scopes);
            });

            builder.Services.AddSingleton<AppRegistrationManager>();
            builder.Services.AddSingleton<EmailManager>();
        }
    }
}
