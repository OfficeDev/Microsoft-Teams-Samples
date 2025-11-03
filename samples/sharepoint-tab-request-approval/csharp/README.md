---
page_type: sample
description: This sample app demonstrates the integration of SharePoint Embedded for storage management within the Teams Tab Request Approval application, featuring Teams SSO, activity feed notifications, and Graph API support.
products:
- office-teams
- office
- office-365
- sharepoint-repository-service
languages:
- csharp
extensions:
 contentType: samples
 createdDate: "12/19/2023 12:00:01 PM"
urlFragment: officedev-microsoft-teams-samples-sharepoint-tab-request-approval-csharp
---

# Tab Request Approval app + SharePoint Embedded

Welcome to the documentation for the Tab Request Approval app leveraging the SharePoint Embedded API. 

The Tab Request Approval app, now integrated with SharePoint Embedded, serves as a practical demonstration of utilizing the SharePoint Embedded API for storage management within Microsoft Teams. This enhanced version retains the original app's capabilities while introducing key features like Teams Single Sign-On, activity feed notifications, and Graph API integration, empowering developers to manage their Teams app's storage needs efficiently.

## Included Features
* SharePoint Embedded
* Teams SSO (tabs)
* Activity Feed Notifications
* Graph API

## Interaction with app
![Tab-page](/samples/sharepoint-tab-request-approval/csharp/TabRequestApproval/Images/SharepointEmbedded.gif)

