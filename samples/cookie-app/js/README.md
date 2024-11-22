---
page_type: sample
description: "This sample application demonstrates key web storage features, including managing cookies, SameSite cookies, and partitioned cookies. It also covers local storage and IndexedDB for data persistence in modern web applications."
products:
- office-teams
- office
- office-365
languages:
- javascript
extensions:
  contentType: samples
  createdDate: "11/22/2024 12:30:00 PM"
urlFragment: officedev-microsoft-teams-samples-cookie-app-js
---

# Microsoft Teams Cookie App

This sample application provides an interactive demonstration of cookie management, including setting, clearing, and customizing attributes such as SameSite and Secure flags. It also explores advanced web storage solutions like local storage and IndexedDB, offering a practical guide to modern client-side data handling.

## Included Features
* Cookies
* SameSite Cookies
* Partitioned Cookies
* LocalStorage
* IndexDB

## Interaction with app

![Cookie App](Images/Cookie_App.gif)

## Try it yourself - experience the App in your Microsoft Teams client
Please find below demo manifest which is deployed on Microsoft Azure and you can try it yourself by uploading the app package (.zip file link below) to your teams and/or as a personal app. (Sideloading must be enabled for your tenant, [see steps here](https://docs.microsoft.com/microsoftteams/platform/concepts/build-and-test/prepare-your-o365-tenant#enable-custom-teams-apps-and-turn-on-custom-app-uploading)).

**Microsoft Teams Cookie App Demo Manifest:** [Manifest](/samples/cookie-app/js/demo-manifest/cookie-app.zip)

## Prerequisites

- To test locally, [NodeJS](https://nodejs.org/en/download/) must be installed on your development machine (version 16.14.2  or higher).

- [dev tunnel](https://learn.microsoft.com/en-us/azure/developer/dev-tunnels/get-started?tabs=windows) or [ngrok](https://ngrok.com/) latest version or equivalent tunnelling solution

## Setup

2. Setup NGROK
 - Run ngrok - point to port 3000

   ```bash
   ngrok http 3000 --host-header="localhost:3000"
   ```  

   Alternatively, you can also use the `dev tunnels`. Please follow [Create and host a dev tunnel](https://learn.microsoft.com/en-us/azure/developer/dev-tunnels/get-started?tabs=windows) and host the tunnel with anonymous user access command as shown below:

   ```bash
   devtunnel host -p 3000 --allow-anonymous
   ```

3. Setup for code

  - Clone the repository

    ```bash
    git clone https://github.com/OfficeDev/Microsoft-Teams-Samples.git
    ```
  
  Install node modules

   Inside js folder,  navigate to `samples/cookie-app/js` open your local terminal and run the below command to install node modules. You can do the same in Visual Studio code terminal by opening the project in Visual Studio code.

   - Repeat the same step in folder `samples/cookie-app/js`

    ```bash
    npm install
    ```
  - Run the sample by following command:

    ```
    npm start
    ``` 

4. Setup Manifest for Teams
- __*This step is specific to Teams.*__
    - **Edit** the `manifest.json` contained in the ./appPackage folder to replace your Microsoft App Id (that was created when you registered your app registration earlier) *everywhere* you see the place holder string `{{ANY-GUID-ID}}` (depending on the scenario the Microsoft App Id may occur multiple times in the `manifest.json`)
    - **Edit** the `manifest.json` for `validDomains` and replace `{{DOMAIN-NAME}}` with base Url of your domain. E.g. if you are using ngrok it would be `https://1234.ngrok-free.app` then your DOMAIN-NAME will be `1234.ngrok-free.app` and if you are using dev tunnels then your domain will be like: `12345.devtunnels.ms`.
    - **Zip** up the contents of the `appPackage` folder to create a `manifest.zip` (Make sure that zip file does not contains any subfolder otherwise you will get error while uploading your .zip package)

- Upload the manifest.zip to Teams (in the Apps view click "Upload a custom app")
   - Go to Microsoft Teams. From the lower left corner, select Apps
   - From the lower left corner, choose Upload a custom App
   - Go to your project directory, the ./appPackage folder, select the zip folder, and choose Open.
   - Select Add in the pop-up dialog box. Your app is uploaded to Teams.

## Running the sample

**Install App**
![side panel ](Images/Install_App.png)

**Index Page:**
![shared content](Images/2.IndexPage.png)

**Page Cookie**
![shared content second user](Images/3.Page_Cookie.png)

**Seam Site Cookie**
![shared content](Images/4.Page_SeamsiteCookie.png)

**Partitioned Cookie:**
![shared content](Images/5.Page_PartitionedCookie.png)

**Local Storage Cookie:**
![shared content](Images/6.Page_LocalStorage.png)

**Index DB:**
![shared content](Images/7.Page_IndexDB.png)


## Further reading

- [Will update once the document is confirmed](https://Needs_To_Be_Added)

<img src="https://pnptelemetry.azurewebsites.net/microsoft-teams-samples/samples/cookie-app-js" />