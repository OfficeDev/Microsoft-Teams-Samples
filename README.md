# Microsoft Teams Samples
##### [Click here to find out what's new with Microsoft Teams Platform](https://docs.microsoft.com/microsoftteams/platform/whats-new)


## [Getting Started - Samples and Tutorials](https://docs.microsoft.com/microsoftteams/platform/tutorials/get-started-dotnet-app-studio)

|    | Sample Name        | Description                                                                      | C#    | TypeScript   |
|:--:|:-------------------|:---------------------------------------------------------------------------------|:--------|:-------------|
|1|Hello World            | Microsoft Teams hello world sample app.          |[View][app-hello-world#cs]     |[View][app-hello-world#ts]


## [Tabs samples](https://docs.microsoft.com/microsoftteams/platform/tabs/what-are-tabs)
|    | Sample Name        | Description                                                                      | C#    | TypeScript   |
|:--:|:-------------------|:---------------------------------------------------------------------------------|:--------|:-------------|
|1|Personal tabs            | Sample app showing custom personal Tab with ASP. NET Core                      |[MVC][personal-tab#cs#mvc], [Razor][personal-tab#cs#razor]     | [Yeoman Generator](https://docs.microsoft.com/microsoftteams/platform/tabs/quickstarts/create-personal-tab-node-yeoman#generate-your-project) |
|2|Channel and group tabs   | Basic example tab apps for Microsoft Teams                                    |[MVC][group-channel-tab#cs#mvc], [Razor][group-channel-tab#cs#razor]     | [Yeoman Generator](https://docs.microsoft.com/microsoftteams/platform/tabs/quickstarts/create-channel-group-tab-node-yeoman#generate-your-project) |
|3|Tab SSO               | Microsoft Teams sample app for tabs Azure AD SSO                                      | | [View][tab-sso#ts] ,[Teams Toolkit](https://docs.microsoft.com/microsoftteams/platform/toolkit/visual-studio-code-tab-sso)

## [Apps in Meetings samples](https://docs.microsoft.com/microsoftteams/platform/apps-in-teams-meetings/teams-apps-in-meetings)

|    | Sample Name        | Description                                                                      | C#    | TypeScript   |
|:--:|:-------------------|:---------------------------------------------------------------------------------|:--------|:-------------|
|1|Apps in Meetings       | Microsoft Teams meeting extensibility sample: token passing |[View][mtgext-token-app#cs]     |

## [Teams bot samples](https://docs.microsoft.com/microsoftteams/platform/bots/what-are-bots) (using the v4 SDK)
>[!NOTE]
>Visit the [Bot Framework Samples repository][botframework] to view Microsoft Bot Framework v4 SDK task-focused samples for C#, JavaScript, TypeScript, and Python.


|    | Sample Name | Description | .NET Core | JavaScript | Python |
|:--:|:-------------------|:---------------------------------------------------------------------------------|:--------|:-------------|:-------------|
|1| Teams Conversation Bot | Messaging and conversation event handling. | [View](https://github.com/microsoft/BotBuilder-Samples/tree/master/samples/csharp_dotnetcore/57.teams-conversation-bot)| [View](https://github.com/microsoft/BotBuilder-Samples/tree/master/samples/javascript_nodejs/57.teams-conversation-bot)| [View](https://github.com/microsoft/BotBuilder-Samples/tree/master/samples/python/57.teams-conversation-bot) | 
|2| Message Reactions | Demonstrates how to create a simple bot that responds to Message Reactions | [View](https://github.com/microsoft/BotBuilder-Samples/tree/main/samples/csharp_dotnetcore/25.message-reaction)  |  [View](https://github.com/microsoft/BotBuilder-Samples/tree/main/samples/javascript_nodejs/25.message-reaction) |
|3| Authentication with OAuthPrompt| Authentication and basic messaging in Bot Framework v4. | [View](https://github.com/microsoft/BotBuilder-Samples/tree/master/samples/csharp_dotnetcore/46.teams-auth)| [View](https://github.com/microsoft/BotBuilder-Samples/tree/master/samples/javascript_nodejs/46.teams-auth)| [View](https://github.com/microsoft/BotBuilder-Samples/tree/master/samples/python/46.teams-auth) | 
|4| Teams File Upload | Exchanging files with a bot in a one-to-one conversation. | [View](https://github.com/microsoft/BotBuilder-Samples/tree/master/samples/csharp_dotnetcore/56.teams-file-upload) | [View](https://github.com/microsoft/BotBuilder-Samples/tree/master/samples/javascript_nodejs/56.teams-file-upload) | [View](https://github.com/microsoft/BotBuilder-Samples/tree/master/samples/python/56.teams-file-upload) |
|5| Task Module | Demonstrating how to retrieve a Task Module and values from cards in it, for a Messaging Extension. | [View](https://github.com/microsoft/BotBuilder-Samples/tree/main/samples/csharp_dotnetcore/54.teams-task-module) | [View](https://github.com/microsoft/BotBuilder-Samples/tree/main/samples/javascript_nodejs/54.teams-task-module) | [View](https://github.com/microsoft/BotBuilder-Samples/tree/main/samples/python/54.teams-task-module) |
|6| Start new thread in a channel | Demonstrating how to create a new thread in a channel. | [View](https://github.com/microsoft/BotBuilder-Samples/tree/main/samples/csharp_dotnetcore/58.teams-start-new-thread-in-channel) | [View](https://github.com/microsoft/BotBuilder-Samples/tree/main/samples/javascript_nodejs/58.teams-start-new-thread-in-channel) | [View](https://github.com/microsoft/BotBuilder-Samples/tree/main/samples/python/58.teams-start-thread-in-channel) |

#### Additional samples


|    | Sample Name | Description | .NET Core | JavaScript | Python |
|:--:|:-------------------|:---------------------------------------------------------------------------------|:--------|:-------------|:-------------|
|1|Proactive Messaging   | Sample to highlight solutions to two challenges with building proactive messaging apps in Microsoft Teams.                                      |[View][bot-proactive-msg#cs]        |

## [Messaging Extensions samples](https://docs.microsoft.com/microsoftteams/platform/messaging-extensions/what-are-messaging-extensions) (using the v4 SDK)
>[!NOTE]
>Visit the [Bot Framework Samples repository][botframework] to view Microsoft Bot Framework v4 SDK task-focused samples for C#, JavaScript, TypeScript, and Python.


|    | Sample Name | Description | .NET Core | JavaScript | Python|
|:--:|:-------------------|:---------------------------------------------------------------------------------|:--------|:-------------|:-------------|
|1|Messaging extensions - search | Messaging Extension that accepts search requests and returns results. | [View](https://github.com/microsoft/BotBuilder-Samples/tree/main/samples/csharp_dotnetcore/50.teams-messaging-extensions-search) | [View](https://github.com/microsoft/BotBuilder-Samples/tree/main/samples/javascript_nodejs/50.teams-messaging-extensions-search) | [View](https://github.com/microsoft/BotBuilder-Samples/tree/main/samples/python/50.teams-messaging-extension-search) |
|2|Messaging extensions - action | Messaging Extension that accepts parameters and returns a card. Also, how to receive a forwarded message as a parameter in a Messaging Extension. | [View](https://github.com/microsoft/BotBuilder-Samples/tree/main/samples/csharp_dotnetcore/51.teams-messaging-extensions-action) | [View](https://github.com/microsoft/BotBuilder-Samples/tree/main/samples/javascript_nodejs/51.teams-messaging-extensions-action) | [View](https://github.com/microsoft/BotBuilder-Samples/tree/main/samples/python/51.teams-messaging-extensions-action) |
|3|Messaging extensions - auth and config | Messaging Extension that has a configuration page, accepts search requests and returns results after the user has signed in. | [View](https://github.com/microsoft/BotBuilder-Samples/tree/main/samples/csharp_dotnetcore/52.teams-messaging-extensions-search-auth-config) | [View](https://github.com/microsoft/BotBuilder-Samples/tree/main/samples/javascript_nodejs/52.teams-messaging-extensions-search-auth-config) |
|4|Messaging extensions - action preview | Demonstrates how to create a Preview and Edit flow for a Messaging Extension. | [View](https://github.com/microsoft/BotBuilder-Samples/tree/main/samples/csharp_dotnetcore/53.teams-messaging-extensions-action-preview) | [View](https://github.com/microsoft/BotBuilder-Samples/tree/main/samples/javascript_nodejs/53.teams-messaging-extensions-action-preview) | [View](https://github.com/microsoft/BotBuilder-Samples/tree/main/samples/python/53.teams-messaging-extensions-action-preview) |
|5|Link unfurling | Messaging Extension that performs link unfurling. | [View](https://github.com/microsoft/BotBuilder-Samples/tree/main/samples/csharp_dotnetcore/55.teams-link-unfurling) | [View](https://github.com/microsoft/BotBuilder-Samples/tree/main/samples/javascript_nodejs/55.teams-link-unfurling) | [View](https://github.com/microsoft/BotBuilder-Samples/tree/main/samples/python/55.teams-link-unfurling) |

#### Additional samples

|    | Sample Name | Description | .NET Core | JavaScript | Python |
|:--:|:-------------------|:---------------------------------------------------------------------------------|:--------|:-------------|:-------------|
|1|Link unfurling demo of Reddit        | Sample of Link Unfurling in Teams.                                      |[View][msgext-link-unfurl#cs]        |


## [Webhooks and Connectors samples](https://docs.microsoft.com/microsoftteams/platform/webhooks-and-connectors/what-are-webhooks-and-connectors)


|    | Sample Name        | Description                                                                      | C#    | TypeScript   |
|:--:|:-------------------|:---------------------------------------------------------------------------------|:--------|:-------------|
|1|Connectors             | Sample Office 365 Connector generating notifications to teams channel.                              |[View][connector#cs]       |[View][connector#ts]
|2|Outgoing Webhooks      | Samples to create "Custom Bots" to be used in Microsoft Teams.                                        |[View][outgoing-webhook#cs]|[View][outgoing-webhook#ts]


## Samples with multiple entry points

|    | Sample Name       | Description                                                                      | C#    | TypeScript   |
|:--:|:------------------|:---------------------------------------------------------------------------------|:--------|:-------------|
|1|Task Modules          | Sample app showing off the Teams Task Module, a way to invoke custom code from a bot, a tab, or both! |[View][app-task-module#cs]     |[View][app-task-module#ts]
|2|Authentication        | Sample illustrating seamless inline authentication for Microsoft Teams apps.                      | | [View][app-auth#ts]
|3|Complete Samples      | A template for building complex bots (SDK V3) for Microsoft Teams.                                      |[View][app-complete#cs]        |[View][app-complete#ts]


[app-hello-world#cs]:csharp/app-hello-world
[app-hello-world#ts]:nodejs/app-hello-world


[personal-tab#cs#razor]:csharp/tab-personal-razor
[personal-tab#cs#mvc]:csharp/tab-personal-mvc

[group-channel-tab#cs#razor]:csharp/tab-channel-group-razor
[group-channel-tab#cs#mvc]:csharp/tab-channel-group-mvc


[connector#cs]:csharp/connector-todo-notification
[connector#ts]:nodejs/connector-github-notification

[app-auth#ts]:nodejs/app-auth

[app-task-module#cs]:csharp/app-task-module
[app-task-module#ts]:nodejs/app-task-module

[app-complete#cs]:csharp/app-complete-sample
[app-complete#ts]:nodejs/app-complete-sample

[outgoing-webhook#cs]:csharp/outgoing-webhook
[outgoing-webhook#ts]:nodejs/outgoing-webhook

[msgext-link-unfurl#cs]:csharp/msgext-link-unfurling-reddit

[tab-sso#ts]:nodejs/tab-sso

[bot-proactive-msg#cs]:csharp/bot-proactive-messaging

[mtgext-token-app#cs]:csharp/mtgext-token-app

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

