# Azure App Registration Monitor

## Summary
Azure App Registration Monitor scans the entire tenancy and consolidates a list of credentails (certificates and secrets) which are due to for expiry in configurable number of days and sends a consolidated email to relavent parties to take necessary action.

## Problem Area
- Most of the time we face the challenge of having to follow up with the credentials expiry dates either by keeping reminders or manully checking the azure portal or in the worst case renew the credentails after they have expired. But no more, this app will monitor the azure tenency for app registrations expiring in the upcoming few days and notify relavent people/owners and azure admins (configurable) to take necessary actions.

## Configration
- Below describes the necessary configruation that are needed for the application.

### Microsoft Entra ID Configuration
- Please create a new app registration in Microsoft Entra ID.
- After creation, make a note of the client Id and tenant Id.
- Generate a new client secret for the above generated client Id.
- Under "Certificates and Secrets", add a new client secret and note it down.
- Under "API Permissions" menu,
   - Click on add permission
	- Select Microsoft Graph
	- Select Delegated Permissions
	- Add the below permissions
	   - Application.ReadAll
		- Application.ReadWriteAll
		- Application.ReadWrite.OwnedBy
		- Directory.ReadAll
		- Mail.Send
		- User.Read

### App Settings Instructions
- ClientId - Follow steps mentioned in Microsoft Entra ID Configuration to get this value.
- ClientSecret - Follow steps mentioned in Microsoft Entra ID Configuration to get this value.
- EmailFromAddress - 
- EmailSubject - Subject of the summary email that contains the list of app registations for whom action needs to be taken.
- FromAddress - An email, from which mailbox the summary email should be sent. Please note that, this user should have an active mailbox.
- EmailToAddress - A comppa separated values of email(s),, to whom the summary email should be sent for action. For example, user1@email.com,user2@email.com
- TenantId - Follow steps mentioned in Microsoft Entra ID Configuration to get this value.
- TimeInDaysForNotification - Number of days before expiration when the notification needs to be sent.