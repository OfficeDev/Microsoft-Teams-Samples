---
page_type: sample
products:
- office-365
languages:
- python
title: Microsoft Teams Python Membership change notification Sample
description: This sample application demonstrates how to manage/handle membership change notification for shared channel, such as users being added, removed, or having their membership updated and when a channel is shared/unshared with a team, using python and the Microsoft Graph API.
extensions:
  contentType: samples
  createdDate: 30/06/2025 10:02:21 PM
urlFragment: officedev-microsoft-teams-samples-graph-membership-change-notification-python
---

# Change Notifications For Indirect Membership Updates Using Microsoft Graph Node.js

This sample application demonstrates how to manage/handle membership change notification for shared channel, such as users being added, removed, or having their membership updated when a channel is shared/unshared with a team. The application leverages Python and the Microsoft Graph API to deliver real-time notifications. It includes comprehensive setup instructions covering Azure AD registration, bot configuration, self-signed certificate management, and deployment using the Microsoft 365 Agents Toolkit for Visual Studio Code.

## Included Features
* Tabs
* Graph API
* RSC Permissions
* Change Notifications

## Interaction with app
![Notifications](Images/ChangeNotifications.gif)

## Try it yourself - experience the App in your Microsoft Teams client
Please find below demo manifest which is deployed on Microsoft Azure and you can try it yourself by uploading the app package (.zip file link below) to your teams and/or as a personal app. (Uploading must be enabled for your tenant, [see steps here](https://docs.microsoft.com/microsoftteams/platform/concepts/build-and-test/prepare-your-o365-tenant#enable-custom-teams-apps-and-turn-on-custom-app-uploading)).


## Prerequisites

- Microsoft Teams is installed and you have an account
- [Python SDK](https://www.python.org/downloads/) min version 3.8
- [dev tunnel](https://learn.microsoft.com/en-us/azure/developer/dev-tunnels/get-started?tabs=windows) or [ngrok](https://ngrok.com/) latest version or equivalent tunnelling solution
-  [M365 developer account](https://docs.microsoft.com/microsoftteams/platform/concepts/build-and-test/prepare-your-o365-tenant) or access to a Teams account with the appropriate permissions to install an app.
- [Microsoft 365 Agents Toolkit for VS Code](https://marketplace.visualstudio.com/items?itemName=TeamsDevApp.ms-teams-vscode-extension) or [TeamsFx CLI](https://learn.microsoft.com/microsoftteams/platform/toolkit/teamsfx-cli?pivots=version-one)

## Run the app (Using Microsoft 365 Agents Toolkit for Visual Studio Code)

The simplest way to run this sample in Teams is to use Microsoft 365 Agents Toolkit for Visual Studio Code.

1. Ensure you have downloaded and installed [Visual Studio Code](https://code.visualstudio.com/docs/setup/setup-overview)
1. Install the [Microsoft 365 Agents Toolkit extension](https://marketplace.visualstudio.com/items?itemName=TeamsDevApp.ms-teams-vscode-extension) and [Python Extension](https://marketplace.visualstudio.com/items?itemName=ms-python.python)
1. Select **File > Open Folder** in VS Code and choose this samples directory from the repo
1. Press **CTRL+Shift+P** to open the command box and enter **Python: Create Environment** to create and activate your desired virtual environment. Remember to select `requirements.txt` as dependencies to install when creating the virtual environment.
1. Using the extension, sign in with your Microsoft 365 account where you have permissions to upload custom apps
1. Select **Debug > Start Debugging** or **F5** to run the app in a Teams web client.
1. In the browser that launches, select the **Add** button to install the app to Teams.

> If you do not have permission to upload custom apps (uploading), Microsoft 365 Agents Toolkit will recommend creating and using a Microsoft 365 Developer Program account - a free program to get your own dev environment sandbox that includes Teams.

### Register your application with Azure AD

1. Register a new application in the [Microsoft Entra ID - App Registrations](https://go.microsoft.com/fwlink/?linkid=2083908) portal.
2. On the overview page, copy and save the **Application (client) ID, Directory (tenant) ID**. You'll need those later when updating your Teams application manifest and in the .env file.
3. Navigate to **API Permissions**, and make sure to add the follow permissions:
-   Select Add a permission
-   Select Microsoft Graph -> Application permissions.
   - `Channel.ReadBasic.All`,`ChannelSettings.Read.All`,`Directory.ReadWrite.All`,`Group.ReadWrite.All`
    `Team.ReadBasic.All`,`TeamSettings.Read.All`,`TeamSettings.ReadWrite.All`

-   Click on Add permissions. Please make sure to grant the admin consent for the required permissions.

4.  Navigate to the **Certificates & secrets**. In the Client secrets section, click on "+ New client secret". Add a description (Name of the secret) for the secret and select "Never" for Expires. Click "Add". Once the client secret is created, copy its value, it need to be placed in the appsettings.json file.

### Create and install Self-Signed certificate

To include resource data of graph notifications, this Graph API require self-signed certificate. Follow the below steps to create and manage certificate.

1. You can self-sign the certificate, since Microsoft Graph does not verify the certificate issuer, and uses the public key for only encryption.

2. Use [Azure Key Vault](https://docs.microsoft.com/en-us/azure/key-vault/key-vault-whatis) as the solution to create, rotate, and securely manage certificates. Make sure the keys satisfy the following criteria:

    - The key must be of type `RSA`
    - The key size must be between 2048 and 4096 bits

3. Follow this documentation for the steps - [**Create and install Self-Signed certificate**](CertificateDocumentation/README.md)

## Run the app (Manually Uploading to Teams)

> Note these instructions are for running the sample on your local machine, the tunnelling solution is required because
the Teams service needs to call into the bot.

1) Clone the repository

    ```bash
    git clone https://github.com/OfficeDev/Microsoft-Teams-Samples.git
    ```

2) Run ngrok - point to port 3978

   ```bash
   ngrok http 3978 --host-header="localhost:3978"
   ```  

   Alternatively, you can also use the `dev tunnels`. Please follow [Create and host a dev tunnel](https://learn.microsoft.com/en-us/azure/developer/dev-tunnels/get-started?tabs=windows) and host the tunnel with anonymous user access command as shown below:

   ```bash
   devtunnel host -p 3978 --allow-anonymous
   ```

3) In a terminal, go to `samples\graph-indirect-membership-change-notification`

4) Activate your desired virtual environment

5) Install dependencies by running ```pip install -r requirements.txt``` in the project folder.

6) Create a key.pem file in helper folder and add your key of certificate in the file.

7) Update the `Base64EncodedCertificate` value with your base64 certificate.

8) Run your app with `python app.py`

### 4. Setup Manifest for Teams

 - **This step is specific to Teams.**

    - **Edit** the `manifest.json` contained in the `graph-indirect-membership-change-notification/python/appManifest` folder to replace your Microsoft App Id (that was created when you registered your bot earlier) *everywhere* you see the place holder string `${{TEAMS_APP_ID}}` with a random GUID and `${{AAD_APP_CLIENT_ID}}` with your app id.
    - **Edit** the `manifest.json` for `configurationUrl` inside `configurableTabs` and `validDomains`. Replace `${{TAB_ENDPOINT}}`/`${{TAB_DOMAIN}}` with base Url domain. E.g. if you are using ngrok it would be `https://1234.ngrok-free.app` then your domain-name will be `1234.ngrok-free.app` and if you are using dev tunnels then your domain will be like: `12345.devtunnels.ms`.
    
    **Note:** If you want to test your app across multi hub like: Outlook/Office.com, please update the `manifest.json` in the `graph-indirect-membership-change-notification/python/appManifest_Hub` folder with the required values.

    - **Zip** up the contents of the `graph-indirect-membership-change-notification/python/appManifest` folder to create a `manifest.zip`(Make sure that zip file does not contains any subfolder otherwise you will get error while uploading your .zip package)
    - **Upload** the `manifest.zip` to Teams (In Teams Apps/Manage your apps click "Upload an app". Browse to and Open the .zip file. At the next dialog, click the Add button.)
    - Add the app to personal/team/groupChat scope (Supported scopes)

## Using RSC Permissions

If you prefer to use Resource-Specific Consent (RSC) permissions instead of application permissions, you can skip the "API Permissions" steps described earlier in the Azure AD registration section. Instead, update your Teams app manifest with the following properties to leverage RSC permissions:

```json
"webApplicationInfo": {
    "id": "${{AAD_APP_CLIENT_ID}}",
    "resource": ""
  },
  "authorization": {
		"permissions": {
			"resourceSpecific": [
				{
					"name": "TeamsAppInstallation.Read.User",
					"type": "Application"
				},
				{
					"name": "Member.Read.Group",
					"type": "Application"
				},
				{
					"name": "ChannelSettings.Read.Group",
					"type": "Application"
				},
				{
					"name": "ChannelMember.Read.Group",
					"type": "Application"
				},
				{
					"name": "ChannelMember.ReadWrite.Group",
					"type": "Application"
				}
			]
		}
	}
```

## Running the sample

1. **App Install**
![AppInstall](Images/1.App_install.png)

2. **Select Shared Channel**
![SelectSharedChannel](Images/2.Select_channel.png)

3. **Configure Page**
![Configure Page](Images/3.Configure.png)

4. **Welcome Page**
![welcomePage](Images/4.StartUp_page.png)

5. **User Added**
![UserAdded](Images/5.User_added.png)

6. **User Removed**
![UserRemoved](Images/6.User_removed.png)

7. **User Membership Updated**
![Membership Updated](Images/7.User_membership_updated.png)

8. **Shared with a Team**
![Shared with a team](Images/8.Shared_with_team.png)

9. **Unshared with a Team**
![Unshared with a team](Images/9.Unshared_with_team.png)

## Further reading
- [Change notifications for Microsoft Teams channel](https://docs.microsoft.com/en-us/graph/teams-changenotifications-team-and-channel)
- [Create subscription permissions for supported resource](https://docs.microsoft.com/en-us/graph/api/subscription-post-subscriptions?view=graph-rest-1.0&tabs=http#team-channel-and-chat)
