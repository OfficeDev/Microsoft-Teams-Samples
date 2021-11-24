---
page_type: sample
products:
- office-teams
- office
- office-365
languages:
- typescript
- nodejs
description: "A task module allows you to create modal popup experiences in your Teams application."
urlFragment: teams-module-node
extensions:
  contentType: samples
  createdDate: 9/17/2018 6:53:22 PM
---

# Microsoft Teams task module

A task module allows you to create modal popup experiences in your Teams application. Inside the popup, you can run your own custom HTML/JavaScript code, show an `<iframe>`-based widget such as a YouTube or Microsoft Stream video, or display an [Adaptive card](https://docs.microsoft.com/en-us/adaptive-cards/).

Task modules build on the foundation of Microsoft Teams tabs: a task module is essentially a tab in a popup window. It uses the same SDK, so if you've built a tab you are already 90% of the way to being able to create a task module.

## Try it yourself

This sample is deployed on Microsoft Azure and you can try it yourself by uploading [TaskModule.zip](./TaskModule.zip) to one of your teams and/or as a personal app. (Sideloading must be enabled for your tenant; see [step 6 here](https://docs.microsoft.com/en-us/microsoftteams/platform/get-started/get-started-tenant#turn-on-microsoft-teams-for-your-organization).) The app is running on the free Azure tier, so it may take a while to load if you haven't used it recently and it goes back to sleep quickly if it's not being used, but once it's loaded it's pretty snappy.

## Overview of this sample

This sample app was developed in conjunction with the task module feature itself to exercise as much of it as possible. Here's what's included:

* **A channel tab.** Add the app to a team, then add a Task Module tab and choose "Task Module Demo" from the dropdown list.
* **A bot.** Add the app to a team, then chat with it (@Task Module)
* **A personal app.** When you upload the [TaskModule.zip](TaskModule.zip) file, choose "Add for you" and "Task Module" will appear in the "..." menu in the Teams app bar. The personal app has both a tab and a bot.

The tab shows how to invoke the task module using the Teams SDK. Source code for the tab is found in [TaskModuleTab.ts](src/TaskModuleTab.ts); the view definition is in [taskmodule.pug](src/views/taskmodule.pug). This sample app uses [Pug](https://pugjs.org) (formerly Jade) for HTML rendering.

The following task modules are supported:

* YouTube, which is comprised of a [generic template for embedded `<iframe>` experiences](src/views/embed.pug) (also used for the PowerApp task module below) plus a [four-line stub containing the YouTube embed URL](src/views/youtube.pug)
* [PowerApp](src/views/powerapp.pug) &mdash; unfortunately it doesn't work out of the box; click the button or see the [source code](src/views/powerapp.pug) for details on how you can customize it for your tenant
* [A simple HTML form](src/views/customform.pug)
* There are two Adaptive card examples:
  * Showing the results of an `Action.Submit` button returned to the tab
  * Showing the results returned to the bot as a message

The sample app also contains a bot with cards allowing you to invoke these task modules. You can invoke them from an Adaptive card (using the _tasks_ command) or from a Bot Framework thumbnail card (using the _bfcard_ command). [RootDialog.ts](src/dialogs/RootDialog.ts) contains the code for the _tasks_ and _bfcard_ commands, and [TeamsBot.ts](src/TeamsBot.ts) contains the code for responding to `task/fetch` and `task/submit` messages. The task modules when invoked from a bot are the same as for the tab, except for the Adaptive card examples:

* _Adaptive Card - Single_ returns the results to the conversation as a message.
* _Adaptive Card - Sequence_ shows how adaptive cards can be chained together: instead of returning the result to the chat, the result is shown in another Adaptive card.

## Implementation notes

* This sample is data-driven as much as possible and shares as much code as possible between the bot and the tabs forms of task module:
  * Metadata used to generate [TaskInfo objects](https://docs.microsoft.com/en-us/microsoftteams/platform/concepts/task-modules/task-modules-overview#the-taskinfo-object) is in [contants.ts](src/constants.ts).
  * All cards are generated via JSON templates in [the cardTemplates collection in CardTemplates.ts](src/dialogs/CardTemplates.ts) and rendered using the [ST-JS (Select Transform) JavaScript library](https://st-js.github.io/reference.html). The values in `cardTemplates` have data placeholders, e.g. `{{title}}`; JavaScript objects are created in [TaskModuleTab.ts](src/TaskModuleTab.ts) and [RootDialog.ts](src/dialogs/RootDialog.ts), and helper functions in [CardUtils.ts](src/utils/CardUtils.ts) are used to render the cards.
  * Deep links are generated in the `taskModuleLink()` function in [DeepLinks.ts](src/utils/DeepLinks.ts).
  * Except for the JavaScript in the task modules themselves, all code is in TypeScript. Three source files, [contants.ts](src/constants.ts), [DeepLinks.ts](src/utils/DeepLinks.ts), and [CardTemplates.ts](src/dialogs/CardTemplates.ts) are used on both the client (tab) and server (bot). [Browserify](https://browserify.org) is used in the [gulpfile.js](gulpfile.js) build script to bundle the JavaScript generated by TypeScript; this script file is referenced by [taskmodule.pug](src/views/taskmodule.pug).
* The logic for handling `task/fetch` messages is also data-driven, shared across card types (Adaptive and Bot Framework), and supports chaining of Adaptive card task modules. The data-driven approach for card generation combined with the fact that the Adaptive and Bot Framework card schemas are quite different unfortunately means that the way it works is not obvious. Here's how it works:
  * All the `task/fetch` and `task/submit` requests and responses are JavaScript objects defined in the `fetchTemplates[]` collection in [CardTemplates.ts](src/dialogs/CardTemplates.ts) and generated in [TeamsBot.ts](src/TeamsBot.ts) in the `onInvoke()` function.
  * The custom properties `data.taskModule` (for Adaptive card `Action.Submit` buttons) and `value.taskModule` (for Bot Framework card actions) are used as lookup values to select a specific response from the `fetchTemplates[]` collection when a `task/fetch` message is received.
  * Adaptive card chaining &mdash; showing multiple adaptive cards in a sequence &mdash; is implemented using the `data.taskResponse` property on an Adaptive card `Action.Submit` button. With _Adaptive Card - Single_, the card is rendered with `data.taskResponse` set to "message" (show a message to the user); for _Adaptive Card - Sequence_, the same Adaptive card is rendered with `data.taskResponse` set to "continue" (show another Adaptive card). This is the only difference between `fetchTemplates.adaptivecard1` and `fetchTemplates.adaptivecard2`. The second Adaptive card (`cardTemplates.acSubmitResponse` in [CardTemplates.ts](src/dialogs/CardTemplates.ts)) in the sequence just shows the JSON submitted in the first; its `data.taskResponse` is set to "final". The logic for handling the various values of `data.taskResponse` is in [TeamsBot.ts](src/TeamsBot.ts). The `taskResponse` values form a simple "protocol" for responding to the different ways to respond to `task/submit` messages: do nothing (complete), show a message to the user, load a subsequent adaptive card in a sequence, or load the final adaptive card in a sequence.
* To avoid hardcoding an `appId` in [customform.pug](src/views/customform.pug), the `appId` is passed in at Pug template rendering time by [tabs.ts, the TypeScript  which implements the tab routes](src/tabs.ts).
* Theming and simple keyboard handling for the Esc key are supported.

## Bonus features

There's also an _actester_ bot command. It's a handy way of seeing what an Adaptive card looks like in Teams - simply copy/paste the JSON of Adaptive cards, e.g. the [Samples page on the Adaptive card site](http://adaptivecards.io/samples/), and the bot will render it as a reply.

## Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit <https://cla.microsoft.com.>

When you submit a pull request, a CLA-bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., label, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.
