---
page_type: sample
description: This is a sample application which demonstrates how to use CART link to send live captions in the meeting.
products:
- office-teams
- office
- office-365
languages:
- nodejs
- javascript
extensions:
contentType: samples
createdDate: "24-06-2022 00:15:15"
---

# Meeting side panel application uses CART link to send caption in live meeting.

This is a sample meeting side panel application which demonstrates how to enable live caption in the meeting and using the CART link how to send caption in live meeting. Meeting side panel application uses CART link to send caption in live meeting.

## Enable CART Captions
Once the meeting is scheduled. Follow this doc to enable [CART Catptions](https://support.microsoft.com/office/use-cart-captions-in-a-microsoft-teams-meeting-human-generated-captions-2dd889e8-32a8-4582-98b8-6c96cf14eb47).
Copy the CART link it will used while configuring tab for meeting.

## Key features

1. Schedule the meeting and add Meeting Caption Tab in that particular scheduled meeting.
![Add Tab](Images/AddMeetingCaption.png)
2. Once meeting started, turn on live caption.
![Start live caption](Images/TurnOnLiveCaption.png)
3. Once the live caption has started, you can use the app to send live caption.
![Send live caption](Images/MeetingCaptionSidePanel.png)
4. After clicking on `Submit` button, you will see the caption in the meeting.
![Caption in meeting](Images/LiveCaption.png)

![Key Features](Images/MeetingCaption.gif)

## Prerequisites

- Microsoft Teams is installed and you have an account (not a guest account)
-  [NodeJS](https://nodejs.org/en/)
-  [ngrok](https://ngrok.com/) or equivalent tunneling solution
-  [M365 developer account](https://docs.microsoft.com/en-us/microsoftteams/platform/concepts/build-and-test/prepare-your-o365-tenant) or access to a Teams account with the appropriate permissions to install an app.

## To try this sample

> Note these instructions are for running the sample on your local machine, the tunnelling solution is required because
> the Teams service needs to call the tab.

### 1. Start ngrok on localhost:3978
- Open ngrok and run command `ngrok http -host-header=rewrite 3978` 
- Once started you should see URL  `https://41ed-abcd-e125.ngrok.io`. Copy it, this is your baseUrl that will used as endpoint for Azure bot and webhook.

![Ngrok](Images/NgrokScreenshot.png)

### 2. Manually update the manifest.json
- Edit the `manifest.json` contained in the  `/appPackage` folder to and fill in App Domain (ngrok Url that was created in step 1) Update *everywhere* you see the place holder string `<<App-Domain>>` (depending on the scenario it may occur multiple times in the `manifest.json`)
- Zip up the contents of the `/appPackage` folder to create a `manifest.zip`
- Upload the manifest.zip to Teams (in the Apps view click "Upload a custom app")

### 3. Run your tab sample
1) Clone the repository

    ```bash
    git clone https://github.com/OfficeDev/Microsoft-Teams-Samples.git
    ```

2) In a terminal, navigate to `samples/meetings-live-caption/nodejs`

3) Install modules

    ```bash
    npm install
    ```

**NOTE: If you are unable to send caption, try configuring tab again.**

