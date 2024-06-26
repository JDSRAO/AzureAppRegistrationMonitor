﻿using AzureAppRegistrationMonitor.Core.Models;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Users.Item.SendMail;
using System.Text;

namespace AzureAppRegistrationMonitor.Core.Managers
{
    public class EmailManager
    {
        private readonly GraphServiceClient graphClient;

        public EmailManager(GraphServiceClient graphClient)
        {
            this.graphClient = graphClient;
        }

        public string GenerateEmailBody(List<CredentialModel> apps)
        {
            var content = new StringBuilder();
            content.AppendLine("<html>");
            content.AppendLine("<head>");

            content.AppendLine("<style>");
            content.AppendLine("table { border-collapse:collapse; }");
            content.AppendLine("th { font-family: Helvetica; padding: 5px; border: 1px solid black;}");
            content.AppendLine("td { font-family: Calibri; padding: 5px; border: 1px solid black; } ");
            content.AppendLine(" .red { background-color:red; font-family: Calibri; padding: 5px; border: 1px solid black; } ");
            content.AppendLine(" . yellow { background-color:yellow; font-family: Calibri; padding: 5px; border: 1px solid black; }");
            content.AppendLine("</style>");
            
            content.AppendLine("</head>");
            content.AppendLine("<body>");

            content.AppendLine("<p> Hi, </br> Please make a note on the expiry dates for the below app registrations and take necessary actions </p>");
            content.AppendLine("<p> <b> <i> </i> Please note that this an auto generated email, please contact people mentioned in \"To\" section for any queries. </b> </p>");

            content.AppendLine("<table>");
            content.AppendLine("<thead>");

            content.AppendLine($"<th>{nameof(CredentialModel.AppId)}</th>");
            content.AppendLine($"<th>{nameof(CredentialModel.AppDisplayName)}</th>");
            content.AppendLine($"<th>{nameof(CredentialModel.CredentialId)}</th>");
            content.AppendLine($"<th>{nameof(CredentialModel.CredentialDisplayName)}</th>");
            content.AppendLine($"<th>{nameof(CredentialModel.Type)}</th>");
            content.AppendLine($"<th>{nameof(CredentialModel.DaysUntilExpiry)}</th>");
            content.AppendLine($"<th>{nameof(CredentialModel.ExpiryDate)}</th>");
            content.AppendLine($"<th>{nameof(CredentialModel.Owners)}</th>");

            content.AppendLine("</thead>");

            content.AppendLine("<tbody>");
            foreach (var app in apps)
            {
                content.AppendLine($"<tr>");
                content.AppendLine($"<td>{app.AppId}</td>");
                content.AppendLine($"<td>{app.AppDisplayName}</td>");
                content.AppendLine($"<td>{app.CredentialId}</td>");
                content.AppendLine($"<td>{app.CredentialDisplayName}</td>");
                content.AppendLine($"<td>{app.Type}</td>");
                content.AppendLine($"<td>{app.DaysUntilExpiry}</td>");
                content.AppendLine($"<td>{app.ExpiryDate}</td>");
                content.AppendLine($"<td>{string.Join(";", app.Owners)}</td>");                
            }

            content.AppendLine("</tbody>");
            content.AppendLine("</table>");

            content.AppendLine("/<body>");
            content.AppendLine("/<html>");

            return content.ToString();
        }

        public async Task SendEmail(string fromAddress, string[] toAddresss, string[] ccEmails, string subject, string content, BodyType bodyType = BodyType.Html)
        {
            if (toAddresss.Length == 0)
            {
                throw new Exception("Specify values in to address");
            }

            Message message = new()
            {
                Subject = subject,
                Body = new ItemBody
                {
                    ContentType = bodyType,
                    Content = content
                },
            };

            List<Recipient> toReceipents = new List<Recipient>(toAddresss.Length);
            foreach (var toAddress in toAddresss)
            {
                toReceipents.Add(new Recipient
                {
                    EmailAddress = new EmailAddress
                    {
                        Address = toAddress,
                    },
                });
            }

            List<Recipient> ccRecipients = new List<Recipient>(ccEmails.Length);
            foreach (var ccEmail in ccEmails)
            {
                toReceipents.Add(new Recipient
                {
                    EmailAddress = new EmailAddress
                    {
                        Address = ccEmail,
                    },
                });
            }

            message.CcRecipients = ccRecipients;
            message.ToRecipients = toReceipents;

            SendMailPostRequestBody body = new()
            {
                Message = message,
                SaveToSentItems = true,
            };

            await this.graphClient.Users[fromAddress].SendMail.PostAsync(body);
        }
    }
}
