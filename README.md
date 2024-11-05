# Microsoft Teams Samples
[![Sample code build status](https://github.com/OfficeDev/Microsoft-Teams-Samples/actions/workflows/build-complete-samples.yml/badge.svg)](https://github.com/OfficeDev/Microsoft-Teams-Samples/actions/workflows/build-complete-samples.yml)
##### [Click here to find out what's new with Microsoft Teams Platform](https://docs.microsoft.com/microsoftteams/platform/whats-new)

>NOTE: These samples are designed to help understand Microsoft Teams platform capabilities and scenarios. If you are looking for production ready apps then please visit [App Templates for Microsoft Teams](https://docs.microsoft.com/microsoftteams/platform/samples/app-templates)

The easy way to browse through all the samples is to visit the [Microsoft 365 Sample Solution Gallery](https://adoption.microsoft.com/en-us/sample-solution-gallery/?keyword=&sort-by=updateDateTime-true&page=1&product=Teams)

# Sample lists

1. [Copilot for Microsoft 365 samples](#Copilot-for-Microsoft-365-samples)
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

## [Copilot for Microsoft 365 samples](https://github.com/OfficeDev/Copilot-for-M365-Samples)
| Name    | Description |  .NET    | TypeScript   | JavaScript |
| -------- | :------- | ------- | ------- | ------- |
| Northwind Inventory  | Plugin that allows users to query the Northwind Database | [View](https://github.com/OfficeDev/Copilot-for-M365-Samples/tree/main/samples/msgext-northwind-inventory-csharp)| [View](https://github.com/OfficeDev/Copilot-for-M365-Plugins-Samples/tree/main/samples/msgext-northwind-inventory-ts) | |
| Document Search | Plugin that enables Hybrid Search (Vector + Semantic) | [View](https://github.com/OfficeDev/Copilot-for-M365-Plugins-Samples/tree/main/samples/msgext-doc-search-csharp)  | [View](https://github.com/OfficeDev/Copilot-for-M365-Plugins-Samples/tree/main/samples/msgext-doc-search-js) | |
| Product support | Plugin that allows users to query the Products held in SharePoint Online team site via Microsoft Graph | [View](https://github.com/OfficeDev/Copilot-for-M365-Plugins-Samples/tree/main/samples/msgext-product-support-sso-csharp) | |
| Multi Parameters  |Plugin that demonstrates how to implement complex utterances and support deep retrieval | [View](https://github.com/OfficeDev/Copilot-for-M365-Plugins-Samples/tree/main/samples/msgext-multiparam-csharp)  |[View](https://github.com/OfficeDev/Copilot-for-M365-Plugins-Samples/tree/main/samples/msgext-multiparam-ts) | [View](https://github.com/OfficeDev/Copilot-for-M365-Plugins-Samples/tree/main/samples/msgext-multiparam-js)|

## [Samples built using new generation of Teams development tool - Teams Toolkit](https://github.com/OfficeDev/TeamsFx-Samples)

The [Teams Toolkit](https://marketplace.visualstudio.com/items?itemName=TeamsDevApp.ms-teams-vscode-extension) is an extension on Visual Studio Code and Visual Studio. It enable Teams developers to create and deploy Teams apps with integrated identity, access to cloud storage, data from Microsoft Graph, and other services in Azure and M365 with a "zero-configuration" approach to the developer experience. And [Sample Apps](https://github.com/OfficeDev/TeamsFx-Samples) are provided in Teams Toolkit. Download and try it by today! [Learn more about Teams Toolkit](https://docs.microsoft.com/microsoftteams/platform/toolkit/visual-studio-code-overview).


## [Getting Started - Samples and Tutorials](https://docs.microsoft.com/microsoftteams/platform/tutorials/get-started-dotnet-app-studio)

|    | Sample Name        | Description                                                                                                                | C#    | TypeScript | App Manifest  
|:--:|:-------------------|:---------------------------------------------------------------------------------------------------------------------------|:--------|:-------------|:-------------|
|1|Hello World|A Microsoft Teams Hello World sample app built with .NET/NodeJS that demonstrates essential features like tabs, bots, and messaging extensions for seamless interaction within the Teams environment.|[View][app-hello-world#cs]|[View][app-hello-world#ts] ![toolkit-icon](assets/toolkit-icon.png) |[View](/samples/app-hello-world/csharp/demo-manifest/app-hello-world.zip)

## [AI samples](https://learn.microsoft.com/en-us/azure/ai-services/document-intelligence/?view=doc-intel-4.0.0)

|    | Sample Name        | Description                                                                                                                | C#    | JavaScript | App Manifest  
|:--:|:-------------------|:---------------------------------------------------------------------------------------------------------------------------|:--------|:-------------|:-------------|
|1|api-doc-search|This is a sample API application that demonstrates how to perform RAG-based semantic search using NoSQL CosmosDB.||[View][api-doc-search#nodejs]|-
|2|Compliance Checker|This sample demonstrates how to integrate Open AI into a Teams message extension to evaluate contract proposals against policy checklists.||[View][msgext-ai-doc-compliance-checker#nodejs] ![toolkit-icon](assets/toolkit-icon.png)|[View](/samples/msgext-ai-doc-compliance-checker/demo-manifest/AIComplianceChecker.zip)
|3|bot-ai-meeting-helper|This sample demonstrates how Azure Open AI extracts action items from meeting transcriptions for all participants who subscribed to the meeting, and subsequently sends these action items to each individual user in a 1:1 chat after the meeting concludes.||[View][bot-ai-meeting-helper#nodejs] ![toolkit-icon](assets/toolkit-icon.png)|[View](/samples/bot-ai-meeting-helper/demo-manifest/bot-ai-meeting-helper.zip)
|4|bot-virtual-assistant|This is a sample application that demonstrates a root bot that routes inputs to skill bots for tailored responses.||[View][bot-virtual-assistant#nodejs]|-
|5|msgext-expert-finder-js|This Teams message extension acts as a plugin for Microsoft Copilot, allowing users to conduct multi-parameter searches for candidates using criteria like skills, location, and availability. It also supports Single Sign-On (SSO) for seamless authentication, ensuring a smooth user experience.||[View][msgext-expert-finder#nodejs]|-

## [Tabs samples](https://docs.microsoft.com/microsoftteams/platform/tabs/what-are-tabs)
|    | Sample Name        | Description                                                                      | C#    | TypeScript   | JavaScript | App Manifest| 
|:--:|:-------------------|:----------------------------------------------------------------------------------------------|:--------|:-------------|:---------|:--------|
|1|Personal tabs            | This sample showcases the development of a custom personal tab for Microsoft Teams, utilizing ASP.NET Core MVC and Razor Pages to enhance user interaction.

                      |[MVC][personal-tab#cs#mvc], [Razor][personal-tab#cs#razor]     | [Yeoman Generator](https://docs.microsoft.com/en-us/microsoftteams/platform/get-started/get-started-yeoman) | |[View](/samples/tab-personal/mvc-csharp/demo-manifest/tab-personal.zip) |
|2|Personal tab quick-start| This sample demonstrates how to create a personal tab in Microsoft Teams using JavaScript.                                          |                               |[View][personal-tab-quickstart#ts] ![toolkit-icon](assets/toolkit-icon.png) |[View][personal-tab-quickstart#js] ![toolkit-icon](assets/toolkit-icon.png) 
|3|Personal tab with SSO quick-start| Sample personal tab with SSO hello world app.                          |[View][personal-tab-sso-quickstart#csharp]|[View][personal-tab-sso-quickstart#ts] ![toolkit-icon](assets/toolkit-icon.png) |[View][personal-tab-sso-quickstart#js] ![toolkit-icon](assets/toolkit-icon.png) 
|4|Channel and group tabs   | Sample app showing custom group and channel Tab with ASP. NET Core                                    |[MVC][group-channel-tab#cs#mvc], [Razor][group-channel-tab#cs#razor]     | [Yeoman Generator](https://docs.microsoft.com/en-us/microsoftteams/platform/get-started/get-started-yeoman) | |[View](/samples/tab-channel-group/mvc-csharp/demo-manifest/tab-channel-group.zip) |
|5|Channel and group tab quick-start| This sample application illustrates how to create channel and group tabs in Microsoft Teams, enabling collaborative spaces for web-based content. It includes setup instructions, prerequisites, and features for embedding Teams-aware webpages directly into your Teams environment.                          |                               |[View][group-tab-quickstart#ts] ![toolkit-icon](assets/toolkit-icon.png) |[View][group-tab-quickstart#js] ![toolkit-icon](assets/toolkit-icon.png) |[View](/samples/tab-channel-group-quickstart/js/demo-manifest/tab-channel-group-quickstart.zip) |
|6|Channel and group tab with SSO quick-start| This sample application illustrates how to implement Microsoft Entra SSO authentication in channel tabs within Microsoft Teams, enabling secure access to user data. |[View][group-tab-sso-quickstart#csharp]|[View][group-tab-sso-quickstart#ts] ![toolkit-icon](assets/toolkit-icon.png) |[View][group-tab-sso-quickstart#js] ![toolkit-icon](assets/toolkit-icon.png) 
|7|SPFx Tab | Sample app showing Microsoft Teams tabs using SharePoint Framework                                    |   | [View][group-channel-tab#ts#spfx] |
|8|Tab SSO               | Microsoft Teams sample app for tabs Azure AD SSO                                      |[View][tab-sso#cs] | [View][tab-sso#ts] ![toolkit-icon](assets/toolkit-icon.png) 
|9|Config Tab Authentication               | This sample showcases how to implement Microsoft Entra ID and silent authentication in a configurable Teams tab, enabling secure access to user profile information. | [View][tab-channel-group=config-csharp]                      | | |[View](/samples/tab-channel-group-config-page-auth/csharp/demo-manifest/tab-channel-group-config-page-auth.zip)
|10|Deep Link consuming Subentity ID      |This Teams sample application highlights deeplinks for various functionalities, such as initiating calls, chats, and navigating within tabs and apps. It features a comprehensive setup guide and supports interactions with both bots and tabs for enhanced user engagement. | [View][tab-deeplink#csharp]                       | | [View][tab-deeplink#nodejs] ![toolkit-icon](assets/toolkit-icon.png) |
|11|Integrate graph toolkit component in teams tab      | This demo app illustrates how to implement a Microsoft Teams tab using the Microsoft Graph Toolkit, enabling seamless interaction with user data. It includes features like tabs and easy integration with Microsoft Graph for enhanced functionality. |[View][tab-graph-toolkit#csharp]|[View][tab-graph-toolkit#js] ![toolkit-icon](assets/toolkit-icon.png)|
|12|Device permissions      | This sample app for Microsoft Teams demonstrates how to handle device permissions, including audio, video, and geolocation, within a tab interface. It provides insights into device permission usage across desktop and mobile views, allowing developers to enhance user interactions effectively. |                      | [View][tab-device-permissions#js] ![toolkit-icon](assets/toolkit-icon.png) ||[View](/samples/tab-device-permissions/nodejs/demo-manifest/tab-device-permissions.zip)|
|13|Tab in stage view   | Microsoft Teams tab sample app for demonstrating tab in stage view |[View][tab-stage-view#csharp] |          | [View][tab-stage-view#js] ![toolkit-icon](assets/toolkit-icon.png) |[View](/samples/tab-stage-view/csharp/demo-manifest/tab-stage-view.zip)|
|14|Create Conversational tab     | This Microsoft Teams sample app showcases how to create conversational tabs for users to discuss sub-entities within a tab, enhancing collaboration and interaction. | [View][tab-conversation#csharp]          |           |[View][tab-conversation#nodejs] ![toolkit-icon](assets/toolkit-icon.png) |[View](/samples/tab-conversations/csharp/demo-manifest/tab-conversations.zip)
|15| Product inspection | This application demonstrates how to scan products directly within Microsoft Teams, capturing images and allowing users to approve or reject items. |[View][tab-product-inspection#csharp]| |[View][productinspection#nodejs] ![toolkit-icon](assets/toolkit-icon.png) |[View](/samples/tab-product-inspection/csharp/demo-manifest/Tab-Product-Inspection.zip)
|16| Staggered Permission | This sample demos to get staggered graph api permissions. |[View][tab-staggered-permission#csharp]||[View][tab-staggered-permission#nodejs] ![toolkit-icon](assets/toolkit-icon.png) |[View](/samples/tab-staggered-permission/csharp/demo-manifest/tab-staggered-permission.zip)
|17| Tab people picker |  This sample tab app showcases the People Picker feature using the Teams JavaScript client SDK for selecting users within Microsoft Teams. |[View][tab-people-picker#csharp]||[View][tab-people-picker#nodejs] ![toolkit-icon](assets/toolkit-icon.png) |[View](/samples/tab-people-picker/csharp/demo-manifest/Tab-People-Picker.zip)
|18| Tab channel context | This sample application demonstrates how to access and diFsplay the tab context object in both private and shared channels within Microsoft Teams. ||[View][tab-channel-context#nodejs] ![toolkit-icon](assets/toolkit-icon.png) |
|19| Tab app monetization | This sample tab application showcases how to initiate a purchase flow using the Teams JS SDK. ||[View][tab-app-monetization#nodejs] ![toolkit-icon](assets/toolkit-icon.png) |
|20| Personal Tab with Nav-Bar menu | This sample application illustrates the use of a navbar menu in personal tabs within Microsoft Teams, enabling users to access multiple actions seamlessly. Designed for mobile clients, it includes features like an overflow menu for additional actions, enhancing user navigation and experience. ||[View][tab-navbar-menu#ts] ![toolkit-icon](assets/toolkit-icon.png) |
|21| App Caching in Meetings | This sample shows how to improve the subsequent loading time of an App. |||[View][app-cache-meetings#nodejs] ![toolkit-icon](assets/toolkit-icon.png) |
|22| Tab Multi Hub | This sample application allows users to seamlessly interact with their Calendar, Mail, Profile, and geolocation from personal tabs within Microsoft Teams. With easy setup and integration using TypeScript, it enhances user productivity by centralizing essential tools in one place. ||[View][tab-multi-hub#ts] ![toolkit-icon](assets/toolkit-icon.png) |
|23| tab-ui-templates | This sample app can help you better understand how apps should look and behave in Microsoft Teams. The app includes examples of tested, high-quality UI templates that work across common Teams use cases (such as dashboards or forms). ||[View][tab-ui-templates#ts] ![toolkit-icon](assets/toolkit-icon.png) |
|24| Tab External Auth | This sample app demonstrates the integration of Google OAuth2 for user authentication within Microsoft Teams. It features a tab-based interface that allows users to log in using their Google accounts and interact with the application seamlessly. |[View][tab-external-auth#csharp]||
|25| Tab Meeting Tanscript Recording |This sample application for Microsoft Teams enables users to automatically capture meeting transcripts and recordings, enhancing meeting documentation and accessibility. With a straightforward setup and integration process, it allows teams to focus on discussions while ensuring all details are preserved. |[View][tab-meeting-transcript-recording#csharp]|[View][tab-meeting-transcript-recording#nodejs] ![toolkit-icon](assets/toolkit-icon.png)|
|26| Tab DeepLink Pass Values |This sample app for Microsoft Teams illustrates the creation and usage of dynamic deep links to pass contextual values to tab and standalone web applications. It highlights the differences in link formatting and consumption based on the user’s access environment, enhancing navigation and data display capabilities. ||[View][tab-deeplink-pass-values#ts]|
|27| Tab Meeting Recording and transcript with auto recording |This sample showcases a Microsoft Teams app that enables automatic meeting recordings and transcripts, enhancing collaboration and record-keeping. With a user-friendly interface and seamless integration, it empowers teams to focus on discussions without worrying about missing key details. |[View][tab-meeting-auto-recording#csharp]||
|28| Offline Support Tickets |This sample app demonstrates a classic CRUD application that functions offline within Microsoft Teams. When the Android device reconnects to the internet, users can sync their data with blob storage. ||[View][tab-support-offline#nodejs] ![toolkit-icon](assets/toolkit-icon.png)|[View](/samples/tab-support-offline/nodejs/demo-manifest/tab-support-offline.zip)
|29| Nested App Authentication |This sample showcases Azure AD single sign-on (SSO) within a Microsoft Teams tab, utilizing the On-Behalf-Of (OBO) flow to call |[View][tab-nested-auth#cs]|[View][tab-nested-auth#nodejs] ![toolkit-icon](assets/toolkit-icon.png)|

## [Bots samples](https://docs.microsoft.com/microsoftteams/platform/bots/what-are-bots) (using the v4 SDK)
>NOTE:
>Visit the [Bot Framework Samples repository][botframework] to view Microsoft Bot Framework v4 SDK task-focused samples for C#, JavaScript, TypeScript, and Python.

|    | Sample Name | Description | .NET | JavaScript | Python | Java | App Manifest
|:--:|:-------------------|:---------------------------------------------------------------------------------|:--------|:-------------|:-------------|:-------------|:-------------|
|1| Teams Conversation Bot quick-start | Messaging and conversation event handling hello world. | | [View][bot-conversation-quickstart#js] ![toolkit-icon](assets/toolkit-icon.png) | | |
|2| Teams Conversation Bot SSO quick-start | Messaging and conversation event handling hello world with SSO. | [View][bot-conversation-sso-quickstart#csharp_dotnetcore] | [View][bot-conversation-sso-quickstart#js] ![toolkit-icon](assets/toolkit-icon.png) | | |
|3| Teams Conversation Bot | Messaging and conversation event handling also includes read receipt event functionality in personal scope. Also include AI label, Citations,Feedback buttons,Sensitivity label. | [View][bot-conversation#cs]| [View][bot-conversation#js] ![toolkit-icon](assets/toolkit-icon.png) | [View][bot-conversation#python] | [View][bot-conversation#java] |[View](/samples/bot-conversation/csharp/demo-manifest/bot-conversation.zip)
|4| Message Reactions | Demonstrates how to create a simple bot that responds to Message Reactions | [View][bot-message-reaction#cs] |  [View][bot-message-reaction#js] ![toolkit-icon](assets/toolkit-icon.png) | | [View][bot-message-reaction#java]|[View](/samples/bot-message-reaction/csharp/demo-manifest/bot-message-reaction.zip)
|5| Authentication with OAuthPrompt| Authentication and basic messaging in Bot Framework v4. | [View][bot-teams-authentication#cs]| [View][bot-teams-authentication#nodejs] ![toolkit-icon](assets/toolkit-icon.png) | [View][bot-teams-authentication#python] | [View][bot-teams-authentication#java]|[View](/samples/bot-teams-authentication/csharp/demo-manifest/bot-teams-authentication.zip)
|6| Teams File Upload | Exchanging files with a bot in a one-to-one conversation. | [View][bot-file-upload#cs] | [View][bot-file-upload#js] ![toolkit-icon](assets/toolkit-icon.png) | [View][bot-file-upload#python] | [View][bot-file-upload#java]|[View](/samples/bot-file-upload/csharp/demo-manifest/bot-file-upload.zip)
|7| Task Module | Demonstrating how to retrieve a Task Module and values from cards in it, for a Messaging Extension. | [View][bot-task-module#cs] | [View][bot-task-module#js] ![toolkit-icon](assets/toolkit-icon.png) | [View][bot-task-module#python] | [View][bot-task-module#java]|[View](/samples/bot-task-module/csharp/demo-manifest/bot-task-module.zip)
|8| Start new thread in a channel | Demonstrating how to create a new thread in a channel. | [View][bot-initiate-thread-in-channel#cs] | [View][bot-initiate-thread-in-channel#js] ![toolkit-icon](assets/toolkit-icon.png) | [View][bot-initiate-thread-in-channel#python] | [View][bot-initiate-thread-in-channel#java] |[View](/samples/bot-initiate-thread-in-channel/csharp/demo-manifest/bot-initiate-thread-in-channel.zip)
|9| Universal bots  | Teams catering bot demonstrating how to use Universal bots in Teams | [View](samples/bot-teams-catering/csharp) |  | | |
|10| Sequential workflow adaptive cards | Demonstrating on how to implement sequential flow, user specific view and upto date adaptive cards in bot. | [View][sequential#workflow#csharp] | [View][sequential#workflow#js] ![toolkit-icon](assets/toolkit-icon.png) | | |[View](/samples/bot-sequential-flow-adaptive-cards/csharp/demo-manifest/bot-sequential-flow-adaptive-cards.zip)
|11| Channel messages with RSC permissions | Demonstrating on how a bot can receive all channel messages with RSC without @mention. | [View][messageswithrsc#csharp] |[View][messageswithrsc#js] ![toolkit-icon](assets/toolkit-icon.png) | | |[View](/samples/bot-receive-channel-messages-withRSC/csharp/demo-manifest/Bot-RSC.zip)
|12| Bot with SharePoint file to view in Teams file viewer | This sample demos a bot with capability to upload files to SharePoint site and same files can be viewed in Teams file viewer. |[View][botwithsharepointfileviewer#csharp]|[View][botfileviewer#js] ![toolkit-icon](assets/toolkit-icon.png) 
|13| Type ahead search control on Adaptive Cards | This sample shows the feature of type ahead search (static and dynamic) control in Adaptive Cards. |[View][typeaheadsearch#csharp]|[View][typeaheadsearchonadaptivecard#js] ![toolkit-icon](assets/toolkit-icon.png) | | |[View](/samples/bot-type-ahead-search-adaptive-cards/csharp/demo-manifest/Typeahead-search-adaptive-cards.zip)
|14| People picker control in Adaptive Cards | This sample shows the feature of people picker control in Adaptive Cards. |[View][peoplepickeronadaptivecard#csharp]|[View][peoplepickeronadaptivecard#js] ![toolkit-icon](assets/toolkit-icon.png) | | |[View](/samples/bot-people-picker-adaptive-card/csharp/demo-manifest/People-picker-adaptive-card.zip)
|15| Proactive Messaging sample | This sample shows how to save user's conversation reference information to send proactive reminder message using Bots. This uses Teams toolkit for Visual Studio Code to provide scaffolding experience. ||[View][bot-proactive-msg-teamsfx#js] ![toolkit-icon](assets/toolkit-icon.png) 
|16| Proactive Tab Conversations | Using a bot to create and store conversations that can be later used inside a sub-entity, tab conversation. This sample includes the details required to proactively message a channel from a bot, set-up and store details for channel tab conversations, and viewing channel conversations from a personal app. |[View][bot-tab-conversations#csharp]|
|17| Suggested Actions Bot | This sample shows the feature where user can send suggested actions using bot. |[View][suggestedactionsbot#csharp]|[View][suggestedactionsbot#nodejs] ![toolkit-icon](assets/toolkit-icon.png) | | |[View](/samples/bot-suggested-actions/csharp/demo-manifest/bot-suggested-actions.zip)
|18| Adaptive Card Actions Bot | This sample shows the feature where user can send adaptive card with different actions using bot. |[View][AdaptiveCardActions#csharp]|[View][AdaptiveCardActions#nodejs] ![toolkit-icon](assets/toolkit-icon.png) | | |[View](/samples/bot-adaptive-card-actions/csharp/demo-manifest/bot-adaptivecard-actions.zip)
|19| SSO for your Adaptive Cards | This sample code demonstrates how to get enable SSO authentication for your Adaptive Cards Universal Actions in bot. |[View][AdaptiveCardSSO#csharp]|[View][BotSSOAdaptiveCard#nodejs] ![toolkit-icon](assets/toolkit-icon.png) 
|20| Different formatting on cards | This sample feature shows how to use different formatting on cards using bot. |[View][BotFormattingCards#csharp]|[View][BotFormattingCards#nodejs] ![toolkit-icon](assets/toolkit-icon.png) 
|21| Different types of cards using bot | This sample shows the feature where user can interact with different types of cards using bot like Adaptive, hero, list, Office 365 Connector, receipt, sign in, and thumbnail cards and card collections.|[View][TypesCards#csharp] | [View][TypesCards#nodejs] ![toolkit-icon](assets/toolkit-icon.png) 
|22| User specific views in Adaptive cards | This sample shows the different ways developers can consume user specific views in Adaptive cards. |[View][botadaptivecardsuserspecificviews#csharp]| | | |[View](/samples/bot-adaptivecards-user-specific-views/csharp/demo-manifest/bot-adaptivecards-user-specific-views.zip)
|23| Bot configuration app with type ahead search control.|This sample demonstrates the features of bot configuration and reconfiguration for both teams and group chats, including a type-ahead search (static and dynamic) control on Adaptive Cards.|[View][BotConfigurationApp#csharp]|[View][BotConfigurationApp#nodejs
|23| Bot configuration app with auth |This sample demonstrates the features of bot configuration auth and reconfiguration for both teams and group chats.|[View][BotConfigurationAppAuth#csharp]|[View][BotConfigurationAppAuth#nodejs]
|24| Tag mention bot  | This sample app demonstrates the use of tag mention funtionality in teams scope using Bot Framework. |[View][Tagmention#csharp]|[View][Tagmention#nodejs] ![toolkit-icon](assets/toolkit-icon.png) | | |
|25| Commands menu and prompt starters bot  | This sample add a command menu with a dropdown list of commands for your bot. Users can select a command from the list, which will insert the command title into the compose box. Then, they can select Send to execute the command. |[View][CommandsMenu#csharp]|[View][CommandsMenu#nodejs] ![toolkit-icon](assets/toolkit-icon.png) | | |
|26| Bot All Cards | Sample which demonstrates sending different types cards supported in teams using bot. |[View][TypesCards#csharp]|[View][TypesCards#nodejs] ![toolkit-icon](assets/toolkit-icon.png)  | | |[View](/samples/bot-all-cards/csharp/demo-manifest/bot-all-cards.zip)
|27| Apps in Federated | This sample app allows users to easily view a list of group members. When a new member is added, their details are promptly displayed. ||[View][bot-feed-members#js] ![toolkit-icon](assets/toolkit-icon.png)
|28| Requirement Targeting OneWay Dependency | Microsoft M365 RT sample app in Node.js which specify one-way-dependency relationships between app capabilities (using elementRelationshipSet) and functionality using hostMustSupportFunctionalities. ||[View][RequirementTargetingOneWayDependency#nodejs] ![toolkit-icon](assets/toolkit-icon.png)
|29| Requirement Targeting Mutual Dependency | Microsoft M365 RT sample app in Node.js which specify mutual-dependency relationships between app capabilities using elementRelationshipSet. ||[View][RequirementTargetingMutualDependency#nodejs] ![toolkit-icon](assets/toolkit-icon.png)

#### Additional samples

|    | Sample Name | Description | .NET | JavaScript | 
|:--:|:-------------------|:---------------------------------------------------------------------------------|:--------|:-------------|
|1|Proactive Messaging   | Sample to highlight solutions to two challenges with building proactive messaging apps in Microsoft Teams.                                      |[View][bot-proactive-msg#cs]        |
|2| Sharepoint List Bot| This sample app shows the interaction between teams bot and SharePoint List, Bot saves the specified details in SharePoint List as back-end| [View][bot-sharepoint-list#cs] |  |  |
|3|Teams Virtual Assistant| Customized virtual assistant template to support teams capabilities.        |[View][app-virtual-assistant#cs]|

## [Messaging Extensions samples](https://docs.microsoft.com/microsoftteams/platform/messaging-extensions/what-are-messaging-extensions) (using the v4 SDK)
>NOTE:
>Visit the [Bot Framework Samples repository][botframework] to view Microsoft Bot Framework v4 SDK task-focused samples for C#, JavaScript, TypeScript, and Python.

|    | Sample Name | Description | .NET | JavaScript | Python| Java | App Manifest|
|:--:|:-------------------|:---------------------------------------------------------------------------------|:--------|:-------------|:-------------|:-------------|:-------------
|1|Messaging extensions - search quick-start | Hello world Messaging Extension that accepts search requests and returns results. | | [View][msgext-search-quickstart#js] ![toolkit-icon](assets/toolkit-icon.png) | ||[View](/samples/msgext-search-quickstart/js/demo-manifest/msgext-search-quickstart.zip)
|2|Messaging extensions - search | Messaging Extension that accepts search requests and returns results. | [View][msgext-search#cs] | [View][msgext-search#js] ![toolkit-icon](assets/toolkit-icon.png) | [View][msgext-search#python] | [View][msgext-search#java]|[View](/samples/msgext-search/csharp/demo-manifest/msgext-search.zip)
|3|Messaging extensions - action quick-start | This sample app showcases a Hello World Messaging Extension that processes parameters and generates a card in Microsoft Teams. It also illustrates how to receive and handle forwarded messages, enhancing interaction through action commands and message extensions. | | [View][msgext-action-quickstart#js] ![toolkit-icon](assets/toolkit-icon.png) | | |[View](/samples/msgext-action-quickstart/js/demo-manifest/msgext-action-quickstart.zip)
|4|Messaging extensions - action | This sample demonstrates how to create Action-Based Messaging Extensions for Microsoft Teams, enabling users to interactively generate content. It features bots, message extensions, and seamless integration with user inputs for enhanced functionality.| [View][msgext-action#cs] | [View][msgext-action#js] ![toolkit-icon](assets/toolkit-icon.png) | [View][msgext-action#python] | [View][msgext-action#java]|[View](/samples/msgext-action/csharp/demo-manifest/msgext-action.zip)
|5|Messaging extensions - auth and config | Messaging Extension that has a configuration page, accepts search requests and returns results after the user has signed in. | [View][msgext-search-auth-config#cs] |  | [View][msgext-search-auth-config#python]| |[View](/samples/msgext-search-auth-config/csharp/demo-manifest/msgext-search-auth-config.zip)
|6|Messaging extensions - SSO auth | Messaging Extension that has a configuration page, accepts search requests and returns results with SSO. | [View][msgext-search-sso-config#cs] | [View][msgext-search-sso-config#js] ![toolkit-icon](assets/toolkit-icon.png) |
|7|Messaging extensions - action preview | This sample app illustrates how to utilize action previews in Teams Messaging Extensions, allowing users to create cards from input in a Task Module. It showcases bot interactions that enhance user engagement by attributing messages to users. | [View][msgext-action-preview#cs] | [View][msgext-action-preview#js] ![toolkit-icon](assets/toolkit-icon.png) | [View][msgext-action-preview#python] |[View][msgext-action-preview#java]|[View](/samples/msgext-action-preview/csharp/demo-manifest/msgext-action-preview.zip)
|8|Link unfurling | This sample app showcases a Teams bot built with Bot Framework v4, demonstrating how to implement link unfurling in messaging extensions. | [View][msgext-link-unfurling#cs] | [View][msgext-link-unfurling#js] ![toolkit-icon](assets/toolkit-icon.png) | [View][msgext-link-unfurling#python] | [View][msgext-link-unfurling#java]|[View](/samples/msgext-link-unfurling/csharp/demo-manifest/msgext-link-unfurling.zip)
|9|Messaging extension - Adaptive Card based loop components | Demonstrates how to create Messaging extension with adaptive card-based loop components. | [View][msgext-unfurling-ac-loop-components#cs]    | [View][msgext-unfurling-ac-loop-components#nodejs] ![toolkit-icon](assets/toolkit-icon.png) |   |    |[View](/samples/msgext-unfurling-ac-loop-components/csharp/demo-manifest/msgext-unfurling-ac-loop-components.zip)
|10|Messaging Extensions AI - Sentiment Analysis | This sample allows users to assess the sentiment of messages in Teams chats by utilizing a messaging extension integrated with Open AI. The analysis categorizes messages as positive, negative, or neutral, enhancing understanding of team interactions. |[View][msgext-ai-sentiment-analysis#cs] | [View][msgext-ai-sentiment-analysis#js] ![toolkit-icon](assets/toolkit-icon.png) | | |
|11|Copilot Handoff | This Teams message extension serves as a plugin for Microsoft Copilot, allowing users to efficiently query the Northwind Database. It features copilot handoff capabilities, enabling seamless transitions to custom bots for enhanced interactions. || [View][msgext-copilot-handoff#js] ![toolkit-icon](assets/toolkit-icon.png) | | |

#### Additional samples

|    | Sample Name | Description | .NET | JavaScript | Python |
|:--:|:-------------------|:---------------------------------------------------------------------------------|:--------|:-------------|:-------------|
|1|Link unfurling demo of Reddit        | This sample provides a full implementation of link unfurling for Reddit links in Microsoft Teams, showcasing user authentication and bot integration.                              |[View][msgext-link-unfurl#cs]        |
|2|Link unfurling - setup a meeting     | This sample illustrates a common scenario where a user shares a link to a resource with a group of users, and they collaborate to review it in a meeting.                              |[View][msgext-link-unfurl-meeting#cs]        |

## [Webhooks and Connectors samples](https://docs.microsoft.com/microsoftteams/platform/webhooks-and-connectors/what-are-webhooks-and-connectors)

> [!IMPORTANT]
>
> The existing Microsoft 365 (previously called Office 365) connectors across all cloud platforms are nearing deprecation, and the creation of new Microsoft 365 connectors will soon be blocked. For more information on the schedule and how the Workflows app provides a more flexible and secure experience, see [retirement of Microsoft 365 connectors within Microsoft Teams](https://devblogs.microsoft.com/microsoft365dev/retirement-of-office-365-connectors-within-microsoft-teams/).

|    | Sample Name        | Description                                                                      | C#    | JavaScript   |
|:--:|:-------------------|:-------------------------------------------------------------------------------------------------|:--------|:-------------|
|1|Connectors             | Sample Office 365 Connector generating notifications to teams channel.                              |[View][connector#cs]       |[View][connector#ts]
|2|Generic connectors sample | Sample code for a generic connector that's easy to customize for any system which supports webhooks.   |    |[View][connector-generic#ts]
|3|Outgoing Webhooks      | Samples to create "Custom Bots" to be used in Microsoft Teams.                                        |[View][outgoing-webhook#cs]|[View][outgoing-webhook#ts]
|4|Authentication in todo Connector App      | This is a sample app which shows connector authentication and sends notification on task creation.                                        ||[View][auth-in-connector#nodejs] 
|5|Incoming Webhook      | This sample demonstrates a Teams tab to send message cards using Incoming Webhook, showcasing the HttpPOST action for interactive cards.                                        |[View][incoming-webhook#cs]       |[View][incoming-webhook#nodejs]

## [Graph APIs](https://docs.microsoft.com/graph/teams-concept-overview)

|    | Sample Name        | Description                                                                      | C#    | JavaScript   | App Manifest
|:--:|:-------------------|:-------------------------------------------------------------------------------------------------|:--------|:-------------|:-------------|
|1|Resource Specific Consent (RSC) | This sample illustrates how you can use [Resource Specific Consent (RSC)](https://docs.microsoft.com/en-us/microsoftteams/platform/graph-api/rsc/resource-specific-consent) to call Graph APIs. .                              |[View][graph#rsc#cs]       | [View][graph#rsc#js] ![toolkit-icon](assets/toolkit-icon.png) |[View](/samples/graph-rsc/csharp/demo-manifest/graph-rsc.zip)
|2|Graph API Channel Life Cycle | This sample illustrates how you can use [Graph API Channel Life Cycle](https://docs.microsoft.com/en-us/graph/api/resources/channel?view=graph-rest-beta) to call Graph APIs. .                              |[View][graph#lifecycle#cs]       | [View][graph#lifecycle#js] ![toolkit-icon](assets/toolkit-icon.png) 
|3|Graph API Teams App Installation Life Cycle | This sample illustrates how you can use [Teams App Installation Life Cycle](https://docs.microsoft.com/en-us/graph/api/resources/teamsappinstallation?view=graph-rest-1.0) by calling Microsoft Graph APIs. .                              |[View][graph#instllationlifecycle#cs]       |[View][graph#instllationlifecycle#js] ![toolkit-icon](assets/toolkit-icon.png) |
|4|Graph API Teams App Catalog Life Cycle | This sample demonstrates how to manage the lifecycle of Teams apps in the app catalog using Microsoft Graph APIs through a bot. |[View][graph#appctaloglifecycle#cs]       | [View][graph#appcataloglifecycle#js] ![toolkit-icon](assets/toolkit-icon.png) |[View](/samples/graph-appcatalog-lifecycle/csharp/demo-manifest/graph-appcatalog-lifecycle.zip)
|5|Graph API Chat Life Cycle | This sample illustrates how you can use [Teams App Chat Life Cycle](https://docs.microsoft.com/en-us/graph/api/resources/chat?view=graph-rest-1.0) by calling Microsoft Graph APIs. .                              |[View][graph#chatlifecyle#cs]      |[View][graph#chatlifecycle#js] ![toolkit-icon](assets/toolkit-icon.png) 
|6|Activity Feed Notification  | This Teams sample app sends activity feed notifications using the Graph API within a tab. | [View][graph-activity-feed#cs]                       | [View][graph-activity-feed#js] ![toolkit-icon](assets/toolkit-icon.png) | 
|7|Proactive installation of App and sending proactive notifications | This sample illustrates how you can use [Proactive installation of app for user and send proactive notification](https://docs.microsoft.com/en-us/microsoftteams/platform/graph-api/proactive-bots-and-messages/graph-proactive-bots-and-messages?tabs=csharp) by calling Microsoft Graph APIs.                              |[View][graph#graphproactiveinstallation#cs]       |[View][graph#graphproactiveinstallation#js] ![toolkit-icon](assets/toolkit-icon.png) |[View](/samples/graph-proactive-installation/csharp/demo-manifest/graph-proactive-installation.zip)
|8|Change Notification  | This sample app demonstrates sending change notifications to user presence in Teams based on user presence status. | [View][graph-change-notification#cs]                        |[View][graph-change-notification#js] ![toolkit-icon](assets/toolkit-icon.png) 
|9|Graph Pinned Message  | This is a sample application which demonstrates how to pin messages in chat using Graph api. | [View][graph-pinned-message#cs]|[View][graph-pinned-message#js] ![toolkit-icon](assets/toolkit-icon.png) |
|10|Graph Bulk Meetings  | This is an sample application which shows how to create Teams meetings in bulk using Graph api. | [View][graph-bulk-meetings#cs]|[View][graph-bulk-meetings#js] ![toolkit-icon](assets/toolkit-icon.png)|
|11|Graph Meeting Notification  | This is a sample application which demonstrates the use of online meeting subscription and sends you the notifications in chat using bot. | [View][graph-meeting-notification#cs]|[View][graph-meeting-notification#js] ![toolkit-icon](assets/toolkit-icon.png)|
|12|Change Notifications Team/Channel  | This sample application which demonstrates use of Team/Channel subscription that will post notifications when user create/edit/delete team/channel using Graph api. | [View][change-notifications-team/channel#cs]|[View][change-notifications-team/channel#js] ![toolkit-icon](assets/toolkit-icon.png)|
|13|graph-teams-tag  | This is a sample application which demonstrates how to use CRUD Graph operations within tab related to team tags. | [View][graph-teams-tag#cs]|[View][graph-teams-tag#js] ![toolkit-icon](assets/toolkit-icon.png)|
|14|Graph API Explorer for RSC | Resource-specific consent (RSC) is a Microsoft Teams and Microsoft Graph API integration that enables your app to use API endpoints to manage specific teams within an organization | |[View][graph-rsc-helper#js] ![toolkit-icon](assets/toolkit-icon.png)|

## [Calls and online meetings bots](https://docs.microsoft.com/microsoftteams/platform/bots/calls-and-meetings/calls-meetings-bots-overview)

|    | Sample Name       | Description                                                                      | C#    |
|:--:|:------------------|:---------------------------------------------------------------------------------------------------|:--------|
|1|Calling and Meeting bot          | This sample app demonstrate how a Bot can create a call, join a meeting and transfer the call |[View][bot-calling-meeting#csharp]     |
|2|Local Media Samples        |Local media samples give the developer direct access to the inbound and outbound media streams.                      |[View](https://github.com/microsoftgraph/microsoft-graph-comms-samples/tree/master/Samples/V1.0Samples/LocalMediaSamples) | 
|3|Remote Media Samples      | The Incident Bot sample is a Remote Media sample demonstrating a simple incident process workflow started by a Calling Bot..                                      |[View](https://github.com/microsoftgraph/microsoft-graph-comms-samples/tree/master/Samples/V1.0Samples/RemoteMediaSamples)       |

## [Meetings samples](https://learn.microsoft.com/microsoftteams/platform/apps-in-teams-meetings/teams-apps-in-meetings)
|    | Sample Name    | Description                                                                      | C#    | JavaScript      |App Manifest|
|:--:|:---------------|:---------------------------------------------------------------------------------|:------|:----------------|:-------|
|1|Meetings Extensibility | Microsoft Teams meeting extensibility sample: token passing |[View][meetings-token-app#cs]     |[View][meetings-token-app#js] ![toolkit-icon](assets/toolkit-icon.png) 
|2|Meetings notification | Microsoft Teams meeting extensibility sample for interacting with in-meeting notifications |[View][meetings-notification#cs]    |[View][meetings-notification#js] ![toolkit-icon](assets/toolkit-icon.png) 
|3|Meetings SidePanel | Microsoft Teams meeting extensibility sample for iteracting with Side Panel in-meeting |[View][meetings-sidepanel#cs]     | [View][meetings-sidepanel#js] ![toolkit-icon](assets/toolkit-icon.png) 
|4|Details Tab in Meetings | Microsoft Teams meeting extensibility sample for iteracting with Details Tab in-meeting |[View][meetings-details-tab#cs]  |[View][meetings-details-tab#js] ![toolkit-icon](assets/toolkit-icon.png) |[View](/samples/meetings-details-tab/csharp/demo-manifest/meetings-details-tab.zip)
|5|Meetings Stage View | Enable and configure your apps for Teams meetings to use in stage view |[View][meetings-stage-view#cs] |[View][meetings-stage-view#js] ![toolkit-icon](assets/toolkit-icon.png) |[View](/samples/meetings-stage-view/csharp/demo-manifest/Meeting-stage-view.zip)
|6|Meeting Events | Get real time meeting events  |[View][meetings-events#cs] | [View][meetings-events#js] ![toolkit-icon](assets/toolkit-icon.png) |[View](/samples/meetings-events/csharp/demo-manifest/Meetings-Events.zip)
|7|Meeting Recruitment App | This sample demonstrates a recruitment meeting app with integrated bots for notifications, meeting details, and in-meeting features to assist the interview process.|[View][meeting-recruitment-app#cs] |[View][meeting-recruitment-app#js] ![toolkit-icon](assets/toolkit-icon.png)|[View](/samples/meeting-recruitment-app/csharp/demo-manifest/Meeting-Recruitment-App.zip)
|8|Meeting Transcript App | This is a sample application which demonstrates how to get Transcript using Graph API and show it in the task module.|[View][meetings-transcription-app#cs] |[View][meetings-transcription-app#nodejs] ![toolkit-icon](assets/toolkit-icon.png)|
|9|Meeting signing programmatic share to stage|Demonstrating the programmatic share to stage feature, by means of a document signing in a meeting.|[View][meetings-share-to-stage-signing#csharp]||
|10|Meeting Live Caption     | This is a sample meeting side panel application which demonstrates how to enable live caption in the meeting and using the CART link how to send caption in live meeting.                                       |[View][meetings-live-caption#cs]       |[View][meetings-live-caption#nodejs] ![toolkit-icon](assets/toolkit-icon.png) 
|11|Anonymous user support| This sample demonstrates how to support anonymous users in Microsoft Teams meeting apps, showcasing the setup and interaction flow for integrating guest and tenant users seamlessly.|[View][app-anonymous-users#cs] |[View][app-anonymous-users#js] ![toolkit-icon](assets/toolkit-icon.png) |      
|12|Live coding interview using Shared meeting stage |This sample demos a live coding in a teams meeting stage.|[View][meetings-live-code-interview#csharp]|[View][meetings-live-code-interview#nodejs] ![toolkit-icon](assets/toolkit-icon.png)|
|13|Meeting-Tabs|This app allows muting/unmuting Teams meeting audio and displays stage view in the meeting side panel.|[View][meetingtabs#csharp]|[View][meetingtabs#nodejs] ![toolkit-icon](assets/toolkit-icon.png)|
|14|meetings-attendance-report|This is a sample application which demonstrates how to get meeting attendance report using Graph API and send it in meeting chat using bot.|[View][meetings-attendance-report#csharp]|[View][meetings-attendance-report#nodejs] ![toolkit-icon](assets/toolkit-icon.png)|
|15|meetings-audio-state|This is an sample tab (side panel) application which shows how to mute/unmute Teams meeting audio using toggle Incoming Client Audio API.||[View][meetings-audio-state#nodejs] ![toolkit-icon](assets/toolkit-icon.png)|
|16|meetings-context-app|This sample shows the contents of meeting tab context object in a meeting tab and using bot's meeting API, meeting participant details and meeting details are sent to user.|[View][meetings-context-app#csharp]|[View][meetings-context-app#nodejs] ![toolkit-icon](assets/toolkit-icon.png)|

## [Actions Preview](https://developer.microsoft.com/en-us/microsoft-365/dev-program)
|    | Sample Name    | Description                                                                      | C#    | JavaScript      |App Manifest|
|:--:|:---------------|:---------------------------------------------------------------------------------|:------|:----------------|:-------|
|1|Actions across Microsoft 365 | This sample demonstrates how to build Teams apps using Actions in the Microsoft 365 app to streamline task management and enhance productivity by minimizing context switching.| |[View][m365-actions-preview#js] ![toolkit-icon](assets/toolkit-icon.png) 

## Scenario specific samples

|    | Sample Name    | Description                                                                      | C#    | JavaScript      |App Manifest|
|:--:|:---------------|:---------------------------------------------------------------------------------|:------|:----------------|:-------|
|1|Teams Dialogs (referred as task modules in TeamsJS v1.x)         | Sample app showing off the Teams Dialogs(referred as task modules in TeamsJS v1.x), a way to invoke custom code from a bot, a tab, or both! |[View][bot-task-module#cs]     |[View][bot-task-module#js] ![toolkit-icon](assets/toolkit-icon.png) |[View](/samples/bot-task-module/csharp/demo-manifest/bot-task-module.zip)
|2|Authentication| This sample showcases how to implement authentication in Microsoft Teams using both a bot and a tab, supporting Teams SSO with MSAL.js 2.0.| | [View][app-auth#ts]
|3|Complete Samples      | Sample covering multiple scenarios - dialogs, ME, and facebook auth.                                      |[View][app-complete#cs]        |[View][app-complete#ts] ![toolkit-icon](assets/toolkit-icon.png) 
|4|Region Selection App | This Microsoft Teams sample app enables region selection for data centers using both a Bot and a Tab, with interactions through adaptive cards. It demonstrates how to build configurable tabs and conversational bots with Microsoft Teams SDK. |[View][region-selection-app#cs]     ||[View](/samples/app-region-selection/csharp/demo-manifest/app-region-selection.zip)
|5|App Localization | This sample demonstrates how to add localization support to Microsoft Teams apps with Bots and Tabs integration. It includes detailed steps for enabling language-specific content and interactions for a seamless multilingual experience. |[View][app-localization#cs]  |[View][app-localization#js] ![toolkit-icon](assets/toolkit-icon.png) |[View](/samples/app-localization/csharp/demo-manifest/app-localization.zip)
|6|App SSO | This C# sample app provides single sign-on (SSO) functionality for Microsoft Teams. It includes authentication support for Teams Tab, Bot, and Messaging Extension with features like user profile retrieval and secure API access using Azure AD. |[View][app-sso#cs] |[View][app-sso#js] ![toolkit-icon](assets/toolkit-icon.png) |[View](/samples/app-sso/csharp/demo-manifest/App-SSO.zip)
|7| App Installtion  using QR code |This sample showcases app installation in Microsoft Teams through QR code scanning, enabling users to generate a QR code that contains the app ID for easy installation. Key features include bot interaction, Teams SSO, adaptive cards, task modules, and device permissions for enhanced user experience. | [View][qrappinstallation#csharp] | [View][qrappinstallation#nodejs] ![toolkit-icon](assets/toolkit-icon.png) |[View](/samples/app-installation-using-qr-code/csharp/demo-manifest/App-Installation-Using-QR.zip)
|8| Archive groupchat messages | Demonstrating on how a bot can archive groupchat messages and send it to user as a file. | [View][fetchgroupchatmessages#csharp] |[View][fetchgroupchatmessages#nodejs] ![toolkit-icon](assets/toolkit-icon.png) |
|9| App check in location | This sample app allows users to check in with their current location and view a history of previous check-ins via a bot in Microsoft Teams. It showcases features such as bot interaction, adaptive cards, task modules, and the Device Permission API for location access, providing a seamless experience across supported platforms. | [View][appcheckinlocation#csharp] |[View][checkinlocation#nodejs] ![toolkit-icon](assets/toolkit-icon.png) |[View](/samples/app-checkin-location/csharp/demo-manifest/App-checkin-location.zip)
|10| Message reminder setup through messaging extension | Demonstrating a feature where user can schedule a task from messaging extension action command and get a reminder card at a scheduled time |[View][memessagereminder#csharp]|[View][msgext-message-reminder#nodejs] ![toolkit-icon](assets/toolkit-icon.png) |[View](/samples/msgext-message-reminder/csharp/demo-manifest/msgext-message-reminder.zip)
|11| Bot daily task reminder | This sample demos a feature where user can schedule a recurring task and get a reminder on the scheduled time |[View][botdailytaskreminder#csharp] |[View][bottaskreminder#nodejs] ![toolkit-icon](assets/toolkit-icon.png) |[View](/samples/bot-daily-task-reminder/csharp/demo-manifest/Bot-Daily-Task-Reminder.zip)
|12| Tab request approval | This sample app enables users to request task approvals through activity feed notifications, allowing managers to easily approve or reject requests. |[View][tab-request-approval#csharp]|[View][tab-request-approval#nodejs] ![toolkit-icon](assets/toolkit-icon.png) |[View](/samples/tab-request-approval/csharp/demo-manifest/Tab-Request-Approval.zip)
|13| Bot request approval | Demonstrating a feature where user can send task request to his manager using universal adaptive card and manager can approve/reject the request. |[View][bot-request-approval#csharp]|[View][bot-request-approval#nodejs] ![toolkit-icon](assets/toolkit-icon.png) |[View](/samples/bot-request-approval/csharp/demo-manifest/Bot-Request-Approval.zip)
|14| Join the Team using QR code |This sample demos a feature where user can join a team using QR code containing the team's id. |[View][qrjointeam#csharp]|[View][qrjointeam#nodejs] ![toolkit-icon](assets/toolkit-icon.png) |[View](/samples/bot-join-team-using-qr-code/csharp/demo-manifest/Bot-Join-Team-By-QR.zip)
|15| Activity feed broadcast |This sample enables broadcasting messages to all organization members via Teams activity feed notifications using Graph APIs and SSO integration.|[View][graph-activity-feed-broadcast#csharp]|[View][graph-activity-feed-broadcast#js] ![toolkit-icon](assets/toolkit-icon.png) |
|16|App complete auth|This sample showcases Azure AD and Facebook authentication across bots, tabs, and messaging extensions in Microsoft Teams.|[View][app-complete-auth#cs]|
|17|Release Management     | This is a sample used to send workitem notification using Azure webhook.                                        |[View][release-management#cs]       |[View][release-management#nodejs]
|18|Link unfurling for 'Share to teams'|This sample demos the feature of link unfurling for Share to teams.||[View][link-unfurling-stt#nodejs] ![toolkit-icon](assets/toolkit-icon.png) |
|19|App Icon Badging In Microsoft Teams Meetings|This sample app showcases how to implement app icon badging notifications in Teams meetings.|[View][meetings-app-icon-badging#csharp]|[View][meetings-app-icon-badging#nodejs] ![toolkit-icon](assets/toolkit-icon.png) |
|20|Teams Open AI Embeddings And Redis Search based On Uploaded File Using Bot|This Open AI sample app demonstrates the user search functionality using Open AI embeddings and completion APIs.||[View][bot-ai-enterprise-search#nodejs] ![toolkit-icon](assets/toolkit-icon.png) |
|21|Combined Tab and Office Add-in|This sample app integrates an Outlook add-in with a Teams tab to streamline the creation and management of discount offers. Users can insert discounts into emails and view consolidated data within Teams, enhancing collaboration and efficiency.||[View][tab-add-in-combined#ts] ![toolkit-icon](assets/toolkit-icon.png)|[View](/samples/tab-add-in-combined/nodejs/appPackage/build/appPackage.local.zip)|
|22|Sharepoint tab request approval | This sample app demonstrates the usage of SharePoint Embedded for storage in teams. |[View][sharepoint-tab-request-approval#csharp]||
|23|App HR talent |This sample app demonstrates integration with a simulated recruiting and talent management system, allowing users to create, manage, and search positions and candidates using Microsoft Teams' bots, tabs, and messaging extensions.|[View][app-HR-talent#csharp]||[View](/samples/app-HR-talent/csharp/src/demo-manifest/TeamsTalentMgmt.zip)

## Application templates

|    | App Name       | Description                                                                      | Code   |
|:--:|:------------------|:---------------------------------------------------------------------------------------------------|:--------|
|1|QBot          | QBot is a solution designed for classroom teaching scenarios which allows teachers, tutors, and students to intelligently answer each other's questions within the Microsoft Teams collaboration platform. |[View][msteams-app-qbot]
|2|Resource Hub          | Resource Hub is a solution designed for all the help you need to use Teams, all in one place. |[View][msteams-app-resource-hub]


[app-hello-world#cs]:samples/app-hello-world/csharp
[app-hello-world#ts]:samples/app-hello-world/nodejs
[personal-tab-quickstart#ts]:samples/tab-personal-quickstart/ts
[personal-tab-quickstart#js]:samples/tab-personal-quickstart/js
[personal-tab-sso-quickstart#ts]:samples/tab-personal-sso-quickstart/ts
[personal-tab-sso-quickstart#csharp]:samples/tab-personal-sso-quickstart/csharp_dotnetcore
[personal-tab-sso-quickstart#js]:samples/tab-personal-sso-quickstart/js
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
[tab-meeting-auto-recording#csharp]:samples/tab-meeting-auto-recording/csharp
[tab-deeplink-pass-values#ts]:samples/tab-deeplink-pass-values/ts
[tab-nested-auth#nodejs]:samples/tab-nested-auth/nodejs
[tab-support-offline#nodejs]:samples/tab-support-offline/nodejs
[tab-nested-auth#cs]:samples/tab-nested-auth/csharp
[group-channel-tab#cs#razor]:samples/tab-channel-group/razor-csharp
[group-channel-tab#cs#mvc]:samples/tab-channel-group/mvc-csharp
[group-channel-tab#ts#spfx]:samples/tab-channel-group/spfx

[connector#cs]:samples/connector-todo-notification/csharp
[incoming-webhook#cs]:samples/incoming-webhook/csharp
[release-management#cs]:samples/bot-release-management/csharp
[release-management#nodejs]:samples/bot-release-management/nodejs
[connector#ts]:samples/connector-github-notification/nodejs
[connector-generic#ts]:samples/connector-generic/nodejs
[sequential#workflow#csharp]:samples/bot-sequential-flow-adaptive-cards/csharp
[sequential#workflow#js]:samples/bot-sequential-flow-adaptive-cards/nodejs
[app-auth#ts]:samples/app-auth/nodejs
[auth-in-connector#nodejs]:samples/connector-todo-notification/nodejs
[botwithsharepointfileviewer#csharp]:samples/bot-sharepoint-file-viewer/csharp
[typeaheadsearch#csharp]:samples/bot-type-ahead-search-adaptive-cards/csharp
[graph-activity-feed-broadcast#csharp]:samples/graph-activity-feed-broadcast/csharp


[app-task-module#ts]:samples/app-task-module/nodejs
[app-task-module#cs]:samples/app-task-module/csharp
[app-complete#cs]:samples/app-complete-sample/csharp
[app-complete#ts]:samples/app-complete-sample/nodejs

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
[msgext-expert-finder#nodejs]:samples/msgext-expert-finder-js

[tab-sso#ts]:samples/tab-sso/nodejs
[tab-sso#cs]:samples/tab-sso/csharp

[app-virtual-assistant#cs]:samples/app-virtual-assistant/csharp
[identity-linking-with-sso#cs]:samples/app-identity-linking-with-sso/csharp
[bot-proactive-msg#cs]:samples/bot-proactive-messaging/csharp
[bot-proactive-msg-teamsfx#js]:samples/bot-proactive-messaging-teamsfx
[bot-feed-members#js]:samples/bot-feed-members/nodejs
[bot-conversation-quickstart#js]:samples/bot-conversation-quickstart/js
[bot-conversation-sso-quickstart#js]:samples/bot-conversation-sso-quickstart/js
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
[BotConfigurationApp#nodejs]:samples/bot-configuration-app/nodejs  
[BotConfigurationApp#csharp]:samples/bot-configuration-app/csharp  
[BotConfigurationAppAuth#nodejs]:samples/bot-configuration-app-auth/nodejs  
[BotConfigurationAppAuth#csharp]:samples/bot-configuration-app-auth/csharp
[AdaptiveCardActions#nodejs]:samples/bot-adaptive-card-actions/nodejs
[AdaptiveCardSSO#csharp]:samples/bot-sso-adaptivecard/csharp
[BotSSOAdaptiveCard#nodejs]:samples/bot-sso-adaptivecard/nodejs
[TypesCards#csharp]:samples/bot-all-cards/csharp
[TypesCards#nodejs]:samples/bot-all-cards/nodejs
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
[change-notifications-team/channel#cs]:samples/graph-change-notification-team-channel/csharp
[change-notifications-team/channel#js]:samples/graph-change-notification-team-channel/nodejs
[graph-teams-tag#cs]:samples/graph-teams-tag/csharp
[graph-teams-tag#js]:samples/graph-teams-tag/nodejs
[graph-rsc-helper#js]:samples/graph-rsc-helper/nodejs
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

[bot-conversation#js]:samples/bot-conversation/nodejs
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
[bot-conversation-sso-quickstart#js]:samples/bot-conversation-sso-quickstart/js
[bot-teams-authentication#nodejs]:samples/bot-teams-authentication/nodejs

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
