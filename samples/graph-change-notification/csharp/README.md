---
page_type: sample
description: This sample app demonstrates sending change notifications to user presence in Teams based on user presence status.
products:
- office-teams
- office
- office-365
languages:
- csharp
extensions:
 contentType: samples
 createdDate: "07/07/2021 01:38:26 PM"
urlFragment: officedev-microsoft-teams-samples-graph-change-notification-csharp
---

# Change Notification sample

Bot Framework v4 ChangeNotification sample.

This sample app demonstrates sending change notifications to user presence in Teams based on user presence status.

## Interact with app.

 ![image](ChangeNotification/Images/ChangeNotificationModule.gif)

## Prerequisites

- Microsoft Teams is installed and you have an account
- [.NET Core SDK](https://dotnet.microsoft.com/download) version 3.1
- [ngrok](https://ngrok.com/) or equivalent tunnelling solution

## Setup.

1) Clone the repository

    ```bash
    git clone https://github.com/OfficeDev/Microsoft-Teams-Samples.git
    ```

  A) Or from Visual Studio code

  - Launch Visual Studio code
  - File -> Open Folder
  - Navigate to `samples/graph-change-notification/csharp` folder
  - Select `ChangeNotification` folder
  - Press `F5` to run the project

  B) Run ngrok - point to port 3978

   ```bash
     ngrok http -host-header=rewrite 3978
   ```  
    
## Instruction on setting connection string for bot authentication on the behalf of user
1. In the Azure portal, select your resource group from the dashboard.

2. Select your bot channel registration link.

3. Open the resource page and select Configuration under Settings.

