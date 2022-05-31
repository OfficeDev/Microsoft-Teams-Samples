---
page_type: sample
products:
- office-365
languages:
- javascript
title: Microsoft Teams NodeJS Bot Consent Sample
description: Microsoft Teams bot consent sample app in Node.js
extensions:
  contentType: samples
  createdDate: 11/3/2017 12:53:17 PM
---

# Microsoft Teams Bot Consent sample app.

## Official documentation

More information for this sample - and for how to get started with Microsoft Teams development in general - is found in [Get started on the Microsoft Teams platform with Node.js and App Studio](https://docs.microsoft.com/en-us/microsoftteams/platform/get-started/get-started-nodejs-app-studio).

## Using this sample locally

This sample can be run locally using `ngrok` as described in the [official documentation](https://docs.microsoft.com/en-us/microsoftteams/platform/get-started/get-started-nodejs-app-studio), but you'll need to set up some environment variables. There are many ways to do this, but the easiest, if you are using Visual Studio Code, is to add a `.env` file in the src directory with the following content:

```
MicrosoftAppId=00000000-0000-0000-0000-000000000000
MicrosoftAppPassword=YourBotAppPassword
BaseUrl=https://########.ngrok.io
```

Where:

* `########` matches your actual ngrok URL
* `MICROSOFT_APP_ID` and `MICROSOFT_APP_PASSWORD` is the ID and password, respectively, for your bot

# Contributing

This project welcomes contributions and suggestions. Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.microsoft.com.

When you submit a pull request, a CLA-bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., label, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.