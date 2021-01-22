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
  createdDate: "12/8/2020 5:06:47 PM"
---

# Deploying the Microsoft Teams UI templates sample app

<img align="right" width="400" src="https://i.ibb.co/xSLQP14/app-sample.png" />

This sample app can help you better understand how apps should look and behave in Microsoft Teams. The app includes examples of tested, high-quality UI templates that work across common Teams use cases (such as dashboards or forms).

To use the sample app, you need to host it somewhere. We'll focus on deploying the app to a local web server since that's the fastest way to get started.

(If you want to make it easier for others to use the app, consider deploying to [Microsoft Azure](https://azure.microsoft.com/get-started/web-app/) or another hosting service.)

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

- `.env`: Contains the app configurations. When you create the app package, the manifest is dynamically populated with values from this file.

  > [!NOTE]
  > The `.env` file is excluded from source control. If you're deploying to Azure, make sure that you include the `.env` configurations as application settings in your Azure web app (except the `PORT` variable, which is used for local testing and debugging).

## Run the app

1. In the root directory of your project, run the following command.

   ```bash
   yarn start
   ```

   When the app starts, you should see the following terminal output.

   ```
     You can now view microsoft-teams-app-sample in the browser.
       Local:            http://localhost:3000
       On Your Network:  http://192.168.0.10:3000
   ```

   If you see a port number other than `3000`, it's because the `PORT` environment variable in the `.env` file has a different value. You can use that port or change it to 3000.

2. Open a browser and verify that all of the following URLs load:
   - **Required Teams app pages**:
     - About: [http://localhost:3000](http://localhost:3000)
     - Privacy: [http://localhost:3000/#/privacy](http://localhost:3000/#/privacy)
     - Terms of use: [http://localhost:3000/#/termsofuse](http://localhost:3000/#/termsofuse)
   - **Sample app tabs**:
     - [http://localhost:3000/#/welcome](http://localhost:3000/#/welcome)
     - [http://localhost:3000/#/dashboard](http://localhost:3000/#/dashboard)
     - [http://localhost:3000/#/list](http://localhost:3000/#/list)
     - [http://localhost:3000/#/board](http://localhost:3000/#/board)

## Set up a secure tunnel to the app

Teams doesn't display app content unless it's accessible via HTTPS. We recommend using ngrok to establish a secure tunnel to where you're hosting the app locally (for example, `http://localhost:3000`).

1. Install [ngrok](https://ngrok.io).

1. Run the following command to create the tunnel to your `localhost`.

   ```bash
   yarn serve
   ```

1. Save the HTTPS URL in the output (for example, https://468b9ab725e9.ngrok.io). You may need this later if you plan to register the app with App Studio.

> [!IMPORTANT]
> If you're using the free version of ngrok and plan to share the app with others, remember that ngrok quits if your machine shuts down or goes to sleep. When you restart ngrok, the URL also will be different. (A paid version of ngrok provides persistent URLs.)

## Create the app package

You need an app package to sideload the app in Teams.

1. Open a separate terminal so that you don't interfere with the running app.

1. Run the following command to generate the app package.

   ```bash
   yarn package
   ```

   This process validates the manifest and saves the package as a `zip` file in the `package` folder.

## Sideload the app in Teams

1. In the Teams client, go to **Apps**.

1. Select **Upload a custom app** and upload the app package, which is the generated `zip` file in the `package` folder.

   <img type="content" src="https://docs.microsoft.com/en-us/microsoftteams/platform/assets/images/build-your-first-app/upload-custom-app-closeup.png" alt-text="Illustration showing where in Teams you can upload a custom app." />

## Enable logging

To view app logs, the `DEBUG` environment variable must be set to `msteams`. This is enabled by default in your project's `.env` file.

If disabled, run the following command in a terminal to see logs.

```bash
SET DEBUG=msteams
```

If you're hosting the app in Azure, set `DEBUG` to `msteams` in application settings.

For more information, read about the [debug package](https://www.npmjs.com/package/debug).

## Next steps

- Design your Teams app with [UI templates](https://docs.microsoft.com/microsoftteams/platform/concepts/design/design-teams-app-ui-templates).
- Implement UI templates with the [Microsoft Teams UI Library](https://www.npmjs.com/package/@fluentui/react-teams).
