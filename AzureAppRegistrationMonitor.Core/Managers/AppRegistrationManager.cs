using AzureAppRegistrationMonitor.Core.Models;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Kiota.Abstractions;
using System.Text;

namespace AzureAppRegistrationMonitor.Core.Managers
{
    public class AppRegistrationManager
    {
        private readonly GraphServiceClient graphServiceClient;
        private readonly ConfigurationModel configuration;

        public AppRegistrationManager(GraphServiceClient graphServiceClient, ConfigurationModel configuration)
        {
            this.graphServiceClient = graphServiceClient;
            this.configuration = configuration;
        }

        public async Task<List<Application>> GetAppRegistrationsAcync()
        {
            var apps = new List<Application>();
            string? odataNextLink = null;
            do
            {
                if (string.IsNullOrWhiteSpace(this.configuration.SearchCriteria))
                {
                    var applications = await this.graphServiceClient.Applications.GetAsync();
                    odataNextLink = applications.OdataNextLink;
                    apps.AddRange(applications.Value);
                }
                else
                {
                    var applications = await this.graphServiceClient.Applications.GetAsync((requestConfiguration) =>
                    {
                        var filters = new List<string>();
                        foreach (var filter in this.configuration.SearchCriteria.Split(","))
                        {
                            filters.Add($"\"displayName:{filter}\"");
                        }

                        var apiFilter = new StringBuilder();
                        apiFilter.Append(string.Join(" OR ", filters));

                        requestConfiguration.QueryParameters.Search = apiFilter.ToString();
                        requestConfiguration.Headers.Add("ConsistencyLevel", "eventual");
                    });
                    odataNextLink = applications.OdataNextLink;
                    apps.AddRange(applications.Value);
                }
            } while (odataNextLink != null);

            return apps;
        }

        public async Task<List<CredentialModel>> GetAppDetailsToBeNotifiedAsync(Application application, int numberOfDays)
        {
            var invalidCredentials = new List<CredentialModel>();
            var owners = new List<string>();

            if (application.Owners != null)
            {
                foreach (User owner in application.Owners)
                {
                    owners.Add(owner.UserPrincipalName);
                }
            }

            if (application.KeyCredentials != null)
            {
                foreach (var credential in application.KeyCredentials)
                {
                    if (credential.EndDateTime.HasValue)
                    {
                        var interval = (credential.EndDateTime - DateTime.UtcNow).Value.TotalDays;
                        if (interval < numberOfDays)
                        {
                            invalidCredentials.Add(new CredentialModel
                            {
                                AppId = application.AppId,
                                AppDisplayName = application.DisplayName,
                                CredentialId = credential.KeyId.Value,
                                CredentialDisplayName = credential.DisplayName,
                                DaysUntilExpiry = interval,
                                Type = CredentialTypes.KeyCredential,
                                ExpiryDate = credential.EndDateTime.Value.Date,
                                Owners = owners.ToArray(),
                            });
                        }
                    }
                }
            }

            if (application.PasswordCredentials != null)
            {
                foreach (var credential in application.PasswordCredentials)
                {
                    if (credential.EndDateTime.HasValue)
                    {
                        var interval = (credential.EndDateTime - DateTime.UtcNow).Value.TotalDays;
                        if (true)
                        {
                            invalidCredentials.Add(new CredentialModel
                            {
                                AppId = application.AppId,
                                AppDisplayName = application.DisplayName,
                                CredentialId = credential.KeyId.Value,
                                CredentialDisplayName = credential.DisplayName,
                                DaysUntilExpiry = interval,
                                Type = CredentialTypes.PasswordCredential,
                                ExpiryDate = credential.EndDateTime.Value.Date,
                                Owners = owners.ToArray(),
                            });
                        }
                    }
                }
            }

            return invalidCredentials;
        }
    }
}
