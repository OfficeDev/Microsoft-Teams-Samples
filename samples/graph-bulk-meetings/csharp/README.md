---
page_type: sample
description: This is a sample application which demonstrates how to create multiple meeting using excel sheet upload.
products:
- office-teams
- office
- office-365
languages:
- csharp
extensions:
contentType: samples
createdDate: "07-09-2022 00:02:15"
---

# This is a sample application that shows the usage of create meeting to upload excel sheet.

This is a sample application where user can create, multiple meeting using excel sheet to  be performed within this sample.

## Key features

1. Meeting Event.

![All Meeting List](EventMeeting/Images/Dashboard.png)

2. Upload excel sheet to create Meeting event.

![Create Event Meeting](EventMeeting/Images/UploadExcelSheet.PNG)


## Prerequisites

- Microsoft Teams is installed and you have an account (not a guest account)
-  .[NET 6.0](https://dotnet.microsoft.com/en-us/download) SDK.
    ```bash
        # determine dotnet version
        dotnet --version
    ```
-  [ngrok](https://ngrok.com/) or equivalent tunneling solution
-  [M365 developer account](https://docs.microsoft.com/en-us/microsoftteams/platform/concepts/build-and-test/prepare-your-o365-tenant) or access to a Teams account with the appropriate permissions to install an app.

## Run app locally

### Register your application with Azure AD

1. Register a new application in the [Azure Active Directory – App Registrations](https://go.microsoft.com/fwlink/?linkid=2083908) portal.
2. On the overview page, copy and save the **Application (client) ID, Directory (tenant) ID**. You’ll need those later when updating your Teams application manifest and in the appsettings.json.
3. Navigate to **API Permissions**, and make sure to add the follow permissions:
-   Select Add a permission
-   Select Microsoft Graph -> Application permissions.
   - `Calendars.ReadWrite`

-   Click on Add permissions. Please make sure to grant the admin consent for the required permissions.

4.  Navigate to the **Certificates & secrets**. In the Client secrets section, click on "+ New client secret". Add a description (Name of the secret) for the secret and select “Never” for Expires. Click "Add". Once the client secret is created, copy its value, it need to be placed in the appsettings.json file.


## To try this sample

> Note these instructions are for running the sample on your local machine, the tunnelling solution is required because
> the Teams service needs to call into the app.

### 1. Clone the repository
   ```bash
   git clone https://github.com/OfficeDev/Microsoft-Teams-Samples.git
   ```

### 2. Launch Visual Studio
   - File -> Open -> Project/Solution
   - Navigate to folder where repository is cloned then `samples/graph-meetings-bulks/csharp/EventMeeting.sln`
    
### 3. Start ngrok on localhost:3978
- Open ngrok and run command `ngrok http -host-header=rewrite 3978` 
- Once started you should see link  `https://41ed-abcd-e125.ngrok.io`. Copy it, this is your baseUrl that will used as endpoint for Azure bot.


![Ngrok](EventMeeting/Images/NgrokScreenshot.png)

### 4. Update appsettings.json
Update configuration with the ```MicrosoftAppId```,  ```MicrosoftAppPassword``` and ```MicrosoftAppTenantId```.

### 5. Modify the `manifest.json` in the `/AppPackage` folder 
Replace the following details:
- `{{APP-ID}}` with any guid id value.
- `{{BASE-URL}}` with base Url domain. E.g. if you are using ngrok it would be `https://1234.ngrok.io` then your domain-name will be `1234.ngrok.io`.
- **Zip** up the contents of the `Manifest` folder to create a `manifest.zip`
- **Upload** the `manifest.zip` to Teams (in the Apps view click "Upload a custom app")

## Features of this sample

1. User can see list of Meeting Event.
![Event Meeting List](EventMeeting/Images/Dashboard.PNG)
2. User can create new Meeting Events.
![Create new Meeting Event](EventMeeting/Images/UploadExcelSheet.PNG)

## Further reading
- [Event resource type](https://docs.microsoft.com/en-us/graph/api/user-post-events?view=graph-rest-1.0&tabs=http)
