---
page_type: sample
products:
  - office
  - office-teams
  - office-365
languages:
  - typescript
  - javascript
  - html
description: "This sample demonstrates @fluentui/react-teams library in Microsoft Teams apps."
extensions:
  contentType: samples
  createdDate: "12/08/2020 05:06:47 PM"
urlFragment: officedev-microsoft-teams-samples-tab-ui-templates-ts
---

# Deploying the Microsoft Teams UI templates sample app

This sample app can help you better understand how apps should look and behave in Microsoft Teams. The app includes examples of tested, high-quality UI templates that work across common Teams use cases (such as dashboards or forms).

- **Interaction with tab**
![Tab-page](Images/tab-ui-templates.gif)

## Prerequisites

- <a href="https://git-scm.com/" target="_blank">Install Git</a>
- [Node.js and npm](https://nodejs.org). Run the command `node --version` to verify that Node.js is installed.
- Set up a [Microsoft 365 developer account](https://docs.microsoft.com/microsoftteams/platform/build-your-first-app/build-first-app-overview#set-up-your-development-account), which allows app sideloading in Teams.

## Set up your app project

Open a terminal and clone the sample app repository.

```bash
git clone https://github.com/OfficeDev/Microsoft-Teams-Samples.git
cd Microsoft-Teams-Samples/samples/tab-ui-templates/ts
yarn install
```

You can find the app source code in `./src`:

- `app`: Includes the app scaffolding.

- `manifest`: Includes the app manifest (`manifest.json`) and the color and outline versions of the app icon.

- `assets`: Includes the app assets.

## Run the app

1. In the root directory of your project, run the following command.

   ```bash
   yarn start
   ```

   When the app starts, you should see the following terminal output.

   ```
     You can now view microsoft-teams-app-sample in the browser.
       Local:            http://localhost:3978
       On Your Network:  http://192.168.0.10:3978
   ```

2. Open a browser and verify that all of the following URLs load:
   - **Required Teams app pages**:
     - About: [http://localhost:3978](http://localhost:3978)
     - Privacy: [http://localhost:3978/#/privacy](http://localhost:3978/#/privacy)
     - Terms of use: [http://localhost:3978/#/termsofuse](http://localhost:3978/#/termsofuse)
   - **Sample app tabs**:
     - [http://localhost:3978/#/welcome](http://localhost:3978/#/welcome)
     - [http://localhost:3978/#/dashboard](http://localhost:3978/#/dashboard)
     - [http://localhost:3978/#/list](http://localhost:3978/#/list)
     - [http://localhost:3978/#/board](http://localhost:3978/#/board)

## Set up a secure tunnel to the app

Teams doesn't display app content unless it's accessible via HTTPS. We recommend using ngrok to establish a secure tunnel to where you're hosting the app locally (for example, `http://localhost:3978`).

1. Install [ngrok](https://ngrok.io).

1. Run ngrok - point to port 3978

```bash
# ngrok http -host-header=rewrite 3978
```

1. Save the HTTPS URL in the output (for example, https://468b9ab725e9.ngrok.io). You may need this later if you plan to register the app with App Studio.

> [!IMPORTANT]
> If you're using the free version of ngrok and plan to share the app with others, remember that ngrok quits if your machine shuts down or goes to sleep. When you restart ngrok, the URL also will be different. (A paid version of ngrok provides persistent URLs.)

## Create the app package
1) __*This step is specific to Teams.*__
    - **Edit** the `manifest.json` contained in the  `src/manifest` folder to replace `<<GUID_ID>>` with any GUID value.
    - **Edit** the `manifest.json` for `staticTabs` inside `contentUrl` . Replace `<<HOSTNAME>>` with base Url domain. E.g. if you are using ngrok it would be `https://1234.ngrok.io` then your domain-name will be `1234.ngrok.io`.
    - **Edit** the `manifest.json` for `validDomains` with base Url domain. E.g. if you are using ngrok it would be `https://1234.ngrok.io` then your domain-name will be `1234.ngrok.io`.
    - **Zip** up the contents of the `manifest` folder to create a `manifest.zip` (Make sure that zip file does not contains any subfolder otherwise you will get error while uploading your .zip package)
    - **Upload** the `manifest.zip` to Teams (In Teams Apps/Manage your apps click "Upload an app". Browse to and Open the .zip file. At the next dialog, click the Add button.)
    - Add the app to personal/team/groupChat scope (Supported scopes)


## Sideload the app in Teams

1. In the Teams client, go to **Apps**.

1. Select **Upload a custom app** and upload the app package, which is the generated `zip` file in the `package` folder.

   <img type="content" src="https://docs.microsoft.com/en-us/microsoftteams/platform/assets/images/build-your-first-app/upload-custom-app-closeup.png" alt-text="Illustration showing where in Teams you can upload a custom app." />

## Running the sample

- Welcome Page
- ![welcome-page ](Images/tabui-welcome.png)

- Dashboard Page
- ![dashboard-page ](Images/tabui-dashboard.png)

- List Page
- ![list-page ](Images/tabui-list.png)

- Board Page
- ![board-page ](Images/tabui-board.png)

## Next steps

- Design your Teams app with [UI templates](https://docs.microsoft.com/microsoftteams/platform/concepts/design/design-teams-app-ui-templates).
- Implement UI templates with the [Microsoft Teams UI Library](https://www.npmjs.com/package/@fluentui/react-teams).