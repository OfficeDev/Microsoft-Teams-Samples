## Create the resource group

The resource group and the service plan aren't strictly necessary, but they allow you to conveniently release the resources you create. This is good practice for keeping your resources organized and manageable.

You use a resource group to create individual resources for the Bot Framework. For performance, ensure that these resources are located in the same Azure region.

1. In your browser, sign into the [Azure portal](https://portal.azure.com/).
1. In the left navigation panel, select **Resource groups**.
1. In the upper left of the displayed window, select **Add** tab to create a new resource group. You'll be prompted to provide the following:
    1. **Subscription**. Use your existing subscription.
    1. **Resource group**. Enter the name for the resource group. An example could be  *TeamsResourceGroup*. Remember that the name must be unique.
    1. From the **Region** drop-down menu, select *West US*, or a region close to your applications.
    1. Select the **Review and create** button. You should see a banner that reads *Validation passed*.
    1. Select the **Create** button. It may take a few minutes to create the resource group.

> **TIP:**
> As with the resources you'll create later in this guide, it's a good idea to pin this resource group to your dashboard for easy access. If you'd like to do so, select the pin icon &#128204; in the upper right of the dashboard.

## Create the service plan

1. In the [Azure portal](https://portal.azure.com/), on the left navigation panel, select **Create a resource**.
1. In the search box, type *App Service Plan*. Select the **App Service Plan** card from the search results.
1. Select **Create**.
1. You'll be asked to provide the following information:
    1. **Subscription**. You can use an existing subscription.
    1. **Resource Group**. Select the group you created earlier.
    1. **Name**. Enter the name for the service plan. An example could be  *MeetingTokensServicePlan*. Remember that the name must be unique, within the group.
    1. **Operating System**. Select *Windows* or your applicable OS.
    1. **Region**. Select *West US* or a region close to your applications.
    1. **Pricing Tier**. Make sure that *Standard S1* is selected. This should be the default value.
    1. Select the **Review and create** button. You should see a banner that reads *Validation passed*.
    1. Select **Create**. It may take a few minutes to create the app service plan. The plan will be listed in the resource group.

## Create the bot channels registration

The bot channels registration registers your web service as a bot with the Bot Framework, provided you have a Microsoft App Id and App password (client secret).

> **IMPORTANT:**
> You only need to register your bot if it is not hosted in Azure. If you [created a bot](https://docs.microsoft.com/en-us/azure/bot-service/abs-quickstart?view=azure-bot-service-4.0&viewFallbackFrom=azure-bot-service-3.0&preserve-view=true) through the Azure portal then it is already registered with the service. If you created your bot through the [Bot Framework](https://dev.botframework.com/bots/new) or [AppStudio](https://docs.microsoft.com/en-us/microsoftteams/platform/concepts/build-and-test/app-studio-overview) your bot isn't registered in Azure.

1. In the [Azure portal](https://portal.azure.com/), under Azure services, select **Create a resource**.
1. In the search box enter "bot". And in the drop-down list, select **Bot Channels Registration**.
1. Select the **Create** button.
1. In the **Bot Channel Registration** blade, provide the requested information about your bot.
1. In the **Messaging endpoint** box, use same ngrok endpoint created earlier and append `/api/messages` to that endpoint. Example: `https://f631****.ngrok.io/api/messages`. 
    - The following picture shows an example of the registration settings:

    ![bot app channels registration](https://user-images.githubusercontent.com/85108465/123810977-77f23c00-d910-11eb-8442-3a61ed57f00d.png)

1. Click **Microsoft App ID and password** and then **Create New**.
1. Click **Create App ID in the App Registration Portal** link.
1. In the displayed **App registration** window, click the **New registration** tab in the upper left.
1. Enter the name of the bot application you are registering, we used *BotTeamsAuth* (you need to select your own unique name).
1. For the **Supported account types** select *Accounts in any organizational directory (Any Azure AD directory - Multitenant) and personal Microsoft accounts (e.g. Skype, Xbox)*.
1. Click the **Register** button. Once completed, Azure displays the *Overview* page for the application.
1. Copy and save to a file the **Application (client) ID** value.
1. In the left panel, click **Certificate and secrets**.
    1. Under *Client secrets*, click **New client secret**.
    1. Add a description to identify this secret from others you might need to create for this app.
    1. Set *Expires* to your selection.
    1. Click **Add**.
    1. Copy the client secret and save it to a file.
1. Go back to the **Bot Channel Registration** window and copy the *App ID* and the *Client secret* in the **Microsoft App ID** and **Password** boxes, respectively.
1. Click **OK**.
1. Finally, click **Create**.

After Azure has created the registration resource it will be included in the resource group list.  

![bot app channels registration group](https://user-images.githubusercontent.com/85108465/123811009-804a7700-d910-11eb-9568-9eb8878cca0b.png)

Once your bot channels registration is created, you'll need to enable the Teams channel.

1. In the [Azure portal](https://portal.azure.com/), under Azure services, select the **Bot Channel Registration** you just created.
1. In the left panel, click **Channels**.
1. Click the Microsoft Teams icon, then choose **Save**.


> **NOTE:**
> The Bot Channels Registration resource will show the **Global** region even if you selected West US. This is expected.
