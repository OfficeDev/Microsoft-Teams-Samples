# QBot Deployment Script(s)

## Prerequisites
1. Clone the repository to your system.
2. The deployment script uses Az modules, if you have AzureRM installed, remove it before running this script.
To remove, run this command: `Uninstall-AzureRM`.
3. Upgrade to Powershell [5.1.22000.653 or later](https://docs.microsoft.com/en-us/powershell/scripting/install/installing-powershell-on-windows?view=powershell-7.2). Run `$PSVersionTable.PSVersion` to check the installed version.
4. Install .Net 3.1 SDK. [Download](https://dotnet.microsoft.com/en-us/download/dotnet/3.1)
5. Install npm. [Instructions](https://docs.npmjs.com/cli/v8/configuring-npm/install)
6. Run the below command in powershell (run-as admin).</br>
`Set-ExecutionPolicy -ExecutionPolicy RemoteSigned` </br> This will allow you to run `DeployQBotInteractive.ps1`. By default, the execution policy is restricted. You may change it back to restricted after deployment is completed. 

## Deployment
The Qbot deployment scripts are aimed at providing a quick and painless way of deploying the Qbot app

In Powershell, navigate to `Scripts` folder and run `DeployQBotInteractive.ps1` script.

### 1. Install/Import Modules
The script requires certain modules to run. Enter `Y` to allow script to install/import the required modules and continue with the deployment.

### 2. Login
The script will prompt you to login with the following accounts:
1. M365 account: Login with a user from the M365 tenant where the AAD apps will be registered. This is tenant typically the organization where QBot will be used. This user must have the ability to create AAD applications.
2. Azure Account: Login with a user account that can create resources in the Azure subscription to which QBot will be deployed.

### 3. Subscription and Region
The script will ask you to choose a subscription and region for deployment.

### 4. Organization Information
The script will next ask you to enter organization's information that will be used to create application manifest:
1. https:// URL to the organization's website.
2. https:// URL to the organization's privacy policy.
3. https:// URL to the organization's terms of use.
4. Organization's name.

> Note: Deployment will start after this step and may take upto 15 minutes to complete.

The script will ask you to confirm zip deployment. Enter `Y` to proceed.

### 5. Admin Consent
Script will launch a url for admin consent to certain application permissions. Login with a user account who has a global admin role in the M365 tenant to grant consent.

### 6. Application Package
The script will create an application package and drop the zip file in the `scripts` folder. Add the application package to your organization's app catalog.

### Congratulations! You should be able to consume the application in Teams now.

## Troubleshooting
The script will write deployment logs to `deployment.log` file in the `scripts` folder. If a deployment fails, check the `deployment.log` file for failure reason.

Make sure that you have upgraded to Powershell [5.1.22000.653 or later](https://docs.microsoft.com/en-us/powershell/scripting/install/installing-powershell-on-windows?view=powershell-7.2). Run `$PSVersionTable.PSVersion` to check the installed version.

### Max students in a team
The solution is tested for upto 1,000 users in a team. If your tenant has more than 1,000 users in a team, we suggest you to make a few changes to the app logic.

When the application is installed in a team, it fetches the team and all the members details in that team (refer to `BotActivityHandler#BotAddedToTeamAsync`), and this can result in a long running task which will timeout if it takes more than 15 seconds to execute. Refer to this [documentation](https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-howto-long-operations-guidance?view=azure-bot-service-4.0) to understand how you can handle such long running operations.

## Contents

### DeployQBotInteractive.ps1
This interactive script is responsible for installing all the dependencies & gathering the necessary fields from the maintainer to run the `New-QBotDeployment` script.

### QBot.zip
This is the output of the `dotnet publish` command from the web directory. This is the running code for Qbot. 
This file is built at runtime and deployed.

## Update App logic
If you want to update the application logic (REST APIs or React App), you can make the changes locally and deploy them to the App Service instance.

You can deploy directly from Visual Studio. Right click the web project and select Publish. [Learn more](https://docs.microsoft.com/en-us/troubleshoot/azure/general/web-apps-deployment-faqs)

You can also prepare a zip package and deploy it. [Learn more](https://docs.microsoft.com/en-us/azure/app-service/deploy-run-package)

> Make sure you select the App service instance that was deployed previously. (`qBot-XXXXX-webapp`)