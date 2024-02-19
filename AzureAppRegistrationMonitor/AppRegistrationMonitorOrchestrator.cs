using AzureAppRegistrationMonitor.Core.Managers;
using AzureAppRegistrationMonitor.Core.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Graph.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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

        [FunctionName("StartOrchestrator")]
        public async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "orchestrators/{functionName}")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter)
        {
            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("Orchestrator", null);

            this.logger.LogInformation("Started orchestration with ID = '{instanceId}'.", instanceId);

            return starter.CreateCheckStatusResponse(req, instanceId);
        }

        [FunctionName("Orchestrator")]
        public async Task RunOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            try
            {
                var apps = await context.CallActivityAsync<List<Application>>(nameof(GetAppRegistrationsAcync), null);
                var tasks = new List<Task<List<AppRegistrationModel>>>();
                foreach (var app in apps)
                {
                    var task = context.CallActivityAsync<List<AppRegistrationModel>>(nameof(GetAppDetailsToBeNotifiedAsync), app);
                    tasks.Add(task);
                }

                await Task.WhenAll(tasks);

                var appsToBeNotified = new List<AppRegistrationModel>();
                foreach (var task in tasks)
                {
                    appsToBeNotified.AddRange(task.Result);
                }

                var emailContent = this.emailManager.GenerateEmailBody(appsToBeNotified.ToList());
                await context.CallActivityAsync<Task>(nameof(SendEmail), emailContent);
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
        public async Task<List<AppRegistrationModel>> GetAppDetailsToBeNotifiedAsync([ActivityTrigger] Application app)
        {
            return await this.appRegistrationManager.GetAppDetailsToBeNotifiedAsync(app, this.configuration.TimeInDaysForNotification);
        }

        [FunctionName(nameof(SendEmail))]
        public async Task SendEmail([ActivityTrigger] string content)
        {
            await this.emailManager.SendEmail(this.configuration.EmailFromAddress, this.configuration.EmailToAddress.Split(','), this.configuration.EmailSubject, content, BodyType.Html);
        }
    }
}