using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace AzureAppRegistrationMonitor
{
    public class AppRegistrationMonitorOrchestratorTriggers
    {
        private readonly ILogger<AppRegistrationMonitorOrchestratorTriggers> logger;

        public AppRegistrationMonitorOrchestratorTriggers(ILogger<AppRegistrationMonitorOrchestratorTriggers> logger)
        {
            this.logger = logger;
        }

        [FunctionName("TimerOrchestrator")]
        public async Task TimerOrchestratorAsync
            ([TimerTrigger("%AppRegistrationMonitorOrchestratorTimerScheduleCron%")]TimerInfo myTimer,
            [DurableClient] IDurableOrchestrationClient starter)
        {
            try
            {
                this.logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
                // Function input comes from the request content.
                string instanceId = await starter.StartNewAsync("Orchestrator", null);

                this.logger.LogInformation("Started orchestration with ID = '{instanceId}'.", instanceId);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, ex.Message);
            }
        }

        [FunctionName("HttpOrchestrator")]
        public async Task<HttpResponseMessage> HttpOrchestratorAsync
            ([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "orchestrators/HttpOrchestrator")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter)
        {
            try
            {
                // Function input comes from the request content.
                string instanceId = await starter.StartNewAsync("Orchestrator", null);

                this.logger.LogInformation("Started orchestration with ID = '{instanceId}'.", instanceId);

                return starter.CreateCheckStatusResponse(req, instanceId);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, ex.Message);
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
        }
    }
}
