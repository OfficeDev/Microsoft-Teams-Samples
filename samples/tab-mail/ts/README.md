---
page_type: sample
description: This sample app shows how to compose mail in Outlook using personal tab app.
products:
- office-teams
- office
- office-365
languages:
- typescript
extensions:
 contentType: samples
 createdDate: "20/06/2023 01:38:27 PM"
urlFragment: officedev-microsoft-teams-samples-tab-mail-ts
---

# Tab-Mail

This sample app shows how to compose mail in Outlook using personal tab app.

 ## Included Features
* Tabs

## Interaction with app

![Tab-Mail](Images/tab-mail.gif)

## Prerequisites
-  [NodeJS](https://nodejs.org/en/)

-  [M365 developer account](https://docs.microsoft.com/en-us/microsoftteams/platform/concepts/build-and-test/prepare-your-o365-tenant) or access to a Teams account with the appropriate permissions to install an app.
- [Ngrok](https://ngrok.com/download) (For local environment testing) Latest (any other tunneling software can also be used)
  
- [Teams](https://teams.microsoft.com) Microsoft Teams is installed and you have an account

## Setup
    
1. Setup NGROK
  - Run ngrok - point to port 3978

    ```bash
    ngrok http 3978 --host-header="localhost:3978"
    ```
2. Setup for code
 - Clone the repository

    ```bash
    git clone https://github.com/OfficeDev/Microsoft-Teams-Samples.git
    ```

-  In a terminal, navigate to `samples/tab-mail/ts`

 - Install modules

    ```bash
    npm install
    ```
 - Run your app

    ```bash
    npm start
    ```
3. Setup Manifest for Teams

- **This step is specific to Teams.**
    **Edit** the `manifest.json` contained in the `Manifest/`, you can use any GUID Id in place of `{{Manifest-Id}}` or [Generate Guid](https://guidgenerator.com/).
    **Edit** the `manifest.json` for `validDomains` and replace `{{domain-name}}` with base Url of your domain. E.g. if you are using ngrok it would be `https://1234.ngrok-free.app` then your domain-name will be `1234.ngrok-free.app`.
    **Zip** up the contents of the `Manifest/` folder to create a `manifest.zip`.
    **Upload** the `manifest.zip` to Teams (in the left-bottom *Apps* view, click "Upload a custom app")
    

## Running the sample

**Install App:**

![InstallApp](Images/1.add_app_teams.png)

**App In Teams:**

![Tab-Mail](Images/2.teams_mail_form.png)

**App In Outlook**

![OpenOutlook](Images/3.app_outlook.png)

**Compose Mail From Tab**

![Compose Mail](Images/4.compose_mail.png)

## Further Reading.
[tab-mail](https://learn.microsoft.com/en-us/javascript/api/@microsoft/teams-js/mail?view=msteams-client-js-latest)

[M365 mail support](https://learn.microsoft.com/en-us/microsoftteams/platform/m365-apps/teamsjs-support-m365#mail)

<img src="https://pnptelemetry.azurewebsites.net/microsoft-teams-samples/samples/tab-mail-ts" />
