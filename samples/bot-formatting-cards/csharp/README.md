---
page_type: sample
description: Sample which demonstrates different formatting supported in cards using bot.
products:
- office-teams
- office
- office-365
languages:
- csharp
extensions:
 contentType: samples
 createdDate: "01/30/2023 05:00:17 PM"
urlFragment: officedev-microsoft-teams-samples-bot-formatting-cards-csharp

---
## Different formatting on cards

This sample feature shows how to use different formatting on cards using bot.

## Included Features
* Bots
* Adaptive Cards

## Interaction with app

![Types Of Cards](BotFormattingCards/Images/DifferentFormattingCards.gif)

## Try it yourself - experience the App in your Microsoft Teams client
Please find below demo manifest which is deployed on Microsoft Azure and you can try it yourself by uploading the app manifest (.zip file link below) to your teams and/or as a personal app. (Sideloading must be enabled for your tenant, [see steps here](https://docs.microsoft.com/microsoftteams/platform/concepts/build-and-test/prepare-your-o365-tenant#enable-custom-teams-apps-and-turn-on-custom-app-uploading)).

**Send different formatting on cards:** [Manifest](/samples/bot-formatting-cards/csharp/demo-manifest/bot-formatting-cards.zip)

## Prerequisites

- Microsoft Teams is installed and you have an account (not a guest account).

- [.NET Core SDK](https://dotnet.microsoft.com/download) version 6.0

  ```bash
  # determine dotnet version
  dotnet --version
  ```
- Publicly addressable https url or tunnel such as [dev tunnel](https://learn.microsoft.com/en-us/azure/developer/dev-tunnels/get-started?tabs=windows) or [ngrok](https://ngrok.com/) latest version or [Tunnel Relay](https://github.com/OfficeDev/microsoft-teams-tunnelrelay)
- [Teams Toolkit for Visual Studio](https://learn.microsoft.com/en-us/microsoftteams/platform/toolkit/toolkit-v4/install-teams-toolkit-vs?pivots=visual-studio-v17-7)

## Run the app (Using Teams Toolkit for Visual Studio)

The simplest way to run this sample in Teams is to use Teams Toolkit for Visual Studio.
1. Install Visual Studio 2022 **Version 17.10 Preview 4 or higher** [Visual Studio](https://visualstudio.microsoft.com/downloads/)
1. Install Teams Toolkit for Visual Studio [Teams Toolkit extension](https://learn.microsoft.com/en-us/microsoftteams/platform/toolkit/toolkit-v4/install-teams-toolkit-vs?pivots=visual-studio-v17-7)
1. In the debug dropdown menu of Visual Studio, select Dev Tunnels > Create A Tunnel (set authentication type to Public) or select an existing public dev tunnel.
1. In the debug dropdown menu of Visual Studio, select default startup project > **Microsoft Teams (browser)**
1. In Visual Studio, right-click your **TeamsApp** project and **Select Teams Toolkit > Prepare Teams App Dependencies**
1. Using the extension, sign in with your Microsoft 365 account where you have permissions to upload custom apps.
1. Select **Debug > Start Debugging** or **F5** to run the menu in Visual Studio.
1. In the browser that launches, select the **Add** button to install the app to Teams.
> If you do not have permission to upload custom apps (sideloading), Teams Toolkit will recommend creating and using a Microsoft 365 Developer Program account - a free program to get your own dev environment sandbox that includes Teams.

## Setup

1. Register a new application in the [Microsoft Entra ID – App Registrations](https://go.microsoft.com/fwlink/?linkid=2083908) portal.

    1) Select **New Registration** and on the *register an application page*, set following values:
        * Set **name** to your app name.
        * Choose the **supported account types** (any account type will work)
        * Leave **Redirect URI** empty.
        * Choose **Register**.

    2) On the overview page, copy and save the **Application (client) ID, Directory (tenant) ID**. You’ll need those later when updating your Teams application manifest and in the appsettings.json.

    3) Navigate to the **Certificates & secrets**. In the Client secrets section, click on "+ New client secret". Add a description (Name of the secret) for the secret and select “Never” for Expires. Click "Add". Once the client secret is created, copy its value, it need to be placed in the appsettings.json.

2. Setup for Bot
    - In Azure portal, create a [Azure Bot resource](https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-authentication?view=azure-bot-service-4.0&tabs=csharp%2Caadv2).
   - Ensure that you've [enabled the Teams Channel](https://docs.microsoft.com/en-us/azure/bot-service/channel-connect-teams?view=azure-bot-service-4.0)
   - While registering the bot, use `https://<your_tunnel_domain>/api/messages` as the messaging endpoint.

3. Run ngrok - point to port 3978

   ```bash
   ngrok http 3978 --host-header="localhost:3978"
   ```  

   Alternatively, you can also use the `dev tunnels`. Please follow [Create and host a dev tunnel](https://learn.microsoft.com/en-us/azure/developer/dev-tunnels/get-started?tabs=windows) and host the tunnel with anonymous user access command as shown below:

   ```bash
   devtunnel host -p 3978 --allow-anonymous
   ```

4. Setup for code  
   - Clone the repository

    ```bash
    git clone https://github.com/OfficeDev/Microsoft-Teams-Samples.git
    ```
    Run the bot from a terminal or from Visual Studio:

  A) From a terminal, navigate to `samples/bot-formatting-cards/csharp/BotFormattingCards`

     ```bash
     # run the bot
     dotnet run
     ```
     
  B) Or from Visual Studio

    - Launch Visual Studio
    - File -> Open -> Project/Solution
    - Navigate to `samples/bot-formatting-cards/csharp/BotFormattingCards` folder
    - Select `BotFormattingCards.csproj` file
    - Press `F5` to run the project   
  
- Update the `appsettings.json` configuration file and replace with placeholder `{{Microsoft-App-Id}}` and `{{Microsoft-App-Password}}`. (Note the MicrosoftAppId is the AppId created in step 1 (Setup Microsoft Entra ID app registration in your Azure portal), the MicrosoftAppPassword is referred to as the "client secret" in step 1 (Setup for Bot) and you can always create a new client secret anytime.)

**Update mentionSupport json**
- Bots support user mention with the Azure AD Object ID and UPN, in addition to the existing IDs. The support for two new IDs is available in bots for text messages, Adaptive Cards body, and message extension response. Bots support the mention IDs in conversation and invoke scenarios. The user gets activity feed notification when being @mentioned with the IDs.

   - Navigate to samples\bot-formatting-cards\csharp\BotFormattingCards\Resources\mentionSupport.json
      1) On line 14, replace {{new-Ids}}  
      2) On line 23, replace {{Email-Id}}
      3) On line 31, replace {{Microsoft-App-Id}}
        - E.g. 
        ```
        "text": "Hi <at>Adele UPN</at>, <at>Adele Azure AD</at>"
            }
        ],
        "msteams": {
            "entities": [
            {
                "type": "mention",
                "text": "<at>Adele UPN</at>",
                "mentioned": {
                "id": "AdeleV@contoso.onmicrosoft.com",
                "name": "Adele Vance"
                }
            },
            {
                "type": "mention",
                "text": "<at>Adele Azure AD</at>",
                "mentioned": {
                "id": "87d349ed-44d7-43e1-9a83-5f2406dee5bd",
                "name": "Adele Vance"
                }
            }
            ]
        ```
   
   **Note: In adaptive card, what we are defining (User details) should be exist in the same tenant where you are testing the app (teams' login) etc...**
  - Update the user Microsoft Entra object ID in your adaptive card JSON from your tenant's Microsoft Entra ID users available in the Azure portal.
    - Navigate to samples\bot-formatting-cards\csharp\BotFormattingCards\Resources\adaptivePeoplePersonaCardIcon.json
      1) On line 16, replace {{User-Object-ID}}  
      2) On line 17, replace {{User-Display-Name}}
      3) On line 18, replace {{User-Principal-Name}}

        - E.g. 
        ```
        "properties": {
        "id": "87d349ed-xxxx-434a-9e14-xxxx",
        "displayName": "Joe Smith",
        "userPrincipalName": "JoeSmith@xxxx.com"
      }
        ```
    - Navigate to samples\bot-formatting-cards\csharp\BotFormattingCards\Resources\adaptivePeoplePersonaCardSetIcon.json
      1) On line 18, replace {{User-Object-ID}}  
      2) On line 19, replace {{User-Display-Name}}
      3) On line 20, replace {{User-Principal-Name}}
      4) On line 23, replace {{User-Object-ID}}  
      5) On line 24, replace {{User-Display-Name}}
      6) On line 25, replace {{User-Principal-Name}}
      
        - E.g. 
        ```
      "properties": {
        "users": [
          {
            "id": "95d349ed-xxxx-434a-9e14-xxxx",
            "displayName": "Vance Agrawal",
            "userPrincipalName": "VanceAgrawal@xxxx.com"
          },
          {
            "id": "45d349ed-xxxx-434a-9e14-xxxx",
            "displayName": "ku Mao",
            "userPrincipalName": "kuMao@xxxx.com"
          }
        ]
      }
        ```
