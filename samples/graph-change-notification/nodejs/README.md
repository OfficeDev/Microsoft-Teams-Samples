---
page_type: sample
description: This sample app demonstrates sending change notifications to user presence in Teams based on user presence status.
urlFragment: teams-graph-change-notification-nodejs
products:
- office-teams
- office
- office-365
languages:
- nodejs
extensions:
contentType: samples
createdDate: "07-07-2021 13:38:26"
---

# Change Notification sample using nodejs

Bot Framework v4 ChangeNotification sample.

This sample app demonstrates sending notifications to users when presence status is changed.


## Prerequisites

- Microsoft Teams is installed and you have an account
- [Node.js](https://nodejs.org) version 10.14 or higher

    ```bash
    #determine node version
    node --version
    ```
- [ngrok](https://ngrok.com/) or equivalent tunnelling solution

1. Clone the repository

    ```bash
    git clone https://github.com/OfficeDev/Microsoft-Teams-Samples.git
    ```
1. Open from Visual Studio code
    - Launch Visual Studio code
    - File -> Open Folder
    - Navigate to `samples/graph-change-notification/nodejs` folder
    - Run npm command  in the terminal
        ```bash
        npm install
        ``` 
    - Press `F5` to run the project

3. Run ngrok - point to port 3978

   ```bash
     ngrok http -host-header=rewrite 3978
   ```  

### Instruction on setting connection string for bot authentication on the behalf of user
1. In the Azure portal, select your resource group from the dashboard.

2. Select your bot channel registration link.

3. Open the resource page and select Configuration under Settings.

4. Select Add OAuth Connection Settings.

    ![image](https://user-images.githubusercontent.com/85864414/121879805-df15cb00-cd2a-11eb-8076-1236ccb1bbfc.PNG)
5. Complete the form as follows:

    ![image](https://user-images.githubusercontent.com/85864414/122000240-1d16fb80-cdcc-11eb-8aeb-a1dc898f947e.PNG)

a. Enter a name for the connection. You'll use this name in your bot in the appsettings.json file. For example BotTeamsAuthADv1.

b. Service Provider. Select Azure Active Directory. Once you select this, the Azure AD-specific fields will be displayed.

c. Client id. Enter the Application (client) ID that you recorded for your Azure identity provider app in the steps above.

d. Client secret. Enter the secret that you recorded for your Azure identity provider app in the steps above.

e. Grant Type. Enter authorization_code.

f. Login URL. Enter https://login.microsoftonline.com.

g. Tenant ID, enter the Directory (tenant) ID that you recorded earlier for your Azure identity app or common depending on the supported account type selected when you created the identity provider app.
h. For Resource URL, enter https://graph.microsoft.com/
i. Provide  Scopes like "Presence.Read, Presence.Read.All"
![image](https://user-images.githubusercontent.com/85864414/121880473-af1af780-cd2b-11eb-8166-837425ef186f.PNG)

- Ensure that you've [enabled the Teams Channel](https://docs.microsoft.com/en-us/azure/bot-service/channel-connect-teams?view=azure-bot-service-4.0)

### Update the appsetting
1. Update `MicrosoftAppId` and `MicrosoftAppPassword` in the .env that is created in Azure.
2. Add `connectionName` created in step 5.
3. Update `notificationUrl` as  `{NgrokBaseURL}/api/notifications`


### Concepts introduced in this sample
- After sucessfully installation of app you will get a sign in button. When sign in is complete then you get your current status in adapative card
![image](https://user-images.githubusercontent.com/85864414/122000447-741cd080-cdcc-11eb-9833-54f87cd7567f.PNG)
![image](https://user-images.githubusercontent.com/85864414/121878949-ebe5ef00-cd29-11eb-8ab0-683ce3ffbfcb.PNG)

- After that when the user status chagnes you will get notify about their status: 
- Change user status from available to busy like
![image](https://user-images.githubusercontent.com/85864414/121879184-30718a80-cd2a-11eb-88b5-2a422042990b.PNG)
- Change user status from busy to busy offline
 ![image](https://user-images.githubusercontent.com/85864414/121879374-63b41980-cd2a-11eb-8ed4-1b92035ff9c1.PNG)


## Further reading
- [Bot Authentication](https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-authentication?view=azure-bot-service-4.0&tabs=aadv2%2Ccsharp)
- [Change Notification](https://docs.microsoft.com/en-us/graph/api/resources/webhooks?view=graph-rest-beta)
- [App in Catalog](https://docs.microsoft.com/en-us/graph/api/resources/teamsapp?view=graph-rest-1.0)
- [Bot Framework Documentation](https://docs.botframework.com)
- [Bot Basics](https://docs.microsoft.com/azure/bot-service/bot-builder-basics?view=azure-bot-service-4.0)

