# Microsoft Teams Samples
[![Sample code build status](https://github.com/OfficeDev/Microsoft-Teams-Samples/actions/workflows/build-complete-samples.yml/badge.svg)](https://github.com/OfficeDev/Microsoft-Teams-Samples/actions/workflows/build-complete-samples.yml)

> [!IMPORTANT]
> The **Bot Framework SDK** repository on GitHub is being archived. This project is no longer updated or maintained. Existing customers can continue to use it without disruption. The tool is no longer supported through service tickets in the Azure portal and won’t receive product updates. Support tickets for the Bot Framework SDK are **no longer serviced as of December 31, 2025**.
>
> **Simplify building AI-powered bots for Microsoft Teams with the new Teams SDK.** Teams SDK reduces boilerplate code and external dependencies while providing native AI capabilities and Teams-specific abstractions. This enables you to focus on building intelligent bot experiences without complex setup or configuration.
>
> Consider using the **Teams SDK** for a faster, streamlined development process. The Teams SDK supports C#, JavaScript, and Python. If you have an existing bot built with the Bot Framework SDK, you can update it to the Teams SDK. For details on core changes and migration steps, see [Bot Framework SDK to Teams SDK (Teams AI Library) migration guidance](https://learn.microsoft.com/en-us/microsoftteams/platform/teams-ai-library/migrations/botbuilder/overview?pivots=typescript).

##### [Click here to find out what's new with Microsoft Teams Platform](https://docs.microsoft.com/microsoftteams/platform/whats-new)

>NOTE: These samples are designed to help understand Microsoft Teams platform capabilities and scenarios. If you are looking for production ready apps then please visit [App Templates for Microsoft Teams](https://docs.microsoft.com/microsoftteams/platform/samples/app-templates)

The easy way to browse through all the samples is to visit the [Microsoft 365 Sample Solution Gallery](https://adoption.microsoft.com/en-us/sample-solution-gallery/?keyword=&sort-by=updateDateTime-true&page=1&product=Teams)

# Sample lists

1. [Microsoft 365 Copilot samples](#Microsoft-365-Copilot-samples)
1. [Teams Toolkit samples](#samples-built-using-new-generation-of-teams-development-tool---teams-toolkit)
1. [Getting Started Samples and Tutorials](#getting-started---samples-and-tutorials)
1. [Tabs samples](#Tabs-samples)
1. [Bots samples (using the v4 SDK)](#Bots-samples-using-the-v4-SDK)
1. [Messaging Extensions samples (using the v4 SDK)](#Messaging-Extensions-samples-using-the-v4-SDK)
1. [Webhooks and Connectors samples](#Webhooks-and-Connectors-samples)
1. [Graph APIs](#Graph-APIs)
1. [Calls and online meetings bots](#Calls-and-online-meetings-bots)
1. [Meetings samples](#Meetings-samples)
1. [Actions Preview](#Actions-preview)
1. [Scenario specific samples](#Scenario-specific-samples)

## [Microsoft 365 Copilot samples](https://github.com/OfficeDev/Copilot-for-M365-Samples)
| Name               | Description                                                                 | Level        | .NET                                                                                     | TypeScript                                                                                      | JavaScript                                                                                      | Python                                                                                      |
| ------------------ | :----------------------------------------------------------------------- | :----------: | --------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------- |
| Trey Research       | Declarative Agent with API Plugin											  | Advanced | [View](https://github.com/OfficeDev/Microsoft-365-Copilot-Samples/tree/main/samples/cext-trey-research-csharp) | [View](https://github.com/OfficeDev/Microsoft-365-Copilot-Samples/tree/main/samples/cext-trey-research) |                                                                                               |[View](https://github.com/OfficeDev/Microsoft-365-Copilot-Samples/tree/main/samples/cext-trey-research-python)          |

## [Samples built using new generation of Teams development tool - Teams Toolkit](https://github.com/OfficeDev/TeamsFx-Samples)

The [Teams Toolkit](https://marketplace.visualstudio.com/items?itemName=TeamsDevApp.ms-teams-vscode-extension) is an extension on Visual Studio Code and Visual Studio. It enable Teams developers to create and deploy Teams apps with integrated identity, access to cloud storage, data from Microsoft Graph, and other services in Azure and M365 with a "zero-configuration" approach to the developer experience. And [Sample Apps](https://github.com/OfficeDev/TeamsFx-Samples) are provided in Teams Toolkit. Download and try it by today! [Learn more about Teams Toolkit](https://docs.microsoft.com/microsoftteams/platform/toolkit/visual-studio-code-overview).


## [Getting Started - Samples and Tutorials](https://docs.microsoft.com/microsoftteams/platform/tutorials/get-started-dotnet-app-studio)

|    | Sample Name        | Description                                                                                                                | Level       | C#    | TypeScript | App Manifest  |
|:--:|:-------------------|:---------------------------------------------------------------------------------------------------------------------------|:-----------:|:--------|:-------------|:-------------|
| 1  | Hello World        | A Microsoft Teams Hello World sample app built with .NET/NodeJS that demonstrates essential features like tabs, bots, and messaging extensions for seamless interaction within the Teams environment. | Basic    | [View][app-hello-world#cs] | [View][app-hello-world#ts] ![toolkit-icon](assets/toolkit-icon.png) | [View](/samples/app-hello-world/csharp/demo-manifest/app-hello-world.zip) |

## [AI samples](https://learn.microsoft.com/en-us/azure/ai-services/document-intelligence/?view=doc-intel-4.0.0)

|    | Sample Name             | Description                                                                                                               | Level        | JavaScript                                  | App Manifest                                                        |
|----|-------------------------|---------------------------------------------------------------------------------------------------------------------------|--------------|---------------------------------------------|--------------------------------------------------------------------|
| 1  | msgext-expert-finder-js  | This sample implements a Teams message extension for Copilot, enabling users to query a database of candidates based on skills, location, and availability. | Basic        | [View][msgext-expert-finder#nodejs]         | -                                                                  |
| 2  | api-doc-search           | This is a sample API application that demonstrates how to perform RAG-based semantic search using NoSQL CosmosDB.          | Intermediate | [View][api-doc-search#nodejs]              | -                                                                  |
| 3  | Compliance Checker       | Use Azure OpenAI to compare proposal documents against a guideline document and return whether the proposal complies and why/why not. | Advanced     | [View][msgext-ai-doc-compliance-checker#nodejs] ![toolkit-icon](assets/toolkit-icon.png) | [View](/samples/msgext-ai-doc-compliance-checker/demo-manifest/AIComplianceChecker.zip) |
| 4  | bot-virtual-assistant    | This is a sample application that demonstrates a root bot that routes inputs to skill bots for tailored responses.          | Advanced     | [View][bot-virtual-assistant#nodejs]        | -                                                                  |
| 5  | bot-ai-meeting-helper    | This sample demonstrates how Azure Open AI extracts action items from meeting transcriptions for all participants who subscribed to the meeting, and subsequently sends these action items to each individual user in a 1:1 chat after the meeting concludes. | Advanced     | [View][bot-ai-meeting-helper#nodejs] ![toolkit-icon](assets/toolkit-icon.png) | [View](/samples/bot-ai-meeting-helper/demo-manifest/bot-ai-meeting-helper.zip) |

## [Tabs samples](https://docs.microsoft.com/microsoftteams/platform/tabs/what-are-tabs)
| #  | Sample Name                                | Description                                                                                                                                                                                                                       | Level        | C#                                      | TypeScript                                   | JavaScript                                   | App Manifest                                                        |
|----|--------------------------------------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|--------------|-----------------------------------------|----------------------------------------------|---------------------------------------------|--------------------------------------------------------------------|
| 1  | Personal tabs                              | Sample app showing custom personal Tab with ASP.NET Core.                                                                                                                                                                       | Basic        | [MVC][personal-tab#cs#mvc], [Razor][personal-tab#cs#razor] | [Yeoman Generator](https://docs.microsoft.com/en-us/microsoftteams/platform/get-started/get-started-yeoman) |                                             | [View](/samples/tab-personal/mvc-csharp/demo-manifest/tab-personal.zip) |
| 2  | Personal tab quick-start                   | Sample personal tab quick-start app.                                                                                                                                                                                            | Basic        |                                         | [View][personal-tab-quickstart#ts] ![toolkit-icon](assets/toolkit-icon.png) | [View][personal-tab-quickstart#js] ![toolkit-icon](assets/toolkit-icon.png) |                                                                    |
| 3  | Channel and group tabs                     | Sample app showing custom group and channel Tab with ASP.NET Core.                                                                                                                                                              | Basic        | [MVC][group-channel-tab#cs#mvc], [Razor][group-channel-tab#cs#razor] | [Yeoman Generator](https://docs.microsoft.com/en-us/microsoftteams/platform/get-started/get-started-yeoman) |                                             | [View](/samples/tab-channel-group/mvc-csharp/demo-manifest/tab-channel-group.zip) |
| 4  | Channel and group tab quick-start          | Sample channel and group tab hello world app.                                                                                                                                                                                   | Basic        | [View][group-tab-quickstart#csharp]     | [View][group-tab-quickstart#ts] ![toolkit-icon](assets/toolkit-icon.png) | [View][group-tab-quickstart#js] ![toolkit-icon](assets/toolkit-icon.png) | [View](/samples/tab-channel-group-quickstart/js/demo-manifest/tab-channel-group-quickstart.zip) |
| 5  | SPFx Tab                                   | Sample app showing Microsoft Teams tabs using SharePoint Framework.                                                                                                                                                             | Basic        |                                         | [View][group-channel-tab#ts#spfx]           |                                             |                                                                    |
| 6  | Tab people picker                          | This is a tab app that shows the feature of the client SDK people picker.                                                                                                                                                         | Basic        | [View][tab-people-picker#csharp]        |                                             | [View][tab-people-picker#nodejs] ![toolkit-icon](assets/toolkit-icon.png) | [View](/samples/tab-people-picker/csharp/demo-manifest/Tab-People-Picker.zip) |
| 7  | Tab channel context                        | This sample shows the contents of the tab context object in a private and shared channel.                                                                                                                                         | Basic        |                                         | [View][tab-channel-context#nodejs] ![toolkit-icon](assets/toolkit-icon.png) |                                             |                                                                    |
| 8  | tab-ui-templates                           | This sample app provides tested, high-quality UI templates to help developers understand how to create effective applications for Microsoft Teams, covering common use cases like dashboards and forms.                             | Basic        |                                         | [View][tab-ui-templates#ts] ![toolkit-icon](assets/toolkit-icon.png) |                                             |                                                                    |
| 9  | Personal tab with SSO quick-start          | Sample personal tab with SSO hello world app.                                                                                                                                                                                   | Intermediate | [View][personal-tab-sso-quickstart#csharp] | [View][personal-tab-sso-quickstart#ts] ![toolkit-icon](assets/toolkit-icon.png) | [View][personal-tab-sso-quickstart#js] ![toolkit-icon](assets/toolkit-icon.png) |[View](/samples/tab-personal-sso-quickstart/csharp_dotnetcore/demo-manifest/tab-personal-sso-quickstart.zip) |                                                                    |
| 10 | Channel and group tab with SSO quick-start | Sample channel and group tab with SSO hello world app.                                                                                                                                                                          | Intermediate | [View][group-tab-sso-quickstart#csharp] | [View][group-tab-sso-quickstart#ts] ![toolkit-icon](assets/toolkit-icon.png) | [View][group-tab-sso-quickstart#js] ![toolkit-icon](assets/toolkit-icon.png) |[View](/samples/tab-channel-group-sso-quickstart/csharp_dotnetcore/demo-manifest/tab-channel-group-sso-quickstart.zip) |                                                                  |
| 11 | Tab SSO | Microsoft Teams sample app demonstrating Azure AD SSO in a Teams tab using C#. | Intermediate | [View][tab-sso#cs] | [View][tab-sso#ts] ![toolkit-icon](assets/toolkit-icon.png) |      | [View](/samples/tab-sso/csharp/demo-manifest/tab-sso.zip) |
| 12 | Deep Link consuming Subentity ID           | Microsoft Teams sample app for demonstrating deeplink from Bot chat to Tab consuming Subentity ID.                                                                                                                              | Intermediate | [View][tab-deeplink#csharp]             |                                             | [View][tab-deeplink#nodejs] ![toolkit-icon](assets/toolkit-icon.png) | | [View]("/samples/tab-deeplink/csharp/demo-manifest/tab-deeplink.zip")                                                                |
| 13 | Device permissions                         | Microsoft Teams tab sample app for demonstrating device permissions.                                                                                                                                                              | Intermediate |                                         | [View][tab-device-permissions#js] ![toolkit-icon](assets/toolkit-icon.png) |                                             | [View](/samples/tab-device-permissions/nodejs/demo-manifest/tab-device-permissions.zip) |
| 14 | Tab in stage view                          | This sample app demonstrates the use of Teams tab in stage view using C#, showcasing collaborative features and interactive elements.                                                                                             | Intermediate | [View][tab-stage-view#csharp]          |                                             | [View][tab-stage-view#js] ![toolkit-icon](assets/toolkit-icon.png) | [View](/samples/tab-stage-view/csharp/demo-manifest/tab-stage-view.zip) |
| 15 | Product inspection                         | Demonstrating a feature where the user can scan a product and mark it as approved/rejected.                                                                                                                                       | Intermediate | [View][tab-product-inspection#csharp]  |                                             | [View][productinspection#nodejs] ![toolkit-icon](assets/toolkit-icon.png) | [View](/samples/tab-product-inspection/csharp/demo-manifest/Tab-Product-Inspection.zip) |
| 16 | Tab Meeting Transcript Recording           | This sample shows meeting transcripts and recordings.                                                                                                                                                                             | Intermediate | [View][tab-meeting-transcript-recording#csharp] |                                             | [View][tab-meeting-transcript-recording#nodejs] ![toolkit-icon](assets/toolkit-icon.png) |                                                                    |
| 17 | Tab DeepLink Pass Values                   | Microsoft Teams sample which displays deeplinks based on the values passed dynamically to the context.                                                                                                                              | Intermediate |                                         | [View][tab-deeplink-pass-values#ts]          |                                             |                                                                    |
| 18 | Offline Support Tickets                    | This sample app demonstrates a classic CRUD application that functions offline within Microsoft Teams. When the Android device reconnects to the internet, users can sync their data with blob storage.                          | Intermediate |                                         | [View][tab-support-offline#nodejs] ![toolkit-icon](assets/toolkit-icon.png) | [View](/samples/tab-support-offline/nodejs/demo-manifest/tab-support-offline.zip) |
| 19 | Tab AdhocCalls Transcripts & Recording           | AdhocCalls for group call recording, transcription, and UI display.                                                                                                                                                                             | Intermediate |    |                                             | [View][tab-adhoccalls-transcript-recording#nodejs] ![toolkit-icon](assets/toolkit-icon.png) |                                                                    |
| 20 | Tab DeepLink Pass Values                   | Microsoft Teams sample which displays deeplinks based on the values passed dynamically to the context.                                                                                                                              | Intermediate |                                         | [View][tab-deeplink-pass-values#ts]          |                                             |                                                                    |
| 21 | Offline Support Tickets                    | This sample app demonstrates a classic CRUD application that functions offline within Microsoft Teams. When the Android device reconnects to the internet, users can sync their data with blob storage.                          | Intermediate |                                         | [View][tab-support-offline#nodejs] ![toolkit-icon](assets/toolkit-icon.png) | [View](/samples/tab-support-offline/nodejs/demo-manifest/tab-support-offline.zip) |
| 22 | Staggered Permission                       | This sample demos app to request staggered Graph API permissions in Teams tabs.                                                                                                                                                   | Advanced     | [View][tab-staggered-permission#csharp] |                                             | [View][tab-staggered-permission#nodejs] ![toolkit-icon](assets/toolkit-icon.png) | [View](/samples/tab-staggered-permission/csharp/demo-manifest/tab-staggered-permission.zip) |
| 23 | Tab Multi Hub                              | Interact with the user's calendar, mail, profile, geolocation from a personal tabs app.                                                                                                                                           | Advanced     |                                         | [View][tab-multi-hub#ts] ![toolkit-icon](assets/toolkit-icon.png) |                                             |                                                                    |
| 24 | Tab External Auth                          | This sample illustrates how to implement Google auth using external auth providers.                                                                                                                                               | Advanced     | [View][tab-external-auth#csharp]       |                                             |                                             |                                                                    |
| 25 | Nested App Authentication                  | This sample shows nested app authentication (NAA), a new protocol that simplifies single sign-on (SSO) for single-page applications.                                           | Advanced     | [View][tab-nested-auth#cs]             | [View][tab-nested-auth#nodejs] ![toolkit-icon](assets/toolkit-icon.png) |               |[View](/samples/tab-nested-auth/csharp/demo-manifest/tab-nested-auth.zip) |                                            |                                                                    |
| 26 | Tab Meeting Recording and transcript with auto recording | This sample shows meeting recording and transcript with auto recording.                                                                                                         | Advanced     | [View][tab-meeting-auto-recording#csharp] |                                             |                                             |                                                                    |
| 27 | Tab External Auth                          | This sample illustrates how to implement Google auth using external auth providers.                                                                                                                                               | Advanced     | [View][tab-external-auth#csharp]       |                                             |                                             |                                                                    |
| 28 | Tab Meeting Recording and transcript with auto recording | This sample shows meeting recording and transcript with auto recording.                                                                                                         | Advanced     | [View][tab-meeting-auto-recording#csharp] |                                             |                                             |                                                                    |
| 29 | Tab App Navigation                         | Sample app demonstrating tab navigation features in Microsoft Teams using ASP.NET Core.                                                                                                                                          | Basic        | [View][tab-app-navigation#csharp]       |                                             |                                             |                                                                    |
## [Bots samples](https://docs.microsoft.com/microsoftteams/platform/bots/what-are-bots) (using the v4 SDK)
>NOTE:
>Visit the [Bot Framework Samples repository][botframework] to view Microsoft Bot Framework v4 SDK task-focused samples for C#, JavaScript, TypeScript, and Python.

| Serial | Sample Name | Description | Level       | .NET | JavaScript | Python | Java | App Manifest |
|:------:|:-------------------|:---------------------------------------------------------------------------------|:------------|:--------|:-------------|:-------------|:-------------|:-------------|
| 1 | Teams Conversation Bot quick-start | This app showcases bot conversation events and messaging extensions for Microsoft Teams using the Bot Framework v4. | Basic       | | [View][bot-conversation-quickstart#js] ![toolkit-icon](assets/toolkit-icon.png) | | |
| 2 | Teams Conversation Bot SSO quick-start | This sample bot demonstrates implementing SSO in Microsoft Teams using Azure AD. | Basic       | [View][bot-conversation-sso-quickstart#csharp_dotnetcore] | [View][bot-conversation-sso-quickstart#js] ![toolkit-icon](assets/toolkit-icon.png) | [View][bot-conversation-sso-quickstart#python] | |[View](/samples/bot-conversation-sso-quickstart/csharp_dotnetcore/demo-manifest/bot-conversation-sso-quickstart.zip) |
| 3 | Teams Conversation Bot | Messaging and conversation event handling also includes read receipt event functionality in personal scope. Also include AI label, Citations, Feedback buttons, Sensitivity label. | Intermediate | [View][bot-conversation#cs] | [View][bot-conversation#js] ![toolkit-icon](assets/toolkit-icon.png) | [View][bot-conversation#python] | [View][bot-conversation#java] | [View](/samples/bot-conversation/csharp/demo-manifest/bot-conversation.zip) |
| 4 | Message Reactions | This sample app demonstrates how to use message reactions in Microsoft Teams with a bot built on the Bot Framework. | Intermediate | [View][bot-message-reaction#cs] | [View][bot-message-reaction#js] ![toolkit-icon](assets/toolkit-icon.png) | | [View][bot-message-reaction#java] | [View](/samples/bot-message-reaction/csharp/demo-manifest/bot-message-reaction.zip) |
| 5 | Authentication with OAuthPrompt | Authentication and basic messaging in Bot Framework v4. | Intermediate | [View][bot-teams-authentication#cs] | [View][bot-teams-authentication#nodejs] ![toolkit-icon](assets/toolkit-icon.png) | [View][bot-teams-authentication#python] | [View][bot-teams-authentication#java] | [View](/samples/bot-teams-authentication/csharp/demo-manifest/bot-teams-authentication.zip) |
| 6 | Teams File Upload | This bot sample for Teams demonstrates file upload capabilities using Bot Framework v4, enabling users to upload files and view inline images within chats. | Intermediate | [View][bot-file-upload#cs] | [View][bot-file-upload#js] ![toolkit-icon](assets/toolkit-icon.png) | [View][bot-file-upload#python] | [View][bot-file-upload#java] | [View](/samples/bot-file-upload/csharp/demo-manifest/bot-file-upload.zip) |
| 7 | Task Module | Demonstrating how to retrieve a Task Module and values from cards in it, for a Messaging Extension. | Intermediate | [View][bot-task-module#cs] | [View][bot-task-module#js] ![toolkit-icon](assets/toolkit-icon.png) | [View][bot-task-module#python] | [View][bot-task-module#java] | [View](/samples/bot-task-module/csharp/demo-manifest/bot-task-module.zip) |
| 8 | Start new thread in a channel | This app demonstrates how to start a conversation thread in a specific Teams channel using Bot Framework v4. | Intermediate | [View][bot-initiate-thread-in-channel#cs] | [View][bot-initiate-thread-in-channel#js] ![toolkit-icon](assets/toolkit-icon.png) | [View][bot-initiate-thread-in-channel#python] | [View][bot-initiate-thread-in-channel#java] | [View](/samples/bot-initiate-thread-in-channel/csharp/demo-manifest/bot-initiate-thread-in-channel.zip) |
| 9 | Universal bots | This Teams Catering Bot allows users to place food and drink orders seamlessly within Microsoft Teams. It leverages Universal Adaptive Cards for dynamic interactions and supports features like recent orders and order confirmation. | Intermediate | [View](samples/bot-teams-catering/csharp) | | | |
| 10 | Sequential workflow adaptive cards | Demonstrating on how to implement sequential flow, user specific view and upto date adaptive cards in bot. | Intermediate | [View][sequential#workflow#csharp] | [View][sequential#workflow#js] ![toolkit-icon](assets/toolkit-icon.png) |[View][sequential#workflow#python] ![toolkit-icon](assets/toolkit-icon.png) | | [View](/samples/bot-sequential-flow-adaptive-cards/csharp/demo-manifest/bot-sequential-flow-adaptive-cards.zip) |
| 11 | Channel messages with RSC permissions | This bot can capture all channel messages in Teams using RSC permissions, without the need for @mentions. | Intermediate | [View][messageswithrsc#csharp] | [View][messageswithrsc#js] ![toolkit-icon](assets/toolkit-icon.png) | | | [View](/samples/bot-receive-channel-messages-withRSC/csharp/demo-manifest/Bot-RSC.zip) |
| 12 | Bot with SharePoint file to view in Teams file viewer | This sample demos a bot with capability to upload files to SharePoint site and same files can be viewed in Teams file viewer. | Intermediate | [View][botwithsharepointfileviewer#csharp] | [View][botfileviewer#js] ![toolkit-icon](assets/toolkit-icon.png) | | | |
| 13 | Type ahead search control on Adaptive Cards | This sample shows the feature of type ahead search (static and dynamic) control in Adaptive Cards. | Intermediate | [View][typeaheadsearch#csharp] | [View][typeaheadsearchonadaptivecard#js] ![toolkit-icon](assets/toolkit-icon.png) | | | [View](/samples/bot-type-ahead-search-adaptive-cards/csharp/demo-manifest/Typeahead-search-adaptive-cards.zip) |
| 14 | People picker control in Adaptive Cards | This sample shows the feature of people picker control in Adaptive Cards. | Intermediate | [View][peoplepickeronadaptivecard#csharp] | [View][peoplepickeronadaptivecard#js] ![toolkit-icon](assets/toolkit-icon.png) | | | [View](/samples/bot-people-picker-adaptive-card/csharp/demo-manifest/People-picker-adaptive-card.zip) |
| 15 | Proactive Messaging sample | This sample shows how to save user's conversation reference information to send proactive reminder message using Bots. This uses Teams toolkit for Visual Studio Code to provide scaffolding experience. | Intermediate | | [View][bot-proactive-msg-teamsfx#js] ![toolkit-icon](assets/toolkit-icon.png) |[View][bot-proactive-msg-teamsfx#python] ![toolkit-icon](assets/toolkit-icon.png) | | [View](/samples/bot-proactive-messaging-teamsfx/demo-manifest/bot-proactive-messaging-teamsfx.zip) |
| 16 | Proactive Tab Conversations | Using a bot to create and store conversations that can be later used inside a sub-entity, tab conversation. This sample includes the details required to proactively message a channel from a bot, set-up and store details for channel tab conversations, and viewing channel conversations from a personal app. | Advanced | [View][bot-tab-conversations#csharp] | | | |
| 17 | Suggested Actions Bot | This sample shows the feature where user can send suggested actions using bot. | Advanced | [View][suggestedactionsbot#csharp] | [View][suggestedactionsbot#nodejs] ![toolkit-icon](assets/toolkit-icon.png) | | | [View](/samples/bot-suggested-actions/csharp/demo-manifest/bot-suggested-actions.zip) |
| 18 | Adaptive Card Actions Bot | This sample demonstrates how to create and send Adaptive Cards with different action types using a Microsoft Teams bot. It includes features like submitting actions, showing cards, toggling visibility, and more. | Advanced | [View][AdaptiveCardActions#csharp] | [View][AdaptiveCardActions#nodejs] ![toolkit-icon](assets/toolkit-icon.png) | | | [View](/samples/bot-adaptive-card-actions/csharp/demo-manifest/bot-adaptivecard-actions.zip) |
| 19 | SSO for your Adaptive Cards | This sample code demonstrates how to get enable SSO authentication for your Adaptive Cards Universal Actions in bot. | Advanced | [View][AdaptiveCardSSO#csharp] | [View][BotSSOAdaptiveCard#nodejs] ![toolkit-icon](assets/toolkit-icon.png) | | |[View](/samples/bot-sso-adaptivecard/csharp/demo-manifest/bot-sso-adaptivecard.zip) |
| 20 | Different formatting on cards | This sample feature shows how to use different formatting on cards using bot. | Advanced | [View][BotFormattingCards#csharp] | [View][BotFormattingCards#nodejs] ![toolkit-icon](assets/toolkit-icon.png) |[View][BotFormattingCards#python] ![toolkit-icon](assets/toolkit-icon.png) | |[View](/samples/bot-formatting-cards/csharp/demo-manifest/bot-formatting-cards.zip) |
| 21 | Different types of cards using bot | This sample shows the feature where user can interact with different types of cards using bot like Adaptive, hero, list, Office 365 Connector, receipt, sign in, and thumbnail cards and card collections. | Advanced | [View][TypesCards#csharp] | [View][TypesCards#nodejs] ![toolkit-icon](assets/toolkit-icon.png) | | |
| 22 | User specific views in Adaptive cards | This sample demonstrates how to create user-specific views in Adaptive Cards within Microsoft Teams, using features like Action.Execute and auto-refresh for dynamic updates. The app enables personalized content, allowing seamless interaction across personal, group, and team contexts. | Advanced | [View][botadaptivecardsuserspecificviews#csharp] ![toolkit-icon](assets/toolkit-icon.png) | | | | [View](/samples/bot-adaptivecards-user-specific-views/csharp/demo-manifest/bot-adaptivecards-user-specific-views.zip) |
| 23 | Bot configuration app with type ahead search control. | This sample demonstrates the features of bot configuration and reconfiguration for both teams and group chats, including a type-ahead search (static and dynamic) control on Adaptive Cards. | Advanced | [View][BotConfigurationApp#csharp] | [View][BotConfigurationApp#nodejs] ![toolkit-icon](assets/toolkit-icon.png) | | |[View](/samples/bot-configuration-app/csharp/demo-manifest/bot-configuration-app.zip)|
| 24 | Bot configuration app with auth | This sample demonstrates the features of bot configuration auth and reconfiguration for both teams and group chats. | Advanced | [View][BotConfigurationAppAuth#csharp] | [View][BotConfigurationAppAuth#nodejs]![toolkit-icon](assets/toolkit-icon.png) | | |[View](/samples/bot-configuration-app-auth/csharp/demo-manifest/bot-configuration-auth-app.zip)|
| 25 | Tag mention bot | This sample app demonstrates the use of tag mention functionality in teams scope using Bot Framework. | Advanced | [View][Tagmention#csharp] | [View][Tagmention#nodejs] ![toolkit-icon](assets/toolkit-icon.png) | | |
| 26 | Commands menu and prompt starters bot | This sample adds a command menu with a dropdown list of commands for your bot. Users can select a command from the list, which will insert the command title into the compose box. Then, they can select Send to execute the command. | Advanced | [View][CommandsMenu#csharp] | [View][CommandsMenu#nodejs] ![toolkit-icon](assets/toolkit-icon.png) | | | [View](/samples/bot-commands-menu/csharp/demo-manifest/bot-commands-menu.zip)|
| 27 | Bot All Cards | Sample which demonstrates sending different types cards supported in teams using bot. | Advanced | [View][TypesCards#csharp] | [View][TypesCards#nodejs] ![toolkit-icon](assets/toolkit-icon.png) |[View][TypesCards#python] ![toolkit-icon](assets/toolkit-icon.png) | | [View](/samples/bot-all-cards/csharp/demo-manifest/bot-all-cards.zip) |
| 28 | Apps in Federated | This sample app allows users to easily view a list of group members. When a new member is added, their details are promptly displayed. | Advanced | [View][bot-feed-members#js] ![toolkit-icon](assets/toolkit-icon.png) | | | | [View](/samples/bot-feed-members/nodejs/demo-manifest/bot-feed-members.zip) |
| 29 | Requirement Targeting OneWay Dependency | Microsoft M365 RT sample app in Node.js which specifies one-way-dependency relationships between app capabilities (using elementRelationshipSet) and functionality using hostMustSupportFunctionalities. | Advanced | | [View][RequirementTargetingOneWayDependency#nodejs] ![toolkit-icon](assets/toolkit-icon.png) | | |
| 30 | Requirement Targeting Mutual Dependency | Microsoft M365 RT sample app in Node.js which specifies mutual-dependency relationships between app capabilities using elementRelationshipSet. | Advanced | | [View][RequirementTargetingMutualDependency#nodejs] ![toolkit-icon](assets/toolkit-icon.png) | | |
| 31 | Streaming Bot | This sample showcases the conversational streaming token scenario for teams bot in personal scope. | Advanced | [View][botstreaming#csharp] |[View][botstreaming#nodejs]![toolkit-icon](assets/toolkit-icon.png) | | |
| 32 | Auth0 Bot | This sample demonstrates how to authenticate users in a Microsoft Teams bot using Auth0 login and retrieve their profile details. After authentication, the bot displays the user's name, email, and profile picture in an Adaptive Card. | Intermediate | [View][bot-auth0-adaptivecard#cs] ![toolkit-icon](assets/toolkit-icon.png) | [View][bot-auth0-adaptivecard#js] ![toolkit-icon](assets/toolkit-icon.png) | [View][bot-auth0-adaptivecard#python] ![toolkit-icon](assets/toolkit-icon.png) | | [View](/samples/bot-auth0-adaptivecard/csharp/demo-manifest/bot-auth0-adaptivecard.zip) |
| 33 | Agent Knowledge Hub | Contoso Knowledge Hub is an intelligent guidance agent built on the Teams SDK, designed to empower students in their academic and career journeys. It offers personalized course recommendations, career-aligned planning, institutional insights, and expert-endorsed AI course recommendations. | Intermediate | [View][agent-knowledge-hub#cs] ![toolkit-icon](assets/toolkit-icon.png) | [View][agent-knowledge-hub#js] ![toolkit-icon](assets/toolkit-icon.png) | [View][agent-knowledge-hub#python] ![toolkit-icon](assets/toolkit-icon.png) | | |
| 34 | Bot Shared Channel Events | Microsoft Teams bot can receive transitive member add and remove events in shared channels.| Intermediate | [View][bot-shared-channel-events#cs] ![toolkit-icon](assets/toolkit-icon.png) |  |  | |  |

#### Additional samples
| No. | Sample Name                      | Description                                                                                                                                                                                                 | Level         | .NET                                  | JavaScript                           |  
|-----|:---------------------------------|:----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|:--------------|:--------------------------------------|:-------------------------------------|  
|  1  | Microsoft Teams Tab App Cookie   | This sample application demonstrates key web storage features, including managing cookies, SameSite cookies, and partitioned cookies. It also covers local storage and IndexedDB for data persistence in modern web applications. | Intermediate |                                      | [View][tab-app-cookie#js]            |  
|  2  | User Scope Web App               | This sample app demonstrates posting notifications for team/channel changes using Microsoft Graph API in Node.js.                                                                                        | Intermediate  |                                      | [View][user-scope-web-application#js] |  
|  3  | SharePoint List Bot              | This sample bot for Microsoft Teams captures user input using adaptive cards and stores the data in a SharePoint list. It includes setup for SharePoint registration, permissions configuration, and deployment in Teams for streamlined data management. | Intermediate | [View][bot-sharepoint-list#cs]        |                                      |  
|  4  | Proactive Messaging              | Sample to highlight solutions to two challenges with building proactive messaging apps in Microsoft Teams.                                                                                              | Advanced      | [View][bot-proactive-msg#cs]          |                                      |  
|  5  | Teams Virtual Assistant          | Customized virtual assistant template to support teams capabilities.                                                                                                                                      | Advanced      | [View][app-virtual-assistant#cs]      |                                      |

## [Messaging Extensions samples](https://docs.microsoft.com/microsoftteams/platform/messaging-extensions/what-are-messaging-extensions) (using the v4 SDK)
>NOTE:
>Visit the [Bot Framework Samples repository][botframework] to view Microsoft Bot Framework v4 SDK task-focused samples for C#, JavaScript, TypeScript, and Python.

| #  | Sample Name                               | Description                                                                                                                                                 | Level       | .NET                                                                | JavaScript                                                          | Python | Java | App Manifest                                                |
|----|-------------------------------------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------|-------------|---------------------------------------------------------------------|---------------------------------------------------------------------|--------|------|-------------------------------------------------------------|
| 1  | Messaging extensions - action quick-start | Hello world Messaging Extension that accepts parameters and returns a card. Also, how to receive a forwarded message as a parameter in a Messaging Extension.    | Basic    |                                                                     | [View][msgext-action-quickstart#js] ![toolkit-icon](assets/toolkit-icon.png) |        |      | [View](/samples/msgext-action-quickstart/js/demo-manifest/msgext-action-quickstart.zip) |
| 2  | Messaging extensions - search quick-start | This sample demonstrates a Messaging Extension that accepts search requests and returns results within Microsoft Teams.                                      | Basic    |                                                                     | [View][msgext-search-quickstart#js] ![toolkit-icon](assets/toolkit-icon.png) |        |      | [View](/samples/msgext-search-quickstart/js/demo-manifest/msgext-search-quickstart.zip) |
| 3  | Messaging extensions - action             | Messaging Extension that accepts parameters and returns a card. Also, how to receive a forwarded message as a parameter in a Messaging Extension.             | Intermediate | [View][msgext-action#cs]                                           | [View][msgext-action#js] ![toolkit-icon](assets/toolkit-icon.png)   | [View][msgext-action#python] | [View][msgext-action#java] | [View](/samples/msgext-action/csharp/demo-manifest/msgext-action.zip) |
| 4  | Messaging extensions - search             | This sample demonstrates how to create a Messaging Extension in Microsoft Teams that enables users to perform searches and retrieve results seamlessly.        | Intermediate | [View][msgext-search#cs]                                           | [View][msgext-search#js] ![toolkit-icon](assets/toolkit-icon.png)   | [View][msgext-search#python] | [View][msgext-search#java] | [View](/samples/msgext-search/csharp/demo-manifest/msgext-search.zip) |
| 5  | Messaging extensions - action preview     | Demonstrates how to create a Preview and Edit flow for a Messaging Extension.                                                                                 | Intermediate | [View][msgext-action-preview#cs]                                   | [View][msgext-action-preview#js] ![toolkit-icon](assets/toolkit-icon.png) | [View][msgext-action-preview#python] | [View][msgext-action-preview#java] | [View](/samples/msgext-action-preview/csharp/demo-manifest/msgext-action-preview.zip) |
| 6  | Link unfurling                             | This sample app demonstrates how to implement link unfurling in a Teams messaging extension using the Bot Framework v4.                                       | Intermediate | [View][msgext-link-unfurling#cs]                                   | [View][msgext-link-unfurling#js] ![toolkit-icon](assets/toolkit-icon.png) | [View][msgext-link-unfurling#python] | [View][msgext-link-unfurling#java] | [View](/samples/msgext-link-unfurling/csharp/demo-manifest/msgext-link-unfurling.zip) |
| 7  | Messaging extensions - SSO auth           | This sample demonstrates a Messaging Extension for Microsoft Teams that includes a configuration page and supports SSO authentication for enhanced user experience. | Advanced    | [View][msgext-search-sso-config#cs]                               | [View][msgext-search-sso-config#js] ![toolkit-icon](assets/toolkit-icon.png) |        |      | [View](/samples/msgext-search-sso-config/csharp/demo-manifest/msgext-search-sso-config.zip)                                                             |
| 8  | Messaging extensions - auth and config    | This sample demonstrates how to create a search Messaging Extension in Microsoft Teams that includes a configuration page and supports user authentication.     | Advanced    | [View][msgext-search-auth-config#cs]                               |                                                                     | [View][msgext-search-auth-config#python] |      | [View](/samples/msgext-search-auth-config/csharp/demo-manifest/msgext-search-auth-config.zip) |
| 9  | Messaging Extensions AI - Sentiment Analysis | Messaging Extension with AI capability that performs sentiment analysis (positive/negative/neutral) for messages posted in Microsoft Teams chat               | Advanced    | [View][msgext-ai-sentiment-analysis#cs]                             | [View][msgext-ai-sentiment-analysis#js] ![toolkit-icon](assets/toolkit-icon.png) |[View][msgext-ai-sentiment-analysis#python] ![toolkit-icon](assets/toolkit-icon.png)        |      |                                                             |
| 10 | Messaging extension - Adaptive Card based loop components | Demonstrates a Messaging Extension for Microsoft Teams that utilizes adaptive card-based loop components, enabling link unfurling and dynamic interactions within the compose area. | Advanced    | [View][msgext-unfurling-ac-loop-components#cs]                     | [View][msgext-unfurling-ac-loop-components#nodejs] ![toolkit-icon](assets/toolkit-icon.png) |        |      | [View](/samples/msgext-unfurling-ac-loop-components/csharp/demo-manifest/msgext-unfurling-ac-loop-components.zip) |
| 11 | Copilot Handoff                            | This sample implements a Teams message extension that can be used as a plugin for Microsoft Copilot for Microsoft 365. The message extension showcases copilot handoff along with allowing users to query the Northwind Database | Advanced    | | [View][msgext-copilot-handoff#js] ![toolkit-icon](assets/toolkit-icon.png) |        |      |                                                             |
| 12 | Third-Party Cloud Storage | This app enables seamless integration with third-party cloud storage providers for files dragged and dropped in Teams chats or channels. It uses the Microsoft Teams JavaScript SDK's thirdPartyCloudStorage module to fetch and upload files efficiently. | Advanced | [View][msgext-thirdparty-storage#cs]  | [View][msgext-thirdparty-storage#nodejs] ![toolkit-icon](assets/toolkit-icon.png) |  [View][msgext-thirdparty-storage#python]  |    |   |

#### Additional samples

| #  | Sample Name                              | Description                                                                 | Level      | .NET                                                               | JavaScript | Python |
|----|------------------------------------------|-----------------------------------------------------------------------------|------------|--------------------------------------------------------------------|------------|--------|
| 1  | Link unfurling demo of Reddit            | Messaging Extension with Link Unfurling Samples for Reddit Links            | Intermediate| [View][msgext-link-unfurl#cs]                                       |            |        |
| 2  | Link unfurling - setup a meeting         | This sample illustrates a common scenario where a user shares a link to a resource with a group of users, and they collaborate to review it in a meeting. | Intermediate| [View][msgext-link-unfurl-meeting#cs]                               |            |        |

## [Webhooks and Connectors samples](https://docs.microsoft.com/microsoftteams/platform/webhooks-and-connectors/what-are-webhooks-and-connectors)

> [!IMPORTANT]
>
> The existing Microsoft 365 (previously called Office 365) connectors across all cloud platforms are nearing deprecation, and the creation of new Microsoft 365 connectors will soon be blocked. For more information on the schedule and how the Workflows app provides a more flexible and secure experience, see [retirement of Microsoft 365 connectors within Microsoft Teams](https://devblogs.microsoft.com/microsoft365dev/retirement-of-office-365-connectors-within-microsoft-teams/).

| #  | Sample Name                      | Description                                                                                             | Level      | C#                                | JavaScript                          |
|----|-----------------------------------|---------------------------------------------------------------------------------------------------------|------------|-----------------------------------|-------------------------------------|
| 1  | Connectors                        | Sample Office 365 Connector generating notifications to Teams channel.                                  | Intermediate| [View][connector#cs]              | [View][connector#ts]               |
| 2  | Generic connectors sample         | Sample code for a generic connector that's easy to customize for any system which supports webhooks.     | Intermediate|                                   | [View][connector-generic#ts]       |
| 3  | Incoming Webhook                  | This sample demonstrates a Teams tab to send message cards using Incoming Webhook, showcasing the HttpPOST action for interactive cards. | Intermediate| [View][incoming-webhook#cs]       | [View][incoming-webhook#nodejs]    |
| 4  | Outgoing Webhooks                 | Implement Outgoing Webhook for Microsoft Teams for external integrations.                                | Intermediate| [View][outgoing-webhook#cs]       | [View][outgoing-webhook#ts]        |
| 5  | Authentication in Todo Connector App | This is a sample app which shows connector authentication and sends notification on task creation.       | Advanced   |                                   | [View][auth-in-connector#nodejs]   |

## [Graph APIs](https://docs.microsoft.com/graph/teams-concept-overview)

| #  | Sample Name                                          | Description                                                                                             | Level        | C#                                | JavaScript                          | App Manifest                                                        |
|----|------------------------------------------------------|---------------------------------------------------------------------------------------------------------|--------------|-----------------------------------|-------------------------------------|--------------------------------------------------------------------|
| 1  | Activity Feed Notification                           | Microsoft Teams sample app for Sending Activity feed notification using Graph API with custom activity icons in a Teams Tab. | Basic        | [View][graph-activity-feed#cs]   | [View][graph-activity-feed#js] ![toolkit-icon](assets/toolkit-icon.png) |                                                                   |
| 2  | Graph API Explorer for RSC                          | Resource-specific consent (RSC) is a Microsoft Teams and Microsoft Graph API integration that enables your app to use API endpoints to manage specific teams within an organization | Basic        |                                   | [View][graph-rsc-helper#js] ![toolkit-icon](assets/toolkit-icon.png) |                                                                   |
| 3  | Graph API Channel Life Cycle                         | This sample illustrates how you can use [Graph API Channel Life Cycle](https://docs.microsoft.com/en-us/graph/api/resources/channel?view=graph-rest-beta) to call Graph APIs. | Intermediate | [View][graph#lifecycle#cs]       | [View][graph#lifecycle#js] ![toolkit-icon](assets/toolkit-icon.png) |[View](samples/graph-channel-lifecycle/csharp/demo-manifest/graph-channel-lifecycle.zip)                                                          |
| 4  | Graph API Teams App Installation Life Cycle          | This sample illustrates how you can use [Teams App Installation Life Cycle](https://docs.microsoft.com/en-us/graph/api/resources/teamsappinstallation?view=graph-rest-1.0) by calling Microsoft Graph APIs. | Intermediate | [View][graph#instllationlifecycle#cs] | [View][graph#instllationlifecycle#js] ![toolkit-icon](assets/toolkit-icon.png) | [View](samples/graph-app-installation-lifecycle/csharp/demo-manifest/graph-app-installation-lifecycle.zip) |
| 5  | Graph API Chat Life Cycle                            | This sample illustrates how you can use [Teams App Chat Life Cycle](https://docs.microsoft.com/en-us/graph/api/resources/chat?view=graph-rest-1.0) by calling Microsoft Graph APIs. | Intermediate | [View][graph#chatlifecyle#cs]      | [View][graph#chatlifecycle#js] ![toolkit-icon](assets/toolkit-icon.png) |[View](samples/graph-chat-lifecycle/csharp/demo-manifest/graph-chat-lifecycle.zip)|
| 6  | Proactive installation of App and sending proactive notifications | This sample application demonstrates proactive installation of a Teams app and sending notifications to users using Microsoft Graph APIs. | Intermediate | [View][graph#graphproactiveinstallation#cs]       | [View][graph#graphproactiveinstallation#js] ![toolkit-icon](assets/toolkit-icon.png) | [View](samples/graph-proactive-installation/csharp/demo-manifest/graph-proactive-installation.zip) |
| 7  | Change Notification                                  | This sample application demonstrates how to send real-time change notifications for user presence in Microsoft Teams. | Intermediate | [View][graph-change-notification#cs]                        | [View][graph-change-notification#js] ![toolkit-icon](assets/toolkit-icon.png) |        [View](samples/graph-change-notification/csharp/demo-manifest/graph-change-notification.zip)                                                           |
| 8  | Graph Pinned Message                                 | This sample application demonstrates how to pin messages in Microsoft Teams chat using the Graph API and displays all pinned messages in a group chat. | Intermediate | [View][graph-pinned-message#cs]| [View][graph-pinned-message#js] ![toolkit-icon](assets/toolkit-icon.png) |                                                                   |
| 9  | Graph Bulk Meetings                                  | This sample app demonstrates bulk meeting creation on Teams calendars using an Excel upload. | Intermediate | [View][graph-bulk-meetings#cs]| [View][graph-bulk-meetings#js] ![toolkit-icon](assets/toolkit-icon.png) |                                                                   |
| 10 | Graph Meeting Notification                            | This sample application demonstrates how to use an online meeting subscription to send notifications in Microsoft Teams chat when users join, leave, or when meetings start and end. | Intermediate | [View][graph-meeting-notification#cs]| [View][graph-meeting-notification#js] ![toolkit-icon](assets/toolkit-icon.png) |                                                                   |
| 11 | Change Notifications Team/Channel                     | This sample application showcases how to post notifications in Microsoft Teams when users create, edit, or delete teams and channels using Microsoft Graph and C#. | Intermediate | [View][change-notifications-team/channel#cs]| [View][change-notifications-team/channel#js] ![toolkit-icon](assets/toolkit-icon.png) |                                                                   |
| 12 | graph-teams-tag                                      | This sample application demonstrates how to perform CRUD operations on team tags using Microsoft Graph API within a Microsoft Teams tab. | Intermediate | [View][graph-teams-tag#cs]| [View][graph-teams-tag#js] ![toolkit-icon](assets/toolkit-icon.png) |                                                                   |
| 13 | Resource Specific Consent (RSC)                      | This sample illustrates how you can use [Resource Specific Consent (RSC)](https://docs.microsoft.com/en-us/microsoftteams/platform/graph-api/rsc/resource-specific-consent) to call Graph APIs. | Advanced     | [View][graph#rsc#cs]              | [View][graph#rsc#js] ![toolkit-icon](assets/toolkit-icon.png) | [View](samples/graph-rsc/csharp/demo-manifest/graph-rsc.zip)     |
| 14 | Graph API Teams App Catalog Life Cycle               | This sample demonstrates how to manage the lifecycle of Teams apps in the app catalog using Microsoft Graph APIs through a bot. | Advanced     | [View][graph#appctaloglifecycle#cs] | [View][graph#appcataloglifecycle#js] ![toolkit-icon](assets/toolkit-icon.png) | [View](samples/graph-appcatalog-lifecycle/csharp/demo-manifest/graph-appcatalog-lifecycle.zip) |
| 15 | Graph File Fetch| This sample demonstrates how to fetch shared files in teams group chat and team channel using graph API. | Advanced | [View][graph-file-fetch#cs] | [View][graph-file-fetch#js] | [View](samples/graph-file-fetch/csharp/demo-manifest/graph-file-fetch.zip) |
| 16 | Membership Change Notification       | This sample application demonstrates how to manage/handle membership change notification for shared channel, such as users being added, removed, or having their membership updated, using Node.js and the Microsoft Graph API.  | Advanced     | [View][graph#membershipchangenotification#cs] | [View][graph#graph#appcataloglifecycle#js#js] ![toolkit-icon](assets/toolkit-icon.png) |  |
| 17 | Graph chat migration bot | This sample demonstrates how to migrate external chats to teams using microsoft graph API's. | Intermediate | graph-chat-migration#cs | [View][graph-chat-migration#js] ![toolkit-icon](assets/toolkit-icon.png) | | | |


## [Calls and online meetings bots](https://docs.microsoft.com/microsoftteams/platform/bots/calls-and-meetings/calls-meetings-bots-overview)

| #  | Sample Name           | Description                                                                                                       | Level      | C#                                    |
|----|-----------------------|-------------------------------------------------------------------------------------------------------------------|------------|---------------------------------------|
| 1  | Local Media Samples    | Local media samples give the developer direct access to the inbound and outbound media streams.                   | Intermediate| [View](https://github.com/microsoftgraph/microsoft-graph-comms-samples/tree/master/Samples/V1.0Samples/LocalMediaSamples) |
| 2  | Remote Media Samples   | The Incident Bot sample is a Remote Media sample demonstrating a simple incident process workflow started by a Calling Bot. | Intermediate| [View](https://github.com/microsoftgraph/microsoft-graph-comms-samples/tree/master/Samples/V1.0Samples/RemoteMediaSamples) |
| 3  | Calling and Meeting bot| This sample demonstrates how a bot can create, join, and transfer calls or meetings within Microsoft Teams.        | Advanced   | [View][bot-calling-meeting#csharp]    |

## [Meetings samples](https://learn.microsoft.com/microsoftteams/platform/apps-in-teams-meetings/teams-apps-in-meetings)

| #  | Sample Name                             | Description                                                                                                        | Level        | C#                                             | JavaScript                                    | App Manifest                                                                                 |
|----|----------------------------------------|--------------------------------------------------------------------------------------------------------------------|--------------|-----------------------------------------------|----------------------------------------------|----------------------------------------------------------------------------------------------|
| 1  | Meetings Audio State                   | This sample tab application demonstrates how to control meeting audio in Microsoft Teams by muting and unmuting using the Incoming Client Audio API. | Basic        |                                               | [View][meetings-audio-state#nodejs] ![toolkit-icon](assets/toolkit-icon.png)                 |                                                                                              |
| 2  | Meetings Notification                  | This sample application demonstrates how to implement in-meeting and targeted notifications within Microsoft Teams meetings using adaptive cards and bot interactions. | Intermediate | [View][meetings-notification#cs]              | [View][meetings-notification#js] ![toolkit-icon](assets/toolkit-icon.png)             |  [View](/samples/meetings-notification/csharp/demo-manifest/meetings-notification.zip) |
| 3  | Meetings Context App                   | This sample demonstrates how to retrieve and display meeting context and participant details in Microsoft Teams using bot APIs. | Intermediate | [View][meetings-context-app#csharp]           | [View][meetings-context-app#nodejs] ![toolkit-icon](assets/toolkit-icon.png)                 |                                                                                              |
| 4  | Meetings Details Tab in Meetings       | This sample demonstrates how to create and manage polls during Microsoft Teams meetings, allowing participants to respond and view results through adaptive cards. | Intermediate | [View][meetings-details-tab#cs]               | [View][meetings-details-tab#js] ![toolkit-icon](assets/toolkit-icon.png)             | [View](/samples/meetings-details-tab/csharp/demo-manifest/meetings-details-tab.zip)            |
| 5  | Meeting Events                         | Receive real-time meeting and participant events with this Teams bot sample.                                       | Intermediate | [View][meetings-events#cs]                    | [View][meetings-events#js] ![toolkit-icon](assets/toolkit-icon.png)                   | [View](/samples/meetings-events/csharp/demo-manifest/Meetings-Events.zip)                     |
| 6  | Meeting Recruitment App                | Sample app showing meeting app experience for interview scenario.                                                 | Intermediate | [View][meeting-recruitment-app#cs]            | [View][meeting-recruitment-app#js] ![toolkit-icon](assets/toolkit-icon.png)          | [View](/samples/meeting-recruitment-app/csharp/demo-manifest/Meeting-Recruitment-App.zip)     |
| 7  | Meeting-Tabs                           | This sample shows app stage view, Mute/Unmute Teams meeting audio call, and Side panel.                           | Intermediate | [View][meetingtabs#csharp]                    | [View][meetingtabs#nodejs] ![toolkit-icon](assets/toolkit-icon.png)                          |                                                                                              |
| 8  | Meeting Transcript App                 | This sample app retrieves meeting transcripts via the Graph API and showcases them in a task module.              | Intermediate | [View][meetings-transcription-app#cs]         | [View][meetings-transcription-app#nodejs] ![toolkit-icon](assets/toolkit-icon.png)   | [View](/samples/meetings-transcription/csharp/demo-manifest/meetings-transcription.zip) |
| 9  | Meetings Attendance Report             | This sample application demonstrates how to retrieve meeting attendance reports using the Graph API and send them through a bot in the meeting chat. | Intermediate | [View][meetings-attendance-report#csharp]     | [View][meetings-attendance-report#nodejs] ![toolkit-icon](assets/toolkit-icon.png)     |                                                     |
| 10 | Anonymous User Support                 | This sample demonstrates how to support anonymous users in Microsoft Teams meeting apps, showcasing the setup and interaction flow for integrating guest and tenant users seamlessly. | Intermediate | [View][app-anonymous-users#cs]                | [View][app-anonymous-users#js] ![toolkit-icon](assets/toolkit-icon.png)                     |                                                                                              |
| 11 | Meetings SidePanel                     | This Microsoft Teams sample app demonstrates how to use the Live Share SDK to implement a side panel that allows for real-time data sharing during meetings. | Advanced     | [View][meetings-sidepanel#cs]                 | [View][meetings-sidepanel#js] ![toolkit-icon](assets/toolkit-icon.png)               |                                                                                              |
| 12 | Meetings Stage View                    | This sample app demonstrates how to enable and configure applications for Microsoft Teams meetings, allowing users to interact with a shared meeting stage using the Live Share SDK. | Advanced     | [View][meetings-stage-view#cs]                | [View][meetings-stage-view#js] ![toolkit-icon](assets/toolkit-icon.png)              | [View](/samples/meetings-stage-view/csharp/demo-manifest/Meeting-stage-view.zip)              |
| 13 | Meeting Live Caption                   | This sample application demonstrates how to utilize CART links to send live captions in Microsoft Teams meetings.  | Advanced     | [View][meetings-live-caption#cs]              | [View][meetings-live-caption#nodejs] ![toolkit-icon](assets/toolkit-icon.png)               |[View](/samples/meetings-live-caption/csharp/demo-manifest/meetings-live-caption.zip)              |                                                                                              |
| 14 | Meetings Extensibility                 | The Meeting Token Generator app for Microsoft Teams allows participants to request and manage interaction tokens, ensuring equitable participation during meetings like scrums and Q&A sessions. | Advanced     | [View][meetings-token-app#cs]                 | [View][meetings-token-app#js] ![toolkit-icon](assets/toolkit-icon.png)              |                                                                                              |
| 15 | Live Coding Interview using Shared Meeting Stage | This sample application showcases live coding interviews in Microsoft Teams using the Live Share SDK, allowing participants to collaborate in real-time on coding questions. | Advanced | [View][meetings-live-code-interview#csharp]   | [View][meetings-live-code-interview#nodejs] ![toolkit-icon](assets/toolkit-icon.png)  |                                                      |
| 16 | Meeting Signing Programmatic Share to Stage | This Microsoft Teams sample app enables users to review and sign documents in real time during meetings, facilitating seamless collaboration. | Advanced | [View][meetings-share-to-stage-signing#csharp] |                                              |                                                                                              |
| 17  | App Complete Sample                | This sample demonstrates how to create a Microsoft Teams bot in C# using Bot Framework v4, featuring Facebook authentication and various integration capabilities.                                                | Advanced | [View][app-complete-sample#cs]            | [View][app-complete-sample#js] ![toolkit-icon](assets/toolkit-icon.png)          | [View](/samples/app-complete-sample/csharp/demo-manifest/App-Complete-Sample.zip)     |
| 18  | In-meeting AI insights   | This sample demonstrates how to use the Microsoft Graph API to retrieve AI insights generated by Copilot for recorded meetings.                                                | Advanced |             | [View][meeting-ai-insights#js] ![toolkit-icon](assets/toolkit-icon.png)          |      |

## [Actions Preview](https://developer.microsoft.com/en-us/microsoft-365/dev-program)

| #  | Sample Name                      | Description                                                                                                        | Level        | JavaScript                                    | App Manifest                                      |
|----|-----------------------------------|--------------------------------------------------------------------------------------------------------------------|--------------|-----------------------------------------------|---------------------------------------------------|
| 1  | Actions across Microsoft 365      | This sample demonstrates how to build Teams apps using Actions in the Microsoft 365 app to streamline task management and enhance productivity by minimizing context switching. | Intermediate | [View][m365-actions-preview#js] ![toolkit-icon](assets/toolkit-icon.png)                 |           |

## Scenario specific samples
| #  | Sample Name                                     | Description                                                                                                           | Level        | C#     | JavaScript                                    | App Manifest                                      |
|----|------------------------------------------------|-----------------------------------------------------------------------------------------------------------------------|--------------|--------|-----------------------------------------------|---------------------------------------------------|
| 1  | Teams Dialogs (referred as task modules in TeamsJS v1.x) | Sample app showing off the Teams Dialogs(referred as task modules in TeamsJS v1.x), a way to invoke custom code from a bot, a tab, or both! | Intermediate | [View][bot-task-module#cs]                   | [View][bot-task-module#js] ![toolkit-icon](assets/toolkit-icon.png)                        | [View](/samples/bot-task-module/csharp/demo-manifest/bot-task-module.zip) |
| 2  | Authentication                                 | This sample showcases how to implement authentication in Microsoft Teams using both a bot and a tab, supporting Teams SSO with MSAL.js 2.0. | Intermediate |        | [View][app-auth#ts] ![toolkit-icon](assets/toolkit-icon.png)                          |                                                   |
| 3  | Region Selection App                           | Enables region selection for data centers using both a Bot and a Tab, with interactions through adaptive cards.       | Intermediate | [View][region-selection-app#cs]             |                | [View](/samples/app-region-selection/csharp/demo-manifest/app-region-selection.zip) |
| 4  | App Localization                               | Demonstrates adding localization support to Teams apps with Bots and Tabs integration.                                | Intermediate | [View][app-localization#cs]                  | [View][app-localization#js] ![toolkit-icon](assets/toolkit-icon.png)                      | [View](/samples/app-localization/csharp/demo-manifest/app-localization.zip) |
| 5  | App Installation using QR code                | Showcases app installation via QR codes, featuring adaptive cards and task modules.                                   | Intermediate | [View][qrappinstallation#csharp]            | [View][qrappinstallation#nodejs] ![toolkit-icon](assets/toolkit-icon.png)                | [View](/samples/app-installation-using-qr-code/csharp/demo-manifest/App-Installation-Using-QR.zip) |
| 6  | Archive Groupchat messages                    | Demonstrates archiving group chat messages and sending them to users as a file.                                       | Intermediate | [View][fetchgroupchatmessages#csharp]       | [View][fetchgroupchatmessages#nodejs] ![toolkit-icon](assets/toolkit-icon.png)           |  [View](/samples/bot-archive-groupchat-messages/csharp/demo-manifest/bot-archive-groupchat-messages.zip)                                                 |
| 7  | App check-in location                         | Enables location check-ins with bot integration and adaptive cards.                                                   | Intermediate | [View][appcheckinlocation#csharp]           | [View][checkinlocation#nodejs] ![toolkit-icon](assets/toolkit-icon.png)                  | [View](/samples/app-checkin-location/csharp/demo-manifest/App-checkin-location.zip) |
| 8  | Message reminder setup through messaging extension | Allows users to schedule tasks and receive reminders in Teams.                                                        | Intermediate | [View][memessagereminder#csharp]            | [View][msgext-message-reminder#nodejs] ![toolkit-icon](assets/toolkit-icon.png)          | [View](/samples/msgext-message-reminder/csharp/demo-manifest/msgext-message-reminder.zip) |
| 9  | Bot daily task reminder                       | Helps users schedule recurring tasks and receive reminders at specified times.                                        | Intermediate | [View][botdailytaskreminder#csharp]         | [View][bottaskreminder#nodejs] ![toolkit-icon](assets/toolkit-icon.png)                  | [View](/samples/bot-daily-task-reminder/csharp/demo-manifest/Bot-Daily-Task-Reminder.zip) |
| 10 | Tab request approval                          | Enables users to raise requests for managers to approve or reject, with notifications and task module redirections.   | Intermediate | [View][tab-request-approval#csharp]         | [View][tab-request-approval#nodejs] ![toolkit-icon](assets/toolkit-icon.png)            | [View](/samples/tab-request-approval/csharp/demo-manifest/Tab-Request-Approval.zip) |
| 11 | Bot request approval                          | Allows users to request task approval within group chats, with managers approving or rejecting requests.              | Intermediate | [View][bot-request-approval#csharp]         | [View][bot-request-approval#nodejs] ![toolkit-icon](assets/toolkit-icon.png)            | [View](/samples/bot-request-approval/csharp/demo-manifest/Bot-Request-Approval.zip) |
| 12 | Join the Team using QR code                  | Enables users to join Teams directly by scanning QR codes, integrating bots, Adaptive Cards, and Graph API.           | Intermediate | [View][qrjointeam#csharp]                   | [View][qrjointeam#nodejs] ![toolkit-icon](assets/toolkit-icon.png)                       | [View](/samples/bot-join-team-using-qr-code/csharp/demo-manifest/Bot-Join-Team-By-QR.zip) |
| 13 | Activity feed broadcast                       | Demonstrates notifying all members of the organization using activity feed notifications.                             | Intermediate | [View][graph-activity-feed-broadcast#csharp] | [View][graph-activity-feed-broadcast#js] ![toolkit-icon](assets/toolkit-icon.png)       |                                                   |
| 14 | App complete auth                             | Demonstrates authentication features in bot, tab, and messaging extensions.                                                            | Intermediate | [View][app-complete-auth#cs]                 |                                                 |                                                   |
| 15 | Release Management                              | Sends work item notifications using Azure webhook.                                                                                     | Intermediate | [View][release-management#cs]               | [View][release-management#nodejs] ![toolkit-icon](assets/toolkit-icon.png)                |                                                   |
| 16 | Link unfurling for 'Share to Teams'             | Demonstrates link unfurling for the "Share to Teams" feature.                                                                           | Intermediate | [View][link-unfurling-stt#csharp]                                             | [View][link-unfurling-stt#nodejs] ![toolkit-icon](assets/toolkit-icon.png)               |   [View](/samples/app-link-unfurling-in-share-to-teams/csharp/demo-manifest/app-link-unfurling-stt.zip)                                                |
| 17 | App Icon Badging In Microsoft Teams Meetings    | Shows how to send app icon badging notifications in Teams meetings.                                                                    | Intermediate | [View][meetings-app-icon-badging#csharp]     | [View][meetings-app-icon-badging#nodejs] ![toolkit-icon](assets/toolkit-icon.png)         |[View](/samples/meetings-app-icon-badging/csharp/demo-manifest/meetings-app-icon-badging.zip)                                                   |
| 18 | SharePoint Tab Request Approval                | Demonstrates using SharePoint Embedded for storage in Teams with tab request approval functionality.                                   | Intermediate | [View][sharepoint-tab-request-approval#csharp]|                                                |                                                   |
| 19 | AI-Powered Enterprise Search Bot               | Demonstrates user search functionality powered by Azure Open AI embeddings and Redis DB, supporting file-based queries in Teams bots.   | Advanced     |                                              | [View][bot-ai-enterprise-search#nodejs] ![toolkit-icon](assets/toolkit-icon.png)          |                                                   |
| 20 | Complete Samples                               | Sample covering multiple scenarios - dialogs, ME, and Facebook auth.                                                  | Advanced     | [View][app-complete#cs]                      | [View][app-complete#ts] ![toolkit-icon](assets/toolkit-icon.png)                          |                                                   |
| 21 | App SSO                                        | Provides SSO functionality for Teams, supporting Tabs, Bots, and Messaging Extensions with secure API access.         | Advanced     | [View][app-sso#cs]                           | [View][app-sso#js] ![toolkit-icon](assets/toolkit-icon.png)                               | [View](/samples/app-sso/csharp/demo-manifest/App-SSO.zip) |
| 22 | Combined Tab and Office Add-in                 | Demonstrates using an add-in to act on Office documents and a tab to aggregate data about these actions.                               | Advanced     |                                              | [View][tab-add-in-combined#ts] ![toolkit-icon](assets/toolkit-icon.png)                   | [View](/samples/tab-add-in-combined/nodejs/demo-manifest/tab-add-in-combined.zip) |
| 23 | App HR Talent                                   | Integrates with a simulated recruiting and talent management system, supporting bot, tab, and messaging extensions for HR operations.   | Advanced     | [View][app-HR-talent#csharp]                 |                                                 | [View](/samples/app-HR-talent/csharp/src/demo-manifest/TeamsTalentMgmt.zip) |

## Application templates

| #  | App Name       | Description                                                                                                     | Level         | Code                          |
|----|----------------|-----------------------------------------------------------------------------------------------------------------|---------------|-------------------------------|
| 2  | Resource Hub   | Resource Hub is a solution designed to provide all the help you need to use Teams, all in one place.            | Basic         | [View][msteams-app-resource-hub] |
| 1  | QBot           | QBot is a solution for Microsoft Teams designed to facilitate intelligent Q&A interactions among educators, tutors, and students within classroom settings. | Advanced      | [View][msteams-app-qbot]     |


[app-hello-world#cs]:samples/app-hello-world/csharp
[app-hello-world#ts]:samples/app-hello-world/nodejs
[personal-tab-quickstart#ts]:samples/tab-personal-quickstart/ts
[personal-tab-quickstart#js]:samples/tab-personal-quickstart/js
[personal-tab-sso-quickstart#ts]:samples/tab-personal-sso-quickstart/ts
[personal-tab-sso-quickstart#csharp]:samples/tab-personal-sso-quickstart/csharp_dotnetcore
[personal-tab-sso-quickstart#js]:samples/tab-personal-sso-quickstart/js
[group-tab-quickstart#csharp]:samples/tab-channel-group-quickstart/csharp
[group-tab-quickstart#ts]:samples/tab-channel-group-quickstart/ts
[group-tab-quickstart#js]:samples/tab-channel-group-quickstart/js
[group-tab-sso-quickstart#ts]:samples/tab-channel-group-sso-quickstart/ts
[group-tab-sso-quickstart#js]:samples/tab-channel-group-sso-quickstart/js
[group-tab-sso-quickstart#csharp]:samples/tab-channel-group-sso-quickstart/csharp_dotnetcore

[tab-deeplink#csharp]:samples/tab-deeplink/csharp
[tab-deeplink#nodejs]:samples/tab-deeplink/nodejs
[personal-tab#cs#razor]:samples/tab-personal/razor-csharp
[personal-tab#cs#mvc]:samples/tab-personal/mvc-csharp
[tab-graph-toolkit#js]:samples/tab-graph-toolkit/nodejs
[tab-graph-toolkit#csharp]:samples/tab-graph-toolkit/csharp
[tab-device-permissions#js]:samples/tab-device-permissions/nodejs
[tab-conversation#csharp]:samples/tab-conversations/csharp
[app-complete-auth#cs]:samples/app-complete-auth/csharp
[app-complete-auth#nodejs]:samples/app-complete-auth/nodejs

[tab-app-navigation#csharp]:samples/tab-app-navigation/csharp
[tab-conversation#nodejs]:samples/tab-conversations/nodejs
[tab-stage-view#js]:samples/tab-stage-view/nodejs
[tab-stage-view#csharp]:samples/tab-stage-view/csharp
[tab-product-inspection#csharp]:samples/tab-product-inspection/csharp
[tab-staggered-permission#csharp]:samples/tab-staggered-permission/csharp
[tab-people-picker#csharp]:samples/tab-people-picker/csharp
[tab-channel-group=config-csharp]:samples/tab-channel-group-config-page-auth/csharp
[tab-channel-context#nodejs]:samples/tab-channel-context/nodejs
[tab-app-monetization#nodejs]:samples/tab-app-monetization/nodejs
[app-cache-meetings#nodejs]:samples/app-cache-meetings/nodejs
[tab-request-approval#nodejs]:samples/tab-request-approval/nodejs
[bot-request-approval#nodejs]:samples/bot-request-approval/nodejs
[tab-navbar-menu#ts]:samples/tab-navbar-menu/ts
[tab-multi-hub#ts]:samples/tab-multi-hub/ts
[tab-ui-templates#ts]:samples/tab-ui-templates/ts
[tab-external-auth#csharp]:samples/tab-external-auth/csharp
[tab-meeting-transcript-recording#nodejs]:samples/tab-meeting-transcript-recording/nodejs
[tab-meeting-transcript-recording#csharp]:samples/tab-meeting-transcript-recording/csharp
[tab-adhoccalls-transcript-recording#nodejs]:samples/tab-adhoccalls-transcript-recording/nodejs
[tab-meeting-auto-recording#csharp]:samples/tab-meeting-auto-recording/csharp
[tab-deeplink-pass-values#ts]:samples/tab-deeplink-pass-values/ts
[tab-nested-auth#nodejs]:samples/tab-nested-auth/nodejs
[tab-support-offline#nodejs]:samples/tab-support-offline/nodejs
[tab-nested-auth#cs]:samples/tab-nested-auth/csharp
[group-channel-tab#cs#razor]:samples/tab-channel-group/razor-csharp
[group-channel-tab#cs#mvc]:samples/tab-channel-group/mvc-csharp
[group-channel-tab#ts#spfx]:samples/tab-channel-group/spfx
[tab-channel-group#js]:samples/tab-channel-group/nodejs

[connector#cs]:samples/connector-todo-notification/csharp
[incoming-webhook#cs]:samples/incoming-webhook/csharp
[release-management#cs]:samples/bot-release-management/csharp
[release-management#nodejs]:samples/bot-release-management/nodejs
[connector#ts]:samples/connector-github-notification/nodejs
[connector-generic#ts]:samples/connector-generic/nodejs
[sequential#workflow#csharp]:samples/bot-sequential-flow-adaptive-cards/csharp
[sequential#workflow#js]:samples/bot-sequential-flow-adaptive-cards/nodejs
[sequential#workflow#python]:samples/bot-sequential-flow-adaptive-cards/python
[app-auth#ts]:samples/app-auth/nodejs
[auth-in-connector#nodejs]:samples/connector-todo-notification/nodejs
[botwithsharepointfileviewer#csharp]:samples/bot-sharepoint-file-viewer/csharp
[typeaheadsearch#csharp]:samples/bot-type-ahead-search-adaptive-cards/csharp
[graph-activity-feed-broadcast#csharp]:samples/graph-activity-feed-broadcast/csharp


[app-task-module#ts]:samples/app-task-module/nodejs
[app-task-module#cs]:samples/app-task-module/csharp
[app-complete-sample#cs]:samples/app-complete-sample/csharp
[app-complete-sample#js]:samples/app-complete-sample/nodejs
[meeting-ai-insights#js]:samples/bot-meeting-ai-insights/nodejs

[outgoing-webhook#cs]:samples/outgoing-webhook/csharp
[outgoing-webhook#ts]:samples/outgoing-webhook/nodejs

[msgext-link-unfurl#cs]:samples/msgext-link-unfurling-reddit/csharp
[msgext-link-unfurl-meeting#cs]:samples/msgext-link-unfurling-meeting/csharp
[msgext-action-quickstart#js]:samples/msgext-action-quickstart/js
[msgext-search-quickstart#js]:samples/msgext-search-quickstart/js
[msgext-search-sso-config#js]:samples/msgext-search-sso-config/nodejs
[msgext-unfurling-ac-loop-components#nodejs]:samples/msgext-unfurling-ac-loop-components/nodejs
[msgext-unfurling-ac-loop-components#cs]:samples/msgext-unfurling-ac-loop-components/csharp
[msgext-search-sso-config#cs]:samples/msgext-search-sso-config/csharp
[msgext-copilot-handoff#js]:samples/msgext-copilot-handoff/ts
[msgext-thirdparty-storage#nodejs]:samples/msgext-thirdparty-storage/nodejs
[msgext-thirdparty-storage#cs]:samples/msgext-thirdparty-storage/csharp
[msgext-thirdparty-storage#python]:samples/msgext-thirdparty-storage/python
[msgext-expert-finder#nodejs]:samples/msgext-expert-finder-js

[tab-sso#ts]:samples/tab-sso/nodejs
[tab-sso#cs]:samples/tab-sso/csharp

[app-virtual-assistant#cs]:samples/app-virtual-assistant/csharp
[user-scope-web-application#js]:samples/user-scope-web-application/nodejs
[tab-app-cookie#js]:samples/tab-app-cookie/js
[identity-linking-with-sso#cs]:samples/app-identity-linking-with-sso/csharp
[bot-proactive-msg#cs]:samples/bot-proactive-messaging/csharp
[bot-proactive-msg-teamsfx#js]:samples/bot-proactive-messaging-teamsfx/nodejs
[bot-proactive-msg-teamsfx#python]:samples/bot-proactive-messaging-teamsfx/python
[bot-feed-members#js]:samples/bot-feed-members/nodejs
[bot-conversation-quickstart#js]:samples/bot-conversation-quickstart/js
[bot-conversation-sso-quickstart#js]:samples/bot-conversation-sso-quickstart/js
[bot-conversation-sso-quickstart#python]:samples/bot-conversation-sso-quickstart/python
[bot-sharepoint-list#cs]:samples/bot-sharepoint-list/csharp
[bot-conversation-sso-quickstart#csharp_dotnetcore]:samples/bot-conversation-sso-quickstart/csharp_dotnetcore
[bot-calling-meeting#csharp]:samples/bot-calling-meeting/csharp
[bot-tab-conversations#csharp]:samples/bot-tab-conversations/csharp
[botfileviewer#js]:samples/bot-sharepoint-file-viewer/nodejs
[meetings-token-app#cs]:samples/meetings-token-app/csharp
[apps-in-meeting#cs]:samples/apps-in-meeting/csharp
[meetings-token-app#js]:samples/meetings-token-app/nodejs
[m365-actions-preview#js]:samples/m365-actions-preview/nodejs
[region-selection-app#cs]: samples/app-region-selection/csharp  
[meetings-notification#cs]:samples/meetings-notification/csharp
[meetings-sidepanel#cs]:samples/meetings-sidepanel/csharp
[meetings-sidepanel#js]:samples/meetings-sidepanel/nodejs
[meetings-notification#js]:samples/meetings-notification/nodejs
[messageswithrsc#csharp]:samples/bot-receive-channel-messages-withRSC/csharp
[messageswithrsc#js]:samples/bot-receive-channel-messages-withRSC/nodejs
[app-in-meeting#cs]:samples/app-in-meeting/csharp
[fetchgroupchatmessages#csharp]:samples/bot-archive-groupchat-messages/csharp
[fetchgroupchatmessages#nodejs]:samples/bot-archive-groupchat-messages/nodejs
[appcheckinlocation#csharp]:samples/app-checkin-location/csharp
[checkinlocation#nodejs]:samples/app-checkin-location/nodejs
[productinspection#nodejs]:samples/tab-product-inspection/nodejs
[msgext-message-reminder#nodejs]:samples/msgext-message-reminder/nodejs
[botdailytaskreminder#csharp]:samples/bot-daily-task-reminder/csharp
[bottaskreminder#nodejs]:samples/bot-daily-task-reminder/nodejs
[memessagereminder#csharp]:samples/msgext-message-reminder/csharp
[tab-request-approval#csharp]:samples/tab-request-approval/csharp
[sharepoint-tab-request-approval#csharp]:samples/sharepoint-tab-request-approval/csharp
[app-HR-talent#csharp]:samples/app-HR-talent/csharp
[bot-request-approval#csharp]:samples/bot-request-approval/csharp
[typeaheadsearchonadaptivecard#js]:samples/bot-type-ahead-search-adaptive-cards/nodejs
[meetings-live-code-interview#csharp]:samples/meetings-live-code-interview/csharp
[meetings-live-code-interview#nodejs]:samples/meetings-live-code-interview/nodejs
[suggestedactionsbot#csharp]:samples/bot-suggested-actions/csharp
[suggestedactionsbot#nodejs]:samples/bot-suggested-actions/nodejs
[botadaptivecardsuserspecificviews#csharp]:samples/bot-adaptivecards-user-specific-views/csharp
[botstreaming#csharp]:samples/bot-streaming/csharp
[botstreaming#nodejs]:samples/bot-streaming/nodejs
[Tagmention#csharp]:samples/bot-tag-mention/csharp
[Tagmention#nodejs]:samples/bot-tag-mention/nodejs
[CommandsMenu#csharp]:samples/bot-commands-menu/csharp
[CommandsMenu#nodejs]:samples/bot-commands-menu/nodejs
[RequirementTargetingOneWayDependency#nodejs]:samples/requirement-targeting-oneway-dependency/nodejs
[RequirementTargetingMutualDependency#nodejs]:samples/requirement-targeting-mutual-dependency/nodejs
[AdaptiveCardActions#csharp]:samples/bot-adaptive-card-actions/csharp
[AdaptiveCardActions#nodejs]:samples/bot-adaptive-card-actions/nodejs  
[BotFormattingCards#csharp]:samples/bot-formatting-cards/csharp
[BotFormattingCards#nodejs]:samples/bot-formatting-cards/nodejs
[BotFormattingCards#python]:samples/bot-formatting-cards/python
[BotConfigurationApp#nodejs]:samples/bot-configuration-app/nodejs  
[BotConfigurationApp#csharp]:samples/bot-configuration-app/csharp  
[BotConfigurationAppAuth#nodejs]:samples/bot-configuration-app-auth/nodejs  
[BotConfigurationAppAuth#csharp]:samples/bot-configuration-app-auth/csharp
[AdaptiveCardActions#nodejs]:samples/bot-adaptive-card-actions/nodejs
[AdaptiveCardSSO#csharp]:samples/bot-sso-adaptivecard/csharp
[BotSSOAdaptiveCard#nodejs]:samples/bot-sso-adaptivecard/nodejs
[TypesCards#csharp]:samples/bot-all-cards/csharp
[TypesCards#nodejs]:samples/bot-all-cards/nodejs
[TypesCards#python]:samples/bot-all-cards/python
[app-localization#cs]:samples/app-localization/csharp
[app-localization#js]:samples/app-localization/nodejs
[meetings-details-tab#cs]:samples/meetings-details-tab/csharp
[meetings-details-tab#js]:samples/meetings-details-tab/nodejs
[app-sso#cs]:samples/app-sso/csharp
[app-sso#js]:samples/app-sso/nodejs
[meetings-stage-view#js]:samples/meetings-stage-view/nodejs
[meetings-stage-view#cs]:samples/meetings-stage-view/csharp
[meetings-events#cs]:samples/meetings-events/csharp
[meetings-events#js]:samples/meetings-events/nodejs
[meeting-recruitment-app#cs]:samples/meeting-recruitment-app/csharp
[meeting-recruitment-app#js]:samples/meeting-recruitment-app/nodejs
[meetings-transcription-app#cs]:samples/meetings-transcription/csharp
[meetings-transcription-app#nodejs]:samples/meetings-transcription/nodejs
[meetings-share-to-stage-signing#csharp]:samples/meetings-share-to-stage-signing/csharp
[qrappinstallation#csharp]:samples/app-installation-using-qr-code/csharp
[qrappinstallation#nodejs]:samples/app-installation-using-qr-code/nodejs
[qrjointeam#csharp]:samples/bot-join-team-using-qr-code/csharp
[qrjointeam#nodejs]:samples/bot-join-team-using-qr-code/nodejs
[incoming-webhook#nodejs]:samples/incoming-webhook/nodejs
[meetingtabs#csharp]:samples/meeting-tabs/csharp
[meetingtabs#nodejs]:samples/meeting-tabs/nodejs
[meetings-attendance-report#csharp]:samples/meetings-attendance-report/csharp
[meetings-attendance-report#nodejs]:samples/meetings-attendance-report/nodejs
[meetings-audio-state#nodejs]:samples/meetings-audio-state/nodejs
[meetings-context-app#csharp]:samples/meetings-context-app/csharp
[msgext-ai-doc-compliance-checker#nodejs]:samples/msgext-ai-doc-compliance-checker
[meetings-context-app#nodejs]:samples/meetings-context-app/nodejs
[bot-ai-meeting-helper#nodejs]:samples/bot-ai-meeting-helper
[api-doc-search#nodejs]:samples/api-doc-search
[bot-virtual-assistant#nodejs]:samples/bot-virtual-assistant

[graph#rsc#cs]:samples/graph-rsc/csharp
[graph#rsc#js]:samples/graph-rsc/nodeJs
[graph#lifecycle#cs]:samples/graph-channel-lifecycle/csharp
[graph#lifecycle#js]:samples/graph-channel-lifecycle/nodejs
[graph#instllationlifecycle#cs]:samples/graph-app-installation-lifecycle/csharp
[graph#instllationlifecycle#js]:samples/graph-app-installation-lifecycle/nodejs
[graph#graphproactiveinstallation#cs]: samples/graph-proactive-installation/csharp
[graph#chatlifecyle#cs]:samples/graph-chat-lifecycle/csharp
[graph#chatlifecycle#js]:samples/graph-chat-lifecycle/nodejs
[graph#appctaloglifecycle#cs]:samples/graph-appcatalog-lifecycle/csharp
[graph#appcataloglifecycle#js]:samples/graph-appcatalog-lifecycle/nodejs
[graph#membershipchangenotification#cs]:samples/graph-membership-change-notification/csharp
[graph#membershipchangenotification#js]:samples/graph-membership-change-notification/nodejs
[graph#graphproactiveinstallation#cs]:samples/graph-proactive-installation/csharp
[graph#graphproactiveinstallation#js]:samples/graph-proactive-installation/nodejs
[graph-activity-feed#cs]:samples/graph-activity-feed/csharp
[graph-activity-feed#js]:samples/graph-activity-feed/nodejs
[graph-change-notification#cs]:samples/graph-change-notification/csharp
[graph-change-notification#js]:samples/graph-change-notification/nodejs
[graph-meeting-notification#cs]:samples/graph-meeting-notification/csharp
[graph-meeting-notification#js]:samples/graph-meeting-notification/nodejs
[peoplepickeronadaptivecard#js]:samples/bot-people-picker-adaptive-card/nodejs
[peoplepickeronadaptivecard#csharp]:samples/bot-people-picker-adaptive-card/csharp
[graph-activity-feed-broadcast#js]:samples/graph-activity-feed-broadcast/nodejs
[graph-pinned-message#cs]:samples/graph-pinned-messages/csharp
[graph-pinned-message#js]:samples/graph-pinned-messages/nodejs
[graph-bulk-meetings#cs]:samples/graph-bulk-meetings/csharp
[graph-bulk-meetings#js]:samples/graph-bulk-meetings/nodejs
[graph-file-fetch#cs]:samples/graph-file-fetch/csharp
[graph-file-fetch#js]:samples/graph-file-fetch/nodejs
[change-notifications-team/channel#cs]:samples/graph-change-notification-team-channel/csharp
[change-notifications-team/channel#js]:samples/graph-change-notification-team-channel/nodejs
[graph-teams-tag#cs]:samples/graph-teams-tag/csharp
[graph-teams-tag#js]:samples/graph-teams-tag/nodejs
[graph-rsc-helper#js]:samples/graph-rsc-helper/nodeJs
[link-unfurling-stt#csharp]:samples/app-link-unfurling-in-share-to-teams/csharp
[link-unfurling-stt#nodejs]:samples/app-link-unfurling-in-share-to-teams/nodejs
[tab-staggered-permission#nodejs]:samples/tab-staggered-permission/nodejs
[botframework]:https://github.com/microsoft/BotBuilder-Samples#teams-samples
[tab-people-picker#nodejs]:samples/tab-people-picker/nodejs
[meetings-live-caption#cs]:samples/meetings-live-caption/csharp
[meetings-live-caption#nodejs]:samples/meetings-live-caption/nodejs
[app-anonymous-users#cs]:samples/app-anonymous-users/csharp
[app-anonymous-users#js]:samples/app-anonymous-users/nodejs
[bot-ai-enterprise-search#nodejs]:samples/bot-ai-enterprise-search/nodejs
[meetings-app-icon-badging#csharp]:samples/meetings-app-icon-badging/csharp
[meetings-app-icon-badging#nodejs]:samples/meetings-app-icon-badging/nodejs
[tab-add-in-combined#ts]:samples/tab-add-in-combined/nodejs

[msteams-app-qbot]:samples/msteams-application-qbot/
[msteams-app-resource-hub]:samples/msteams-application-resourcehub/

[bot-conversation#python]:samples/bot-conversation/python
[bot-file-upload#python]:samples/bot-file-upload/python
[bot-initiate-thread-in-channel#python]:samples/bot-initiate-thread-in-channel/python
[bot-message-reaction#python]:samples/bot-message-reaction/python
[bot-task-module#python]:samples/bot-task-module/python
[bot-teams-authentication#python]:samples/bot-teams-authentication/python
[msgext-action-preview#python]:samples/msgext-action-preview/python
[msgext-action#python]:samples/msgext-action/python
[msgext-link-unfurling#python]:samples/msgext-link-unfurling/python
[msgext-search-auth-config#python]:samples/msgext-search-auth-config/python
[msgext-search#python]:samples/msgext-search/python
[meetings-context-app#python]:samples/meetings-context-app/python
[bot-auth0-adaptivecard#python]:samples/bot-auth0-adaptivecard/python

[agent-knowledge-hub#js]:samples/agent-knowledge-hub/nodejs
[agent-knowledge-hub#cs]:samples/agent-knowledge-hub/csharp
[agent-knowledge-hub#python]:samples/agent-knowledge-hub/python

[bot-conversation#js]:samples/bot-conversation/nodejs
[bot-auth0-adaptivecard#js]:samples/bot-auth0-adaptivecard/nodejs
[bot-file-upload#js]:samples/bot-file-upload/nodejs
[bot-initiate-thread-in-channel#js]:samples/bot-initiate-thread-in-channel/nodejs
[bot-message-reaction#js]:samples/bot-message-reaction/nodejs
[bot-task-module#js]:samples/bot-task-module/nodejs
[msgext-search#js]:samples/msgext-search/nodejs
[msgext-action-preview#js]:samples/msgext-action-preview/nodejs
[msgext-action#js]:samples/msgext-action/nodejs
[msgext-link-unfurling#js]:samples/msgext-link-unfurling/nodejs
[msgext-ai-sentiment-analysis#js]:samples/msgext-ai-sentiment-analysis/nodejs
[msgext-ai-sentiment-analysis#cs]:samples/msgext-ai-sentiment-analysis/csharp
[msgext-ai-sentiment-analysis#python]:samples/msgext-ai-sentiment-analysis/python
[bot-conversation-sso-quickstart#js]:samples/bot-conversation-sso-quickstart/js
[graph-chat-migration#js]:samples\graph-chat-migration\nodejs
[graph-chat-migration#cs]:samples\graph-chat-migration\csharp


[bot-conversation#java]:samples/bot-conversation/java
[bot-file-upload#java]:samples/bot-file-upload/java
[bot-initiate-thread-in-channel#java]:samples/bot-initiate-thread-in-channel/java
[bot-message-reaction#java]:samples/bot-message-reaction/java
[bot-task-module#java]:samples/bot-task-module/java
[bot-teams-authentication#java]:samples/bot-teams-authentication/java
[msgext-action-preview#java]:samples/msgext-action-preview/java
[msgext-action#java]:samples/msgext-action/java
[msgext-link-unfurling#java]:samples/msgext-link-unfurling/java
[msgext-search-auth-config#java]:samples/msgext-search-auth-config/java
[msgext-search#java]:samples/msgext-search/java

[bot-conversation#cs]:samples/bot-conversation/csharp
[bot-auth0-adaptivecard#cs]:samples/bot-auth0-adaptivecard/csharp
[bot-shared-channel-events#cs]:samples/bot-shared-channel-events/csharp
[bot-file-upload#cs]:samples/bot-file-upload/csharp
[bot-initiate-thread-in-channel#cs]:samples/bot-initiate-thread-in-channel/csharp
[bot-message-reaction#cs]:samples/bot-message-reaction/csharp
[bot-task-module#cs]:samples/bot-task-module/csharp
[bot-teams-authentication#cs]:samples/bot-teams-authentication/csharp
[msgext-action-preview#cs]:samples/msgext-action-preview/csharp
[msgext-link-unfurling#cs]:samples/msgext-link-unfurling/csharp
[msgext-search-auth-config#cs]:samples/msgext-search-auth-config/csharp
[msgext-search#cs]:samples/msgext-search/csharp
[msgext-action#cs]:samples/msgext-action/csharp



## Submitting issues

The issue tracker is for **[issues](https://github.com/OfficeDev/Microsoft-Teams-Samples/issues)**, in other words, bugs and suggestions.
If you have a *question*, *feedback* or *suggestions*, please check our [support page](https://docs.microsoft.com/microsoftteams/platform/feedback).


## Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.opensource.microsoft.com.

When you submit a pull request, a CLA bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

<img src="https://pnptelemetry.azurewebsites.net/microsoft-teams-samples/samples/landing-page" />