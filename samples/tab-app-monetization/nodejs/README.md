---
page_type: sample
description: "This is an sample tab application which shows how to open purchase dialog and trigger purchase flow using teams-js sdk"
products:
- office-teams
- office
- office-365
languages:
- nodejs
- javascript
extensions:
  contentType: samples
  createdDate: "07/20/2022 12:00:00 AM"
urlFragment: officedev-microsoft-teams-samples-tab-app-monetization-nodejs
---

# App monetization in tab

This sample shows how to open purchase dialog and trigger purchase flow using teams-js sdk.

## Included Features
* Tabs
* App Monetization

## Interaction with tab
![tab-app-monetization](Images/tab-app-monetization.gif)

## Try it yourself - experience the App in your Microsoft Teams client
Please find below demo manifest which is deployed on Microsoft Azure and you can try it yourself by uploading the app package (.zip file link below) to your teams and/or as a personal app. (Sideloading must be enabled for your tenant, [see steps here](https://docs.microsoft.com/microsoftteams/platform/concepts/build-and-test/prepare-your-o365-tenant#enable-custom-teams-apps-and-turn-on-custom-app-uploading)).

**App monetization in tab:** [Manifest](/samples/tab-app-monetization/csharp/demo-manifest/tab-app-monetization.zip)

## Prerequisites

- [NodeJS](https://nodejs.org/en/)
- [ngrok](https://ngrok.com/) or equivalent tunnelling solution
- [Teams](https://teams.microsoft.com) Microsoft Teams is installed and you have an account
- [Publish an offer to marketplace](https://docs.microsoft.com/microsoftteams/platform/concepts/deploy-and-publish/appsource/prepare/include-saas-offer)


## 1) Setup for App registration
1. Register a new application in the [Azure Active Directory – App Registrations](https://go.microsoft.com/fwlink/?linkid=2083908) portal.
2. Select **New Registration** and on the *register an application page*, set following values:
    * Set **name** to your app name.
    * Choose the **supported account types** (any account type will work)
    * Leave **Redirect URI** empty.
    * Choose **Register**.
3. On the overview page, copy and save the **Application (client) ID**. You’ll need this later when updating your Teams application manifest.

## 2) Setup for SAAS offer
1) Register a SAAS offer in market place and generate an plan id for it [Create SAAS Offer](https://docs.microsoft.com/microsoftteams/platform/concepts/deploy-and-publish/appsource/prepare/include-saas-offer)

###  3) Setup NGROK
1) Run ngrok - point to port 3978

```bash
# ngrok http 3978 --host-header="localhost:3978"
```

## 4) Setup for code
1) Clone the repository
   ```bash
   git clone https://github.com/OfficeDev/Microsoft-Teams-Samples.git
   ```
2) Install node modules

   Inside node js folder,  navigate to `samples/tab-app-monetization/nodejs/ClientApp` open your local terminal and run the below command to install node modules. You can do the same in Visual Studio code terminal by opening the project in Visual Studio code.

    ```bash
    npm install
    ```
   **_Note_** - Navigate to `samples/tab-app-monetization/nodejs/ClientApp/src/components/tab.tsx` and update the planId and term on line 14 and 15 with plan details created in step 2

3) Run the solution from the same path terminal using below command.

    ```
    npm start
    ```

###  5) Setup Manifest for Teams
1. Modify the `manifest.json` in the `/AppPackage` folder and replace the following details
   - `{{App-id}}` with your application id created in step 1.
   - `{{Domain-Name}}` with your application's base url domain, e.g. For https://1234.ngrok-free.app the Domain Name will be 1234.ngrok-free.app
   - `{{Plan-id}}` with plan id generated in step 2.

2. Zip the contents of `AppPackage` folder into a `manifest.zip`, and use the `manifest.zip` to deploy in app store or add to Teams.
    
3. Upload the manifest.zip to Teams (in the Apps view click "Upload a custom app")
   - Go to Microsoft Teams. From the lower left corner, select Apps
   - From the lower left corner, choose Apps -> Manage your apps -> Upload an app.
   - Go to your project directory, the ./AppPackage folder, select the zip folder, and choose Open.
   - Select Add in the pop-up dialog box. Your app is uploaded to Teams.
   
4. Add the tab in personal scope.

## 6) Features of the sample

- Add the tab in personal scope.
![app-install](Images/app-purchase-install.png)
![tab-page](Images/app-purchase-tab.png)

- On click of upgrade button will trigger the purchase flow.
![offer-region](Images/app-purchase-popup1.png)
![offer-popup](Images/app-purchase-popup2.png)
![offer-popup-checkout](Images/app-purchase-popup3.png)

## Further reading

- [Inapp Purchases](https://docs.microsoft.com/microsoftteams/platform/concepts/deploy-and-publish/appsource/prepare/in-app-purchase-flow)
- [Tab Basics](https://docs.microsoft.com/microsoftteams/platform/tabs/how-to/create-channel-group-tab?pivots=node-java-script)
- [Azure Portal](https://portal.azure.com)

<img src="https://pnptelemetry.azurewebsites.net/microsoft-teams-samples/samples/tab-app-monetization-nodejs" />