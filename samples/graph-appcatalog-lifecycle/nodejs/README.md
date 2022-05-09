---
page_type: sample
description: This sample illustrates how you programmatically manage lifecycle for your teams App in catalog by calling Microsoft Graph APIs. .
products:
- office-teams
- office
- office-365
languages:
- nodejs
extensions:
contentType: samples
createdDate: "07-07-2021 13:38:26"
---

# AppCatalog sample

Bot Framework v4 AppCatalog sample.

This sample shows a AppCatalog bot and demonstrates teamsApp lifecycle in catalog followed by commands given to Bot.

## Prerequisites

## Prerequisites

- Microsoft Teams is installed and you have an account
- [.NET Core SDK](https://dotnet.microsoft.com/download) version 3.1
- [ngrok](https://ngrok.com/) or equivalent tunnelling solution

1) Clone the repository

    ```bash
    git clone https://github.com/OfficeDev/Microsoft-Teams-Samples.git
    ```

  B) Or from Visual Studio code

  - Launch Visual Studio code
  - File -> Open Folder
  - Navigate to `samples/graph-appcatalog-lifecycle/node` folder
  - Select `AppCatalogSample` folder
  - Press `F5` to run the project

  C) Run ngrok - point to port 3978

    ```bash
    ngrok http -host-header=rewrite 3978
    ```
## Register Azure AD application
Register one Azure AD application in your tenant's directory: for the bot and tab app authentication.

