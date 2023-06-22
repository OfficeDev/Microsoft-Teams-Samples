---
page_type: sample
description: This is an example to show how to write a connector for Microsoft Teams using GitHub's APIs.
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

# GitHub Connector for Microsoft Teams

This is an example to show how to write a connector for Microsoft Teams using GitHub's APIs. It is not the source code for the GitHub connector in Teams/Outlook/Yammer.
 
#### Prerequisites
1. Register a new OAuth application at GitHub. Note the GitHub client id and secret.
2. Download ngrok from https://ngrok.com/. Run the following command to setup a tunnel to localhost:3000
 `ngrok http 3000`
 Note the ngrok address, which looks something like `https://013e0d3f.ngrok-free.app`.
3. Put the callback Url in the Oauth app as 'your-ngrok/auth/github/callback'.
4. Replace the clientId,clientSecret,callbackUrl and serviceUrl in Default.json.

### Configuration 
 - Default configuration is in `config\default.json`
 
### How to Run
 - install all the dependencies through npm install.
 - run node server.js.
 - Zip manifest.json file and sideload to any team. Alternative you can set your own connector at Microsoft connector portal (https://outlook.office.com/connectors/publish) and follow instructions here to get a new connector for microsoft teams ( https://msdn.microsoft.com/en-us/microsoft-teams/connectors).

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