**Note:**
-   If you are facing any issue in your app,  [please uncomment this line](https://github.com/OfficeDev/Microsoft-Teams-Samples/blob/fb5beb01271099430655ea0e56e8b6230c0e424e/samples/bot-formatting-cards/csharp/BotFormattingCards/AdapterWithErrorHandler.cs#L27) and put your debugger for local debug.

5. Setup Manifest for Teams
- __*This step is specific to Teams.*__
    - **Edit** the `manifest.json` contained in the ./appPackage folder to replace your MicrosoftAppId (that was created when you registered your app registration earlier) *everywhere* you see the place holder string `{{Microsoft-App-Id}}` (depending on the scenario the Microsoft App Id may occur multiple times in the `manifest.json`)
    - **Edit** the `manifest.json` for `validDomains` and replace `{{domain-name}}` with base Url of your domain. E.g. if you are using ngrok it would be `https://1234.ngrok-free.app` then your domain-name will be `1234.ngrok-free.app` and if you are using dev tunnels then your domain will be like: `12345.devtunnels.ms`.
    - **Zip** up the contents of the `appPackage` folder to create a `manifest.zip` (Make sure that zip file does not contains any subfolder otherwise you will get error while uploading your .zip package)

- Upload the manifest.zip to Teams (in the Apps view click "Upload a custom app")
   - Go to Microsoft Teams. From the lower left corner, select Apps
   - From the lower left corner, choose Upload a custom App
   - Go to your project directory, the ./appPackage folder, select the zip folder, and choose Open.
   - Select Add in the pop-up dialog box. Your app is uploaded to Teams.

## Running the sample

**Install App:**

![InstallApp](BotFormattingCards/Images/1.InstallApp.png)

**Welcome Message:**

![WelcomeMessage](BotFormattingCards/Images/2.WelcomeMessage.png)

**Type Of Cards:**

![TypeOfCards](BotFormattingCards/Images/3.TypeOfCards.png)

**Mention Card:**

![MentionCard](BotFormattingCards/Images/4.MentionCard.png)

**Information Mask Card:**

![InformationMaskCard](BotFormattingCards/Images/5.InformationMaskCard.png)

**FullWidth Adaptive Card:**

![FullWidthCard](BotFormattingCards/Images/6.FullWidthCard.png)

**Stage View Card:**

![StageViewCard](BotFormattingCards/Images/7.StageViewCard.png)

**Overflow Menu Card:**

![OverflowMenuCard](BotFormattingCards/Images/8.OverflowMenuCard.png)

**HTML Connector Card:**

![HTMLFormatCard](BotFormattingCards/Images/9.HTMLFormatCard.png)

**AdaptiveCard With Emoji:**

![CardWithEmoji](BotFormattingCards/Images/10.CardWithEmoji.png)

**Persona Card Icon:**

![PersonaCardIcon](BotFormattingCards/Images/12.PersonaCardIcon.png)

**Persona Set Icon:**

![PersonaCardSetIcon](BotFormattingCards/Images/13.PersonaCardSetIcon.png)

**Adaptive Card updated to be responsive using targetWidth:**

![Layout](BotFormattingCards/Images/14.Layout.png)

**Mobile:**

![Layout](BotFormattingCards/Images/15.LayoutMobile.png)

## Deploy the bot to Azure

To learn more about deploying a bot to Azure, see [Deploy your bot to Azure](https://aka.ms/azuredeployment) for a complete list of deployment instructions.

## Further reading
- [Format cards in Microsoft Teams](https://learn.microsoft.com/en-us/microsoftteams/platform/task-modules-and-cards/cards/cards-format?tabs=adaptive-md%2Cdesktop%2Cconnector-html)
- [Format cards with HTML](https://learn.microsoft.com/en-us/microsoftteams/platform/task-modules-and-cards/cards/cards-format?tabs=adaptive-md%2Cdesktop%2Cconnector-html#format-cards-with-html)
- [People icon in an Adaptive Card](https://learn.microsoft.com/en-us/microsoftteams/platform/task-modules-and-cards/cards/cards-format?tabs=adaptive-md%2Cdesktop%2Cconnector-html#people-icon-in-an-adaptive-card)


<img src="https://pnptelemetry.azurewebsites.net/microsoft-teams-samples/samples/bot-formatting-cards-csharp" />