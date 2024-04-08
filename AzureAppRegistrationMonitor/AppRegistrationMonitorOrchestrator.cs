using AzureAppRegistrationMonitor.Core.Managers;
using AzureAppRegistrationMonitor.Core.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Microsoft.Graph.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureAppRegistrationMonitor
{
    public class AppRegistrationMonitorOrchestrator
    {
        private readonly ILogger<AppRegistrationMonitorApi> logger;
        private readonly AppRegistrationManager appRegistrationManager;
        private readonly EmailManager emailManager;
        private readonly ConfigurationModel configuration;

        public AppRegistrationMonitorOrchestrator(ILogger<AppRegistrationMonitorApi> logger, AppRegistrationManager appRegistrationManager, EmailManager emailManager, ConfigurationModel configurationModel)
        {
            this.logger = logger;
            this.appRegistrationManager = appRegistrationManager;
            this.emailManager = emailManager;
            this.configuration = configurationModel;
        }

        [FunctionName(nameof(AppRegistrationMonitorOrchestrator.AppRegistrationOrchestrator))]
        public async Task AppRegistrationOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            try
            {
                var apps = await context.CallActivityAsync<List<Application>>(nameof(GetAppRegistrationsAcync), null);
                var tasks = new List<Task<List<CredentialModel>>>();
                foreach (var app in apps)
                {
                    var task = context.CallActivityAsync<List<CredentialModel>>(nameof(GetAppDetailsToBeNotifiedAsync), app);
                    tasks.Add(task);
                }

                await Task.WhenAll(tasks);

                var appsToBeNotified = new List<CredentialModel>();
                foreach (var task in tasks)
                {
                    appsToBeNotified.AddRange(task.Result);
                }

                if (appsToBeNotified.Any())
                {
                    await context.CallActivityAsync<Task>(nameof(SendEmail), appsToBeNotified);
                }
            }
            catch (System.Exception ex)
            {
                this.logger.LogError(ex, ex.Message);
            }
        }

        [FunctionName(nameof(GetAppRegistrationsAcync))]
        public async Task<List<Application>> GetAppRegistrationsAcync([ActivityTrigger] string name)
        {
            return await this.appRegistrationManager.GetAppRegistrationsAcync();
        }

        [FunctionName(nameof(GetAppDetailsToBeNotifiedAsync))]
        public async Task<List<CredentialModel>> GetAppDetailsToBeNotifiedAsync([ActivityTrigger] Application app)
        {
            return await this.appRegistrationManager.GetAppDetailsToBeNotifiedAsync(app, this.configuration.TimeInDaysForNotification);
        }

        [FunctionName(nameof(SendEmail))]
        public async Task SendEmail([ActivityTrigger] List<CredentialModel> appsToBeNotified)
        {
            var content = this.emailManager.GenerateEmailBody(appsToBeNotified);
            var owners = new List<string>();
            appsToBeNotified.Select(x =>
            {
                owners.AddRange(x.Owners);
                return x;
            });

            await this.emailManager.SendEmail(this.configuration.EmailFromAddress, this.configuration.EmailToAzureAdmins.Split(';'), owners.ToArray(), this.configuration.EmailSubject, content);
        }
    }
}