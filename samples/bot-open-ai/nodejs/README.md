# How to use this application. 

This application is created using the Visual Studio Code and Teams toolkit. This application has a conversation bot within Teams. User can ask question and bot will answer the queries. Application is a sample example to show usage of Azure Open AI ChatGPT for virtual assistant scenario.

Use Azure Open AI studio [https://oai.azure.com/] to plan for different models. Deploy the model using the ChatGPT playground.
You can learn about Azure Open AI api from here. [https://learn.microsoft.com/en-us/azure/cognitive-services/openai/overview]
Design your prompt for Azure OpenAI using Chat Markup Language (ChatML). [https://learn.microsoft.com/en-us/azure/cognitive-services/openai/how-to/chatgpt]

## Prerequisites

- [Node.js](https://nodejs.org/en/), supported versions: 14, 16, 18
- An M365 account. If you do not have M365 account, apply one from [M365 developer program](https://developer.microsoft.com/en-us/microsoft-365/dev-program)
- [Teams Toolkit Visual Studio Code Extension](https://aka.ms/teams-toolkit) version after 1.55 or [TeamsFx CLI](https://aka.ms/teamsfx-cli)

- Create Azure Open AI resources from Azure subscription. [https://portal.azure.com/]. Create Aziure OpeAI resources. 
- Request for access if not already done by filling the form while creating the Azure OpenAI resource. It may take anywhere from 8-10 days.
- Replace the below values in config file ".env.teamsfx.local"
- AZURE_OPENAI_KEY=OpenAIapi endpoint from azure portal
- BASE_URL=https://resourcegroup.openai.azure.com
- DEPLOYMENT_NAME=deploymentname
## Debug

- From Visual Studio Code: Start debugging the project by hitting the `F5` key in Visual Studio Code. 
- Alternatively use the `Run and Debug Activity Panel` in Visual Studio Code and click the `Run and Debug` green arrow button.
- From TeamsFx CLI: Start debugging the project by executing the command `teamsfx preview --local` in your project directory.

## Edit the manifest

You can find the Teams app manifest in `templates/appPackage` folder. The folder contains one manifest file:
* `manifest.template.json`: Manifest file for Teams app running locally or running remotely (After deployed to Azure).

This file contains template arguments with `{...}` statements which will be replaced at build time. You may add any extra properties or permissions you require to this file. See the [schema reference](https://docs.microsoft.com/en-us/microsoftteams/platform/resources/schema/manifest-schema) for more information.

## Deploy to Azure

Deploy your project to Azure by following these steps:

| From Visual Studio Code                                                                                                                                                                                                                                                                                                                                                  | From TeamsFx CLI                                                                                                                                                                                                                    |
| :----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| <ul><li>Open Teams Toolkit, and sign into Azure by clicking the `Sign in to Azure` under the `ACCOUNTS` section from sidebar.</li> <li>After you signed in, select a subscription under your account.</li><li>Open the Teams Toolkit and click `Provision in the cloud` from DEPLOYMENT section or open the command palette and select: `Teams: Provision in the cloud`.</li><li>Open the Teams Toolkit and click `Deploy to the cloud` or open the command palette and select: `Teams: Deploy to the cloud`.</li></ul> | <ul> <li>Run command `teamsfx account login azure`.</li> <li>Run command `teamsfx account set --subscription <your-subscription-id>`.</li> <li> Run command `teamsfx provision`.</li> <li>Run command: `teamsfx deploy`. </li></ul> |

> Note: Provisioning and deployment may incur charges to your Azure Subscription.

## Preview

Once the provisioning and deployment steps are finished, you can preview your app:

- From Visual Studio Code

  1. Open the `Run and Debug Activity Panel`.
  1. Select `Launch Remote (Edge)` or `Launch Remote (Chrome)` from the launch configuration drop-down.
  1. Press the Play (green arrow) button to launch your app - now running remotely from Azure.

- From TeamsFx CLI: execute `teamsfx preview --remote` in your project directory to launch your application.

## Validate manifest file

To check that your manifest file is valid:

- From Visual Studio Code: open the command palette and select: `Teams: Validate manifest file`.
- From TeamsFx CLI: run command `teamsfx validate` in your project directory.

## Package

- From Visual Studio Code: open the Teams Toolkit and click `Zip Teams metadata package` or open the command palette and select `Teams: Zip Teams metadata package`.
- Alternatively, from the command line run `teamsfx package` in the project directory.

## Publish to Teams

Once deployed, you may want to distribute your application to your organization's internal app store in Teams. Your app will be submitted for admin approval.

- From Visual Studio Code: open the Teams Toolkit and click `Publish to Teams` or open the command palette and select: `Teams: Publish to Teams`.
- From TeamsFx CLI: run command `teamsfx publish` in your project directory.

## Play with Message Extension

This template provides some sample functionality:

- You can search for `npm` packages from the search bar.

- You can create and send an adaptive card.

  ![CreateCard](./images/AdaptiveCard.png)

- You can share a message in an adaptive card form.

  ![ShareMessage](./images/ShareMessage.png)

- You can paste a link that "unfurls" (`.botframework.com` is monitored in this template) and a card will be rendered.

  ![ComposeArea](./images/LinkUnfurlingImage.png)

To trigger these functions, there are multiple entry points:

- `@mention` Your message extension, from the `search box area`.

  ![AtBotFromSearch](./images/AtBotFromSearch.png)

- `@mention` your message extension from the `compose message area`.

  ![AtBotFromMessage](./images/AtBotInMessage.png)

- Click the `...` under compose message area, find your message extension.

  ![ComposeArea](./images/ThreeDot.png)

- Click the `...` next to any messages you received or sent.

  ![ComposeArea](./images/ThreeDotOnMessage.png)

## Further reading

### Bot

- [Bot Basics](https://docs.microsoft.com/azure/bot-service/bot-builder-basics?view=azure-bot-service-4.0)
- [Bot Framework Documentation](https://docs.botframework.com/)
- [Azure Bot Service Introduction](https://docs.microsoft.com/azure/bot-service/bot-service-overview-introduction?view=azure-bot-service-4.0)

### Message Extension

- [Search Command](https://docs.microsoft.com/en-us/microsoftteams/platform/messaging-extensions/how-to/search-commands/define-search-command)
- [Action Command](https://docs.microsoft.com/en-us/microsoftteams/platform/messaging-extensions/how-to/action-commands/define-action-command)
- [Link Unfurling](https://docs.microsoft.com/en-us/microsoftteams/platform/messaging-extensions/how-to/link-unfurling?tabs=dotnet)
