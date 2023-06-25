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
Please find below demo manifest which is deployed on Microsoft Azure and you can try it yourself by uploading the app package (.zip file link below) to your teams and/or as a personal app. (Sideloading must be enabled for your tenant, [see steps here](https://docs.microsoft.com/microsoftteams/platform/concepts/build-and-test/prepare-your-o365-tenant#enable-custom-teams-apps-and-turn-on-custom-app-uploading)).

**Send different formatting on cards:** [Manifest](/samples/bot-formatting-cards/csharp/demo-manifest/bot-formatting-cards.zip)

## Prerequisites

- Microsoft Teams is installed and you have an account (not a guest account).

- [.NET Core SDK](https://dotnet.microsoft.com/download) version 6.0

  ```bash
  # determine dotnet version
  dotnet --version
  ```
- Publicly addressable https url or tunnel such as [ngrok](https://ngrok.com/) or [Tunnel Relay](https://github.com/OfficeDev/microsoft-teams-tunnelrelay)

## Setup

1. Register a new application in the [Azure Active Directory – App Registrations](https://go.microsoft.com/fwlink/?linkid=2083908) portal.

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
   - While registering the bot, use `https://<your_ngrok_url>/api/messages` as the messaging endpoint.

3. Setup NGROK  
   Run ngrok - point to port 3978

    ```bash
    ngrok http 3978 --host-header="localhost:3978"
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
  
- Update the `appsettings.json` configuration file and replace with placeholder `{{Microsoft-App-Id}}` and `{{Microsoft-App-Password}}`. (Note the MicrosoftAppId is the AppId created in step 1 (Setup AAD app registration in your Azure portal), the MicrosoftAppPassword is referred to as the "client secret" in step 1 (Setup for Bot) and you can always create a new client secret anytime.)

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
**Note:**
-   If you are facing any issue in your app,  [please uncomment this line](https://github.com/OfficeDev/Microsoft-Teams-Samples/blob/fb5beb01271099430655ea0e56e8b6230c0e424e/samples/bot-formatting-cards/csharp/BotFormattingCards/AdapterWithErrorHandler.cs#L27) and put your debugger for local debug.

5. Setup Manifest for Teams
- __*This step is specific to Teams.*__
    - **Edit** the `manifest.json` contained in the ./Manifest folder to replace your MicrosoftAppId (that was created when you registered your app registration earlier) *everywhere* you see the place holder string `{{Microsoft-App-Id}}` (depending on the scenario the Microsoft App Id may occur multiple times in the `manifest.json`)
    - **Edit** the `manifest.json` for `validDomains` and replace `{{domain-name}}` with base Url of your domain. E.g. if you are using ngrok it would be `https://1234.ngrok-free.app` then your domain-name will be `1234.ngrok-free.app`.
    - **Zip** up the contents of the `Manifest` folder to create a `manifest.zip` (Make sure that zip file does not contains any subfolder otherwise you will get error while uploading your .zip package)

- Upload the manifest.zip to Teams (in the Apps view click "Upload a custom app")
   - Go to Microsoft Teams. From the lower left corner, select Apps
   - From the lower left corner, choose Upload a custom App
   - Go to your project directory, the ./AppPackage folder, select the zip folder, and choose Open.
   - Select Add in the pop-up dialog box. Your app is uploaded to Teams.

## Running the sample

**Install App:**

![InstallApp](BotFormattingCards/Images/InstallApp.png)

**Welcome Message:**

![WelcomeMessage](BotFormattingCards/Images/WelcomeMessage.png)

**Type Of Cards:**

![TypeOfCards](BotFormattingCards/Images/TypeOfCards.png)

**Mention Card:**

![MentionCard](BotFormattingCards/Images/MentionCard.png)

**Information Mask Card:**

![InformationMaskCard](BotFormattingCards/Images/InformationMaskCard.png)

**FullWidth Adaptive Card:**

![FullWidthCard](BotFormattingCards/Images/FullWidthCard.png)

**Stage View Card:**

![StageViewCard](BotFormattingCards/Images/StageViewCard.png)

**Overflow Menu Card:**

![OverflowMenuCard](BotFormattingCards/Images/OverflowMenuCard.png)

**HTML Connector Card:**

![HTMLFormatCard](BotFormattingCards/Images/HTMLFormatCard.png)

**AdaptiveCard With Emoji:**

![CardWithEmoji](BotFormattingCards/Images/CardWithEmoji.png)

## Deploy the bot to Azure

To learn more about deploying a bot to Azure, see [Deploy your bot to Azure](https://aka.ms/azuredeployment) for a complete list of deployment instructions.

## Further reading
- [Format cards in Microsoft Teams](https://learn.microsoft.com/en-us/microsoftteams/platform/task-modules-and-cards/cards/cards-format?tabs=adaptive-md%2Cdesktop%2Cconnector-html)
- [Format cards with HTML](https://learn.microsoft.com/en-us/microsoftteams/platform/task-modules-and-cards/cards/cards-format?tabs=adaptive-md%2Cdesktop%2Cconnector-html#format-cards-with-html)


<img src="https://pnptelemetry.azurewebsites.net/microsoft-teams-samples/samples/bot-formatting-cards-csharp" />