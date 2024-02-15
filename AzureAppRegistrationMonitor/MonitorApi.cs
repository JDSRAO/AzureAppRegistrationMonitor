using AzureAppRegistrationMonitor.Core.Managers;
using AzureAppRegistrationMonitor.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Graph.Models;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace AzureAppRegistrationMonitor
{
    public class MonitorApi
    {
        private readonly ILogger<MonitorApi> logger;
        private readonly AppRegistrationManager appRegistractionManager;
        private readonly EmailManager emailManager;
        private readonly ConfigurationModel configuration;

        public MonitorApi()
        {
                
        }

        [FunctionName("MonitorApps")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req)
        {
            try
            {
                this.logger.LogInformation("C# HTTP trigger function processed a request.");

                var apps = await this.appRegistractionManager.GetAppRegistrationsAcync();
                ConcurrentBag<AppRegistrationModel> appsToBeNotified = new ConcurrentBag<AppRegistrationModel>();
                if (apps.Any())
                {
                    await Parallel.ForEachAsync(apps, new ParallelOptions { MaxDegreeOfParallelism = 100 }, async (app, _) =>
                    {
                        var invalidCredentails = await this.appRegistractionManager.GetAppDetailsToBeNotifiedAsync(app, this.configuration.TimeInDaysForNotification);
                        invalidCredentails.ForEach(x =>
                        {
                            appsToBeNotified.Add(x);
                        });
                    });
                }

                var emailContent = this.emailManager.GenerateEmailBody(appsToBeNotified.ToList());
                await this.emailManager.SendEmail(this.configuration.FromAddress, this.configuration.ToAddress, this.configuration.EmailSubject, emailContent, BodyType.Html);

                return new OkObjectResult("");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, ex.Message);
                return new InternalServerErrorResult();
            }
        }
    }
}
