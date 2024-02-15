﻿namespace AzureAppRegistrationMonitor.Core.Models
{
    public class AppRegistrationModel
    {
        public string AppId { get; set; } = default!;

        public string AppDisplayName { get; set; } = default!;

        public string CredentialDisplayName { get; set; } = default!;

        public Guid CredentialId { get; set; }

        public double DaysUntilExpiry { get; set; }

        public string Type { get; set; } = default!;

        public DateTime ExpiryDate { get; set; }
    }
}
