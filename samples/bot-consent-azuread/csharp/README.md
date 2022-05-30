---
page_type: sample
products:
- office-teams
- office
- office-365
languages:
- csharp
title: Microsoft Teams C# Azure AD Consent Bot Sample
description: Microsoft Teams Azure AD Consent Bot Sample application for .NET/C#
extensions:
  contentType: samples
  platforms:
  - CSS
  createdDate: 05/30/2021 10:02:21 PM
---

# Microsoft Teams Azure AD Consent Bot Sample.

- Microsoft Teams Azure AD Consent Bot Sample.

## Official documentation

This Teams Bot Sample demonstrates how to handle Azure AD Consent, when required. It uses special Action Types in Bot Adaptive Cards, to open a modal pop-out window (not an iFrame), which then will allow you to use MSAL to get consent from the user.
Once the user has consented succesfully, we are then able to handle this in the bot, and provide the user with a profile card, that is populated using information that was gathered from Graph API about the logged in user.

Once granted, if you want to revoke consent, log in to the [Azure Portal](https://portal.azure.com/) as an administrator, in the tenant where you are messaging this bot from, go to Azure AD/Enterprise Apps, find the application, go to Properties, and then delete the Enterprise App.
After a minute or 2, the consent that was previously provided will be revoked, and if you send a 'hello' to the bot and ask it to get your profile, you will be walked through the consent process again.

To get started with this sample, firstly you'll need to create an Azure AD App Registration & Azure Bot Service resource.
In your Azure AD App Registration, add the following Application Permission Graph API Scopes:
- User.Read.All

And add the following Delegated Permission Graph API Scopes:
- User.Read
- User.Presence.Read

You'll then need to follow this guide to configure your App Registration to support [Teams SSO](https://docs.microsoft.com/en-us/microsoftteams/platform/bots/how-to/authentication/auth-aad-sso-bots)

Next, edit the manifest.json file, so that it contains the correct values for your application, replacing the Id, botId & webApplicationInfo values.

Then, you'll need to package up the contents of the Manifest folder into a zip, which can be uploaded into Teams as a sideloaded (or organisation) app for you to test.

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
