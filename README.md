# Microsoft Teams Samples
##### [Click here to find out what's new with Microsoft Teams Platform](https://docs.microsoft.com/microsoftteams/platform/whats-new)

>NOTE: These samples are designed to help understand Microsoft Teams platform capabilities and scenarios. If you are looking for production ready apps then please visit [App Templates for Microsoft Teams](https://docs.microsoft.com/microsoftteams/platform/samples/app-templates)

# Sample lists

1. [Ready to try sample manifests](#try-it-yourself---experience-the-apps-in-your-microsoft-teams-client)
1. [Teams Toolkit samples](#samples-built-using-new-generation-of-teams-development-tool---teams-toolkit)
1. [Getting Started Samples and Tutorials](#getting-started---samples-and-tutorials)
1. [Tabs samples](#Tabs-samples)
1. [Bots samples (using the v4 SDK)](#Bots-samples-using-the-v4-SDK)
1. [Messaging Extensions samples (using the v4 SDK)](#Messaging-Extensions-samples-using-the-v4-SDK)
1. [Webhooks and Connectors samples](#Webhooks-and-Connectors-samples)
1. [Graph APIs](#Graph-APIs)
1. [Calls and online meetings bots](#Calls-and-online-meetings-bots)
1. [Scenario specific samples](#Scenario-specific-samples)


## Try it yourself - experience the Apps in your Microsoft Teams client

Here are all the samples which are deployed on Microsoft Azure and you can try it yourself by uploading respective app packages (.zip files links below) to one of your teams and/or as a personal app. (Sideloading must be enabled for your tenant; [see steps here](https://docs.microsoft.com/en-us/microsoftteams/platform/concepts/build-and-test/prepare-your-o365-tenant#enable-custom-teams-apps-and-turn-on-custom-app-uploading).) These apps are running on the free Azure tier, so it may take a while to load (warm up) if you haven't used it recently and it goes back to sleep quickly if it's not being used. Rest assured, once it's loaded it's pretty snappy.


|    | Sample Name        | Description                                                                                                                | Manifest| 
|:--:|:-------------------|:---------------------------------------------------------------------------------------------------------------------------|:--------|
|1|SSO Demo        | Microsoft Teams app SSO for Tab, Bot, ME - search, action, linkunfurl |[View](/demo%20manifest/SSO-Demo.zip)     |
|2|Complete sample Demo       | Complete sample to showcase various capabilities including bots, tabs, messaging extension |[View](/demo%20manifest/Complete-Sample.zip)     |
|3|Task Module Demo       | Sample app showing off the Teams Task Module, a way to invoke custom code from a bot, a tab, or both |[View](/demo%20manifest/Task-Module.zip)     |
|4|Connectors        | Sample Office 365 Connector generating notifications to teams channel. |[View](/demo%20manifest/Connector.zip)     |
|5|Tabs with Adaptive Cards  | Microsoft Teams tab sample code which demonstrates how to Build tabs with Adaptive Cards. |[View](/demo%20manifest/JSON-based-Tabs.zip)     |
|6|Meetings Stage View Demo  | Enable and configure your apps for Teams meetings to use in stage view. |[View](/demo%20manifest/Meetings-StageView.zip)     |
|7|Meeting Events | Microsoft Teams sample app to get real time meeting events. |[View](/demo%20manifest/Meetings-Events.zip)     |
|8|Channel messages with RSC permissions | Demonstrating on how a bot can receive all channel messages with RSC without @mention. |[View](/demo%20manifest/Bot-RSC.zip)     |
|9|Meeting Recruitment App | Sample app showing meeting app experience for interview scenario. |[View](/demo%20manifest/Meeting-Recruitment-App.zip)     |
|10|App installation using QR code | Sample app that generates a QR or installs app using the QR code.|[View](/demo%20manifest/App-Installation-Using-QR.zip)     |
|11|App checkin location | Demonstrating feature where user can checkin with current location and view all previous checkins.|[View](/demo%20manifest/App-checkin-location.zip)     |
|12|Teams tab graph toolkit | This sample demos building teams tab using microsoft graph toolkit|[View](/demo%20manifest/Teams-tab-graph-toolkit.zip)     |
|13| Product inspection | Demonstrating a feature where user can scan a product and mark it as approved/rejected. |[View](/demo%20manifest/Tab-Product-Inspection.zip)     |
|14| Bot with SharePoint file to view in Teams file viewer| This sample demos a bot with capability to upload files to SharePoint site and same files can be viewed in Teams file viewer.|[View](/demo%20manifest/Bot-SharePoint-File-Viewer.zip)     |
|15|Bot daily task reminder | This sample demos a feature where user can schedule a recurring task and get a reminder on the scheduled time|[View](/demo%20manifest/Bot-Daily-Task-Reminder.zip)     |
|16| Message reminder setup through messaging extension | This sample demos a feature where user can schedule a task from messaging extension action command and get a reminder card at scheduled date and time.|[View](/demo%20manifest/Message-Reminder-Messaging-extension.zip)     |
|17| Task approval using activity feed notification|Demonstrating a feature where user can raise the requests and manager will be notified about the requests and manager will be redirected to approve/reject the request from received notification.|[View](/demo%20manifest/Tab-Request-Approval.zip)     |
|18|Bot request approval|Demonstrating a feature where user can send task request to his manager using universal adaptive card and manager can approve/reject the request.|[View](/demo%20manifest/Bot-Request-Approval.zip) |
|19|Incoming Webhook|This sample is used to send card using incoming webhook.|[View](/demo%20manifest/Incoming-Webhook.zip) |
|20|People picker control in Adaptive Cards | This sample shows the feature of people picker control in Adaptive Cards.|[View](/demo%20manifest/People-Picker-Adaptive-Card.zip) |
|21|App complete auth|This sample demos authentication feature in bot,tab and messaging extension.|[View](/demo%20manifest/App-Complete-Auth.zip) |
|22|Meeting signing programmatic share to stage|Demonstrating the programmatic share to stage feature, by means of a document signing in a meeting. |[View](/demo%20manifest/Meeting-Share-To-Stage-Signing.zip) |
|23|Tab people picker|This is an tab app which shows the feature of client sdk people picker. |[View](/demo%20manifest/Tab-People-Picker.zip) |
## [Samples built using new generation of Teams development tool - Teams Toolkit](https://github.com/OfficeDev/TeamsFx-Samples)

The [Teams Toolkit](https://marketplace.visualstudio.com/items?itemName=TeamsDevApp.ms-teams-vscode-extension) is an extension on Visual Studio Code and Visual Studio. It enable Teams developers to create and deploy Teams apps with integrated identity, access to cloud storage, data from Microsoft Graph, and other services in Azure and M365 with a "zero-configuration" approach to the developer experience. And [Sample Apps](https://github.com/OfficeDev/TeamsFx-Samples) are provided in Teams Toolkit. Download and try it by today! [Learn more about Teams Toolkit](https://docs.microsoft.com/en-us/microsoftteams/platform/toolkit/visual-studio-code-overview).


## [Getting Started - Samples and Tutorials](https://docs.microsoft.com/microsoftteams/platform/tutorials/get-started-dotnet-app-studio)

|    | Sample Name        | Description                                                                                                                | C#    | TypeScript   
|:--:|:-------------------|:---------------------------------------------------------------------------------------------------------------------------|:--------|:-------------|
|1|Hello World            | Microsoft Teams hello world sample app.                                           |[View][app-hello-world#cs]     |[View][app-hello-world#ts]

## [Tabs samples](https://docs.microsoft.com/microsoftteams/platform/tabs/what-are-tabs)
|    | Sample Name        | Description                                                                      | C#    | TypeScript   | JavaScript
|:--:|:-------------------|:----------------------------------------------------------------------------------------------|:--------|:-------------|:---------
|1|Personal tabs            | Sample app showing custom personal Tab with ASP. NET Core                      |[MVC][personal-tab#cs#mvc], [Razor][personal-tab#cs#razor]     | [Yeoman Generator](https://docs.microsoft.com/microsoftteams/platform/tabs/quickstarts/create-personal-tab-node-yeoman#generate-your-project) |
|2|Personal tab quick-start| Sample personal tab quick-start app.                                            |                               |[View][personal-tab-quickstart#ts]|[View][personal-tab-quickstart#js]
|3|Personal tab with SSO quick-start| Sample personal tab with SSO hello world app.                          |[View][personal-tab-sso-quickstart#csharp]|[View][personal-tab-sso-quickstart#ts]|[View][personal-tab-sso-quickstart#js]
|4|Channel and group tabs   | Sample app showing custom group and channel Tab with ASP. NET Core                                    |[MVC][group-channel-tab#cs#mvc], [Razor][group-channel-tab#cs#razor]     | [Yeoman Generator](https://docs.microsoft.com/en-us/microsoftteams/platform/get-started/get-started-yeoman) |
|5|Channel and group tab quick-start| Sample channel and group tab hello world app.                          |                               |[View][group-tab-quickstart#ts]|[View][group-tab-quickstart#js]
|6|Channel and group tab with SSO quick-start| Sample channel and group tab with SSO hello world app.        |[View][group-tab-sso-quickstart#csharp]|[View][group-tab-sso-quickstart#ts]|[View][group-tab-sso-quickstart#js]
|7|SPFx Tab | Sample app showing Microsoft Teams tabs using SharePoint Framework                                    |   | [View][group-channel-tab#ts#spfx] |
|8|Tab SSO               | Microsoft Teams sample app for tabs Azure AD SSO                                      | | [View][tab-sso#ts] ,[Teams Toolkit](https://docs.microsoft.com/microsoftteams/platform/toolkit/visual-studio-code-tab-sso)
|9|Config Tab Authentication               | Microsoft Teams sample app for config tabs Azure AD authentication | [View]()                       | | 
|10|Deep Link consuming Subentity ID      | Microsoft Teams sample app for demonstrating deeplink from Bot chat to Tab consuming Subentity ID | [View][tab-deeplink#csharp]                       | | [View][tab-deeplink#nodejs]|
|11|Integrate graph toolkit component in teams tab      | Microsoft Teams tab sample app for demonstrating graph toolkit component |[View][tab-graph-toolkit#csharp]|[View][tab-graph-toolkit#js]|
|12|Device permissions      | Microsoft Teams tab sample app for demonstrating device permissions |                      | [View][tab-device-permissions#js]|
|13|Build tabs with Adaptive Cards | Microsoft Teams tab sample code which demonstrates how to [Build tabs with Adaptive Cards](https://docs.microsoft.com/microsoftteams/platform/tabs/how-to/build-adaptive-card-tabs) | [View][tab-adaptive-cards#csharp]|| [View][tab-adaptive-cards#js]|
|14|Tab in stage view   | Microsoft Teams tab sample app for demonstrating tab in stage view |[View][tab-stage-view#csharp] |          | [View][tab-stage-view#js] |
|15|Create Conversational tab     | Microsoft Teams tab sample app for demonstrating create conversation tab | [View][tab-conversation#csharp]          |           |[View][tab-conversation#nodejs]
|16| Product inspection | Demonstrating a feature where user can scan a product and mark it as approved/rejected. |[View][tab-product-inspection#csharp]|[View][productinspection#nodejs]|
|17| Staggered Permission | This sample demos to get staggered graph api permissions. |[View][tab-staggered-permission#csharp]|[View][tab-staggered-permission#nodejs]|
|18| Tab people picker | This is an tab app which shows the feature of client sdk people picker. |[View][tab-people-picker#csharp]|[View][tab-people-picker#nodejs]|
|19| Tab channel context | This sample shows the contents of tab context object in a private and shared channel. ||[View][tab-channel-context#nodejs]|
|20| Tab meeting context | This sample shows the contents of meeting tab context object. ||[View][tab-meeting-context#nodejs]|
|21| Tab app monetization | This sample shows how to open purchase dialog and trigger purchase flow using teams-js sdk. ||[View][tab-app-monetization#nodejs]|

## [Bots samples](https://docs.microsoft.com/microsoftteams/platform/bots/what-are-bots) (using the v4 SDK)
>NOTE:
>Visit the [Bot Framework Samples repository][botframework] to view Microsoft Bot Framework v4 SDK task-focused samples for C#, JavaScript, TypeScript, and Python.

|    | Sample Name | Description | .NET Core | JavaScript | Python |
|:--:|:-------------------|:---------------------------------------------------------------------------------|:--------|:-------------|:-------------|
|1| Teams Conversation Bot quick-start | Messaging and conversation event handling hello world. | | [View][bot-conversation-quickstart#js]|| 
|2| Teams Conversation Bot SSO quick-start | Messaging and conversation event handling hello world with SSO. | [View][bot-conversation-sso-quickstart#csharp_dotnetcore] | [View][bot-conversation-sso-quickstart#js]|| 
|3| Teams Conversation Bot | Messaging and conversation event handling. | [View](https://github.com/microsoft/BotBuilder-Samples/tree/main/samples/csharp_dotnetcore/57.teams-conversation-bot)| [View](https://github.com/microsoft/BotBuilder-Samples/tree/main/samples/javascript_nodejs/57.teams-conversation-bot)| [View](https://github.com/microsoft/BotBuilder-Samples/tree/main/samples/python/57.teams-conversation-bot) | 
|4| Message Reactions | Demonstrates how to create a simple bot that responds to Message Reactions | [View](https://github.com/microsoft/BotBuilder-Samples/tree/main/samples/csharp_dotnetcore/25.message-reaction)  |  [View](https://github.com/microsoft/BotBuilder-Samples/tree/main/samples/javascript_nodejs/25.message-reaction) |
|5| Authentication with OAuthPrompt| Authentication and basic messaging in Bot Framework v4. | [View](https://github.com/microsoft/BotBuilder-Samples/tree/main/samples/csharp_dotnetcore/46.teams-auth)| [View](https://github.com/microsoft/BotBuilder-Samples/tree/main/samples/javascript_nodejs/46.teams-auth)| [View](https://github.com/microsoft/BotBuilder-Samples/tree/main/samples/python/46.teams-auth) | 
|6| Teams File Upload | Exchanging files with a bot in a one-to-one conversation. | [View](https://github.com/microsoft/BotBuilder-Samples/tree/main/samples/csharp_dotnetcore/56.teams-file-upload) | [View](https://github.com/microsoft/BotBuilder-Samples/tree/main/samples/javascript_nodejs/56.teams-file-upload) | [View](https://github.com/microsoft/BotBuilder-Samples/tree/main/samples/python/56.teams-file-upload) |
|7| Task Module | Demonstrating how to retrieve a Task Module and values from cards in it, for a Messaging Extension. | [View](https://github.com/microsoft/BotBuilder-Samples/tree/main/samples/csharp_dotnetcore/54.teams-task-module) | [View](https://github.com/microsoft/BotBuilder-Samples/tree/main/samples/javascript_nodejs/54.teams-task-module) | [View](https://github.com/microsoft/BotBuilder-Samples/tree/main/samples/python/54.teams-task-module) |
|8| Start new thread in a channel | Demonstrating how to create a new thread in a channel. | [View](https://github.com/microsoft/BotBuilder-Samples/tree/main/samples/csharp_dotnetcore/58.teams-start-new-thread-in-channel) | [View](https://github.com/microsoft/BotBuilder-Samples/tree/main/samples/javascript_nodejs/58.teams-start-new-thread-in-channel) | [View](https://github.com/microsoft/BotBuilder-Samples/tree/main/samples/python/58.teams-start-thread-in-channel) |
|9| Universal bots  | Teams catering bot demonstrating how to use Universal bots in Teams | [View](samples/bot-teams-catering/csharp) |  | |
|10| Sequential workflow adaptive cards | Demonstrating on how to implement sequential flow, user specific view and upto date adaptive cards in bot. | [View][sequential#workflow#csharp] | [View][sequential#workflow#js]
|11| Channel messages with RSC permissions | Demonstrating on how a bot can receive all channel messages with RSC without @mention. | [View][messageswithrsc#csharp] |[View][messageswithrsc#js]
|12| Bot with SharePoint file to view in Teams file viewer | This sample demos a bot with capability to upload files to SharePoint site and same files can be viewed in Teams file viewer. |[View][botwithsharepointfileviewer#csharp]|[View][botfileviewer#js]
|13|  Type ahead search control on Adaptive Cards | This sample shows the feature of type ahead search (static and dynamic) control in Adaptive Cards. |[View][typeaheadsearch#csharp]|[View][typeaheadsearchonadaptivecard#js]
|14|  People picker control in Adaptive Cards | This sample shows the feature of people picker control in Adaptive Cards. |[View][peoplepickeronadaptivecard#csharp]|[View][peoplepickeronadaptivecard#js]
|15|  Proactive Messaging sample | This sample shows how to save user's conversation reference information to send proactive reminder message using Bots. This uses Teams toolkit for Visual Studio Code to provide scaffolding experience. ||[View][bot-proactive-msg-teamsfx#js]
|16|  Proactive Tab Conversations | Using a bot to create and store conversations that can be later used inside a sub-entity, tab conversation. This sample includes the details required to proactively message a channel from a bot, set-up and store details for channel tab conversations, and viewing channel conversations from a personal app. |[View][bot-tab-conversations#csharp]|
|17|  Suggested Actions Bot | This sample shows the feature where user can send suggested actions using bot. |[View][suggestedactionsbot#csharp]|[View][suggestedactionsbot#nodejs]
#### Additional samples

|    | Sample Name | Description | .NET Core | JavaScript | 
|:--:|:-------------------|:---------------------------------------------------------------------------------|:--------|:-------------|
|1|Proactive Messaging   | Sample to highlight solutions to two challenges with building proactive messaging apps in Microsoft Teams.                                      |[View][bot-proactive-msg#cs]        |
|2| Sharepoint List Bot| This sample app shows the interaction between teams bot and SharePoint List, Bot saves the specified details in SharePoint List as back-end| [View][bot-sharepoint-list#cs] |  |  |
|3|Teams Virtual Assistant| Customized virtual assistant template to support teams capabilities.        |[View][app-virtual-assistant#cs]|

## [Messaging Extensions samples](https://docs.microsoft.com/microsoftteams/platform/messaging-extensions/what-are-messaging-extensions) (using the v4 SDK)
>NOTE:
>Visit the [Bot Framework Samples repository][botframework] to view Microsoft Bot Framework v4 SDK task-focused samples for C#, JavaScript, TypeScript, and Python.

|    | Sample Name | Description | .NET Core | JavaScript | Python|
|:--:|:-------------------|:---------------------------------------------------------------------------------|:--------|:-------------|:-------------|
|1|Messaging extensions - search quick-start | Hello world Messaging Extension that accepts search requests and returns results. | | [View][msgext-search-quickstart#js] | |
|2|Messaging extensions - search | Messaging Extension that accepts search requests and returns results. | [View](https://github.com/microsoft/BotBuilder-Samples/tree/main/samples/csharp_dotnetcore/50.teams-messaging-extensions-search) | [View](https://github.com/microsoft/BotBuilder-Samples/tree/main/samples/javascript_nodejs/50.teams-messaging-extensions-search) | [View](https://github.com/microsoft/BotBuilder-Samples/tree/main/samples/python/50.teams-messaging-extension-search) |
|3|Messaging extensions - action quick-start | Hello world Messaging Extension that accepts parameters and returns a card. Also, how to receive a forwarded message as a parameter in a Messaging Extension. | | [View][msgext-action-quickstart#js] | |
|4|Messaging extensions - action | Messaging Extension that accepts parameters and returns a card. Also, how to receive a forwarded message as a parameter in a Messaging Extension. | [View](https://github.com/microsoft/BotBuilder-Samples/tree/main/samples/csharp_dotnetcore/51.teams-messaging-extensions-action) | [View](https://github.com/microsoft/BotBuilder-Samples/tree/main/samples/javascript_nodejs/51.teams-messaging-extensions-action) | [View](https://github.com/microsoft/BotBuilder-Samples/tree/main/samples/python/51.teams-messaging-extensions-action) |
|5|Messaging extensions - auth and config | Messaging Extension that has a configuration page, accepts search requests and returns results after the user has signed in. | [View](https://github.com/microsoft/BotBuilder-Samples/tree/main/samples/csharp_dotnetcore/52.teams-messaging-extensions-search-auth-config) | [View](https://github.com/microsoft/BotBuilder-Samples/tree/main/samples/javascript_nodejs/52.teams-messaging-extensions-search-auth-config) |
|6|Messaging extensions - auth and config | Messaging Extension that has a configuration page, accepts search requests and returns results with SSO. |     | [View][msgext-search-sso-config#js] |
|7|Messaging extensions - action preview | Demonstrates how to create a Preview and Edit flow for a Messaging Extension. | [View](https://github.com/microsoft/BotBuilder-Samples/tree/main/samples/csharp_dotnetcore/53.teams-messaging-extensions-action-preview) | [View](https://github.com/microsoft/BotBuilder-Samples/tree/main/samples/javascript_nodejs/53.teams-messaging-extensions-action-preview) | [View](https://github.com/microsoft/BotBuilder-Samples/tree/main/samples/python/53.teams-messaging-extensions-action-preview) |
|8|Link unfurling | Messaging Extension that performs link unfurling. | [View](https://github.com/microsoft/BotBuilder-Samples/tree/main/samples/csharp_dotnetcore/55.teams-link-unfurling) | [View](https://github.com/microsoft/BotBuilder-Samples/tree/main/samples/javascript_nodejs/55.teams-link-unfurling) | [View](https://github.com/microsoft/BotBuilder-Samples/tree/main/samples/python/55.teams-link-unfurling) |

#### Additional samples

|    | Sample Name | Description | .NET Core | JavaScript | Python |
|:--:|:-------------------|:---------------------------------------------------------------------------------|:--------|:-------------|:-------------|
|1|Link unfurling demo of Reddit        | Messaging Extension with Link Unfurling Samples for Reddit Links                              |[View][msgext-link-unfurl#cs]        |
|2|Link unfurling - setup a meeting     | This sample illustrates a common scenario where a user shares a link to a resource with a group of users, and they collaborate to review it in a meeting.                              |[View][msgext-link-unfurl-meeting#cs]        |

## [Webhooks and Connectors samples](https://docs.microsoft.com/microsoftteams/platform/webhooks-and-connectors/what-are-webhooks-and-connectors)

|    | Sample Name        | Description                                                                      | C#    | JavaScript   |
|:--:|:-------------------|:-------------------------------------------------------------------------------------------------|:--------|:-------------|
|1|Connectors             | Sample Office 365 Connector generating notifications to teams channel.                              |[View][connector#cs]       |[View][connector#ts]
|2|Generic connectors sample | Sample code for a generic connector that's easy to customize for any system which supports webhooks.   |    |[View][connector-generic#ts]
|3|Outgoing Webhooks      | Samples to create "Custom Bots" to be used in Microsoft Teams.                                        |[View][outgoing-webhook#cs]|[View][outgoing-webhook#ts]
|4|Authentication in todo Connector App      | This is a sample app which shows connector authentication and sends notification on task creation.                                        ||[View][auth-in-connector#nodejs] 
|5|Incoming Webhook      | This is a sample  used to send card using incoming webhook.                                        |[View][incoming-webhook#cs]       |[View][incoming-webhook#nodejs]

## [Graph APIs](https://docs.microsoft.com/graph/teams-concept-overview)

|    | Sample Name        | Description                                                                      | C#    | JavaScript   |
|:--:|:-------------------|:-------------------------------------------------------------------------------------------------|:--------|:-------------|
|1|Resource Specific Consent (RSC) | This sample illustrates how you can use [Resource Specific Consent (RSC)](https://docs.microsoft.com/en-us/microsoftteams/platform/graph-api/rsc/resource-specific-consent) to call Graph APIs. .                              |[View][graph#rsc#cs]       | [View][graph#rsc#js]
|2|Graph API Channel Life Cycle | This sample illustrates how you can use [Graph API Channel Life Cycle](https://docs.microsoft.com/en-us/graph/api/resources/channel?view=graph-rest-beta) to call Graph APIs. .                              |[View][graph#lifecycle#cs]       | [View][graph#lifecycle#js]
|3|Graph API Teams App Installation Life Cycle | This sample illustrates how you can use [Teams App Installation Life Cycle](https://docs.microsoft.com/en-us/graph/api/resources/teamsappinstallation?view=graph-rest-1.0) by calling Microsoft Graph APIs. .                              |[View][graph#instllationlifecycle#cs]       |[View][graph#instllationlifecycle#js] |
|4|Graph API Teams App Catalog Life Cycle | This sample illustrates how you programmatically manage lifecycle for your teams App in catalog by calling Microsoft Graph APIs. .                               |[View][graph#appctaloglifecycle#cs]       | [View][graph#appcataloglifecycle#js]
|5|Graph API Chat Life Cycle | This sample illustrates how you can use [Teams App Chat Life Cycle](https://docs.microsoft.com/en-us/graph/api/resources/chat?view=graph-rest-1.0) by calling Microsoft Graph APIs. .                              |[View][graph#chatlifecyle#cs]      |[View][graph#chatlifecycle#js]
|6|Activity Feed Notification  | Microsoft Teams sample app for Sending Activity feed notification using Graph API in a Teams Tab. | [View][graph-activity-feed#cs]                       | [View][graph-activity-feed#js]| 
|7|Proactive installation of App and sending proactive notifications | This sample illustrates how you can use [Proactive installation of app for user and send proactive notification](https://docs.microsoft.com/en-us/microsoftteams/platform/graph-api/proactive-bots-and-messages/graph-proactive-bots-and-messages?tabs=csharp) by calling Microsoft Graph APIs. .                              |[View][graph#graphproactiveinstallation#cs]       |[View][graph#graphproactiveinstallation#js]
|8|Change Notification  | This sample app demonstrates sending change notifications to user presence in Teams based on user presence status. | [View][graph-change-notification#cs]                        |[View][graph-change-notification#js]
|9|Graph Pinned Message  | This is a sample application which demonstrates how to pin messages in chat using Graph api. | [View][graph-pinned-message#cs]|[View][graph-pinned-message#js]|
|10|Graph Bulk Meetings  | This is an sample application which shows how to create Teams meetings in bulk using Graph api. | [View][graph-bulk-meetings#cs]|[View][graph-bulk-meetings#js]|
|11|Graph Meeting Notification  | This is a sample application which demonstrates the use of online meeting subscription and sends you the notifications in chat using bot. | [View][graph-meeting-notification#cs]|[View][graph-meeting-notification#js]|



## [Calls and online meetings bots](https://docs.microsoft.com/en-us/microsoftteams/platform/bots/calls-and-meetings/calls-meetings-bots-overview)

|    | Sample Name       | Description                                                                      | C#    |
|:--:|:------------------|:---------------------------------------------------------------------------------------------------|:--------|
|1|Calling and Meeting bot          | This sample app demonstrate how a Bot can create a call, join a meeting and transfer the call |[View][bot-calling-meeting#csharp]     |
|2|Local Media Samples        |Local media samples give the developer direct access to the inbound and outbound media streams.                      |[View](https://github.com/microsoftgraph/microsoft-graph-comms-samples/tree/master/Samples/V1.0Samples/LocalMediaSamples) | 
|3|Remote Media Samples      | The Incident Bot sample is a Remote Media sample demonstrating a simple incident process workflow started by a Calling Bot..                                      |[View](https://github.com/microsoftgraph/microsoft-graph-comms-samples/tree/master/Samples/V1.0Samples/RemoteMediaSamples)       |


## Scenario specific samples

|    | Sample Name       | Description                                                                      | C#    | JavaScript   |
|:--:|:------------------|:---------------------------------------------------------------------------------------------------|:--------|:-------------|
|1|Task Modules          | Sample app showing off the Teams Task Module, a way to invoke custom code from a bot, a tab, or both! |[View][app-task-module#cs]     |[View][app-task-module#ts]
|2|Authentication        | Sample illustrating seamless inline authentication for Microsoft Teams apps.                      | | [View][app-auth#ts]
|3|Complete Samples      | Sample covering multiple scenarios - dialogs, ME, and facebook auth.                                      |[View][app-complete#cs]        |[View][app-complete#ts]
|4|Meetings Extensibility | Microsoft Teams meeting extensibility sample: token passing |[View][meetings-token-app#cs]     |[View][meetings-token-app#js]
|5|Meetings Content Bubble Bot | Microsoft Teams meeting extensibility sample for iteracting with Content Bubble Bot in-meeting |[View][meetings-content-bubble#cs]    |[View][meetings-content-bubble#js]
|6|Meetings SidePanel | Microsoft Teams meeting extensibility sample for iteracting with Side Panel in-meeting |[View][meetings-sidepanel#cs]     | [View][meetings-sidepanel#js]
|7|Region Selection App | This app contains a bot and Tab which is helpful to set the region |[View][region-selection-app#cs]     |
|8|App Localization | Microsoft Teams app localization using Bot and Tab |[View][app-localization#cs]  | [View][app-localization#js]
|9|Details Tab in Meetings | Microsoft Teams meeting extensibility sample for iteracting with Details Tab in-meeting |[View][meetings-details-tab#cs]  |[View][meetings-details-tab#js]
|10|App SSO | Microsoft Teams app SSO for Tab, Bot, ME - search, action, linkunfurl |[View][app-sso#cs] |[View][app-sso#js] 
|11|Meetings Stage View | Enable and configure your apps for Teams meetings to use in stage view |[View][meetings-stage-view#cs] |[View][meetings-stage-view#js]|
|12|Meeting Events | Get real time meeting events  |[View][meetings-events#cs] | [View][meetings-events#js] |
|13|Meeting Recruitment App | Sample app showing meeting app experience for interview scenario.|[View][meeting-recruitment-app#cs] |[View][meeting-recruitment-app#js]|
|13|Meeting Transcript App | This is a sample application which demonstrates how to get Transcript using Graph API and show it in the task module.|[View][meetings-transcription-app#cs] |[View][meetings-transcription-app#nodejs]|
|14| App Installtion  using QR code |This sample demos app installation using QR code of application's app id | [View][qrappinstallation#csharp] | [View][qrappinstallation#nodejs]
|15| Archive groupchat messages | Demonstrating on how a bot can archive groupchat messages and send it to user as a file. | [View][fetchgroupchatmessages#csharp] |[View][fetchgroupchatmessages#nodejs]|
|16| App check in location | Demonstrating feature where user can checkin with current location and view all previous checkins. | [View][appcheckinlocation#csharp] |[View][checkinlocation#nodejs]|
|17| Message reminder setup through messaging extension | Demonstrating a feature where user can schedule a task from messaging extension action command and get a reminder card at a scheduled time |[View][memessagereminder#csharp]|[View][msgext-message-reminder#nodejs]|
|18| Bot daily task reminder | This sample demos a feature where user can schedule a recurring task and get a reminder on the scheduled time |[View][botdailytaskreminder#csharp] |[View][bottaskreminder#nodejs]|
|19| Tab request approval | Demonstrating a feature where user can raise the requests and manager will be notified about the requests and manager will be redirected to approve/reject the request from received notification. |[View][tab-request-approval#csharp]|[View][tab-request-approval#nodejs]|
|20| Bot request approval | Demonstrating a feature where user can send task request to his manager using universal adaptive card and manager can approve/reject the request. |[View][bot-request-approval#csharp]|[View][bot-request-approval#nodejs]|
|21| Join the Team using QR code |This sample demos a feature where user can join a team using QR code containing the team's id. |[View][qrjointeam#csharp]|[View][qrjointeam#nodejs]
|22| Activity feed broadcast |Demonstrating a feature to notify any message to all members of the organisation using activity feed notification .|[View][graph-activity-feed-broadcast#csharp]|[View][graph-activity-feed-broadcast#js]|
|23|App complete auth|This sample demos authentication feature in bot,tab and messaging extension.|[View][app-complete-auth#cs]|
|24|App identity linking with sso|This sample demos mapping with aad id, facebook, and google account of user in bot, ME and tab.||[View][app-identity-link-with-sso#nodejs]|
|25|Meeting signing programmatic share to stage|Demonstrating the programmatic share to stage feature, by means of a document signing in a meeting.|[View][meetings-share-to-stage-signing#csharp]||
|26|Live coding interview using Shared meeting stage |This sample demos a live coding in a teams meeting stage.|[View][meetings-live-code-interview#csharp]|[View][meetings-live-code-interview#nodejs]|
|27|Release Management     | This is a sample used to send workitem notification using Azure webhook.                                        |[View][release-management#cs]       |[View][release-management#nodejs]
|28|Meeting Live Caption     | This is a sample meeting side panel application which demonstrates how to enable live caption in the meeting and using the CART link how to send caption in live meeting.                                       |[View][meetings-live-caption#cs]       |[View][meetings-live-caption#nodejs]

## Application templates

|    | App Name       | Description                                                                      | Code   |
|:--:|:------------------|:---------------------------------------------------------------------------------------------------|:--------|
|1|QBot          | QBot is a solution designed for classroom teaching scenarios which allows teachers, tutors, and students to intelligently answer each other's questions within the Microsoft Teams collaboration platform. |[View][msteams-app-qbot]

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

[tab-conversation#nodejs]:samples/tab-conversations/nodejs
[tab-adaptive-cards#csharp]:samples/tab-adaptive-cards/csharp
[tab-adaptive-cards#js]:samples/tab-adaptive-cards/nodejs
[tab-stage-view#js]:samples/tab-stage-view/nodejs
[tab-stage-view#csharp]:samples/tab-stage-view/csharp
[tab-product-inspection#csharp]:samples/tab-product-inspection/csharp
[tab-staggered-permission#csharp]:samples/tab-staggered-permission/csharp
[tab-people-picker#csharp]:samples/tab-people-picker/csharp
[tab-channel-context#nodejs]:samples/tab-channel-context/nodejs
[tab-meeting-context#nodejs]:samples/tab-meeting-context/nodejs
[tab-app-monetization#nodejs]:samples/tab-app-monetization/nodejs
[tab-request-approval#nodejs]:samples/tab-request-approval/nodejs
[bot-request-approval#nodejs]:samples/bot-request-approval/nodejs

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

[app-task-module#cs]:samples/app-task-module/csharp
[app-task-module#ts]:samples/app-task-module/nodejs

[app-complete#cs]:samples/app-complete-sample/csharp
[app-complete#ts]:samples/app-complete-sample/nodejs

[outgoing-webhook#cs]:samples/outgoing-webhook/csharp
[outgoing-webhook#ts]:samples/outgoing-webhook/nodejs

[msgext-link-unfurl#cs]:samples/msgext-link-unfurling-reddit/csharp
[msgext-link-unfurl-meeting#cs]:samples/msgext-link-unfurling-meeting/csharp
[msgext-action-quickstart#js]:samples/msgext-action-quickstart/js
[msgext-search-quickstart#js]:samples/msgext-search-quickstart/js
[msgext-search-sso-config#js]:samples/msgext-search-sso-config

[tab-sso#ts]:samples/tabs-sso/nodejs
[tab-sso#cs]:samples/tab-sso/csharp

[app-virtual-assistant#cs]:samples/app-virtual-assistant/csharp
[identity-linking-with-sso#cs]:samples/app-identity-linking-with-sso/csharp
[bot-proactive-msg#cs]:samples/bot-proactive-messaging/csharp
[bot-proactive-msg-teamsfx#js]:samples/bot-proactive-messaging-teamsfx
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
[region-selection-app#cs]: samples/app-region-selection/csharp  
[meetings-content-bubble#cs]:samples/meetings-content-bubble/csharp
[meetings-sidepanel#cs]:samples/meetings-sidepanel/csharp
[meetings-sidepanel#js]:samples/meetings-sidepanel/nodejs
[meetings-content-bubble#js]:samples/meetings-content-bubble/nodejs
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
[bot-request-approval#csharp]:samples/bot-request-approval/csharp
[typeaheadsearchonadaptivecard#js]:samples/bot-type-ahead-search-adaptive-cards/nodejs
[meetings-live-code-interview#csharp]:samples/meetings-live-code-interview/csharp
[meetings-live-code-interview#nodejs]:samples/meetings-live-code-interview/nodejs
[suggestedactionsbot#csharp]:samples/bot-suggested-actions/csharp
[suggestedactionsbot#nodejs]:samples/bot-suggested-actions/nodejs

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
[app-identity-link-with-sso#nodejs]:samples/app-identity-link-with-sso/nodejs
[tab-staggered-permission#nodejs]:samples/tab-staggered-permission/nodejs
[botframework]:https://github.com/microsoft/BotBuilder-Samples#teams-samples
[tab-people-picker#nodejs]:samples/tab-people-picker/nodejs
[meetings-live-caption#cs]:samples/meetings-live-caption/csharp
[meetings-live-caption#nodejs]:samples/meetings-live-caption/nodejs

[msteams-app-qbot]:samples/msteams-application-qbot/

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
