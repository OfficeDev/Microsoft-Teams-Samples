---
page_type: sample
description: This sample demonstrates how to build a GitHub connector for Microsoft Teams, enabling integration with GitHub's APIs for streamlined notifications.
products:
- office-teams
- office-365
languages:
- javascript
extensions:
  contentType: samples
  technologies:
  - Connectors
  createdDate: "08/21/2017 11:52:11 AM"
urlFragment: officedev-microsoft-teams-samples-connector-github-notification-nodejs
---

> [!IMPORTANT]
>
> The existing Microsoft 365 (previously called Office 365) connectors across all cloud platforms are nearing deprecation, and the creation of new Microsoft 365 connectors will soon be blocked. For more information on the schedule and how the Workflows app provides a more flexible and secure experience, see [retirement of Microsoft 365 connectors within Microsoft Teams](https://devblogs.microsoft.com/microsoft365dev/retirement-of-office-365-connectors-within-microsoft-teams/).

# GitHub Connector for Microsoft Teams

This sample illustrates how to develop a GitHub connector for Microsoft Teams using Node.js, allowing teams to receive GitHub notifications directly within their Teams channels. The setup guides you through creating an OAuth application on GitHub, configuring a tunnel for local testing, and uploading the connector into Teams, making it convenient to monitor GitHub activities.
 
#### Prerequisites
1. Register a new OAuth application at GitHub. Note the GitHub client id and secret.
2. If you want to run this code locally, use a tunnelling service like [dev tunnel](https://learn.microsoft.com/en-us/azure/developer/dev-tunnels/get-started?tabs=windows) or [ngrok](https://ngrok.com/) latest version.
3. If you are using Ngrok as a tunnelling service then download ngrok from https://ngrok.com/. Run the following command to setup a tunnel to localhost:3000
 `ngrok http 3000`
 Note the ngrok address, which looks something like `https://013e0d3f.ngrok-free.app` and if you are using dev tunnels, your URL will be like: https://12345.devtunnels.ms.
3. Put the callback Url in the Oauth app as 'your-ngrok/auth/github/callback'.
4. Replace the clientId,clientSecret,callbackUrl and serviceUrl in Default.json.

### Configuration 
 - Default configuration is in `config\default.json`
 
### How to Run
 - install all the dependencies through npm install.
 - run node server.js.
 - Zip manifest.json file and upload to any team. Alternative you can set your own connector at Microsoft connector portal (https://outlook.office.com/connectors/publish) and follow instructions here to get a new connector for microsoft teams ( https://msdn.microsoft.com/en-us/microsoft-teams/connectors).

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


<img src="https://pnptelemetry.azurewebsites.net/microsoft-teams-samples/samples/connector-github-notification-nodejs" />
