# Teams App Localization
This sample illustrates how to implement localization for Teams App.

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
  - Navigate to `app-localization\csharp` folder
  - Select `Localization.csproj` file

3) Run ngrok - point to port 44374

    ```bash
    ngrok http -host-header=rewrite 44374
    ```
    
4) __*This step is specific to Teams.*__
    - **Edit** the `manifest.json` contained in the `Manifest` folder to replace your Microsoft App Id (that was created when you registered your bot earlier) *everywhere* you see the place holder string `<<YOUR-MICROSOFT-APP-ID>>` (depending on the scenario the Microsoft App Id may occur multiple times in the `manifest.json`)
    -**Provide ngrok Url** (turnnelling Url) in `manifest.json` for contentUrl in case of tabs and for messaging endpoint in case of bots if enabled. 
    - **Zip** up the contents of the `teamsAppManifest` folder to create a `manifest.zip`
    - **Upload** the `manifest.zip` to Teams (in the Apps view click "Upload a custom app")

5) Run your app, either from Visual Studio with `F5` or using `dotnet run` in the appropriate folder.

## Interacting with the app in Teams
In Teams, Once the app is succefully installed, you can interact with tab and bot in your preferred language.

#### To change language in Teams
To change the language in Microsoft Teams, please click your profile picture at the top of the app, then select Settings -> General and go to the Language section. Choose the preferred language and restart to apply the change.

#### To Add more languages for localization in Teams through Code.
 
 Add Resource files for the respective languages, Check culture fallback behaviour and how to add other cultures refer [Globalization and localization Fundamentals](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/localization?view=aspnetcore-5.0). 


  
