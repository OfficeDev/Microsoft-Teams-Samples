# ContentBubbleSample
This sample illustrates how to implement Content Bubble In-Meeting Experience.

## Prerequisites

- Microsoft Teams is installed and you have an account
- [.NET Core SDK](https://dotnet.microsoft.com/download) version 3.1
- [ngrok](https://ngrok.com/) or equivalent tunnelling solution

1) Clone the repository

    ```bash
    git clone https://github.com/OfficeDev/Microsoft-Teams-Samples.git
    ```

2) If you are using Visual Studio
  - Launch Visual Studio
  - File -> Open -> Project/Solution
  - Navigate to `apps-in-meeting/csharp` folder
  - Navigate to `ContentBubbleBot` folder
  - Select `ContentBubbleBot.csproj` file

3) Run ngrok - point to port 3978

    ```bash
    ngrok http -host-header=rewrite 3978
    ```
### Create a bot with Azure Bot Service 
1. This bot has been created using [Bot Framework](https://dev.botframework.com).
2. Navigate to app registration page, in the overview section, copy and save the **Application (client) ID**. You’ll need those later when updating your Teams application manifest and in the appsettings.json.
3. Under **Manage**, select **Expose an API**. 
4. Select the **Set** link to generate the Application ID URI in the form of `api://{AppID}`. Insert your fully qualified domain name (with a forward slash "/" appended to the end) between the double forward slashes and the GUID. The entire ID should have the form of: `api://fully-qualified-domain-name/{AppID}`
    * ex: `api://%ngrokDomain%.ngrok.io/00000000-0000-0000-0000-000000000000`.
5. Navigate to **API Permissions**, and make sure to add the follow permissions:
-   Select Add a permission
-   Select Microsoft Graph -\> Delegated permissions.
    * User.Read (enabled by default)
    * email
    * offline_access
    * OpenId
    * profile
-   Click on Add permissions. Please make sure to grant the admin consent for the required permissions.
6. Navigate to the **Certificates & secrets**. In the Client secrets section, click on "+ New client secret". Add a description      (Name of the secret) for the secret and select “Never” for Expires. Click "Add". Once the client secret is created, copy its value, it need to be placed in the appsettings.json.
 7. __*This step is specific to Teams.*__
    - **Edit** the `manifest.json` contained in the `Manifest` folder to replace your Microsoft App Id (that was created when you registered your bot earlier) *everywhere* you         see the place holder string `<<YOUR-MICROSOFT-APP-ID>>` (depending on the scenario the Microsoft App Id may occur multiple times in the `manifest.json`)
    - **Add** the ngrok domain to the valid domains array in the manifest. 
    - **Zip** up the contents of the `teamsAppManifest` folder to create a `manifest.zip`
    - **Upload** the `manifest.zip` to Teams (in the Apps view click "Upload a custom app")
 8. __*Update External resource URL*__ in `ContentBubbleBot.cs` file. **Replace** AppID, BotID with MicrosoftAppID and Url with ngrok Url.
 9. Run your app, either from Visual Studio with `F5` or using `dotnet run` in the appropriate folder. 

## Interacting with the app in Teams Meeting

Install the App in Teams Meeting, message the Bot by @ mentioning to interact with the content bubble.
