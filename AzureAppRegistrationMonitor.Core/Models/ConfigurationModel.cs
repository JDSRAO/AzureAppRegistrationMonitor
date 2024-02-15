namespace AzureAppRegistrationMonitor.Core.Models
{
    public class ConfigurationModel
    {
        public string ClientId { get; set; } = default!;

        public string ClientSecret { get; set; } = default!;

        public string EmailSubject { get; set; } = default!;

        public string FromAddress { get; set; } = default!;

        public string TenantId { get; set; } = default!;

        public int TimeInDaysForNotification { get; set; } = default!;

        public string[] ToAddress { get; set; } = default!;
    }
}
