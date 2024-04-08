using AzureAppRegistrationMonitor.Core.Managers;
using AzureAppRegistrationMonitor.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace AzureAppRegistrationMonitor
{
    public class AppRegistrationMonitorApi
    {
        private readonly ILogger<AppRegistrationMonitorApi> logger;
        private readonly AppRegistrationManager appRegistrationManager;
        private readonly EmailManager emailManager;
        private readonly ConfigurationModel configuration;

        public AppRegistrationMonitorApi(ILogger<AppRegistrationMonitorApi> logger, AppRegistrationManager appRegistrationManager, EmailManager emailManager, ConfigurationModel configurationModel)
        {
            this.logger = logger;
            this.appRegistrationManager = appRegistrationManager;
            this.emailManager = emailManager;
            this.configuration = configurationModel;
        }

        [FunctionName("MonitorApps")]
        public async Task<IActionResult> MonitorAppsAsync
            ([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req)
        {
            try
            {
                this.logger.LogInformation("C# HTTP trigger function processed a request.");

                var apps = await this.appRegistrationManager.GetAppRegistrationsAcync();
                ConcurrentBag<CredentialModel> appsToBeNotified = new ConcurrentBag<CredentialModel>();
                if (apps.Any())
                {
                    await Parallel.ForEachAsync(apps, new ParallelOptions { MaxDegreeOfParallelism = 100 }, async (app, _) =>
                    {
                        var invalidCredentails = await this.appRegistrationManager.GetAppDetailsToBeNotifiedAsync(app, this.configuration.TimeInDaysForNotification);
                        invalidCredentails.ForEach(x =>
                        {
                            appsToBeNotified.Add(x);
                        });
                    });
                }

                if (appsToBeNotified.Any())
                {
                    var owners = new List<string>();
                    if (this.configuration.IncludeOwnersInEmail)
                    {
                        foreach (var appToBeNotified in appsToBeNotified)
                        {
                            owners.AddRange(appToBeNotified.Owners);
                        }
                    }

                    var emailContent = this.emailManager.GenerateEmailBody(appsToBeNotified.ToList());
                    await this.emailManager.SendEmail(this.configuration.EmailFromAddress, this.configuration.EmailToAzureAdmins.Split(';'), owners.ToArray(), this.configuration.EmailSubject, emailContent);
                }

                return new OkResult();
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, ex.Message);
                return new InternalServerErrorResult();
            }
        }
    }
}