4. Select Add OAuth Connection Settings.
![image](https://user-images.githubusercontent.com/85864414/121879805-df15cb00-cd2a-11eb-8076-1236ccb1bbfc.PNG)
5. Complete the form as follows:

##.Setup Bot Service Connection (TokenStore)

a. Enter a name for the connection. You'll use this name in your bot in the appsettings.json file. For example BotTeamsAuthADv1.

b. Service Provider. Select Azure Active Directory. Once you select this, the Azure AD-specific fields will be displayed.

c. Client id. Enter the Application (client) ID that you recorded for your Azure identity provider app in the steps above.

d. Client secret. Enter the secret that you recorded for your Azure identity provider app in the steps above.

e. Grant Type. Enter authorization_code.

f. Login URL. Enter https://login.microsoftonline.com.

g. Tenant ID, enter the Directory (tenant) ID that you recorded earlier for your Azure identity app or common depending on the supported account type selected when you created the identity provider app.
h. For Resource URL, enter https://graph.microsoft.com/
i. Provide  Scopes like "Presence.Read, Presence.Read.All"

![image](https://user-images.githubusercontent.com/85864414/122000240-1d16fb80-cdcc-11eb-8aeb-a1dc898f947e.PNG)

## App Registration.

This step will create an AAD app, it will be reused wherever it needs AAD throughout this sample to simpler the steps.

- Navigate to [Azure _App Registration_ Blade](https://ms.portal.azure.com/#blade/Microsoft_AAD_RegisteredApps/ApplicationsListBlade)

- Click "New Registration" on the upper left corner

- Fill out name and select third option for supported account type 
- Set Redirect Uri to "https://token.botframework.com/.auth/web/redirect" and click "Register":

    ![App Registration Organization](https://raw.githubusercontent.com/OfficeDev/Microsoft-Teams-Samples/main/samples/bot-conversation-sso-quickstart/js/sso_media/AppRegistration.png)

- Navigate to the AAD app you just created, _copy and paste the Application ID(will referred as **AppId** in this document) somewhere safe_. You'll need it in a future step:
    ![Save Application ID](https://raw.githubusercontent.com/OfficeDev/Microsoft-Teams-Samples/main/samples/bot-conversation-sso-quickstart/js/sso_media/AppId.png)

#### 1.2 Create Client Secret

- Navigate to the "Certificates & secrets" blade and add a client secret by clicking "New Client Secret"

    ![New Secret](https://raw.githubusercontent.com/OfficeDev/Microsoft-Teams-Samples/main/samples/bot-conversation-sso-quickstart/js/sso_media/ClientSecret.png)
</br>

- _Copy and paste the secret value somewhere safe_. You'll need it in a future step


#### 1.3. Expose API endpoint

- Click "_Expose an API_" in the left rail

    - Set your Application ID URL to include your bot id - api://botid-<AppId>, where <AppId> is the id of the bot that will be making the SSO request and found in your Teams Application Manifest, which is the same you create and saved in step1.1:
    ![Application ID URI](https://raw.githubusercontent.com/OfficeDev/Microsoft-Teams-Samples/main/samples/bot-conversation-sso-quickstart/js/sso_media/AppIdUri.png)

    - Click "_Add a scope_"

        - access_as_user as the Scope name.

        - Set Who can consent? to Admins and users

        - Fill in the fields for configuring the admin and user consent prompts with values that are appropriate for the access_as_user scope. Suggestions:

            - Admin consent display name: Teams can access the user’s profile

            - Admin consent description: Allows Teams to call the app’s web APIs as the current user.

            - User consent display name: Teams can access your user profile and make requests on your behalf

            - User consent description: Enable Teams to call this app’s APIs with the same rights that you have

        - Ensure that State is set to Enabled

        - Click on Add scope button (Note: The domain part of the Scope name displayed just below the text field should automatically match the Application ID URI set in the previous step, with /access_as_user appended to the end)

        ![Add Scope](https://raw.githubusercontent.com/OfficeDev/Microsoft-Teams-Samples/main/samples/bot-conversation-sso-quickstart/js/sso_media/CreateScope.png)

#### 1.4. Authorize client applications

Add the following Ids as authorized clients for your application

- 1fec8e78-bce4-4aaf-ab1b-5451cc387264 (Teams mobile/desktop application)

- 5e3ce6c0-2b1f-4285-8d4b-75ee78787346 (Teams web application)

    ![Add Client Application](https://raw.githubusercontent.com/OfficeDev/Microsoft-Teams-Samples/main/samples/bot-conversation-sso-quickstart/js/sso_media/AddClient.png)

#### 1.5. Add any necessary API permissions for downstream calls

- Navigate to "API permissions" blade on the left hand side

- Add any user delegated permissions that your app will need to downstream APIs. This quick start only requires User.Read.

    ![Add Permissions](https://user-images.githubusercontent.com/85864414/121880473-af1af780-cd2b-11eb-8166-837425ef186f.PNG)

#### 1.6. Enable implicit grant

- Navigate to "Authentication"

- Check the *Access tokens* and *ID tokens* boxes and click on Save button.

- Ensure that you've [enabled the Teams Channel](https://docs.microsoft.com/en-us/azure/bot-service/channel-connect-teams?view=azure-bot-service-4.0)

## Instruction for appsetting
1. Provide appId and appPassword in the appsetting that is created in Azure.
2. Provide UserId in appsetting for which you want to subscribe the user presence.
3. Provide the ngrok url as  "BaseUrl" in appsetting on which application is running on.


## Concepts introduced in this sample
- After sucessfully installation of app you will get a sign in button. When sign in is complete then you get your current status in adapative card
![image](https://user-images.githubusercontent.com/85864414/122000447-741cd080-cdcc-11eb-9833-54f87cd7567f.PNG)
![image](https://user-images.githubusercontent.com/85864414/121878949-ebe5ef00-cd29-11eb-8ab0-683ce3ffbfcb.PNG)

- After that when the user status chagnes you will get notify about their status: 
- user to available.
![image](ChangeNotification/Images/BeRightBackPresence.png)

- Change user status Busy from DoNotDistrub.
 ![image](ChangeNotification/Images/BusywithstatusScreen.png)

- Change user status DoNotDistrub from Available.
 ![image](ChangeNotification/Images/DoNotDistrub.png)

- Change user status BeRightBack from Available.
 ![image](ChangeNotification/Images/BeRightBackPresence.png)
 
 
## Further reading
- [Bot Authentication](https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-authentication?view=azure-bot-service-4.0&tabs=aadv2%2Ccsharp)
- [Change Notification](https://docs.microsoft.com/en-us/graph/api/resources/webhooks?view=graph-rest-beta)
- [App in Catalog](https://docs.microsoft.com/en-us/graph/api/resources/teamsapp?view=graph-rest-1.0)
- [Bot Framework Documentation](https://docs.botframework.com)
- [Bot Basics](https://docs.microsoft.com/azure/bot-service/bot-builder-basics?view=azure-bot-service-4.0)

