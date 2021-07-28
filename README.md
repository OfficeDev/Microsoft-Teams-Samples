# Microsoft Teams Samples
##### [Click here to find out what's new with Microsoft Teams Platform](https://docs.microsoft.com/microsoftteams/platform/whats-new)

>NOTE: These samples are designed to help understand Microsoft Teams platform capabilities and scenarios. If you are looking for production ready apps then please visit [App Templates for Microsoft Teams](https://docs.microsoft.com/microsoftteams/platform/samples/app-templates)


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
|4|Channel and group tabs   | Sample app showing custom group and channel Tab with ASP. NET Core                                    |[MVC][group-channel-tab#cs#mvc], [Razor][group-channel-tab#cs#razor]     | [Yeoman Generator](https://docs.microsoft.com/microsoftteams/platform/tabs/quickstarts/create-channel-group-tab-node-yeoman#generate-your-project) |
|5|Channel and group tab quick-start| Sample channel and group tab hello world app.                          |                               |[View][group-tab-quickstart#ts]|[View][group-tab-quickstart#js]
|6|Channel and group tab with SSO quick-start| Sample channel and group tab with SSO hello world app.        |[View][group-tab-sso-quickstart#csharp]|[View][group-tab-sso-quickstart#ts]|[View][group-tab-sso-quickstart#js]
|7|SPFx Tab | Sample app showing Microsoft Teams tabs using SharePoint Framework                                    |   | [View][group-channel-tab#ts#spfx] |
|8|Tab SSO               | Microsoft Teams sample app for tabs Azure AD SSO                                      | | [View][tab-sso#ts] ,[Teams Toolkit](https://docs.microsoft.com/microsoftteams/platform/toolkit/visual-studio-code-tab-sso)
|9|Config Tab Authentication               | Microsoft Teams sample app for config tabs Azure AD authentication | [View]()                       | | 
|10|Deep Link consuming Subentity ID      | Microsoft Teams sample app for demonstrating deeplink from Bot chat to Tab consuming Subentity ID | [View][tab-deeplink#csharp]                       | | [View][tab-deeplink#nodejs]|
|11|Integrate graph toolkit component in teams tab      | Microsoft Teams tab sample app for demonstrating graph toolkit component |                      | [View][tab-graph-toolkit#js]|
|12|Device permissions      | Microsoft Teams tab sample app for demonstrating device permissions |                      | [View][tab-device-permissions#js]|
|13|Show adaptive cards in Teams tab | Microsoft Teams tab sample code which demonstrates how to [show adaptive cards in Teams tab](https://docs.microsoft.com/microsoftteams/platform/tabs/how-to/build-adaptive-card-tabs) | [View][tab-adaptive-cards#csharp]               |                | [View][tab-adaptive-cards#js]|

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


## [Webhooks and Connectors samples](https://docs.microsoft.com/microsoftteams/platform/webhooks-and-connectors/what-are-webhooks-and-connectors)

|    | Sample Name        | Description                                                                      | C#    | TypeScript   |
|:--:|:-------------------|:-------------------------------------------------------------------------------------------------|:--------|:-------------|
|1|Connectors             | Sample Office 365 Connector generating notifications to teams channel.                              |[View][connector#cs]       |[View][connector#ts]
|2|Generic connectors sample | Sample code for a generic connector that's easy to customize for any system which supports webhooks.   |    |[View][connector-generic#ts]
|3|Outgoing Webhooks      | Samples to create "Custom Bots" to be used in Microsoft Teams.                                        |[View][outgoing-webhook#cs]|[View][outgoing-webhook#ts]

## [Graph APIs](https://docs.microsoft.com/graph/teams-concept-overview)

|    | Sample Name        | Description                                                                      | C#    | JavaScript   |
|:--:|:-------------------|:-------------------------------------------------------------------------------------------------|:--------|:-------------|
|1|Resource Specific Consent (RSC) | This sample illustrates how you can use [Resource Specific Consent (RSC)](https://docs.microsoft.com/en-us/microsoftteams/platform/graph-api/rsc/resource-specific-consent) to call Graph APIs. .                              |[View][graph#rsc#cs]       | [View][graph#rsc#js]
|2|Graph API Channel Life Cycle | This sample illustrates how you can use [Graph API Channel Life Cycle](https://docs.microsoft.com/en-us/graph/api/resources/channel?view=graph-rest-beta) to call Graph APIs. .                              |[View][graph#lifecycle#cs]       | [View][graph#lifecycle#js]
|3|Graph API Teams App Installation Life Cycle | This sample illustrates how you can use [Teams App Installation Life Cycle](https://docs.microsoft.com/en-us/graph/api/resources/teamsappinstallation?view=graph-rest-1.0) by calling Microsoft Graph APIs. .                              |[View][graph#instllationlifecycle#cs]       |[View][graph#instllationlifecycle#js] |
|4|Graph API Teams App Catalog Life Cycle | This sample illustrates how you you programattically have an lifecycle for your teams App in catalog by calling Microsoft Graph APIs. .                               |[View][graph#appctaloglifecycle#cs]       | [View][graph#appcataloglifecycle#js]
|5|Graph API Chat Life Cycle | This sample illustrates how you can use [Teams App Chat Life Cycle](https://docs.microsoft.com/en-us/graph/api/resources/chat?view=graph-rest-1.0) by calling Microsoft Graph APIs. .                              |[View][graph#chatlifecyle#cs]      |[View][graph#chatlifecycle#js]
|6|Activity Feed Notification  | Microsoft Teams sample app for Sending Activity feed notification using Graph API in a Teams Tab. | [View][graph-activity-feed#cs]                       | | 
|7|Proactive installation of App and sending proactive notifications | This sample illustrates how you can use [Proactive installation of app for user and send proactive notification](https://docs.microsoft.com/en-us/microsoftteams/platform/graph-api/proactive-bots-and-messages/graph-proactive-bots-and-messages?tabs=csharp) by calling Microsoft Graph APIs. .                              |[View][graph#graphproactiveinstallation#cs]       |[View][graph#graphproactiveinstallation#js]
|8|Change Notification  | This sample app demonstrates sending change notifications to user presence in Teams based on user presence status. | [View][graph-change-notification#cs]                        |[View][graph-change-notification#js] 



## [Calls and online meetings bots](https://docs.microsoft.com/en-us/microsoftteams/platform/bots/calls-and-meetings/calls-meetings-bots-overview)

|    | Sample Name       | Description                                                                      | C#    |
|:--:|:------------------|:---------------------------------------------------------------------------------------------------|:--------|
|1|Calling and Meeting bot          | This sample app demonstarte how an Bot can create a call, join a meeting and transfer the call |[View][bot-calling-meeting#csharp]     |
|2|Local Media Samples        |Local media samples give the developer direct access to the inbound and outbound media streams.                      |[View](https://github.com/microsoftgraph/microsoft-graph-comms-samples/tree/master/Samples/V1.0Samples/LocalMediaSamples) | 
|3|Remote Media Samples      | The Incident Bot sample is a Remote Media sample demonstrating a simple incident process workflow started by a Calling Bot..                                      |[View](https://github.com/microsoftgraph/microsoft-graph-comms-samples/tree/master/Samples/V1.0Samples/RemoteMediaSamples)       |


## Scenario specific samples

|    | Sample Name       | Description                                                                      | C#    | JavaScript   |
|:--:|:------------------|:---------------------------------------------------------------------------------------------------|:--------|:-------------|
|1|Task Modules          | Sample app showing off the Teams Task Module, a way to invoke custom code from a bot, a tab, or both! |[View][app-task-module#cs]     |[View][app-task-module#ts]
|2|Authentication        | Sample illustrating seamless inline authentication for Microsoft Teams apps.                      | | [View][app-auth#ts]
|3|Complete Samples      | A template for building complex bots (SDK V3) for Microsoft Teams.                                      |[View][app-complete#cs]        |[View][app-complete#ts]
|4|Meetings Extensibility | Microsoft Teams meeting extensibility sample: token passing |[View][meetings-token-app#cs]     |[View][meetings-token-app#js]
|5|Meeting Content Bubble Bot | Microsoft Teams meeting extensibility sample for iteracting with Content Bubble Bot in-meeting |[View][meetings-content-bubble#cs]    |[View][meetings-content-bubble#js]
|6|Meeting SidePanel | Microsoft Teams meeting extensibility sample for iteracting with Side Panel in-meeting |[View][meetings-sidepanel#cs]     | [View][meetings-sidepanel#js]
|7|Region Selection App | This app contains a bot and Tab which is helpful to set the region |[View][region-selection-app#cs]     |
|8|App Localization | Microsoft Teams app localization using Bot and Tab |[View][app-localization#cs]  | [View][app-localization#js]
|9|Details Tab in Meeting | Microsoft Teams meeting extensibility sample for iteracting with Details Tab in-meeting |[View][meetings-details-tab#cs]  |[View][meetings-details-tab#js]
|10|App SSO | Microsoft Teams app SSO for Tab, Bot, ME - search, action, linkunfurl |[View][app-sso#cs] |[View][app-sso#js] 

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
[tab-device-permissions#js]:samples/tab-device-permissions/nodejs

[tab-adaptive-cards#csharp]:samples/tab-adaptive-cards/csharp
[tab-adaptive-cards#js]:samples/tab-adaptive-cards/nodejs

[group-channel-tab#cs#razor]:samples/tab-channel-group/razor-csharp
[group-channel-tab#cs#mvc]:samples/tab-channel-group/mvc-csharp
[group-channel-tab#ts#spfx]:samples/tab-channel-group/spfx

[connector#cs]:samples/connector-todo-notification/csharp
[connector#ts]:samples/connector-github-notification/nodejs
[connector-generic#ts]:samples/connector-generic/nodejs
[sequential#workflow#csharp]:samples/bot-sequential-flow-adaptive-cards/csharp
[sequential#workflow#js]:samples/bot-sequential-flow-adaptive-cards/nodejs
[app-auth#ts]:samples/app-auth/nodejs

[app-task-module#cs]:samples/app-task-module/csharp
[app-task-module#ts]:samples/app-task-module/nodejs

[app-complete#cs]:samples/app-complete-sample/csharp
[app-complete#ts]:samples/app-complete-sample/nodejs

[outgoing-webhook#cs]:samples/outgoing-webhook/csharp
[outgoing-webhook#ts]:samples/outgoing-webhook/nodejs

[msgext-link-unfurl#cs]:samples/msgext-link-unfurling-reddit/csharp
[msgext-action-quickstart#js]:samples/msgext-action-quickstart/js
[msgext-search-quickstart#js]:samples/msgext-search-quickstart/js
[msgext-search-sso-config#js]:samples/msgext-search-sso-config

[tab-sso#ts]:samples/tabs-sso/nodejs
[tab-sso#cs]:samples/tab-sso/csharp

[app-virtual-assistant#cs]:samples/app-virtual-assistant/csharp
[bot-proactive-msg#cs]:samples/bot-proactive-messaging/csharp
[bot-conversation-quickstart#js]:samples/bot-conversation-quickstart/js
[bot-conversation-sso-quickstart#js]:samples/bot-conversation-sso-quickstart/js
[bot-sharepoint-list#cs]:samples/bot-sharepoint-list/csharp
[bot-conversation-sso-quickstart#csharp_dotnetcore]:samples/bot-conversation-sso-quickstart/csharp_dotnetcore
[bot-calling-meeting#csharp]:samples/bot-calling-meeting/csharp
[meetings-token-app#cs]:samples/meetings-token-app/csharp
[apps-in-meeting#cs]:samples/apps-in-meeting/csharp
[meetings-token-app#js]:samples/meetings-token-app/nodejs
[region-selection-app#cs]: samples/app-region-selection/csharp  
[meetings-content-bubble#cs]:samples/meetings-content-bubble/csharp
[meetings-sidepanel#cs]:samples/meetings-sidepanel/csharp
[meetings-sidepanel#js]:samples/meetings-sidepanel/nodejs
[meetings-content-bubble#js]:samples/meetings-content-bubble/nodejs

[app-localization#cs]:samples/app-localization/csharp
[app-localization#js]:samples/app-localization/nodejs
[meetings-details-tab#cs]:samples/meetings-details-tab/csharp
[meetings-details-tab#js]:samples/meetings-details-tab/nodejs
[app-sso#cs]:samples/app-sso/csharp
[app-sso#js]:samples/app-sso/nodejs

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
[graph-change-notification#cs]:samples/graph-change-notification/csharp
[graph-change-notification#js]:samples/graph-change-notification/nodejs

[botframework]:https://github.com/microsoft/BotBuilder-Samples#teams-samples

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

