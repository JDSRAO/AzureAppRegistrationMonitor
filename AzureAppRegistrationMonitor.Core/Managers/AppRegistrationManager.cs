using AzureAppRegistrationMonitor.Core.Models;
using Microsoft.Graph;
using Microsoft.Graph.Models;

namespace AzureAppRegistrationMonitor.Core.Managers
{
    public class AppRegistrationManager
    {
        private readonly GraphServiceClient graphServiceClient;

        public AppRegistrationManager(GraphServiceClient graphServiceClient)
        {
            this.graphServiceClient = graphServiceClient;
        }

        public async Task<List<Application>> GetAppRegistrationsAcync()
        {
            var apps = new List<Application>();
            string? odataNextLink = null;
            do
            {
                var applications = await this.graphServiceClient.Applications.GetAsync();
                odataNextLink = applications.OdataNextLink;
                apps.AddRange(applications.Value);
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
