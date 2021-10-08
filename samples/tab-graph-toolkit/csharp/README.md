## Teams tab with microsoft Graph Toolkit

This is the demo app for [Teams tab using miscrosoftgraph toolkit](https://docs.microsoft.com/en-us/graph/toolkit/get-started/build-a-microsoft-teams-tab?tabs=unpkg%2Cjs)

![Login](TabGraphToolkit/Images/login.png)

![agenda](TabGraphToolkit/Images/agenda.png)

![people-picker](TabGraphToolkit/Images/people-picker.png)

![tasks](TabGraphToolkit/Images/tasks.png)

![todo](TabGraphToolkit/Images/todo.png)

![person-card](TabGraphToolkit/Images/person-card.png)

## Prerequisites

- [.NET Core SDK](https://dotnet.microsoft.com/download) version 3.1

  determine dotnet version
  ```bash
  dotnet --version
  ```
- [Ngrok](https://ngrok.com/download) (For local environment testing) Latest (any other tunneling software can also be used)
  
  run ngrok locally
  ```bash
 ngrok http https://localhost:3000 -host-header="localhost:3000"
  ```
  
-  [NodeJS](https://nodejs.org/en/)

-  [M365 developer account](https://docs.microsoft.com/en-us/microsoftteams/platform/concepts/build-and-test/prepare-your-o365-tenant) or access to a Teams account with the appropriate permissions to install an app.

## To try this sample

 1) Configuring MSAL2.0 Auth Provider
 - Register your app with Microsoft identity platform via the Azure AD portal
 - Your app must be registered in the Azure AD portal to integrate with the Microsoft identity platform. See [Register an application with the Microsoft identity platform](https://docs.microsoft.com/en-us/graph/auth-register-app-v2). .
 - Click on Add a Platform in redirect URI section.
 - Select Single Page Application and add following URL `https://localhost:3000/`
 - Save and register.
 - Once App is registerd copy the `client_Id` for your app and update in the app.

2) Clone the repository
   ```bash
   git clone https://github.com/OfficeDev/Microsoft-Teams-Samples.git
   ```
   
3) In a terminal, navigate to `samples/tab-graph-toolkit/csharp`

    change into project folder
    ```bash
    cd # TabGraphToolkit
    ```
4) Install modules
 navigate to `samples/tab-graph-toolkit/csharp/TabGraphToolkit/clientapp`

    ```bash
    npm install
    ```
5) Update `client_Id` copied from step 1 in index.tsx and index.html file.  
 
6) Modify the `manifest.json` in the `/appPackage` folder and replace the following details:
  - `{{Microsoft-App-Id}}` with Application id generated from Step 1
  - `{{base-URl}}` with base Url domain. E.g. if you are using ngrok it would be `1234.ngrok.io`

7) Zip the contents of `appPackage` folder into a `manifest.zip`, and use the `manifest.zip` to deploy in app store or add to Teams using step 9.

8) Run the bot from a terminal or from Visual Studio, choose option A or B.
 
   A) From a terminal
     ```bash
     # run the bot
     dotnet run
     ```

   B) Or from Visual Studio
     - Launch Visual Studio
     - File -> Open -> Project/Solution
     - Navigate to `TabGraphToolkit` folder
     - Select `TabGraphToolkit.csproj` file
     - Press `F5` to run the project 

9) Upload the manifest.zip to Teams (in the Apps view click "Upload a custom app")
   - Go to Microsoft Teams. From the lower left corner, select Apps
   - From the lower left corner, choose Upload a custom App
   - Go to your project directory, the ./appPackage folder, select the zip folder, and choose Open.
   - Select Add in the pop-up dialog box. Your app is uploaded to Teams.
   
## Features of this sample
- Once you access the Tab within your app you will be able to see following microsoft-graph-toolkit component. 

- <mgt-login>

![Login](TabGraphToolkit/Images/login.png)

- <mgt-agenda>

![agenda](TabGraphToolkit/Images/agenda.png)

- <mgt-people-picker>

![people-picker](TabGraphToolkit/Images/people-picker.png)

- <mgt-tasks>

![tasks](TabGraphToolkit/Images/tasks.png)

- <mgt-todo>

![todo](TabGraphToolkit/Images/todo.png)

- <mgt-person-card>

![person-card](TabGraphToolkit/Images/person-card.png)

## Deploy to Teams
Start debugging the project by hitting the `F5` key or click the debug icon in Visual Studio Code and click the `Start Debugging` green arrow button.

### NOTE: First time debug step
On the first time running and debugging your app you need allow the localhost certificate.  After starting debugging when Chrome is launched and you have installed your app it will fail to load.

- Open a new tab `in the same browser window that was opened`
- Navigate to `https://localhost:3000/`
- Click the `Advanced` button
- Select the `Continue to localhost`

### NOTE: Debugging
Ensure you have the Debugger for Chrome/Edge extension installed for Visual Studio Code from the marketplace.

### Build for production
`npm run build`

Builds the app for production to the `build` folder.\
It correctly bundles React in production mode and optimizes the build for the best performance.

The build is minified and the filenames include the hashes.\
Your app is ready to be deployed!

See the section about [deployment](https://facebook.github.io/create-react-app/docs/deployment) for more information.
