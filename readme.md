# Azure App Registration Monitor

Azure App Registration Monitor scans the entire tenancy and consolidates a list of credentails (certificates and secrets) which are due to for expiry in configurable number of days and sends a consolidated email to relavent parties to take necessary action.

## How this helps?
- Most of the time we face the challenge of having to follow up with the credentials expiry dates either by keeping reminders or manully checking the azure portal or in the worst case renew the credentails after they have expired. But no more, this app will monitor the azure tenency for app registrations expiring in the upcoming few days (configurable) and notify relavent people/owners and azure admins (configurable) to take necessary actions.

## Help us improve the application
Help us improve out the application by sending us pull-requests or opening a [GitHub Issue](https://github.com/JDSRAO/FlatFileGenerator/issues)

## Please note that this is still a work in progress

## Table of Contents  
1. [Development](#development)
1. [Configuration](#configration) 
1. [License](#license)

## Development
To use the samples with Git, clone the project repository with `git clone https://github.com/JDSRAO/AzureAppRegistrationMonitor.git`

After cloning the respository:
* To build the proejct, open `AzureAppRegistrationMonitor.sln` solution file in Visual Studio 2022 and build the solution.
* Alternatively, open the project directory in command prompt and type ``` cd AzureAppRegistrationMonitor/AzureAppRegistrationMonitor ``` and build with 'dotnet build' or 'msbuild' specifying the target project file.

The easiest way to use this proejct without using Git is to download the zip file containing the current version. You can then unzip the entire archive and use the solution in Visual Studio 2022.

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

### Function App Configuration Instructions
- ClientId - Follow steps mentioned in Microsoft Entra ID Configuration to get this value.
- ClientSecret - Follow steps mentioned in Microsoft Entra ID Configuration to get this value.
- EmailFromAddress - 
- EmailSubject - Subject of the summary email that contains the list of app registations for whom action needs to be taken.
- FromAddress - An email, from which mailbox the summary email should be sent. Please note that, this user should have an active mailbox.
- EmailToAddress - A comppa separated values of email(s),, to whom the summary email should be sent for action. For example, user1@email.com,user2@email.com
- TenantId - Follow steps mentioned in Microsoft Entra ID Configuration to get this value.
- TimeInDaysForNotification - Number of days before expiration when the notification needs to be sent.

## License
Please refer [here](LICENSE.txt) for license information
