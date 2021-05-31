### AppCatalog sample

Bot Framework v4 AppCatalog sample.

This sample shows a AppCatalog bot and demonstrates teamsApp lifecycle in catalog followed by commands given to Bot.

**Scenarios Covered:**
- List Apps in catalog
- Publish App to catalog
- Update App in catalog
- Delete App from catalog

## Prerequisites

- Microsoft Teams is installed and you have an account
- [.NET Core SDK](https://dotnet.microsoft.com/download) version 3.1
- [ngrok](https://ngrok.com/) or equivalent tunnelling solution

1) Clone the repository

    ```bash
    git clone https://github.com/OfficeDev/Microsoft-Teams-Samples.git
    ```

  A) Or from Visual Studio

  - Launch Visual Studio
  - File -> Open -> Project/Solution
  - Navigate to `samples/graph-appcatalog-lifecycle/csharp` folder
  - Select `AppCatalogSample.sln` file
  - Press `F5` to run the project


  B) Run ngrok - point to port 3978

      ```bash
       ngrok http -host-header=rewrite 3978

## Concepts introduced in this sample
### Descriptions MS TeamsApp resource type
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
- [Add authentication to a bot](https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-authentication?view=azure-bot-service-4.0&tabs=aadv2%2Ccsharp)
- [App in Catalog](https://docs.microsoft.com/en-us/graph/api/resources/teamsapp?view=graph-rest-1.0)
- [Bot Framework Documentation](https://docs.botframework.com)
- [Bot Basics](https://docs.microsoft.com/azure/bot-service/bot-builder-basics?view=azure-bot-service-4.0)