1.  Log in to the Azure portal from your subscription, and go to the "App registrations" blade  [here](https://portal.azure.com/#blade/Microsoft_AAD_IAM/ActiveDirectoryMenuBlade/RegisteredApps). Ensure that you use a tenant where admin consent for API permissions can be provided.

2.  Click on "New registration", and create an Azure AD application.

3.  **Name:**  The name of your Teams app - if you are following the template for a default deployment, we recommend "App catalog lifecycle".

4.  **Supported account types:**  Select "Accounts in any organizational directory"

5.  Leave the "Redirect URL" field blank.   

6.  Click on the "Register" button.

7.  When the app is registered, you'll be taken to the app's "Overview" page. Copy the  **Application (client) ID**; we will need it later. Verify that the "Supported account types" is set to  **Multiple organizations**.

8.  On the side rail in the Manage section, navigate to the "Certificates & secrets" section. In the Client secrets section, click on "+ New client secret". Add a description for the secret and select Expires as "Never". Click "Add".

9.  Once the client secret is created, copy its  **Value**, please take a note of the secret as it will be required later.


At this point you have 3 unique values:
-   Application (client) ID which will be later used during Azure bot creation
-   Client secret for the bot which will be later used during Azure bot creation
-   Directory (tenant) ID


We recommend that you copy these values into a text file, using an application like Notepad. We will need these values later.

10.  Under left menu, select  **Authentication**  under  **Manage**  section.

11. Select 'Accounts in any organizational directory (Any Azure AD directory - Multitenant)' under Supported account types and click "+Add a platform".

12.  On the flyout menu, Select "Web"    

13.  Add  `https://token.botframework.com/.auth/web/redirect`  under Redirect URLs and click Configure button. 

14.  Once the flyout menu close, scroll bottom to section 'Implicit Grant' and select check boxes "Access tokens" and "ID tokens" and click "Save" at the top bar.

15.  Under left menu, navigate to  **API Permissions**, and make sure to add the following permissions of Microsoft Graph API > Delegated permissions:
-    AppCatalog.ReadWrite.All
-  AppCatalog.Submit

Click on “Add Permissions” to commit your changes.

16.  If you are logged in as the Global Administrator, click on the “Grant admin consent for %tenant-name%” button to grant admin consent else, inform your admin to do the same through the portal or follow the steps provided here to create a link and send it to your admin for consent.

17.  Global Administrator can grant consent using following link:  [https://login.microsoftonline.com/common/adminconsent?client_id=](https://login.microsoftonline.com/common/adminconsent?client_id=)<%appId%> 

## Setup bot Service
1. Create new Azure Bot resource in Azure.
2. Select Type of App as "Multi Tenant"
3.  Select Creation type as "Use existing app registration"
4. Use the copied App Id and Client secret from above step and fill in App Id and App secret respectively.
5. Click on Create on the Azure bot.   
6. Go to the created resource, navigate to channels and add "Microsoft Teams".

## Instruction on setting connection string for bot authentication on the behalf of user
1. In the Azure portal, select your resource group from the dashboard.

2. Select your bot channel registration link.

3. Open the resource page and select Configuration under Settings.

4. Select Add OAuth Connection Settings.


5. Complete the form as follows:

    a. **Name:** Enter a name for the connection. You'll use this name in your bot in the appsettings.json file. For example BotTeamsAuthADv1.

    b. **Service Provider:** Select Azure Active Directory. Once you select this, the Azure AD-specific fields will be displayed.

    c. **Client id:** Enter the Application (client) ID that you recorded for your Azure identity provider app in the steps above.

    d. **Client secret:** Enter the secret that you recorded for your Azure identity provider app in the steps above.

    e. **Grant Type:** Enter authorization_code.

    f. **Login URL:** Enter https://login.microsoftonline.com.

    g. **Tenant ID:** enter the Directory (tenant) ID that you recorded earlier for your Azure identity app or common depending on the supported account type selected when you created the identity provider app.
    h. For Resource URL, enter https://graph.microsoft.com/
    i. Provide  Scopes like "AppCatalog.Submit, AppCatalog.Read.All, AppCatalog.ReadWrite.All, Directory.Read.All, Directory.ReadWrite.All"
    ![image](https://user-images.githubusercontent.com/50989436/120277280-7877c280-c2d1-11eb-8bf6-ea65ee650f06.png)
    ![image](https://user-images.githubusercontent.com/50989436/120277389-9fce8f80-c2d1-11eb-8c5a-f70e6fa3ab51.png)

    - Ensure that you've [enabled the Teams Channel](https://docs.microsoft.com/en-us/azure/bot-service/channel-connect-teams?view=azure-bot-service-4.0)


## Concepts introduced in this sample
- List apps in catalog
    ![image](https://user-images.githubusercontent.com/50989436/118778342-9ee83780-b8a7-11eb-93fc-96bf8448e8e0.png)
- Publish app to catalog
    ![image](https://user-images.githubusercontent.com/50989436/118778589-e2db3c80-b8a7-11eb-8159-a7880be1925e.png)
- Update app in catalog
- Delete app from catalog
    ![image](https://user-images.githubusercontent.com/50989436/118778780-0f8f5400-b8a8-11eb-8353-386de052f324.png)
###  List apps in catalog
 - List all applications specific to the tenant : type "listapp" in chat and get all the app available in the same tenant.
    ![image](https://user-images.githubusercontent.com/50989436/118778263-8841e080-b8a7-11eb-8499-5892a05e2922.png)
 - List applications with a given ID : type "app" in the chat and get deatils of app according to their appId.
    ![image](https://user-images.githubusercontent.com/50989436/118778449-bc1d0600-b8a7-11eb-8370-cdd7564f4cd4.png)
 - Find application based on the Teams app manifest ID :  type "findapp" in the chat and get deatils of app according to their manifest Id.:
 - List applications with a given ID, and return the submission review state: type "status" in the chat and get deatils of app either published or not.
    ![image](https://user-images.githubusercontent.com/50989436/118778856-246be780-b8a8-11eb-9dcc-b551f1136ecc.png)
 - List the details of only those apps in the catalog that contain a bot: type "bot" in the chat and get deatils of available bot in appcatalog.
    ![image](https://user-images.githubusercontent.com/50989436/118778526-cdfea900-b8a7-11eb-91fc-219b4d79098b.png)
## Publish app to catalog
 - type "publish" and upload the mainfest.zip of the teamsApp and app uploaded to appcatalog.
## Update app to catalog
 - type "update" and upload the mainfest.zip of the teamsApp and app updated to appcatalog against of the app id.
## Delete app to catalog app will be deleted from appcatalog against of the app id. 
 
 
## Further reading
- [Bot Authentication] (https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-authentication?view=azure-bot-service-4.0&tabs=aadv2%2Ccsharp)
- [App in Catalog] (https://docs.microsoft.com/en-us/graph/api/resources/teamsapp?view=graph-rest-1.0)
- [Bot Framework Documentation](https://docs.botframework.com)
- [Bot Basics](https://docs.microsoft.com/azure/bot-service/bot-builder-basics?view=azure-bot-service-4.0)

