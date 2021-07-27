This App talks about the Teams tab which displays Adaptive card with CSharp.
For reference please check [Build tabs with Adaptive Cards](https://docs.microsoft.com/en-us/microsoftteams/platform/tabs/how-to/build-adaptive-card-tabs)

This feature shown in this sample is in Public Developer Preview and is supported in desktop and mobile.

## Prerequisites

- [.NET Core SDK](https://dotnet.microsoft.com/download) version 3.1

  ```bash
  # determine dotnet version
  dotnet --version
  ```
- Publicly addressable https url or tunnel such as [ngrok](https://ngrok.com/) or [Tunnel Relay](https://github.com/OfficeDev/microsoft-teams-tunnelrelay) 

## Setup

1. Run ngrok - point to port 3978

```bash
# ngrok http -host-header=rewrite 3978
```

2. Create a Bot Registration
   In Azure portal, create a [Bot Framework registration resource](https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-authentication?view=azure-bot-service-4.0&tabs=csharp%2Caadv2).

3. Modify the `manifest.json` in the `/AppPackage` folder and replace the `{{BOT-ID}}` with the id from step 2.

4. Zip the contents of `AppPackage` folder into a `manifest.zip`, and use the `manifest.zip` to deploy in app store or add to Teams as in step 7.

5. Modify the `/appsettings.json` and fill in the `{{ Bot Id }}`,`{{ Bot Password }}` and `{{ Connection Name }}` with the id from step 2.

6. Add `{{ Application Base Url }}`in appsetting.json with ngrok tunnel url or deployed application base url. 

7. Upload the manifest.zip to Teams (in the Apps view click "Upload a custom app")
   - Go to Microsoft Teams. From the lower left corner, select Apps
   - From the lower left corner, choose Upload a custom App
   - Go to your project directory, the ./appPackage folder, select the zip folder, and choose Open.
   - Select Add in the pop-up dialog box. Your tab is uploaded to Teams.
    
## To try this sample

- In a terminal, navigate to `TabWithAdpativeCardFlow`

    ```bash
    # change into project folder
    cd # TabWithAdpativeCardFlow
    ```

- Run the bot from a terminal or from Visual Studio, choose option A or B.

  A) From a terminal

  ```bash
  # run the bot
  dotnet run
  ```

  B) Or from Visual Studio

  - Launch Visual Studio
  - File -> Open -> Project/Solution
  - Navigate to `TabWithAdpativeCardFlow` folder
  - Select `TabWithAdpativeCardFlow.csproj` file
  - Press `F5` to run the project

## Interacting with the tab in Teams
    You can use this tab by following the below steps:
    - In the navigation bar located at the far left in Teams, select the ellipses ●●● and choose your app from the list.

## Features of this sample

- Tab showing Adaptive card with action controls.

![Adaptive Card](TabWithAdpativeCardFlow/Images/SignIn.png)

![Adaptive Card](TabWithAdpativeCardFlow/Images/SignInPrompt.png)

![Adaptive Card](TabWithAdpativeCardFlow/Images/TabAdaptiveCardFlow.png)

![Adaptive Card](TabWithAdpativeCardFlow/Images/SampleTaskModuleFetch.png)

![Adaptive Card](TabWithAdpativeCardFlow/Images/SampleTaskModuleSubmit.png)

![Adaptive Card](TabWithAdpativeCardFlow/Images/SignOutMessage.png)

## Deploy the bot to Azure

To learn more about deploying a bot to Azure, see [Deploy your bot to Azure](https://aka.ms/azuredeployment) for a complete list of deployment instructions.

## Further reading

- [Bot Framework Documentation](https://docs.botframework.com)
- [Bot Basics](https://docs.microsoft.com/azure/bot-service/bot-builder-basics?view=azure-bot-service-4.0)
- [Activity processing](https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-concept-activity-processing?view=azure-bot-service-4.0)
- [Azure Bot Service Introduction](https://docs.microsoft.com/azure/bot-service/bot-service-overview-introduction?view=azure-bot-service-4.0)
- [Azure Bot Service Documentation](https://docs.microsoft.com/azure/bot-service/?view=azure-bot-service-4.0)
- [.NET Core CLI tools](https://docs.microsoft.com/en-us/dotnet/core/tools/?tabs=netcore2x)
- [Azure CLI](https://docs.microsoft.com/cli/azure/?view=azure-cli-latest)
- [Azure Portal](https://portal.azure.com)
- [Language Understanding using LUIS](https://docs.microsoft.com/en-us/azure/cognitive-services/luis/)
- [Channels and Bot Connector Service](https://docs.microsoft.com/en-us/azure/bot-service/bot-concepts?view=azure-bot-service-4.0)
