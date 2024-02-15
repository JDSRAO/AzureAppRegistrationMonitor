using Azure.Identity;
using AzureAppRegistrationMonitor.Core.Managers;
using AzureAppRegistrationMonitor.Core.Models;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graph;
using System;

[assembly: FunctionsStartup(typeof(AzureAppRegistrationMonitor.Startup))]
namespace AzureAppRegistrationMonitor
{
    public class Startup : FunctionsStartup
    {
        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            string cs = Environment.GetEnvironmentVariable("ConnectionString");
            builder.ConfigurationBuilder.AddAzureAppConfiguration(cs);
        }

        public override void Configure(IFunctionsHostBuilder builder)
        {
            //var config = new ConfigurationModel();
            //context.Configuration.Bind(config);

            builder.Services.AddSingleton<ConfigurationModel>();// (config);

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
