# Azure App Registration Monitor Console App

[[_TOC_]]

# Summary
Azure App Registration Monitor Console App scans the entire tenancy and consolidates a list of credentails (certificates and secrets) which are due to for expiry in configurable number of days and sends a consolidated email to relavent parties to take necessary action.


This page describes the steps needed to use the "AzureAppRegistractionMonitor".

# Microsoft Entra ID Configuration
- Please create a new app registration in microsoft entra ID.
- Please make a note of the client Id, tenant Id.
- Generate a new client secret.
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

# App Settings Instructions
- ClientId - Follow steps mentioned in Microsoft Entra ID Configuration.
- ClientSecret - Follow steps mentioned in Microsoft Entra ID Configuration.
- EmailSubject - Subject of the summary email that contains the list of app registations for whom action needs to be taken.
- FromAddress - An email, from which mailbox the summary email should be sent. Please note that, this user should have an active mailbox.
- TenantId - Follow steps mentioned in Microsoft Entra ID Configuration.
- TimeInDaysForNotification - Number of days before expiration when the notification needs to be sent.
- ToAddress - An arry of email, to whom the summary email should be sent for action.

# Usage
- Go to the root directory where the code is present i.e., "AzureAppRegistractionMonitor"
- Open any command line and go to the above path
- Type "dotnet build -c Release"
- Now go to the path "AzureAppRegistractionMonitor/bin/Release/net6.0"
- Run the file named "AzureAppRegistractionMonitor.exe"
