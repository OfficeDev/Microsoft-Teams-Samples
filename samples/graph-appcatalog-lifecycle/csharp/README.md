### AppCatalog sample

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

  B) Or from Visual Studio

  - Launch Visual Studio
  - File -> Open -> Project/Solution
  - Navigate to `graph-appcatalog-lifecycle/csharp` folder
  - Select `AppCatalogSample.sln` file
  - Press `F5` to run the project
  
  
A) Run ngrok - point to port 3978

    ```bash
    ngrok http -host-header=rewrite 3978

## Concepts introduced in this sample

### Descriptions MS TeamsApp resource type
- List apps in catalog
- Publish app to catalog
- Update app in catalog
- Delete app from catalog

###  List apps in catalog
 - List all applications specific to the tenant : type "listapp" in chat and get all the app available in the same tenant.
 - List applications with a given ID : type "app" in the chat and get deatils of app according to their appId.
 - Find application based on the Teams app manifest ID :  type "findapp" in the chat and get deatils of app according to their manifest Id.:
 - List applications with a given ID, and return the submission review state: type "status" in the chat and get deatils of app either published or not.
 - List the details of only those apps in the catalog that contain a bot: type "bot" in the chat and get deatils of available bot in appcatalog.

## Publish app to catalog
 - type "publish" and upload the mainfest.zip of the teamsApp and app uploaded to appcatalog.
## Update app to catalog
 - type "update" and upload the mainfest.zip of the teamsApp and app updated to appcatalog against of the app id.
## Delete app to catalog app will be deleted from appcatalog against of the app id. 
 
 

## Further reading
- [App in Catalog] (https://docs.microsoft.com/en-us/graph/api/resources/teamsapp?view=graph-rest-1.0)
- [Bot Framework Documentation](https://docs.botframework.com)
- [Bot Basics](https://docs.microsoft.com/azure/bot-service/bot-builder-basics?view=azure-bot-service-4.0)