## Overview
This is a modified version of the pre-existing [Tab Request Approval](https://github.com/OfficeDev/Microsoft-Teams-Samples/tree/main/samples/tab-request-approval/csharp) app that has been integrated with [SharePoint Embedded](https://learn.microsoft.com/en-us/sharepoint/dev/embedded/overview). 

The original app's functionality has been maintained and now uses the SharePoint Embedded API. The purpose of this sample is to demonstrate the functionality of the SharePoint Embedded and how it can be used to programmatically manage storage requirements of a Teams app.

Please visit the original [Tab-Request-Approval app](https://github.com/OfficeDev/Microsoft-Teams-Samples/tree/main/samples/tab-request-approval/csharp) to get a better understanding of the original application's functionality and to learn how to use the it. The following features and capabilities are available within the app:

- [Teams SSO](https://learn.microsoft.com/en-us/microsoftteams/platform/concepts/authentication/authentication)
- [Activity Feed Notifications](https://learn.microsoft.com/en-us/graph/teams-send-activityfeednotifications?tabs=http)
- [Graph API](https://learn.microsoft.com/en-us/graph/use-the-api)
- [Change Notification Subscriptions](https://learn.microsoft.com/en-us/graph/webhooks)
- [SharePoint Embedded API](https://learn.microsoft.com/en-us/sharepoint/dev/embedded/overview)

## Set-up Overview
The following instructions will walk developers through the steps required to setup the application.

In this scenario, there are 2 key perspectives: Developer and Consumer. The **developer** refers to the app creator and the **consumer** refers to the customer/ client/ user of the developer's app.


### Prerequisites
- Admin user credentials on 2 M365 Tenants
- [Git](https://github.com/git-guides/install-git)
- [Ngrok](https://ngrok.com/download)
- [Microsoft Teams](https://www.microsoft.com/en-ca/microsoft-teams/log-in)
- [Visual Studio](https://visualstudio.microsoft.com/)
- Windows PowerShell (as an Admin)
- [Postman account](https://www.postman.com/), ideally with the [desktop client](https://www.postman.com/downloads/) installed
- [SharePoint Embedded](https://learn.microsoft.com/en-us/sharepoint/dev/embedded/overview)


### Setup Instructions
1. Obtain [pre-requisites](https://github.com/OfficeDev/Microsoft-Teams-Samples/tree/main/samples/tab-request-approval/csharp#prerequisites) for the original Tab Request Approval app.

2) App Registration

### Register your application with Azure AD

1. Register a new application in the [Microsoft Entra ID – App Registrations](https://go.microsoft.com/fwlink/?linkid=2083908) portal.
2. Select **New Registration** and on the *register an application page*, set following values:
    * Set **name** to your app name.
    * Choose the **supported account types** (any account type will work)
    * Leave **Redirect URI** empty.
    * Choose **Register**.
3. On the overview page, copy and save the **Application (client) ID, Directory (tenant) ID**. You'll need those later when updating your Teams application manifest and in the appsettings.json.
4. Navigate to **API Permissions**, and make sure to add the follow permissions:
    * Select Add a permission
    * Select Microsoft Graph -> Delegated permissions.
    * `User.Read` (enabled by default)
    * Click on Add permissions. Please make sure to grant the admin consent for the required permissions.

3. Complete the [setup](https://github.com/OfficeDev/Microsoft-Teams-Samples/tree/main/samples/tab-request-approval/csharp#setup) for the original Tab Request Approval app under your developer tenant for **this** repo.
4. Complete reading the [SharePoint Embedded documentation](https://learn.microsoft.com/en-us/sharepoint/dev/embedded/overview) section for SharePoint Embedded under your developer tenant, the same one used in Step 3.
5. Configuring app secrets

    - Values to obtain from the **developer** tenant azure portal.
        - ```$TenantId``` represents the tenant id.
        - ```$ClientId``` represents the app id.
        - ```$ClientSecret``` represents the app secret.
    
    - Values to obtain from Ngrok or another hosting service
        - ```$BaseUrl``` represents the entire url. Ex: ```https://12345.ngrok-free.app``` or ```https://12345.devtunnels.ms```
         - ```$DomainName``` represents the domain of the hosting service being used. Ex: ```12345.ngrok-free.app``` instead of the entire base url.
    
    - Values to obtain from the SharePoint Embedded setup procedure.
        - ```$ContainerTypeId``` represents the container type id used for your storage purposes. 
        - ```.cer``` and ```.key``` files. These files can be generated by following the instructions at this [link](https://learn.microsoft.com/en-us/sharepoint/dev/embedded/mslearn/m01-05-hol)
        - Store the files locally.
        - Navigate to the **Certificates and Secrets** tab on the **developer** azure portal and click on the **Certificates** section and upload the ```.cer```.
        - Update the ```CertificatePath``` and ```CertificatePrivateKeyPath``` variables within in the [SubscriberProvider.cs](/Providers/SubscriptionProvider.cs) file.

    - Values to obtain from Teams Admin Center
        - ```$TeamsAppId``` represents the teams app id and is acquired once the app has been published into the store (next step).
        - ```$TeamsAppDisplayName``` represents the teams app display name. It is acquired once the app has been published into the store.

    - Once all the mentioned values have been obtained, replace them where necessary: [appsettings.json](/appsettings.json), [manifest.json](/TabRequestApproval/AppPackage/manifest.json), [config.cshtml](/TabRequestApproval/Views/Home/config.cshtml)
        - In the [config.cshtml](/TabRequestApproval/Views/Home/config.cshtml) file, the value to replace is the ```$BaseUrl``` which is found on the ```contentUrl``` key.


6. Install the Teams App on your consumer tenant
    - Once the Teams App is ready to be used by the consumer, upload the ```manifest.zip``` of the teams app into the [Teams Admin Center](https://admin.teams.microsoft.com/policies/manage-apps) by clicking on the **Upload new app** button
    - Once uploaded, the consumer needs to grant pemissions to the app. This is done by clicking on the **Permissions** tab. Then, clicking on the **Grant Admin Consent** button or **Review permissions** button.

### Explanations
This section is dedicated to explaining particular parts of the code in greater detail. 

#### TeamsAppInstallationScopeId
This id represents the scope from which the request is created. This is important because it helps to map which scopes SharePoint containers were generated in..

It offers the following benefits:
- Know which container stores a certain kind of data.
- Be able to perform operations on containers individually.
- Reduced time complexity by a factor of N (N is the number of containers).

**Note**: If you are facing any issue in your app, please uncomment [this](https://github.com/OfficeDev/Microsoft-Teams-Samples/blob/main/samples/sharepoint-tab-request-approval/csharp/TabRequestApproval/AdapterWithErrorHandler.cs#L35) line and put your debugger for local debug.

#### Subscription Flow
This app provides users with the ability to establish [change notification subscription](https://learn.microsoft.com/en-us/graph/api/resources/webhooks?view=graph-rest-1.0). Subscriptions can be made to messages in teams or chats.

A subscription would be made by sending a ```POST``` request to either of the following endpoints: ```/Subscriptions/createTeamSubscription``` or ```Subscriptions/createChatSubscription```.

- Teams Subscription
![TeamsSubscription](/samples/sharepoint-tab-request-approval/csharp/TabRequestApproval/Images/CreateTeamSubscriptionPostman.png)

- Chat Subscription
![ChatsSubscription](/samples/sharepoint-tab-request-approval/csharp/TabRequestApproval/Images/CreateChatSubscriptionPostman.png)

Subscriptions have been made to inform developers of app installation or uninstallation events so that they would be able to leverage SharePoint Embedded APIs to provision or decommission storage respectively.

Ideally, the subscription should have been made to another resource that would inform developers when a customer installs or uninstalls an application. However, this resource does not yet exist on Microsoft Graph. To simulate these events, it was decided to rely on chat messages as those are the most easily accessible manually.

Installation and uninstallation events are simulated by entering the following respectively into a team or chat: ```#microsoft.graph.teamsAppInstalledEventMessageDetail``` and ```#microsoft.graph.teamsAppRemovedEventMessageDetail``` as shown below:
- Simulating installation in chat
![chatInstallation](/samples/sharepoint-tab-request-approval/csharp/TabRequestApproval/Images/InstallationSimulationInChat.png)

- Simulating installation in team
![teamInstallation](/samples/sharepoint-tab-request-approval/csharp/TabRequestApproval/Images/InstallationSimulationInTeam.png)

- Simulating uninstallation in chat
![teamUninstallation](/samples/sharepoint-tab-request-approval/csharp/TabRequestApproval/Images/UninstallationSimulationInChat.png)

- Simulating uninstallation in team
![teamUninstallation](/samples/sharepoint-tab-request-approval/csharp/TabRequestApproval/Images/UninstallationSimulationInTeam.png)

The purpose of providing these simulations is to show you how storage can be provisioned and decommissioned using SharePoint Embedded. To use this simulation, ensure that you create a subscription in the required chat or team resource via the ```ChangeNotification``` controller.

### Recommendations and FAQs

1. I'm running into errors involving granting admin consent to my consumer tenant when I am uploading the manifest in the Teams Admin Center.
    
    - Undo/ delete the changes on Azure AD (Redirect URIs, API Permissions, Expos an API, Secrets, Certificates) that integrating SharePoint Embedded specifically made. Then upload the manifest with only the Tab Request Approval information and grant admin consent. Then integrate SharePoint Embedded, update the manifest and retry the upload process.
    
    - Ensure that a Service Principal for the Azure AD App that is hosting the Teams App exists. If it does not, please set it up manually by clicking on the **Create Service Principal** link.

    - Ensure that you are able to grant admin consent to that Service Principal (Enterprise Application).
    Restarting the process of setting up SharePoint Embedded manually will resolve this. You can do this by navigating to the **API Permissions Tab** and click on ```Grant Admin Consent for {{Tenant Name}}```. Navigate to the app's Enterprise Application entity and click on the ```Grant Admin Consent for {{Tenant Name}}``` as well. Wait for a few minutes and refresh the permissions **API Permissions Tab** and the **Enterprise Application Tab**. If the statuses of the permissions have not been changed, then wait a few more minutes and repeat the above process again times.

<img src="https://pnptelemetry.azurewebsites.net/microsoft-teams-samples/samples/sharepoint-tab-request-approval" />
    
    
