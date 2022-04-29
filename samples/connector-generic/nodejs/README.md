---
page_type: sample
description: Sample code for a generic connector that's easy to customize for any system which supports webhooks.
products:
- office-teams
- office
- office-365
languages:
- nodejs
extensions:
contentType: samples
createdDate: "07-07-2021 13:38:26"
---
# Sample Connector 
This contains the source for a generic connector that's easy to customize for any system which supports webhooks. 
 
## Prerequisites
To complete this tutorial, you need the following tools. If you don't already have them you can install them from these links.

* [Git](https://git-scm.com/downloads) 
* [Node.js and NPM](https://nodejs.org/)
* Get any text editor or IDE. You can install and use [Visual Studio Code](https://code.visualstudio.com/download) for free.
* An Office 365 account with access to Microsoft Teams, with [sideloading enabled](https://docs.microsoft.com/microsoftteams/platform/concepts/build-and-test/prepare-your-o365-tenant#enable-custom-teams-apps-and-turn-on-custom-app-uploading).
* If you want to run this code locally, use a tunnelling service. These instructions assume you are using [ngrok](https://ngrok.com/). 

### Configure your own connector

1. Start the tunnelling service to get an https endpoint. 
   1. Open a new command prompt window. 
   1. Change to the directory that contains the ngrok.exe application. 
   1. In the command prompt, run the command `ngrok http 3978 --host-header=localhost`.
   1. Ngrok will fill the entire prompt window. Make note of the https:// Forwarding URL. This URL will be your [BASE_URI] referenced below. 
   1. Minimize the ngrok Command Prompt window. It is no longer referenced in these instructions, but it must remain running.
1. [Register a new connector](https://docs.microsoft.com/microsoftteams/platform/webhooks-and-connectors/how-to/connectors-creating#adding-a-connector-to-your-teams-app)
   1. Open [Connector Developer Portal](https://aka.ms/connectorsdashboard) and select New Connector.
   1. Fill in all the basic details such as name, logo, descriptions etc. for the new connector.
   1. For the configuration page, you'll use our sample code's setup endpoint: `https://[BASE_URI]/connector/setup`
   1. For Valid domains, make enter your domain's http or https URL, e.g. XXXXXXXX.ngrok.io.
   1. Click on Save. After the save completes, you will see your connector ID in address bar.
1. In the `~/views/connectorconfig.jade` file line 27 and replace `ngrokURL` to the ngrok https forwarding url from the above.
1. Install all the dependencies by running `npm install` in root directory.
1. Run the sample: `node server.js`
1. Manifest updates:
   1. Replace `ConnectorId` field in `~/app manifest/manifest.json` file with your newly registered connector ID.
   1. Select all three files (manifest.json, outline_icon.png, color_icon.png) and create a zip file. This is your Teams App Manifest package.
1. Now you can [upload your app manifest](https://docs.microsoft.com/microsoftteams/platform/concepts/deploy-and-publish/apps-upload#upload-your-package-into-a-team-using-the-apps-tab) package in a team and test your new connector by following instructions in config UI.

## More Information
For more information about getting started with Teams, please review the following resources:
- Review [Office 365 Connectors](https://docs.microsoft.com/microsoftteams/platform/webhooks-and-connectors/how-to/connectors-creating)
- Review [Understanding Teams app capabilities](https://docs.microsoft.com/microsoftteams/platform/concepts/capabilities-overview)

## Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.microsoft.com.

When you submit a pull request, a CLA-bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., label, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

